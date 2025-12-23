using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace BeerCompetition.BFF.ApiGateway.Extensions;

/// <summary>
/// Extension methods for configuring resilience policies (circuit breaker, retry) for HTTP clients.
/// Implements resilience patterns as specified in issue #67.
/// </summary>
public static class ResilienceExtensions
{
    public static IServiceCollection AddResiliencePolicies(this IServiceCollection services)
    {
        services.AddHttpClient("resilient-client")
            .AddStandardResilienceHandler(options =>
            {
                // Circuit Breaker
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 5;
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

                // Retry with exponential backoff
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;

                // Timeout
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            });

        return services;
    }
}
