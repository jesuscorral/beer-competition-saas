namespace BeerCompetition.Competition.Domain.Entities;

/// <summary>
/// Status of a user's registration for a competition.
/// Judges and Stewards require organizer approval.
/// Entrants are auto-approved.
/// </summary>
public enum CompetitionUserStatus
{
    /// <summary>
    /// User registration is approved and active.
    /// Entrants receive this status automatically.
    /// Judges/Stewards receive this after organizer approval.
    /// </summary>
    ACTIVE,

    /// <summary>
    /// User registration awaiting organizer approval.
    /// Only applies to Judges and Stewards.
    /// </summary>
    PENDING_APPROVAL,

    /// <summary>
    /// User registration was rejected by organizer.
    /// User cannot participate in this competition with this role.
    /// </summary>
    REJECTED
}
