using System.Diagnostics.Metrics;
using Ryze.Domain.Features.Health.Enum;
using Ryze.Domain.Features.Health.ReadModels;

namespace Ryze.Application.Features.Health.Service;

/// <summary>
/// Records application health metrics using .NET metrics instrumentation.
/// </summary>
/// <remarks>
/// Publishes metrics describing overall health check executions as well as
/// per component health status and execution duration. These metrics are intended
/// for consumption by OpenTelemetry and compatible monitoring systems.
/// </remarks>
/// <param name="meter">The meter used to create counters and histograms for health telemetry.</param>
public sealed class HealthMetricsService(Meter meter)
{
    private readonly Counter<long> _healthyCounter = meter.CreateCounter<long>("ryze.health.checks.healthy", "count");
    private readonly Counter<long> _degradedCounter = meter.CreateCounter<long>("ryze.health.checks.degraded", "count");
    private readonly Counter<long> _unhealthyCounter = meter.CreateCounter<long>("ryze.health.checks.unhealthy", "count");
    private readonly Counter<long> _checkTotalCounter = meter.CreateCounter<long>("ryze.health.checks.total", "count");
    private readonly Histogram<double> _checkDurationHistogram = meter.CreateHistogram<double>("ryze.health.check.duration", "ms");

    private readonly Counter<long> _componentHealthyCounter = meter.CreateCounter<long>("ryze.health.component.healthy", "count");
    private readonly Counter<long> _componentDegradedCounter = meter.CreateCounter<long>("ryze.health.component.degraded", "count");
    private readonly Counter<long> _componentUnhealthyCounter = meter.CreateCounter<long>("ryze.health.component.unhealthy", "count");
    private readonly Histogram<double> _componentDurationHistogram = meter.CreateHistogram<double>("ryze.health.component.duration", "ms");

    /// <summary>
    /// Creates metric tag identifying a monitored component.
    /// </summary>
    /// <param name="c">The monitored health component.</param>
    /// <returns> A metric tag containing the component name. </returns>
    private static KeyValuePair<string, object?> ComponentTag(HealthComponent c)
        => new("component", c.ToString());

    /// <summary>
    /// Records metrics for completed system health report.
    /// </summary>
    /// <remarks>
    /// Publishes aggregate counters for healthy, degraded, and unhealthy
    /// components, records the overall health check duration, and emits
    /// per-component metrics for status and execution time.
    /// </remarks>
    /// <param name="report">The completed system health report to record. </param>
    public void Record(SystemHealthReport report)
    {
        var healthy = 0;
        var degraded = 0;
        var unhealthy = 0;

        foreach (var c in report.Components)
        {
            var tag = ComponentTag(c.Component);

            switch (c.Status)
            {
                case HealthStatus.Healthy:
                    healthy++;
                    _componentHealthyCounter.Add(1, tag);
                    break;

                case HealthStatus.Degraded:
                    degraded++;
                    _componentDegradedCounter.Add(1, tag);
                    break;

                case HealthStatus.Unhealthy:
                    unhealthy++;
                    _componentUnhealthyCounter.Add(1, tag);
                    break;
            }

            _componentDurationHistogram.Record(c.Duration.TotalMilliseconds, tag);
        }

        _healthyCounter.Add(healthy);
        _degradedCounter.Add(degraded);
        _unhealthyCounter.Add(unhealthy);
        _checkTotalCounter.Add(1);
        _checkDurationHistogram.Record(report.TotalDuration.TotalMilliseconds);
    }

    /// <summary>
    /// Records metrics for single monitored component.
    /// </summary>
    /// <remarks>
    /// Updates the status counter corresponding to the component's health state
    /// and records the duration of the component health check.
    /// </remarks>
    /// <param name="component">The component health result to record. </param>
    public void RecordComponent(ComponentHealth component)
    {
        var tag = ComponentTag(component.Component);

        switch (component.Status)
        {
            case HealthStatus.Healthy:
                _componentHealthyCounter.Add(1, tag);
                break;

            case HealthStatus.Degraded:
                _componentDegradedCounter.Add(1, tag);
                break;

            case HealthStatus.Unhealthy:
                _componentUnhealthyCounter.Add(1, tag);
                break;
        }

        _componentDurationHistogram.Record(component.Duration.TotalMilliseconds, tag);
    }
}