using MediatR;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Application.Features.RegisterOrganizer;

/// <summary>
/// Command to register a new organizer and create their tenant.
/// This is the entry point for competition organizers to onboard onto the platform.
/// Competition creation will happen in a separate step.
/// </summary>
/// <param name="Email">Organizer's email address (will be used as Keycloak username).</param>
/// <param name="Password">Password for the organizer account.</param>
/// <param name="OrganizationName">Name of the organizing body (club, brewery, etc.).</param>
/// <param name="PlanName">Subscription plan name (TRIAL, BASIC, STANDARD, PRO).</param>
/// <returns>Result with organizer registration response or error message.</returns>
public record RegisterOrganizerCommand(
    string Email,
    string Password,
    string OrganizationName,
    string PlanName
) : IRequest<Result<OrganizerRegistrationResponse>>;

/// <summary>
/// Response DTO for successful organizer registration.
/// Contains IDs of created entities.
/// </summary>
/// <param name="TenantId">The generated tenant (organization) ID.</param>
/// <param name="UserId">The Keycloak user ID.</param>
public record OrganizerRegistrationResponse(
    Guid TenantId,
    string UserId
);
