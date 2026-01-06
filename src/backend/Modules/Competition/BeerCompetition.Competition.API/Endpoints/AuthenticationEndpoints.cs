using BeerCompetition.Competition.Application.Features.UserRegistration;
using BeerCompetition.Competition.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BeerCompetition.Competition.API.Endpoints;

/// <summary>
/// Minimal API endpoints for authentication and user onboarding.
/// Uses unified RegisterUserCommand with Strategy Pattern for role-specific logic.
/// </summary>
public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // POST /api/auth/register-organizer - Register new organizer with tenant
        group.MapPost("/register-organizer", async (
            RegisterOrganizerRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.OrganizationName,  // FullName = OrganizationName for organizers
                CompetitionUserRole.ORGANIZER,
                null,  // No competitionId for organizers
                null,  // No bjcpRank
                request.OrganizationName  // Pass organization name to strategy
            );
            
            var result = await mediator.Send(command, ct);

            if (result.IsFailure)
                return Results.BadRequest(new { error = result.Error });

            // Map response to match frontend OrganizerRegistrationResponse interface
            return Results.Ok(new 
            { 
                tenantId = result.Value.TenantId?.ToString() ?? string.Empty,
                userId = result.Value.UserId
            });
        })
        .WithName("RegisterOrganizer")
        .WithSummary("Register a new organizer")
        .WithDescription(@"
Creates a new organizer account with:
- Keycloak user with 'organizer' role
- New tenant (organization) 
- User attributes: tenant_id
- ACTIVE status (auto-approved)

This is the entry point for competition organizers to onboard onto the platform.
Competition creation happens in a separate workflow after registration.

Now uses unified RegisterUserCommand with Strategy Pattern for consistency.

**No authentication required** - this is a public registration endpoint.")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .AllowAnonymous(); // Public endpoint - no auth required

        // POST /api/auth/register-entrant - Register as entrant for a competition
        group.MapPost("/register-entrant", async (
            RegisterUserRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FullName,
                CompetitionUserRole.ENTRANT,
                request.CompetitionId);
                
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("RegisterEntrant")
        .WithSummary("Register as an entrant for a competition")
        .WithDescription(@"
Creates a new entrant account for a specific competition with:
- Keycloak user with 'entrant' role
- User attributes: tenant_id, competition_id
- ACTIVE status (auto-approved)

Entrants are automatically approved and can immediately submit beer entries.

**No authentication required** - this is a public registration endpoint.")
        .Produces<UserRegistrationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .AllowAnonymous();

        // POST /api/auth/register-judge - Register as judge (requires approval)
        group.MapPost("/register-judge", async (
            RegisterJudgeRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FullName,
                CompetitionUserRole.JUDGE,
                request.CompetitionId,
                request.BjcpRank);
                
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("RegisterJudge")
        .WithSummary("Register as a judge for a competition")
        .WithDescription(@"
Creates a new judge account for a specific competition with:
- Keycloak user with 'judge' role
- User attributes: tenant_id, competition_id, bjcp_rank (optional)
- PENDING_APPROVAL status (requires organizer approval)

Judges must be approved by the competition organizer before accessing judging features.

**No authentication required** - this is a public registration endpoint.")
        .Produces<UserRegistrationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .AllowAnonymous();

        // POST /api/auth/register-steward - Register as steward (requires approval)
        group.MapPost("/register-steward", async (
            RegisterUserRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FullName,
                CompetitionUserRole.STEWARD,
                request.CompetitionId);
                
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("RegisterSteward")
        .WithSummary("Register as a steward for a competition")
        .WithDescription(@"
Creates a new steward account for a specific competition with:
- Keycloak user with 'steward' role
- User attributes: tenant_id, competition_id
- PENDING_APPROVAL status (requires organizer approval)

Stewards must be approved by the competition organizer before accessing steward features.

**No authentication required** - this is a public registration endpoint.")
        .Produces<UserRegistrationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .AllowAnonymous();

        // Future endpoints:
        // POST /api/auth/login - User login (handled by Keycloak)
        // POST /api/auth/refresh - Token refresh (handled by Keycloak)
        // POST /api/auth/logout - User logout
    }
}

/// <summary>
/// Request DTO for user registration (Entrant/Steward).
/// </summary>
public record RegisterUserRequest(
    string Email,
    string Password,
    string FullName,
    Guid CompetitionId);

/// <summary>
/// Request DTO for judge registration (includes optional BJCP rank).
/// </summary>
public record RegisterJudgeRequest(
    string Email,
    string Password,
    string FullName,
    Guid CompetitionId,
    string? BjcpRank = null);

/// <summary>
/// Request DTO for organizer registration.
/// </summary>
public record RegisterOrganizerRequest(
    string Email,
    string Password,
    string OrganizationName,
    string PlanName);
