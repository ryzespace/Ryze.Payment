using Ryze.Domain.Features.Health.Enum;

namespace Ryze.Domain.Features.Health.Events;

/// <summary>
/// Represents the completion of health check execution across all monitored components.
/// </summary>
/// <param name="OverallStatus"> The overall health status determined from all evaluated components.</param>
/// <param name="ComponentsChecked"> The total number of components evaluated during the health check. </param>
/// <param name="HealthyCount"> The number of components reported as <see cref="HealthStatus.Healthy"/>.</param>
/// <param name="DegradedCount"> The number of components reported as <see cref="HealthStatus.Degraded"/>. </param>
/// <param name="UnhealthyCount">The number of components reported as <see cref="HealthStatus.Unhealthy"/>. </param>
/// <param name="TotalDuration"> The total time required to complete the health check. </param>
/// <param name="CheckedAt"> The timestamp when the health check completed.</param>
public sealed record HealthCheckCompleted(
    HealthStatus OverallStatus,
    int ComponentsChecked,
    int HealthyCount,
    int DegradedCount,
    int UnhealthyCount,
    TimeSpan TotalDuration,
    DateTimeOffset CheckedAt
);