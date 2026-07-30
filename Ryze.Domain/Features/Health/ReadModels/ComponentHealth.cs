using Ryze.Domain.Features.Health.Enum;

namespace Ryze.Domain.Features.Health.ReadModels;

/// <summary>
/// Represents the health status of single monitored component.
/// </summary>
public sealed record ComponentHealth
{
    /// <summary>
    /// Gets the monitored component.
    /// </summary>
    public required HealthComponent Component { get; init; }

    /// <summary>
    /// Gets the current health status of the component.
    /// </summary>
    public required HealthStatus Status { get; init; }

    /// <summary>
    /// Gets an optional description of the health result.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets an optional error message describing the failure.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the duration of the health check execution.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets the timestamp when the health check completed.
    /// </summary>
    public required DateTimeOffset CheckedAt { get; init; }
}