using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ryze.Application.Features.Health.Interfaces;
using Ryze.Domain.Features.Health.Enum;
using Ryze.Domain.Features.Health.Repositories;
using Ryze.Infrastructure.Features.Health.Options;

namespace Ryze.Infrastructure.Features.Health.Background;

/// <summary>
/// Background worker responsible for evaluating health reports and tracking
/// persistent component failures.
/// </summary>
/// <remarks>
/// Combines persisted health snapshots with live component verification to avoid
/// stale alert states. Consecutive failure tracking prevents alert noise caused
/// by transient health check failures.
///
/// Failure tracking is maintained in memory because it represents runtime alert
/// evaluation state rather than durable health history.
/// </remarks>
/// <param name="serviceProvider">Service provider used to create scoped dependencies for health evaluation. </param>
/// <param name="logger">Logger used for operational health alert reporting. </param>
/// <param name="options">Configuration controlling alert evaluation frequency and thresholds. </param>
public sealed class HealthAlertBackgroundWorker(
    IServiceProvider serviceProvider,
    ILogger<HealthAlertBackgroundWorker> logger,
    IOptions<HealthAlertOptions> options) : BackgroundService
{
    private readonly HealthAlertOptions _options = options.Value;

    /// <summary>
    /// Tracks consecutive degraded or unhealthy states per component.
    /// </summary>
    private readonly Dictionary<HealthComponent, FailureState> _failures = new();

    /// <summary>
    /// Prevents processing the same persisted health report multiple times.
    /// </summary>
    /// <remarks>
    /// Health report persistence cadence may differ from alert evaluation cadence.
    /// Without this guard, single persisted failure could incorrectly increase
    /// consecutive failure counters on every worker interval.
    /// </remarks>
    private DateTimeOffset? _lastProcessedReportTimestamp;

    /// <summary>
    /// Executes periodic health alert evaluation.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await EvaluateAlertsAsync(stoppingToken);
                CleanupStaleEntries();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error during health alert evaluation.");
            }
        }
    }

    /// <summary>
    /// Evaluates the latest health report and updates component failure state.
    /// </summary>
    /// <remarks>
    /// Components reported as degraded or unhealthy are verified using live
    /// health checks before failure state changes are applied. This prevents
    /// stale reports from keeping invalid alerts active.
    /// </remarks>
    private async Task EvaluateAlertsAsync(
        CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IHealthReportRepository>();
        var collector = scope.ServiceProvider
            .GetRequiredService<IHealthCheckCollector>();

        var report = await repository.GetLatestAsync(cancellationToken);

        if (report is null)
            return;

        if (_lastProcessedReportTimestamp == report.CheckedAt)
            return;

        _lastProcessedReportTimestamp = report.CheckedAt;

        var seenComponents = new HashSet<HealthComponent>();

        foreach (var component in report.Components)
        {
            var key = component.Component;
            seenComponents.Add(key);

            var status = component.Status;

            if (status is HealthStatus.Unhealthy or HealthStatus.Degraded)
            {
                var live = await collector.CheckSingleAsync(key, cancellationToken);
                status = live.Status;
            }

            if (status is HealthStatus.Unhealthy or HealthStatus.Degraded)
            {
                TrackFailure(key, status);
            }
            else
            {
                TrackRecovery(key);
            }
        }

        foreach (var key in _failures.Keys
            .Except(seenComponents)
            .ToArray())
        {
            _failures.Remove(key);
        }
    }

    /// <summary>
    /// Updates failure streak state for health component.
    /// </summary>
    /// <remarks>
    /// Alerts are emitted only after the configured consecutive failure
    /// threshold is reached to avoid reacting to short-lived incidents.
    /// </remarks>
    private void TrackFailure(
        HealthComponent key,
        HealthStatus status)
    {
        if (!_failures.TryGetValue(key, out var state))
        {
            state = new FailureState();
            _failures[key] = state;
        }

        state.ConsecutiveFailures++;
        state.LastSeen = DateTime.UtcNow;
        state.CurrentStreakStart ??= DateTime.UtcNow;

        var threshold = _options.ConsecutiveFailureThreshold;
        var isCritical = status == HealthStatus.Unhealthy;

        if (state.ConsecutiveFailures == threshold)
        {
            var duration =
                DateTime.UtcNow - state.CurrentStreakStart.Value;

            if (isCritical)
            {
                logger.LogCritical(
                    "Component {Component} has been unhealthy for {Count} consecutive checks. Duration: {Duration}.",
                    key,
                    state.ConsecutiveFailures,
                    duration);
            }
            else
            {
                logger.LogWarning(
                    "Component {Component} has been degraded for {Count} consecutive checks. Duration: {Duration}.",
                    key,
                    state.ConsecutiveFailures,
                    duration);
            }
        }
        else if (state.ConsecutiveFailures == 1)
        {
            logger.LogWarning(
                "Component {Component} is {Status} (first occurrence).",
                key,
                status);
        }
        else if (state.ConsecutiveFailures > threshold && state.ConsecutiveFailures % threshold == 0)
        {
            var duration =
                DateTime.UtcNow - state.CurrentStreakStart.Value;

            logger.Log(
                isCritical
                    ? LogLevel.Warning
                    : LogLevel.Information,
                "Component {Component} is still {Status} after {Count} consecutive checks. Duration: {Duration}.",
                key,
                status,
                state.ConsecutiveFailures,
                duration);
        }
    }

    /// <summary>
    /// Clears failure tracking after component recovery.
    /// </summary>
    private void TrackRecovery(HealthComponent key)
    {
        if (!_failures.Remove(key, out var state))
            return;

        var duration = state.CurrentStreakStart.HasValue
            ? DateTime.UtcNow - state.CurrentStreakStart.Value
            : TimeSpan.Zero;

        if (state.ConsecutiveFailures >=
            _options.ConsecutiveFailureThreshold)
        {
            logger.LogInformation(
                "Component {Component} recovered after {Count} consecutive failures. Downtime: {Duration}.",
                key,
                state.ConsecutiveFailures,
                duration);
        }
        else
        {
            logger.LogInformation(
                "Component {Component} recovered after {Count} checks.",
                key,
                state.ConsecutiveFailures);
        }
    }

    /// <summary>
    /// Removes failure entries that have not been observed recently.
    /// </summary>
    private void CleanupStaleEntries()
    {
        var cutoff = DateTime.UtcNow - _options.StaleEntryLifetime;

        foreach (var key in _failures
                     .Where(x => x.Value.LastSeen < cutoff)
                     .Select(x => x.Key)
                     .ToArray())
        {
            _failures.Remove(key);
        }
    }

    /// <summary>
    /// Runtime state representing consecutive component failures.
    /// </summary>
    private sealed class FailureState
    {
        public int ConsecutiveFailures { get; set; }

        public DateTime LastSeen { get; set; }

        public DateTime? CurrentStreakStart { get; set; }
    }
}