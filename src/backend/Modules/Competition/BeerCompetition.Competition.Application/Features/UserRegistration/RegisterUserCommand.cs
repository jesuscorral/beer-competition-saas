using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Shared.Kernel;
using MediatR;

namespace BeerCompetition.Competition.Application.Features.UserRegistration;

/// <summary>
/// Unified command to register a user for a competition with a specific role.
/// Handles Entrant, Judge, Steward, and Organizer registration.
/// </summary>
/// <param name="Email">User's email address.</param>
/// <param name="Password">User's password (hashed by Keycloak).</param>
/// <param name="FullName">User's full name.</param>
/// <param name="Role">Role for this registration (ENTRANT, JUDGE, STEWARD, ORGANIZER).</param>
/// <param name="CompetitionId">Competition ID (required for non-organizers, optional for organizers).</param>
/// <param name="BjcpRank">BJCP rank for judges (optional).</param>
/// <param name="OrganizationName">Organization name for organizers (optional).</param>
public record RegisterUserCommand(
    string Email,
    string Password,
    string FullName,
    CompetitionUserRole Role,
    Guid? CompetitionId = null,
    string? BjcpRank = null,
    string? OrganizationName = null
) : IRequest<Result<UserRegistrationResponse>>;
