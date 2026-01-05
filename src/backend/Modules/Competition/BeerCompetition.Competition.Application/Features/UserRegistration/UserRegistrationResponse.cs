namespace BeerCompetition.Competition.Application.Features.UserRegistration;

/// <summary>
/// Response returned after successful user registration for a competition.
/// Used across all registration types (Entrant, Judge, Steward).
/// </summary>
/// <param name="UserId">Keycloak user ID (string UUID)</param>
/// <param name="Status">Registration status (ACTIVE for entrants, PENDING_APPROVAL for judges/stewards)</param>
/// <param name="Message">Human-readable message</param>
public record UserRegistrationResponse(
    string UserId,
    string Status,
    string Message
);
