using BeerCompetition.Competition.Domain.Events;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Domain.Entities;

/// <summary>
/// Represents a user's registration for a specific competition with a particular role.
/// Supports multi-role registration: a user can be both an Entrant and a Judge in different competitions,
/// or even have different roles in the same competition (though not common).
/// Enforces approval workflows for Judges and Stewards.
/// </summary>
public class CompetitionUser : Entity, IAggregateRoot
{
    /// <summary>
    /// Competition ID this registration is for.
    /// </summary>
    public Guid CompetitionId { get; private set; }

    /// <summary>
    /// Keycloak user ID.
    /// Links to the authentication system user.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Role the user is registering for in this competition.
    /// </summary>
    public CompetitionUserRole Role { get; private set; }

    /// <summary>
    /// Current status of the registration.
    /// ACTIVE: Auto-approved for Entrants, organizer-approved for Judges/Stewards
    /// PENDING_APPROVAL: Awaiting organizer approval (Judges/Stewards only)
    /// REJECTED: Registration rejected by organizer
    /// </summary>
    public CompetitionUserStatus Status { get; private set; }

    /// <summary>
    /// Optional note from organizer when approving or rejecting.
    /// Example: "Approved - BJCP Certified" or "Rejected - No prior judging experience"
    /// </summary>
    public string? ApprovalNote { get; private set; }

    /// <summary>
    /// When the user registered for this competition.
    /// </summary>
    public DateTime RegisteredAt { get; private set; }

    /// <summary>
    /// When the registration was approved or rejected by organizer.
    /// Null if still PENDING_APPROVAL or auto-approved (Entrant).
    /// </summary>
    public DateTime? ApprovedAt { get; private set; }

    /// <summary>
    /// Keycloak user ID of the organizer who approved/rejected.
    /// Null if still PENDING_APPROVAL or auto-approved (Entrant).
    /// </summary>
    public string? ApprovedBy { get; private set; }

