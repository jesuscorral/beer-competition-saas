using BeerCompetition.Competition.Application.Features.GetAvailablePlans;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BeerCompetition.Competition.API.Endpoints;

/// <summary>
/// Minimal API endpoints for Subscription Plans.
/// Allows organizers to view available plans before selection.
/// </summary>
public static class SubscriptionPlanEndpoints
{
    public static void MapSubscriptionPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/subscription-plans")
            .WithTags("Subscription Plans");

        // GET /api/subscription-plans - List available plans
        group.MapGet("/", async (
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetAvailablePlansQuery();
            var result = await mediator.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetAvailablePlans")
        .WithSummary("Get available subscription plans")
        .WithDescription("Retrieves all subscription plans available for the current tenant. " +
                        "Includes pricing, entry limits, and descriptions for each plan. " +
                        "**Authentication required** (any role).")
        .Produces<List<SubscriptionPlanDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization("AuthenticatedUser"); // Any authenticated user can view plans
    }
}
