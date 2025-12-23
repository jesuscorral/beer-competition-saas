namespace BeerCompetition.BFF.ApiGateway.Extensions;

/// <summary>
/// Extension methods for configuring YARP's HTTP client behavior.
/// </summary>
public static class YarpHttpClientExtensions
{
    /// <summary>
    /// Configures YARP's SocketsHttpHandler with optimized settings for reverse proxy scenarios.
    /// </summary>
    /// <remarks>
    /// RESILIENCE NOTE: This BFF acts as a simple reverse proxy. Resilience patterns (circuit breaker, retry, timeout)
    /// are implemented at the downstream service level (Competition Service, Judging Service) rather than at the gateway.
    /// This follows the principle that services should be resilient and handle their own failure scenarios.
    /// 
    /// If gateway-level resilience is needed in the future, implement a custom IForwarderHttpClientFactory
    /// that wraps the HttpMessageInvoker with Polly policies.
    /// </remarks>
    public static void ConfigureForYarp(this SocketsHttpHandler handler)
    {
        // Connection pooling: Reuse connections for up to 5 minutes
        handler.PooledConnectionLifetime = TimeSpan.FromMinutes(5);
        
        // Idle timeout: Close idle connections after 2 minutes
        handler.PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2);
        
        // HTTP/2: Allow multiple streams per connection
        handler.EnableMultipleHttp2Connections = true;
        
        // Proxy: Direct connection to services (no proxy)
        handler.UseProxy = false;
    }
}

