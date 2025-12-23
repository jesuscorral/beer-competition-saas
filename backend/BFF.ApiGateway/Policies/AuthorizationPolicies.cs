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

            // Organizer role
            options.AddPolicy(Organizer, policy =>
                policy.RequireRole("Organizer"));

            // Judge role
            options.AddPolicy(Judge, policy =>
                policy.RequireRole("Judge"));

            // Entrant role
            options.AddPolicy(Entrant, policy =>
                policy.RequireRole("Entrant"));

            // Steward role
            options.AddPolicy(Steward, policy =>
                policy.RequireRole("Steward"));

            // Judge or Organizer
            options.AddPolicy(JudgeOrOrganizer, policy =>
                policy.RequireRole("Judge", "Organizer"));

            // Organizer or Steward
            options.AddPolicy(OrganizerOrSteward, policy =>
                policy.RequireRole("Organizer", "Steward"));
        });
    }
}
