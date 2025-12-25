using Microsoft.AspNetCore.Authorization;

namespace BeerCompetition.BFF.ApiGateway.Policies;

/// <summary>
/// Authorization policy definitions for different user roles.
/// Based on ADR-004 (Authentication & Authorization).
/// </summary>
public static class AuthorizationPolicies
{
    public const string AuthenticatedUser = nameof(AuthenticatedUser);
    public const string Organizer = nameof(Organizer);
    public const string Judge = nameof(Judge);
    public const string Entrant = nameof(Entrant);
    public const string Steward = nameof(Steward);
    public const string JudgeOrOrganizer = nameof(JudgeOrOrganizer);
    public const string OrganizerOrSteward = nameof(OrganizerOrSteward);

    public static void AddBffAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Require authentication only
            options.AddPolicy(AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());

            // Organizer role (lowercase to match Keycloak realm roles)
            options.AddPolicy(Organizer, policy =>
                policy.RequireRole("organizer"));

            // Judge role (lowercase to match Keycloak realm roles)
            options.AddPolicy(Judge, policy =>
                policy.RequireRole("judge"));

            // Entrant role (lowercase to match Keycloak realm roles)
            options.AddPolicy(Entrant, policy =>
                policy.RequireRole("entrant"));

            // Steward role (lowercase to match Keycloak realm roles)
            options.AddPolicy(Steward, policy =>
                policy.RequireRole("steward"));

            // Judge or Organizer (lowercase to match Keycloak realm roles)
            options.AddPolicy(JudgeOrOrganizer, policy =>
                policy.RequireRole("judge", "organizer"));

            // Organizer or Steward (lowercase to match Keycloak realm roles)
            options.AddPolicy(OrganizerOrSteward, policy =>
                policy.RequireRole("organizer", "steward"));
        });
    }
}
