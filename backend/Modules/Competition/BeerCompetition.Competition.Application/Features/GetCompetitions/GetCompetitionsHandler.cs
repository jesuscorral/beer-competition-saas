using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Shared.Kernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeerCompetition.Competition.Application.Features.GetCompetitions;

/// <summary>
/// Handler for GetCompetitionsQuery.
/// Retrieves all competitions for the current tenant.
/// Automatically filtered by tenant_id via Entity Framework global query filter.
/// </summary>
public class GetCompetitionsHandler : IRequestHandler<GetCompetitionsQuery, Result<List<CompetitionDto>>>
{
    private readonly ICompetitionRepository _repository;
    private readonly ILogger<GetCompetitionsHandler> _logger;

    public GetCompetitionsHandler(
        ICompetitionRepository repository,
        ILogger<GetCompetitionsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<CompetitionDto>>> Handle(
        GetCompetitionsQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all competitions for current tenant");

        try
        {
            // Repository automatically filters by tenant_id via EF Core global filter
            var competitions = await _repository.GetAllAsync(cancellationToken);

            // Map domain entities to DTOs
            var competitionDtos = competitions.Select(c => new CompetitionDto(
                Id: c.Id,
                Name: c.Name,
                Description: c.Description,
                RegistrationDeadline: c.RegistrationDeadline,
                JudgingStartDate: c.JudgingStartDate,
                JudgingEndDate: c.JudgingEndDate,
                Status: c.Status.ToString(),
                MaxEntriesPerEntrant: c.MaxEntriesPerEntrant,
                CreatedAt: c.CreatedAt,
                UpdatedAt: c.UpdatedAt
            )).ToList();

            _logger.LogInformation("Retrieved {Count} competitions", competitionDtos.Count);

            return Result<List<CompetitionDto>>.Success(competitionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching competitions");
            return Result<List<CompetitionDto>>.Failure("Failed to retrieve competitions");
        }
    }
}
