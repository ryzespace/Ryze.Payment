namespace Ryze.Domain.Features.Health.Enum;

/// <summary>
/// Represents the health state of monitored component.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// The component is operating normally without detected issues.
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// The component is operational but experiencing reduced performance
    /// or partial functionality.
    /// </summary>
    Degraded = 1,

    /// <summary>
    /// The component is unavailable or unable to perform its expected function.
    /// </summary>
    Unhealthy = 2
}