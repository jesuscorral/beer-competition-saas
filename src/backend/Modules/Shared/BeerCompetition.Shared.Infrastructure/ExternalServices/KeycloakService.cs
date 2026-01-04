using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BeerCompetition.Shared.Kernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BeerCompetition.Shared.Infrastructure.ExternalServices;

/// <summary>
/// Implementation of IKeycloakService using Keycloak Admin REST API.
/// Handles authentication, user creation, role assignment, and attribute management.
/// </summary>
public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakService> _logger;

    private string KeycloakBaseUrl => _configuration["Keycloak:BaseUrl"] ?? "http://localhost:8080";
    private string Realm => _configuration["Keycloak:Realm"] ?? "beercomp";
    private string AdminUsername => _configuration["Keycloak:AdminUsername"] ?? "admin";
    private string AdminPassword => _configuration["Keycloak:AdminPassword"] ?? "admin";

    public KeycloakService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<KeycloakService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Gets an admin access token from Keycloak.
    /// This token is used to authenticate Admin API requests.
    /// </summary>
    private async Task<Result<string>> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var tokenEndpoint = $"{KeycloakBaseUrl}/realms/master/protocol/openid-connect/token";

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = "admin-cli",
                ["username"] = AdminUsername,
                ["password"] = AdminPassword,
                ["grant_type"] = "password"
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get Keycloak admin token: {Error}", error);
                return Result<string>.Failure($"Failed to authenticate with Keycloak: {response.StatusCode}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken: cancellationToken);

            if (tokenResponse?.AccessToken == null)
            {
                return Result<string>.Failure("Invalid token response from Keycloak");
            }

            return Result<string>.Success(tokenResponse.AccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while getting Keycloak admin token");
            return Result<string>.Failure($"Failed to get admin token: {ex.Message}");
        }
    }

    public async Task<Result<string>> CreateUserAsync(
        string email,
        string password,
        bool emailVerified = true,
        bool enabled = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating Keycloak user: {Email}", email);

        var tokenResult = await GetAdminTokenAsync(cancellationToken);
        if (tokenResult.IsFailure)
        {
            return Result<string>.Failure(tokenResult.Error);
        }

        try
        {
            var createUserUrl = $"{KeycloakBaseUrl}/admin/realms/{Realm}/users";

            var userRepresentation = new
            {
                username = email,
                email = email,
                enabled = enabled,
                emailVerified = emailVerified,
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = password,
                        temporary = false
                    }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, createUserUrl)
            {
                Content = JsonContent.Create(userRepresentation),
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value)
                }
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create Keycloak user {Email}: {Error}", email, error);
                return Result<string>.Failure($"Failed to create user in Keycloak: {response.StatusCode}");
            }

            // Extract user ID from Location header
            var locationHeader = response.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(locationHeader))
            {
                return Result<string>.Failure("User created but ID not returned");
            }

            var userId = locationHeader.Split('/').Last();
            _logger.LogInformation("Keycloak user created successfully: {UserId}", userId);

            return Result<string>.Success(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while creating Keycloak user {Email}", email);
            return Result<string>.Failure($"Failed to create user: {ex.Message}");
        }
    }

    public async Task<Result> AssignRoleAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning role {Role} to user {UserId}", roleName, userId);

        var tokenResult = await GetAdminTokenAsync(cancellationToken);
        if (tokenResult.IsFailure)
        {
            return Result.Failure(tokenResult.Error);
        }

        try
        {
            // First, get the role representation
            var getRoleUrl = $"{KeycloakBaseUrl}/admin/realms/{Realm}/roles/{roleName}";
            using var getRoleRequest = new HttpRequestMessage(HttpMethod.Get, getRoleUrl)
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value)
                }
            };

            var getRoleResponse = await _httpClient.SendAsync(getRoleRequest, cancellationToken);
            if (!getRoleResponse.IsSuccessStatusCode)
            {
                var error = await getRoleResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get role {Role}: {Error}", roleName, error);
                return Result.Failure($"Role '{roleName}' not found in Keycloak");
            }

            var role = await getRoleResponse.Content.ReadFromJsonAsync<KeycloakRoleRepresentation>(cancellationToken: cancellationToken);

            // Now assign the role to the user
            var assignRoleUrl = $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}/role-mappings/realm";
            using var assignRoleRequest = new HttpRequestMessage(HttpMethod.Post, assignRoleUrl)
            {
                Content = JsonContent.Create(new[] { role }),
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value)
                }
            };

            var assignRoleResponse = await _httpClient.SendAsync(assignRoleRequest, cancellationToken);
            if (!assignRoleResponse.IsSuccessStatusCode)
            {
                var error = await assignRoleResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to assign role {Role} to user {UserId}: {Error}", roleName, userId, error);
                return Result.Failure($"Failed to assign role: {assignRoleResponse.StatusCode}");
            }

            _logger.LogInformation("Role {Role} assigned to user {UserId} successfully", roleName, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while assigning role {Role} to user {UserId}", roleName, userId);
            return Result.Failure($"Failed to assign role: {ex.Message}");
        }
    }

    public async Task<Result> SetUserAttributeAsync(
        string userId,
        string attributeName,
        string attributeValue,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting attribute {Attribute}={Value} for user {UserId}", attributeName, attributeValue, userId);

        var tokenResult = await GetAdminTokenAsync(cancellationToken);
        if (tokenResult.IsFailure)
        {
            return Result.Failure(tokenResult.Error);
        }

        try
        {
            // First, get the user to retrieve existing attributes
            var getUserUrl = $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}";
            using var getUserRequest = new HttpRequestMessage(HttpMethod.Get, getUserUrl)
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value)
                }
            };

            var getUserResponse = await _httpClient.SendAsync(getUserRequest, cancellationToken);
            if (!getUserResponse.IsSuccessStatusCode)
            {
                var error = await getUserResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get user {UserId}: {Error}", userId, error);
                return Result.Failure($"User not found: {getUserResponse.StatusCode}");
            }

            var user = await getUserResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken: cancellationToken);
            
            // Get existing attributes or create new dictionary
            var attributes = user?.ContainsKey("attributes") == true
                ? JsonSerializer.Deserialize<Dictionary<string, List<string>>>(user["attributes"].ToString() ?? "{}")
                : new Dictionary<string, List<string>>();

            attributes ??= new Dictionary<string, List<string>>();

            // Update or add the attribute
            attributes[attributeName] = new List<string> { attributeValue };

            // Update the user with new attributes
            var updateUserUrl = $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}";
            using var updateUserRequest = new HttpRequestMessage(HttpMethod.Put, updateUserUrl)
            {
                Content = JsonContent.Create(new { attributes }),
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value)
                }
            };

            var updateUserResponse = await _httpClient.SendAsync(updateUserRequest, cancellationToken);
            if (!updateUserResponse.IsSuccessStatusCode)
            {
                var error = await updateUserResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to update user {UserId} with attribute {Attribute}: {Error}", userId, attributeName, error);
                return Result.Failure($"Failed to set attribute: {updateUserResponse.StatusCode}");
            }

            _logger.LogInformation("Attribute {Attribute} set successfully for user {UserId}", attributeName, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while setting attribute {Attribute} for user {UserId}", attributeName, userId);
            return Result.Failure($"Failed to set attribute: {ex.Message}");
        }
    }

    public async Task<Result> DeleteUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting Keycloak user: {UserId}", userId);

        var tokenResult = await GetAdminTokenAsync(cancellationToken);
        if (tokenResult.IsFailure)
        {
            return Result.Failure(tokenResult.Error);
        }

        try
        {
            var deleteUserUrl = $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}";
            using var request = new HttpRequestMessage(HttpMethod.Delete, deleteUserUrl)
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value)
                }
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to delete Keycloak user {UserId}: {Error}", userId, error);
                return Result.Failure($"Failed to delete user: {response.StatusCode}");
            }

            _logger.LogInformation("Keycloak user deleted successfully: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while deleting Keycloak user {UserId}", userId);
            return Result.Failure($"Failed to delete user: {ex.Message}");
        }
    }

    // DTOs for Keycloak API responses
    private class KeycloakTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    private class KeycloakRoleRepresentation
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