    /// <summary>
    /// Navigation property: Competition entity.
    /// Required for EF Core relationship mapping.
    /// </summary>
    public Entities.Competition? Competition { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework.
    /// </summary>
    private CompetitionUser()
    {
    }

    /// <summary>
    /// Factory method to register a user for a competition as an Entrant.
    /// Entrants are auto-approved (ACTIVE status).
    /// </summary>
    /// <param name="competitionId">Competition ID.</param>
    /// <param name="userId">Keycloak user ID.</param>
    /// <param name="tenantId">Tenant ID for multi-tenancy.</param>
    /// <returns>Result with CompetitionUser instance or error message.</returns>
    public static Result<CompetitionUser> CreateEntrant(
        Guid competitionId,
        string userId,
        Guid tenantId)
    {
        if (competitionId == Guid.Empty)
            return Result<CompetitionUser>.Failure("Competition ID is required");

        if (string.IsNullOrWhiteSpace(userId))
            return Result<CompetitionUser>.Failure("User ID is required");

        if (tenantId == Guid.Empty)
            return Result<CompetitionUser>.Failure("Tenant ID is required");

        var competitionUser = new CompetitionUser
        {
            CompetitionId = competitionId,
            UserId = userId,
            Role = CompetitionUserRole.ENTRANT,
            Status = CompetitionUserStatus.ACTIVE, // Auto-approved
            RegisteredAt = DateTime.UtcNow,
            TenantId = tenantId
        };

        // Raise domain event
        competitionUser.AddDomainEvent(new UserRegisteredForCompetitionEvent(
            competitionId,
            userId,
            CompetitionUserRole.ENTRANT,
            CompetitionUserStatus.ACTIVE));

        return Result<CompetitionUser>.Success(competitionUser);
    }

    /// <summary>
    /// Factory method to register a user for a competition as a Judge.
    /// Judges require organizer approval (PENDING_APPROVAL status).
    /// </summary>
    /// <param name="competitionId">Competition ID.</param>
    /// <param name="userId">Keycloak user ID.</param>
    /// <param name="tenantId">Tenant ID for multi-tenancy.</param>
    /// <returns>Result with CompetitionUser instance or error message.</returns>
    public static Result<CompetitionUser> CreateJudge(
        Guid competitionId,
        string userId,
        Guid tenantId)
    {
        if (competitionId == Guid.Empty)
            return Result<CompetitionUser>.Failure("Competition ID is required");

        if (string.IsNullOrWhiteSpace(userId))
            return Result<CompetitionUser>.Failure("User ID is required");

        if (tenantId == Guid.Empty)
            return Result<CompetitionUser>.Failure("Tenant ID is required");

        var competitionUser = new CompetitionUser
        {
            CompetitionId = competitionId,
            UserId = userId,
            Role = CompetitionUserRole.JUDGE,
            Status = CompetitionUserStatus.PENDING_APPROVAL, // Requires approval
            RegisteredAt = DateTime.UtcNow,
            TenantId = tenantId
        };

        // Raise domain event
        competitionUser.AddDomainEvent(new UserRegisteredForCompetitionEvent(
            competitionId,
            userId,
            CompetitionUserRole.JUDGE,
            CompetitionUserStatus.PENDING_APPROVAL));

        return Result<CompetitionUser>.Success(competitionUser);
    }

    /// <summary>
    /// Factory method to register a user for a competition as a Steward.
    /// Stewards require organizer approval (PENDING_APPROVAL status).
    /// </summary>
    /// <param name="competitionId">Competition ID.</param>
    /// <param name="userId">Keycloak user ID.</param>
    /// <param name="tenantId">Tenant ID for multi-tenancy.</param>
    /// <returns>Result with CompetitionUser instance or error message.</returns>
    public static Result<CompetitionUser> CreateSteward(
        Guid competitionId,
        string userId,
        Guid tenantId)
    {
        if (competitionId == Guid.Empty)
            return Result<CompetitionUser>.Failure("Competition ID is required");

        if (string.IsNullOrWhiteSpace(userId))
            return Result<CompetitionUser>.Failure("User ID is required");

        if (tenantId == Guid.Empty)
            return Result<CompetitionUser>.Failure("Tenant ID is required");

        var competitionUser = new CompetitionUser
        {
            CompetitionId = competitionId,
            UserId = userId,
            Role = CompetitionUserRole.STEWARD,
            Status = CompetitionUserStatus.PENDING_APPROVAL, // Requires approval
            RegisteredAt = DateTime.UtcNow,
            TenantId = tenantId
        };

        // Raise domain event
        competitionUser.AddDomainEvent(new UserRegisteredForCompetitionEvent(
            competitionId,
            userId,
            CompetitionUserRole.STEWARD,
            CompetitionUserStatus.PENDING_APPROVAL));

        return Result<CompetitionUser>.Success(competitionUser);
    }

    /// <summary>
    /// Approves the user's registration.
    /// Only applicable for PENDING_APPROVAL status.
    /// </summary>
    /// <param name="approverId">Keycloak user ID of the approving organizer.</param>
    /// <param name="note">Optional note from organizer.</param>
    /// <returns>Result indicating success or failure.</returns>
    public Result Approve(string approverId, string? note = null)
    {
        if (Status != CompetitionUserStatus.PENDING_APPROVAL)
            return Result.Failure("Can only approve registrations with PENDING_APPROVAL status");

        if (string.IsNullOrWhiteSpace(approverId))
            return Result.Failure("Approver ID is required");

        Status = CompetitionUserStatus.ACTIVE;
        ApprovedBy = approverId;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNote = note;

        // Raise domain event
        AddDomainEvent(new UserRegistrationApprovedEvent(
            CompetitionId,
            UserId,
            Role,
            approverId));

        return Result.Success();
    }

    /// <summary>
    /// Rejects the user's registration.
    /// Only applicable for PENDING_APPROVAL status.
    /// </summary>
    /// <param name="rejecterId">Keycloak user ID of the rejecting organizer.</param>
    /// <param name="note">Optional note explaining rejection.</param>
    /// <returns>Result indicating success or failure.</returns>
    public Result Reject(string rejecterId, string? note = null)
    {
        if (Status != CompetitionUserStatus.PENDING_APPROVAL)
            return Result.Failure("Can only reject registrations with PENDING_APPROVAL status");

        if (string.IsNullOrWhiteSpace(rejecterId))
            return Result.Failure("Rejecter ID is required");

        Status = CompetitionUserStatus.REJECTED;
        ApprovedBy = rejecterId;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNote = note;

        // Raise domain event
        AddDomainEvent(new UserRegistrationRejectedEvent(
            CompetitionId,
            UserId,
            Role,
            rejecterId,
            note));

        return Result.Success();
    }
}
