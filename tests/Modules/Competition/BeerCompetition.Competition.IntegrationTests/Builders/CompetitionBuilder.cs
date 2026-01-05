namespace BeerCompetition.Competition.IntegrationTests.Builders;

/// <summary>
/// Builder for creating Competition test data with fluent API.
/// Provides sensible defaults with ability to override any property.
/// </summary>
public class CompetitionBuilder
{
    private Guid _tenantId = Guid.NewGuid();
    private string _name = "Test Competition";
    private string _description = "";
    private DateTime _registrationDeadline = DateTime.UtcNow.AddDays(30);
    private DateTime _judgingStartDate = DateTime.UtcNow.AddDays(35);
    private DateTime? _judgingEndDate = null;

    public CompetitionBuilder WithTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public CompetitionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CompetitionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CompetitionBuilder WithRegistrationDeadline(DateTime registrationDeadline)
    {
        _registrationDeadline = registrationDeadline;
        return this;
    }

    public CompetitionBuilder WithJudgingStartDate(DateTime judgingStartDate)
    {
        _judgingStartDate = judgingStartDate;
        return this;
    }

    public CompetitionBuilder WithJudgingEndDate(DateTime? judgingEndDate)
    {
        _judgingEndDate = judgingEndDate;
        return this;
    }

    /// <summary>
    /// Builds the Competition entity with all configured values.
    /// Uses domain Create method to ensure business rules are enforced.
    /// </summary>
    public Domain.Entities.Competition Build()
    {
        var result = Domain.Entities.Competition.Create(
            _tenantId,
            _name,
            _description,
            _registrationDeadline,
            _judgingStartDate,
            _judgingEndDate
        );
        
        if (!result.IsSuccess)
            throw new InvalidOperationException($"Failed to create Competition: {result.Error}");
        
        return result.Value!;
    }

    /// <summary>
    /// Creates a new builder instance for fluent chaining.
    /// </summary>
    public static CompetitionBuilder Default() => new();
}
