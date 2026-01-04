using BeerCompetition.Competition.Domain.Events;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Domain.Entities;

/// <summary>
/// Aggregate root for a Beer Competition.
/// Manages the lifecycle of a competition from draft to results publication.
/// Enforces BJCP 2021 compliance rules and business invariants.
/// </summary>
public class Competition : Entity, IAggregateRoot
{
    /// <summary>
    /// Competition name (e.g., "Spring Classic 2025").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Competition description with rules and important dates.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Last date entrants can submit entries (UTC).
    /// Must be before JudgingStartDate.
    /// </summary>
    public DateTime RegistrationDeadline { get; private set; }

    /// <summary>
    /// First day of judging (UTC).
    /// Must be after RegistrationDeadline.
    /// </summary>
    public DateTime JudgingStartDate { get; private set; }

    /// <summary>
    /// Last day of judging (UTC).
    /// Optional - if not set, judging is a single day.
    /// </summary>
    public DateTime? JudgingEndDate { get; private set; }

    /// <summary>
    /// Current status of the competition.
    /// Follows state machine: Draft → Open → Judging → Completed → ResultsPublished
    /// </summary>
    public CompetitionStatus Status { get; private set; }

    /// <summary>
    /// Maximum number of entries allowed per entrant.
    /// Default: 10 entries per person.
    /// </summary>
    public int MaxEntriesPerEntrant { get; private set; } = 10;

    /// <summary>
    /// Navigation property: Tenant (organization) that owns this competition.
    /// Required for EF Core relationship mapping.
    /// </summary>
    public Tenant? Tenant { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework.
    /// </summary>
    private Competition()
    {
    }

    /// <summary>
    /// Factory method to create a new competition.
    /// Validates business rules before creation.
    /// </summary>
    /// <param name="tenantId">Tenant that owns this competition.</param>
    /// <param name="name">Competition name.</param>
    /// <param name="registrationDeadline">Last date for entry registration (UTC).</param>
    /// <param name="judgingStartDate">First day of judging (UTC).</param>
    /// <returns>Result with Competition instance or error message.</returns>
    public static Result<Competition> Create(
        Guid tenantId,
        string name,
        DateTime registrationDeadline,
        DateTime judgingStartDate)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            return Result<Competition>.Failure("Competition name is required");

        if (name.Length > 255)
            return Result<Competition>.Failure("Competition name must not exceed 255 characters");

        // Validate dates
        if (registrationDeadline < DateTime.UtcNow)
            return Result<Competition>.Failure("Registration deadline must be in the future");

        if (judgingStartDate < DateTime.UtcNow)
            return Result<Competition>.Failure("Judging start date must be in the future");

        if (registrationDeadline >= judgingStartDate)
            return Result<Competition>.Failure("Registration deadline must be before judging start date");

     
        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            RegistrationDeadline = registrationDeadline,
            JudgingStartDate = judgingStartDate,
            Status = CompetitionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        // Raise domain event
        competition.AddDomainEvent(new CompetitionCreatedEvent(competition.Id, tenantId, name));

        return Result<Competition>.Success(competition);
    }

    /// <summary>
    /// Opens the competition for entry registration.
    /// Transitions from Draft to Open status.
    /// </summary>
    public Result Open()
    {
        if (Status != CompetitionStatus.Draft)
            return Result.Failure($"Can only open competitions in {nameof(CompetitionStatus.Draft)} status");

        if (RegistrationDeadline < DateTime.UtcNow)
            return Result.Failure("Cannot open competition with past registration deadline");

        Status = CompetitionStatus.Open;
        MarkAsUpdated();

        AddDomainEvent(new CompetitionOpenedEvent(Id, TenantId));

        return Result.Success();
    }

    /// <summary>
    /// Closes registration and starts judging.
    /// Transitions from Open to Judging status.
    /// </summary>
    public Result StartJudging()
    {
        if (Status != CompetitionStatus.Open)
            return Result.Failure($"Can only start judging for competitions in {nameof(CompetitionStatus.Open)} status");

        Status = CompetitionStatus.Judging;
        MarkAsUpdated();

        AddDomainEvent(new JudgingStartedEvent(Id, TenantId));

        return Result.Success();
    }

    /// <summary>
    /// Marks competition as completed after judging finishes.
    /// Transitions from Judging to Completed status.
    /// </summary>
    public Result CompleteJudging()
    {
        if (Status != CompetitionStatus.Judging)
            return Result.Failure($"Can only complete competitions in {nameof(CompetitionStatus.Judging)} status");

        Status = CompetitionStatus.Completed;
        MarkAsUpdated();

        AddDomainEvent(new JudgingCompletedEvent(Id, TenantId));

        return Result.Success();
    }

    /// <summary>
    /// Publishes competition results to entrants.
    /// Transitions from Completed to ResultsPublished status.
    /// </summary>
    public Result PublishResults()
    {
        if (Status != CompetitionStatus.Completed)
            return Result.Failure($"Can only publish results for competitions in {nameof(CompetitionStatus.Completed)} status");

        Status = CompetitionStatus.ResultsPublished;
        MarkAsUpdated();

        AddDomainEvent(new ResultsPublishedEvent(Id, TenantId));

        return Result.Success();
    }

    /// <summary>
    /// Cancels the competition at any stage.
    /// Irreversible action.
    /// </summary>
    public Result Cancel()
    {
        if (Status == CompetitionStatus.Canceled)
            return Result.Failure("Competition is already canceled");

        if (Status == CompetitionStatus.ResultsPublished)
            return Result.Failure("Cannot cancel competition after results are published");

        Status = CompetitionStatus.Canceled;
        MarkAsUpdated();

        AddDomainEvent(new CompetitionCanceledEvent(Id, TenantId));

        return Result.Success();
    }

    /// <summary>
    /// Updates competition basic information.
    /// Only allowed in Draft status.
    /// </summary>
    public Result Update(string name, string? description)
    {
        if (Status != CompetitionStatus.Draft)
            return Result.Failure("Can only update competitions in Draft status");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Competition name is required");

        if (name.Length > 255)
            return Result.Failure("Competition name must not exceed 255 characters");

        Name = name;
        Description = description;
        MarkAsUpdated();

        return Result.Success();
    }
}
