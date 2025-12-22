using MediatR;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Shared.Infrastructure.MultiTenancy;
using BeerCompetition.Shared.Kernel;
using Microsoft.Extensions.Logging;

namespace BeerCompetition.Competition.Application.Features.CreateCompetition;

/// <summary>
/// Handler for CreateCompetitionCommand.
/// Implements business logic for competition creation following DDD principles.
/// </summary>
public class CreateCompetitionHandler : IRequestHandler<CreateCompetitionCommand, Result<Guid>>
{
    private readonly ICompetitionRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateCompetitionHandler> _logger;

    public CreateCompetitionHandler(
        ICompetitionRepository repository,
        ITenantProvider tenantProvider,
        ILogger<CreateCompetitionHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateCompetitionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating competition: {CompetitionName}", request.Name);

        // Get current tenant ID from request context
        var tenantId = _tenantProvider.CurrentTenantId;

        // Use domain factory method to create competition
        // This ensures all business rules and invariants are enforced
        var competitionResult = Domain.Entities.Competition.Create(
            tenantId,
            request.Name,
            request.RegistrationDeadline,
            request.JudgingStartDate
        );

        if (competitionResult.IsFailure)
        {
            _logger.LogWarning("Failed to create competition: {Error}", competitionResult.Error);
            return Result<Guid>.Failure(competitionResult.Error);
        }

        var competition = competitionResult.Value!;

        // Update optional fields
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            var updateResult = competition.Update(request.Name, request.Description);
            if (updateResult.IsFailure)
            {
                _logger.LogWarning("Failed to update competition description: {Error}", updateResult.Error);
                return Result<Guid>.Failure(updateResult.Error);
            }
        }

        // Persist to database
        await _repository.AddAsync(competition, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Competition created successfully: {CompetitionId} for tenant {TenantId}",
            competition.Id,
            tenantId);

        // Domain events will be published by infrastructure layer after successful save
        // Following Outbox Pattern for reliable event publishing

        return Result<Guid>.Success(competition.Id);
    }
}
