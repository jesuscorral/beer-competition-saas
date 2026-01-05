using BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Shared.Infrastructure.ExternalServices;
using BeerCompetition.Shared.Kernel;
using MediatR;
using Microsoft.Extensions.Logging;
using CompetitionEntity = BeerCompetition.Competition.Domain.Entities.Competition;

namespace BeerCompetition.Competition.Application.Features.UserRegistration;

/// <summary>
/// Unified handler for user registration across all roles.
/// Uses Strategy Pattern to delegate role-specific logic to the appropriate strategy.
/// 
/// Flow:
/// 1. Validate competition exists (if applicable)
/// 2. Check for duplicate registration
/// 3. Create Keycloak user with role
/// 4. Set Keycloak user attributes
/// 5. Execute role-specific logic via strategy (e.g., tenant creation for organizers)
/// 6. Create CompetitionUser record
/// 7. Return success response
/// </summary>
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<UserRegistration.UserRegistrationResponse>>
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ICompetitionUserRepository _competitionUserRepository;
    private readonly IKeycloakService _keycloakService;
    private readonly RegistrationStrategyFactory _strategyFactory;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        ICompetitionRepository competitionRepository,
        ICompetitionUserRepository competitionUserRepository,
        IKeycloakService keycloakService,
        RegistrationStrategyFactory strategyFactory,
        ILogger<RegisterUserHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _competitionUserRepository = competitionUserRepository;
        _keycloakService = keycloakService;
        _strategyFactory = strategyFactory;
        _logger = logger;
    }

    public async Task<Result<UserRegistration.UserRegistrationResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Registering user {Email} with role {Role} for competition {CompetitionId}",
                command.Email, command.Role, command.CompetitionId);

            // 1. Get the appropriate strategy for this role
            var strategy = _strategyFactory.GetStrategy(command.Role);

            // 2. Get competition and tenant (if applicable)
            CompetitionEntity? competition = null;
            Guid? tenantId = null;

            if (command.CompetitionId.HasValue)
            {
                competition = await _competitionRepository.GetByIdAsync(
                    command.CompetitionId.Value, 
                    cancellationToken);
                
                if (competition == null)
                {
                    _logger.LogWarning("Competition {CompetitionId} not found", command.CompetitionId);
                    return Result<UserRegistration.UserRegistrationResponse>.Failure("Competition not found");
                }

                // Verify competition is public (allows registration)
                if (!competition.IsPublic)
                {
                    _logger.LogWarning(
                        "Competition {CompetitionId} is not public, registration not allowed",
                        command.CompetitionId);
                    return Result<UserRegistration.UserRegistrationResponse>.Failure(
                        "Competition is not open for registration");
                }

                tenantId = competition.TenantId;
            }

            // 3. Create Keycloak user
            var createUserResult = await _keycloakService.CreateUserAsync(
                command.Email,
                command.Password,
                emailVerified: true,
                enabled: true,
                cancellationToken);

            if (createUserResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to create Keycloak user for {Email}: {Error}",
                    command.Email, createUserResult.Error);
                return Result<UserRegistration.UserRegistrationResponse>.Failure(createUserResult.Error);
            }

            var userId = createUserResult.Value;

            try
            {
                // 4. Check for duplicate registration
                if (command.CompetitionId.HasValue)
                {
                    var existingRegistration = await _competitionUserRepository.ExistsAsync(
                        command.CompetitionId.Value,
                        userId,
                        command.Role,
                        cancellationToken);

                    if (existingRegistration)
                    {
                        _logger.LogWarning(
                            "User {UserId} already registered for competition {CompetitionId} with role {Role}",
                            userId, command.CompetitionId, command.Role);
                        
                        // Cleanup: Delete Keycloak user
                        await _keycloakService.DeleteUserAsync(userId, cancellationToken);
                        
                        return Result<UserRegistration.UserRegistrationResponse>.Failure(
                            "You are already registered for this competition with this role");
                    }
                }

                // 5. Assign role
                var roleString = command.Role.ToString().ToLowerInvariant();
                var assignRoleResult = await _keycloakService.AssignRoleAsync(
                    userId,
                    roleString,
                    cancellationToken);

                if (assignRoleResult.IsFailure)
                {
                    _logger.LogError(
                        "Failed to assign role {Role} to user {UserId}: {Error}",
                        roleString, userId, assignRoleResult.Error);
                    
                    await _keycloakService.DeleteUserAsync(userId, cancellationToken);
                    return Result<UserRegistration.UserRegistrationResponse>.Failure(assignRoleResult.Error);
                }

                // 6. Execute role-specific logic (e.g., tenant creation for organizers)
                var additionalData = new Dictionary<string, string>
                {
                    ["email"] = command.Email  // Pass email for tenant creation
                };
                if (!string.IsNullOrEmpty(command.BjcpRank))
                {
                    additionalData["bjcp_rank"] = command.BjcpRank.ToUpperInvariant();
                }
                if (!string.IsNullOrEmpty(command.OrganizationName))
                {
                    additionalData["organization_name"] = command.OrganizationName;
                }

                var strategyResult = await strategy.ExecuteRoleSpecificLogic(
                    userId,
                    command.CompetitionId,
                    tenantId,
                    additionalData,
                    cancellationToken);

                if (strategyResult.IsFailure)
                {
                    _logger.LogError(
                        "Role-specific logic failed for {Role}: {Error}",
                        command.Role, strategyResult.Error);
                    
                    await _keycloakService.DeleteUserAsync(userId, cancellationToken);
                    return Result<UserRegistration.UserRegistrationResponse>.Failure(strategyResult.Error);
                }

                // Get final tenant ID (from strategy for organizers, from competition for others)
                var finalTenantId = strategyResult.Value;

                // 7. Set Keycloak attributes
                var setTenantResult = await _keycloakService.SetUserAttributeAsync(
                    userId,
                    "tenant_id",
                    finalTenantId.ToString(),
                    cancellationToken);

                if (setTenantResult.IsFailure)
                {
                    _logger.LogError(
                        "Failed to set tenant_id for user {UserId}: {Error}",
                        userId, setTenantResult.Error);
                    
                    await _keycloakService.DeleteUserAsync(userId, cancellationToken);
                    return Result<UserRegistration.UserRegistrationResponse>.Failure(setTenantResult.Error);
                }

                if (command.CompetitionId.HasValue)
                {
                    var setCompetitionResult = await _keycloakService.SetUserAttributeAsync(
                        userId,
                        "competition_id",
                        command.CompetitionId.Value.ToString(),
                        cancellationToken);

                    if (setCompetitionResult.IsFailure)
                    {
                        _logger.LogError(
                            "Failed to set competition_id for user {UserId}: {Error}",
                            userId, setCompetitionResult.Error);
                        
                        await _keycloakService.DeleteUserAsync(userId, cancellationToken);
                        return Result<UserRegistration.UserRegistrationResponse>.Failure(setCompetitionResult.Error);
                    }
                }

                // Set BJCP rank if provided (for judges)
                if (!string.IsNullOrEmpty(command.BjcpRank))
                {
                    var setBjcpRankResult = await _keycloakService.SetUserAttributeAsync(
                        userId,
                        "bjcp_rank",
                        command.BjcpRank.ToUpperInvariant(),
                        cancellationToken);

                    if (setBjcpRankResult.IsFailure)
                    {
                        _logger.LogError(
                            "Failed to set bjcp_rank for user {UserId}: {Error}",
                            userId, setBjcpRankResult.Error);
                        
                        await _keycloakService.DeleteUserAsync(userId, cancellationToken);
                        return Result<UserRegistration.UserRegistrationResponse>.Failure(setBjcpRankResult.Error);
                    }
                }

                // 8. Create CompetitionUser record
                Result<CompetitionUser>? competitionUserResult = null;
                
                if (command.Role == CompetitionUserRole.ORGANIZER)
                {
                    // Organizers don't create CompetitionUser at registration
                    // They will be added to competitions when they create them
                    _logger.LogInformation(
                        "Organizer {UserId} registered successfully without CompetitionUser record",
                        userId);
                }
                else
                {
                    competitionUserResult = command.Role switch
                    {
                        CompetitionUserRole.ENTRANT => CompetitionUser.CreateEntrant(
                            command.CompetitionId!.Value, userId, finalTenantId),
                        CompetitionUserRole.JUDGE => CompetitionUser.CreateJudge(
                            command.CompetitionId!.Value, userId, finalTenantId),
                        CompetitionUserRole.STEWARD => CompetitionUser.CreateSteward(
                            command.CompetitionId!.Value, userId, finalTenantId),
                        _ => throw new InvalidOperationException($"Unsupported role: {command.Role}")
                    };
                }

                if (competitionUserResult != null)
                {
                    if (competitionUserResult.IsFailure)
                    {
                        _logger.LogError(
                            "Failed to create CompetitionUser: {Error}",
                            competitionUserResult.Error);
                        
                        await _keycloakService.DeleteUserAsync(userId, cancellationToken);
                        return Result<UserRegistration.UserRegistrationResponse>.Failure(competitionUserResult.Error);
                    }

                    var competitionUser = competitionUserResult.Value;
                    await _competitionUserRepository.AddAsync(competitionUser, cancellationToken);
                    await _competitionUserRepository.SaveChangesAsync(cancellationToken);
                }

                var status = strategy.GetInitialStatus();
                var message = status == CompetitionUserStatus.ACTIVE
                    ? "Registration successful. You can now access the platform."
                    : "Registration submitted. Waiting for organizer approval.";

                _logger.LogInformation(
                    "Successfully registered user {UserId} with role {Role} and status {Status}",
                    userId, command.Role, status);

                return Result<UserRegistration.UserRegistrationResponse>.Success(
                    new UserRegistration.UserRegistrationResponse(
                        userId,
                        status.ToString(),
                        message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error during registration process for user {Email}, rolling back Keycloak user",
                    command.Email);

                // Cleanup: Delete Keycloak user
                await _keycloakService.DeleteUserAsync(userId, cancellationToken);

                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error registering user {Email} with role {Role}",
                command.Email, command.Role);
            
            return Result<UserRegistration.UserRegistrationResponse>.Failure(
                "An error occurred during registration. Please try again.");
        }
    }
}
