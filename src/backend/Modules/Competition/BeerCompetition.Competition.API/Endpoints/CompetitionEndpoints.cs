using BeerCompetition.Competition.Application.Features.CreateCompetition;
using BeerCompetition.Competition.Application.Features.GetCompetitions;
using BeerCompetition.Competition.Application.Features.SelectPlan;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BeerCompetition.Competition.API.Endpoints;

/// <summary>
/// Minimal API endpoints for Competition module.
/// Maps HTTP requests to MediatR commands/queries.
/// </summary>
public static class CompetitionEndpoints
{
    public static void MapCompetitionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/competitions")
            .WithTags("Competitions");

        // GET /api/competitions - List all competitions
        group.MapGet("/", async (
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetCompetitionsQuery();
            var result = await mediator.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetCompetitions")
        .WithSummary("Get all competitions")
        .WithDescription("Retrieves all competitions for the current tenant. Automatically filtered by tenant_id. Requires authentication.")
        .Produces<List<CompetitionDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization("AuthenticatedUser"); // Policy: Authenticated user with tenant_id claim

        // POST /api/competitions - Create new competition
        group.MapPost("/", async (
            CreateCompetitionCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/competitions/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateCompetition")
        .WithSummary("Create a new competition")
        .WithDescription("Creates a new beer competition in Draft status. **Organizer role required**.")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization("OrganizerOnly"); // Policy: Organizer role + tenant_id claim

        // POST /api/competitions/{id}/select-plan - Select subscription plan
        group.MapPost("/{id:guid}/select-plan", async (
            Guid id,
            SelectPlanRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new SelectPlanCommand(id, request.PlanName);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("SelectSubscriptionPlan")
        .WithSummary("Select subscription plan for competition")
        .WithDescription("Selects a subscription plan (TRIAL, BASIC, STANDARD, PRO) for the competition. " +
                        "MVP: MOCK payment with immediate activation. **Organizer role required**.")
        .Produces<SelectPlanResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization("OrganizerOnly");

        // Additional endpoints will be added here:
        // - GET /api/competitions/{id} (get by id) - AuthenticatedUser
        // - PATCH /api/competitions/{id}/open (open for registration) - OrganizerOnly
        // - PATCH /api/competitions/{id}/start-judging - OrganizerOnly
        // - PATCH /api/competitions/{id}/publish-results - OrganizerOnly
    }
}

/// <summary>
/// Request DTO for selecting a subscription plan.
/// </summary>
public record SelectPlanRequest(string PlanName);
