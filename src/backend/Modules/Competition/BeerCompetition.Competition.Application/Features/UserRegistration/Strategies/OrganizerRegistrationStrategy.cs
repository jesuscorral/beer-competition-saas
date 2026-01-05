using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Shared.Kernel;
using Microsoft.Extensions.Logging;

namespace BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;

/// <summary>
/// Registration strategy for Organizers.
/// Organizers create a new tenant when registering and are auto-approved.
/// </summary>
public class OrganizerRegistrationStrategy : IRegistrationStrategy
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<OrganizerRegistrationStrategy> _logger;
    
    public OrganizerRegistrationStrategy(
        ITenantRepository tenantRepository,
        ILogger<OrganizerRegistrationStrategy> logger)
    {
        _tenantRepository = tenantRepository;
        _logger = logger;
    }
    
    public CompetitionUserRole Role => CompetitionUserRole.ORGANIZER;
    
    public CompetitionUserStatus GetInitialStatus() => CompetitionUserStatus.ACTIVE;
    
    public async Task<Result<Guid>> ExecuteRoleSpecificLogic(
        string userId,
        Guid? competitionId,
        Guid? tenantId,
        Dictionary<string, string>? additionalData,
        CancellationToken cancellationToken)
    {
        // Organizers create a new tenant (organization)
        var organizationName = additionalData?.GetValueOrDefault("organization_name") 
            ?? $"Organization for user {userId}";
        var email = additionalData?.GetValueOrDefault("email") 
            ?? $"admin@{userId}.local";
            
        _logger.LogInformation(
            "Creating new tenant for organizer {UserId}: {OrganizationName}",
            userId, organizationName);
        
        var tenantResult = Tenant.Create(organizationName, email);
        if (tenantResult.IsFailure)
        {
            return Result<Guid>.Failure(tenantResult.Error);
        }
        
        await _tenantRepository.AddAsync(tenantResult.Value, cancellationToken);
        await _tenantRepository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation(
            "Tenant created successfully: {TenantId} for organizer {UserId}",
            tenantResult.Value.Id, userId);
        
        return Result<Guid>.Success(tenantResult.Value.Id);
    }
}
