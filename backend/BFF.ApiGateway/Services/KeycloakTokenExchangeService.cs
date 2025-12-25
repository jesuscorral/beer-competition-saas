using System.Net.Http.Headers;
using System.Text.Json;

namespace BeerCompetition.BFF.ApiGateway.Services;

/// <summary>
/// Keycloak implementation of OAuth 2.0 Token Exchange (RFC 8693).
/// Exchanges frontend tokens (BFF audience) for service-specific tokens.
/// </summary>
public class KeycloakTokenExchangeService : ITokenExchangeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakTokenExchangeService> _logger;
    private readonly string _tokenEndpoint;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public KeycloakTokenExchangeService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<KeycloakTokenExchangeService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;

        var keycloakSettings = configuration.GetSection("Keycloak");
        _tokenEndpoint = keycloakSettings["TokenEndpoint"] 
            ?? throw new InvalidOperationException("Keycloak:TokenEndpoint not configured");
        _clientId = keycloakSettings["ClientId"] 
            ?? throw new InvalidOperationException("Keycloak:ClientId not configured");
        _clientSecret = keycloakSettings["ClientSecret"] 
            ?? throw new InvalidOperationException("Keycloak:ClientSecret not configured");
    }

    public async Task<string> ExchangeTokenAsync(
        string subjectToken,
        string targetAudience,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subjectToken))
        {
            throw new ArgumentException("Subject token cannot be null or empty", nameof(subjectToken));
        }
        
        if (string.IsNullOrWhiteSpace(targetAudience))
        {
            throw new ArgumentException("Target audience cannot be null or empty", nameof(targetAudience));
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            // RFC 8693: Token Exchange Request
            using var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:token-exchange" },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "subject_token", subjectToken },
                { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
                { "requested_token_type", "urn:ietf:params:oauth:token-type:access_token" },
                { "audience", targetAudience }
            });

            _logger.LogDebug(
                "Exchanging token for audience {Audience} at endpoint {TokenEndpoint}", 
                targetAudience, 
                _tokenEndpoint);

            var response = await httpClient.PostAsync(_tokenEndpoint, requestContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Token exchange failed with status {StatusCode}: {ErrorContent}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException(
                    $"Token exchange failed with status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<TokenExchangeResponse>(responseContent);

            if (tokenResponse?.AccessToken == null)
            {
                _logger.LogError("Token exchange response missing access_token");
                throw new InvalidOperationException("Token exchange response missing access_token");
            }

            _logger.LogDebug(
                "Successfully exchanged token for audience {Audience}, expires in {ExpiresIn} seconds",
                targetAudience,
                tokenResponse.ExpiresIn);

            return tokenResponse.AccessToken;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Token exchange failed for audience {Audience}", targetAudience);
            throw;
        }
    }

    /// <summary>
    /// OAuth 2.0 Token Exchange response (RFC 8693).
    /// </summary>
    private class TokenExchangeResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}
