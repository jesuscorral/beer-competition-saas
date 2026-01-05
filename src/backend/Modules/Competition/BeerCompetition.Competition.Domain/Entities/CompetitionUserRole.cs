namespace BeerCompetition.Competition.Domain.Entities;

/// <summary>
/// Roles that users can have within a competition.
/// Each user-competition relationship has exactly one role.
/// </summary>
public enum CompetitionUserRole
{
    /// <summary>
    /// Submits beer entries to the competition.
    /// </summary>
    ENTRANT,

    /// <summary>
    /// Evaluates entries and assigns scores according to BJCP guidelines.
    /// </summary>
    JUDGE,

    /// <summary>
    /// Assists judges by preparing flights and managing logistics.
    /// </summary>
    STEWARD,

    /// <summary>
    /// Manages the competition, approves judges/stewards, and oversees operations.
    /// </summary>
    ORGANIZER
}
