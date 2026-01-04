using BeerCompetition.BFF.ApiGateway.Services;
using Yarp.ReverseProxy.Transforms;

namespace BeerCompetition.BFF.ApiGateway.Transforms;

/// <summary>
/// YARP request transform that exchanges frontend JWT tokens for service-specific tokens.
/// Injects the exchanged token into Authorization header before forwarding to microservices.
/// </summary>
public class TokenExchangeTransform : RequestTransform
{
    private readonly ITokenExchangeService _tokenExchangeService;
    private readonly ILogger<TokenExchangeTransform> _logger;
    private readonly string _targetAudience;

    public TokenExchangeTransform(
        ITokenExchangeService tokenExchangeService,
        ILogger<TokenExchangeTransform> logger,
        string targetAudience)
    {
        _tokenExchangeService = tokenExchangeService 
            ?? throw new ArgumentNullException(nameof(tokenExchangeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _targetAudience = targetAudience 
            ?? throw new ArgumentException("Target audience cannot be null", nameof(targetAudience));
    }

    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        try
        {
            // Extract original token from Authorization header
            var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("No Bearer token found in Authorization header for token exchange");
                return;
            }

            var originalToken = authHeader.Substring("Bearer ".Length).Trim();

            // Exchange token for service-specific token
            var exchangedToken = await _tokenExchangeService.ExchangeTokenAsync(
                originalToken,
                _targetAudience,
                context.HttpContext.RequestAborted);

            // Replace Authorization header with exchanged token
            context.ProxyRequest.Headers.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", exchangedToken);

            _logger.LogDebug(
                "Successfully exchanged token for service {Audience} on route {Path}",
                _targetAudience,
                context.HttpContext.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Token exchange failed for service {Audience} on route {Path}",
                _targetAudience,
                context.HttpContext.Request.Path);
            
            // Don't throw - let the downstream service reject the request with original token
            // This prevents cascading failures and provides better error messages to clients
        }
    }
}
