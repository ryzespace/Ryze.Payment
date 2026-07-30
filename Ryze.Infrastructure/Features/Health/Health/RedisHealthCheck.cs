using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Ryze.Infrastructure.Features.Health.Health;

/// <summary>
/// Health check verifying Redis availability and connectivity.
/// </summary>
/// <remarks>
/// Executes  lightweight Redis connectivity probe using the registered
/// <see cref="IConnectionMultiplexer"/> instance.
///
/// The check does not create or modify Redis state. It only performs
/// <c>PING</c> operation against the configured Redis database to verify
/// </remarks>
/// <param name="serviceProvider">Service provider used to resolve Redis connection infrastructure.</param>
public sealed class RedisHealthCheck(
    IServiceProvider serviceProvider) : IHealthCheck
{
    /// <summary>
    /// Executes Redis availability check.
    /// </summary>
    /// <remarks>
    /// A timeout is applied to prevent unavailable Redis infrastructure from
    /// blocking health evaluation indefinitely.
    /// Connectivity failures are reported as unhealthy because Redis availability
    /// is required for runtime operations when configured.
    /// </remarks>
    /// <param name="context">Health check execution context.</param>
    /// <param name="cancellationToken">Token used to cancel the health check execution.</param>
    /// <returns>Health result describing Redis connectivity state.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var redis = serviceProvider.GetService(typeof(IConnectionMultiplexer)) as IConnectionMultiplexer;

        if (redis is null)
            return HealthCheckResult.Degraded("Redis is not configured.");

        try
        {
            var db = redis.GetDatabase();
            await db.PingAsync();
            return HealthCheckResult.Healthy("Redis connection is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis connection failed.", ex);
        }
    }
}
