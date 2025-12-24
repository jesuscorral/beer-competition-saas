using BeerCompetition.Competition.Application.Features.CreateCompetition;
using BeerCompetition.Competition.Application.Features.GetCompetitions;
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
        .WithDescription("Retrieves all competitions for the current tenant. Automatically filtered by tenant_id.")
        .Produces<List<CompetitionDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
        // TODO: Add .RequireAuthorization() when auth is implemented

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
        .WithDescription("Creates a new beer competition in Draft status. Organizer role required.")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
        // TODO: Add .RequireAuthorization("OrganizerOnly") when auth is implemented

        // Additional endpoints will be added here:
        // - GET /api/competitions/{id} (get by id)
        // - PATCH /api/competitions/{id}/open (open for registration)
        // - PATCH /api/competitions/{id}/start-judging
        // - PATCH /api/competitions/{id}/publish-results
    }
}
