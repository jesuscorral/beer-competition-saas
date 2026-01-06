namespace BeerCompetition.Competition.Application.Features.UserRegistration;

/// <summary>
/// Response returned after successful user registration for a competition.
/// Used across all registration types (Entrant, Judge, Steward, Organizer).
/// </summary>
/// <param name="UserId">Keycloak user ID (string UUID)</param>
/// <param name="Status">Registration status (ACTIVE for entrants/organizers, PENDING_APPROVAL for judges/stewards)</param>
/// <param name="Message">Human-readable message</param>
/// <param name="TenantId">Tenant ID (only populated for organizers who create a new tenant)</param>
public record UserRegistrationResponse(
    string UserId,
    string Status,
    string Message,
    Guid? TenantId = null
);
