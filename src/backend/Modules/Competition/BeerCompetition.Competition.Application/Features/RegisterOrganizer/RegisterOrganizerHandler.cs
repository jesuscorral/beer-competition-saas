using MediatR;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Shared.Infrastructure.ExternalServices;
using BeerCompetition.Shared.Kernel;
using Microsoft.Extensions.Logging;

namespace BeerCompetition.Competition.Application.Features.RegisterOrganizer;

/// <summary>
/// Handler for RegisterOrganizerCommand.
/// Creates a new organizer account with tenant, competition, and Keycloak user.
/// Implements atomic transaction - all or nothing.
/// </summary>
public class RegisterOrganizerHandler : IRequestHandler<RegisterOrganizerCommand, Result<OrganizerRegistrationResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<RegisterOrganizerHandler> _logger;

    public RegisterOrganizerHandler(
        ITenantRepository tenantRepository,
        ICompetitionRepository competitionRepository,
        IKeycloakService keycloakService,
        ILogger<RegisterOrganizerHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _competitionRepository = competitionRepository;
        _keycloakService = keycloakService;
        _logger = logger;
    }

    public async Task<Result<OrganizerRegistrationResponse>> Handle(
        RegisterOrganizerCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Registering organizer: {Email}, Organization: {OrganizationName}, Competition: {CompetitionName}",
            request.Email,
            request.OrganizationName,
            request.CompetitionName);

        // Step 1: Check if email already exists
        var existingTenant = await _tenantRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingTenant != null)
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
            return Result<OrganizerRegistrationResponse>.Failure("An organization with this email already exists");
        }

        string? keycloakUserId = null;

        try
        {
            // Step 2: Create Keycloak user
            _logger.LogInformation("Creating Keycloak user for {Email}", request.Email);
            var createUserResult = await _keycloakService.CreateUserAsync(
                request.Email,
                request.Password,
                emailVerified: true,
                enabled: true,
                cancellationToken);

            if (createUserResult.IsFailure)
            {
                _logger.LogError("Failed to create Keycloak user: {Error}", createUserResult.Error);
                return Result<OrganizerRegistrationResponse>.Failure($"Failed to create user account: {createUserResult.Error}");
            }

            keycloakUserId = createUserResult.Value;
            _logger.LogInformation("Keycloak user created: {UserId}", keycloakUserId);

            // Step 3: Assign organizer role
            var assignRoleResult = await _keycloakService.AssignRoleAsync(
                keycloakUserId!,  // Null-forgiving operator - we know it's not null from IsFailure check
                "organizer",
                cancellationToken);

            if (assignRoleResult.IsFailure)
            {
                _logger.LogError("Failed to assign organizer role: {Error}", assignRoleResult.Error);
                await CleanupKeycloakUser(keycloakUserId, cancellationToken);
                return Result<OrganizerRegistrationResponse>.Failure($"Failed to assign role: {assignRoleResult.Error}");
            }

            // Step 4: Create tenant entity
            var tenantResult = Tenant.Create(request.OrganizationName, request.Email);
            if (tenantResult.IsFailure)
            {
                _logger.LogError("Failed to create tenant entity: {Error}", tenantResult.Error);
                await CleanupKeycloakUser(keycloakUserId, cancellationToken);
                return Result<OrganizerRegistrationResponse>.Failure(tenantResult.Error);
            }

            var tenant = tenantResult.Value!;
            await _tenantRepository.AddAsync(tenant, cancellationToken);

            // Step 5: Set tenant_id attribute in Keycloak
            var setAttributeResult = await _keycloakService.SetUserAttributeAsync(
                keycloakUserId,
                "tenant_id",
                tenant.Id.ToString(),
                cancellationToken);

            if (setAttributeResult.IsFailure)
            {
                _logger.LogError("Failed to set tenant_id attribute: {Error}", setAttributeResult.Error);
                await CleanupKeycloakUser(keycloakUserId, cancellationToken);
                return Result<OrganizerRegistrationResponse>.Failure($"Failed to configure user: {setAttributeResult.Error}");
            }

            // Step 6: Create competition entity
            var competitionResult = Domain.Entities.Competition.Create(
                tenant.Id,
                request.CompetitionName,
                DateTime.UtcNow.AddMonths(2), // Default registration deadline: 2 months
                DateTime.UtcNow.AddMonths(3)  // Default judging start: 3 months
            );

            if (competitionResult.IsFailure)
            {
                _logger.LogError("Failed to create competition entity: {Error}", competitionResult.Error);
                await CleanupKeycloakUser(keycloakUserId, cancellationToken);
                return Result<OrganizerRegistrationResponse>.Failure(competitionResult.Error);
            }

            var competition = competitionResult.Value!;
            await _competitionRepository.AddAsync(competition, cancellationToken);

            // Step 7: Set competition_id attribute in Keycloak (optional for future use)
            await _keycloakService.SetUserAttributeAsync(
                keycloakUserId,
                "competition_id",
                competition.Id.ToString(),
                cancellationToken);

            // Step 8: Save all changes to database (atomic transaction)
            await _tenantRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Organizer registered successfully: TenantId={TenantId}, CompetitionId={CompetitionId}, UserId={UserId}",
                tenant.Id,
                competition.Id,
                keycloakUserId);

            return Result<OrganizerRegistrationResponse>.Success(
                new OrganizerRegistrationResponse(
                    tenant.Id,
                    competition.Id,
                    keycloakUserId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during organizer registration for {Email}", request.Email);

            // Cleanup Keycloak user if created
            if (keycloakUserId != null)
            {
                await CleanupKeycloakUser(keycloakUserId, cancellationToken);
            }

            return Result<OrganizerRegistrationResponse>.Failure($"Registration failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleans up Keycloak user in case of transaction rollback.
    /// Ensures no orphaned users remain in Keycloak.
    /// </summary>
    private async Task CleanupKeycloakUser(string userId, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Rolling back Keycloak user creation: {UserId}", userId);

        try
        {
            await _keycloakService.DeleteUserAsync(userId, cancellationToken);
            _logger.LogInformation("Keycloak user deleted successfully: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup Keycloak user {UserId} during rollback", userId);
            // Don't throw - best effort cleanup
        }
    }
}
