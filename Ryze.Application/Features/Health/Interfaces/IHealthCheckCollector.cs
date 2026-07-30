using Ryze.Domain.Features.Health.Enum;
using Ryze.Domain.Features.Health.ReadModels;

namespace Ryze.Application.Features.Health.Interfaces;

/// <summary>
/// Collects health information for monitored system components.
/// </summary>
/// <remarks>
/// Provides operations for generating health reports, persisting the results,
/// and evaluating the health of individual infrastructure or application
/// components.
/// </remarks>
public interface IHealthCheckCollector
{
    /// <summary>
    /// Collects the current health status of all monitored components.
    /// </summary>
    /// <remarks>
    /// Produces an aggregated <see cref="SystemHealthReport"/> without
    /// persisting the result.
    /// </remarks>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    Task<SystemHealthReport> CollectAsync(CancellationToken ct = default);

    /// <summary>
    /// Collects the current health status of all monitored components and
    /// persists the resulting health report.
    /// </summary>
    /// <remarks>
    /// Combines health evaluation with report persistence, making the
    /// generated report available for monitoring, alerting, and historical
    /// analysis.
    /// </remarks>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    Task<SystemHealthReport> CollectAndPersistAsync(CancellationToken ct = default);

    /// <summary>
    /// Evaluates the health of single monitored component.
    /// </summary>
    /// <param name="component">The component whose health should be evaluated. </param>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    Task<ComponentHealth> CheckSingleAsync(HealthComponent component, CancellationToken ct = default);
}