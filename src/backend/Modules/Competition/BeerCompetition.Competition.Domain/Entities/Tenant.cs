using System.Net.Mail;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Domain.Entities;

/// <summary>
/// Represents an organization/organizer account in the multi-tenant system.
/// A Tenant owns multiple competitions and manages subscriptions/bundles.
/// This is the root entity for multi-tenancy isolation.
/// </summary>
public class Tenant : Entity, IAggregateRoot
{
    /// <summary>
    /// Organization or organizer name.
    /// </summary>
    public string OrganizationName { get; private set; } = string.Empty;

    /// <summary>
    /// Contact email for the organization.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Current status of the tenant account.
    /// </summary>
    public TenantStatus Status { get; private set; }

    /// <summary>
    /// Navigation property: Competitions owned by this tenant.
    /// A tenant can create multiple competitions.
    /// </summary>
    public ICollection<Competition> Competitions { get; private set; } = new List<Competition>();

    /// <summary>
    /// Private parameterless constructor for Entity Framework.
    /// </summary>
    private Tenant()
    {
    }

    /// <summary>
    /// Validates email format using System.Net.Mail.MailAddress.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if email is valid, false otherwise.</returns>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var mailAddress = new MailAddress(email);
            // Ensure the parsed address matches the input (prevents issues with display names)
            return mailAddress.Address == email.Trim();
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates tenant input parameters.
    /// </summary>
    /// <param name="organizationName">Name of the organization.</param>
    /// <param name="email">Contact email.</param>
    /// <returns>Result indicating success or validation error.</returns>
    private static Result ValidateInput(string organizationName, string email)
    {
        // Validate organization name
        if (string.IsNullOrWhiteSpace(organizationName))
            return Result.Failure("Organization name is required");

        if (organizationName.Length > 255)
            return Result.Failure("Organization name must not exceed 255 characters");

        // Validate email
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure("Email is required");

        if (email.Length > 255)
            return Result.Failure("Email must not exceed 255 characters");

        if (!IsValidEmail(email))
            return Result.Failure("Invalid email format");

        return Result.Success();
    }

    /// <summary>
    /// Factory method to create a new tenant account.
    /// </summary>
    /// <param name="organizationName">Name of the organization.</param>
    /// <param name="email">Contact email.</param>
    /// <returns>Result with Tenant instance or error message.</returns>
    public static Result<Tenant> Create(string organizationName, string email)
    {
        var validationResult = ValidateInput(organizationName, email);
        if (validationResult.IsFailure)
            return Result<Tenant>.Failure(validationResult.Error);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            OrganizationName = organizationName,
            Email = email,
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Note: TenantId for Tenant entity is same as Id (self-reference)
        // This allows consistent query filtering across all entities
        tenant.TenantId = tenant.Id;

        return Result<Tenant>.Success(tenant);
    }

    /// <summary>
    /// Factory method for creating a tenant with a specific Id for testing purposes only.
    /// This method should NEVER be used in production code.
    /// </summary>
    /// <param name="id">The specific Id to assign to the tenant.</param>
    /// <param name="organizationName">Name of the organization.</param>
    /// <param name="email">Contact email.</param>
    /// <returns>Result with Tenant instance or error message.</returns>
    internal static Result<Tenant> CreateForTesting(Guid id, string organizationName, string email)
    {
        var validationResult = ValidateInput(organizationName, email);
        if (validationResult.IsFailure)
            return Result<Tenant>.Failure(validationResult.Error);

        var tenant = new Tenant
        {
            Id = id,
            OrganizationName = organizationName,
            Email = email,
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Note: TenantId for Tenant entity is same as Id (self-reference)
        tenant.TenantId = id;

        return Result<Tenant>.Success(tenant);
    }

    /// <summary>
    /// Activates the tenant account.
    /// </summary>
    public Result Activate()
    {
        if (Status == TenantStatus.Active)
            return Result.Failure("Tenant is already active");

        Status = TenantStatus.Active;
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Suspends the tenant account.
    /// Prevents creation of new competitions and access to existing data.
    /// </summary>
    public Result Suspend()
    {
        if (Status == TenantStatus.Suspended)
            return Result.Failure("Tenant is already suspended");

        if (Status == TenantStatus.Deleted)
            return Result.Failure("Cannot suspend a deleted tenant");

        Status = TenantStatus.Suspended;
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Marks tenant as deleted (soft delete).
    /// This is irreversible.
    /// </summary>
    public Result Delete()
    {
        if (Status == TenantStatus.Deleted)
            return Result.Failure("Tenant is already deleted");

        Status = TenantStatus.Deleted;
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Updates tenant information.
    /// </summary>
    public Result Update(string organizationName, string email)
    {
        if (Status == TenantStatus.Deleted)
            return Result.Failure("Cannot update a deleted tenant");

        if (string.IsNullOrWhiteSpace(organizationName))
            return Result.Failure("Organization name is required");

        if (organizationName.Length > 255)
            return Result.Failure("Organization name must not exceed 255 characters");

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure("Email is required");

        if (email.Length > 255)
            return Result.Failure("Email must not exceed 255 characters");

        if (!IsValidEmail(email))
            return Result.Failure("Invalid email format");

        OrganizationName = organizationName;
        Email = email;
        MarkAsUpdated();

        return Result.Success();
    }
}
