using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Shared.Infrastructure.ExternalServices;

/// <summary>
/// Interface for Keycloak Admin API operations.
/// Handles user management and authentication integration.
/// </summary>
public interface IKeycloakService
{
    /// <summary>
    /// Creates a new user in Keycloak.
    /// </summary>
    /// <param name="email">User's email address (will be used as username).</param>
    /// <param name="password">Initial password for the user.</param>
    /// <param name="emailVerified">Whether the email should be marked as verified.</param>
    /// <param name="enabled">Whether the user account should be enabled.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with the created user's ID or error message.</returns>
    Task<Result<string>> CreateUserAsync(
        string email,
        string password,
        bool emailVerified = true,
        bool enabled = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user in Keycloak.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="roleName">The role name to assign (e.g., "organizer", "judge", "entrant").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> AssignRoleAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a custom attribute on a Keycloak user.
    /// Used for multi-tenancy (tenant_id) and other custom claims.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="attributeName">The attribute name (e.g., "tenant_id", "competition_id").</param>
    /// <param name="attributeValue">The attribute value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> SetUserAttributeAsync(
        string userId,
        string attributeName,
        string attributeValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user from Keycloak.
    /// Used for cleanup in case of transaction rollback.
    /// </summary>
    /// <param name="userId">The Keycloak user ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> DeleteUserAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
