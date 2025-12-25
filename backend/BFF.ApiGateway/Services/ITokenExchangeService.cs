namespace BeerCompetition.BFF.ApiGateway.Services;

/// <summary>
/// Service for exchanging frontend JWT tokens for service-specific tokens.
/// Implements OAuth 2.0 Token Exchange (RFC 8693) with Keycloak.
/// </summary>
public interface ITokenExchangeService
{
    /// <summary>
    /// Exchanges the user's BFF token for a service-specific token with appropriate audience.
    /// </summary>
    /// <param name="subjectToken">Original JWT token from frontend (BFF audience)</param>
    /// <param name="targetAudience">Target service audience (e.g., "competition-service")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service-specific JWT token with correct audience claim</returns>
    Task<string> ExchangeTokenAsync(
        string subjectToken, 
        string targetAudience, 
        CancellationToken cancellationToken = default);
}
