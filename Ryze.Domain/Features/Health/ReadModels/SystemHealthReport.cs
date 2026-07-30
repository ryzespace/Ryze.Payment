using Ryze.Domain.Features.Health.Enum;

namespace Ryze.Domain.Features.Health.ReadModels;

/// <summary>
/// Represents the aggregated health report for the entire system.
/// </summary>
public sealed record SystemHealthReport
{
    /// <summary>
    /// Gets the unique identifier of the health report.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the overall health status derived from all monitored components.
    /// </summary>
    public required HealthStatus OverallStatus { get; init; }

    /// <summary>
    /// Gets the health results for each monitored component.
    /// </summary>
    public required IReadOnlyList<ComponentHealth> Components { get; init; }

    /// <summary>
    /// Gets the total duration required to execute all health checks.
    /// </summary>
    public required TimeSpan TotalDuration { get; init; }

    /// <summary>
    /// Gets the timestamp when the health report was generated.
    /// </summary>
    public required DateTimeOffset CheckedAt { get; init; }
}