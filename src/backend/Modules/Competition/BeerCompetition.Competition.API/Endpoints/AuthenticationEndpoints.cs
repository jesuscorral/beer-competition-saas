using BeerCompetition.Competition.Application.Features.RegisterOrganizer;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BeerCompetition.Competition.API.Endpoints;

/// <summary>
/// Minimal API endpoints for authentication and user onboarding.
/// Handles organizer registration and user account creation.
/// </summary>
public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // POST /api/auth/register-organizer - Register new organizer with tenant and competition
        group.MapPost("/register-organizer", async (
            RegisterOrganizerCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("RegisterOrganizer")
        .WithSummary("Register a new organizer")
        .WithDescription(@"
Creates a new organizer account with:
- Keycloak user with 'organizer' role
- New tenant (organization) 
- First competition for the tenant
- User attributes: tenant_id, competition_id

This is the entry point for competition organizers to onboard onto the platform.

**No authentication required** - this is a public registration endpoint.")
        .Produces<OrganizerRegistrationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .AllowAnonymous(); // Public endpoint - no auth required

        // Future endpoints:
        // POST /api/auth/register-entrant - Register as entrant for a competition
        // POST /api/auth/register-judge - Register as judge (requires approval)
        // POST /api/auth/register-steward - Register as steward (requires approval)
    }
}
