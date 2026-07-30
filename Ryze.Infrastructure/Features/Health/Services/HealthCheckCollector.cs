using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Ryze.Application.Features.Health.Interfaces;
using Ryze.Application.Features.Health.Service;
using Ryze.Domain.Features.Health.Enum;
using Ryze.Domain.Features.Health.ReadModels;
using Ryze.Domain.Features.Health.Repositories;
using DomainHealthStatus = Ryze.Domain.Features.Health.Enum.HealthStatus;
using DomainHealthComponent = Ryze.Domain.Features.Health.Enum.HealthComponent;
using HealthCheckStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus;

namespace Ryze.Infrastructure.Features.Health.Services;

/// <summary>
/// Collects health information from registered health checks and transforms
/// the results into domain health reports.
/// </summary>
/// <remarks>
/// Executes registered <see cref="IHealthCheck"/> implementations, maps their
/// results into domain health models, records health metrics, and optionally
/// persists generated reports.
/// Type caches health check component mappings because the relationship
/// between health check implementation and its domain component is immutable.
/// </remarks>
/// <param name="scopeFactory">Factory used to create scoped service providers for resolving health checks. </param>
/// <param name="repository">Repository used for persisting health reports. </param>
/// <param name="metrics">Service responsible for recording health related telemetry. </param>
/// <param name="logger">Logger used for diagnostics and component mapping warnings. </param>
public sealed class HealthCheckCollector(
    IServiceScopeFactory scopeFactory,
    IHealthReportRepository repository,
    HealthMetricsService metrics,
    ILogger<HealthCheckCollector> logger) : IHealthCheckCollector
{
    /// <summary>
    /// Cache storing resolved mappings between health check implementation types
    /// and domain health components.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, DomainHealthComponent> ComponentMapCache = new();

    private readonly TimeSpan _checkTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Collects health information from all registered health checks.
    /// </summary>
    /// <returns>
    /// A completed system health report containing the status of all monitored
    /// components.
    /// </returns>
    /// <param name="ct">The cancellation token used to cancel the health collection operation. </param>
    public async Task<SystemHealthReport> CollectAsync(
        CancellationToken ct = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var checks = scope.ServiceProvider
            .GetServices<IHealthCheck>()
            .ToList();

        var sw = Stopwatch.StartNew();

        var tasks = checks
            .Select(check => RunCheckAsync(check, ct))
            .ToList();

        var components = await Task.WhenAll(tasks);

        sw.Stop();

        var overallStatus = DomainHealthStatus.Healthy;

        foreach (var component in components)
        {
            if (component.Status > overallStatus)
                overallStatus = component.Status;
        }

        var report = new SystemHealthReport
        {
            OverallStatus = overallStatus,
            Components = components.ToList(),
            TotalDuration = sw.Elapsed,
            CheckedAt = DateTimeOffset.UtcNow
        };

        metrics.Record(report);

        return report;
    }

    /// <summary>
    /// Collects current health information and persists the generated report.
    /// </summary>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    /// <returns>The generated and persisted system health report.</returns>
    public async Task<SystemHealthReport> CollectAndPersistAsync(
        CancellationToken ct = default)
    {
        var report = await CollectAsync(ct);

        await repository.SaveAsync(report, ct);

        return report;
    }

    /// <summary>
    /// Executes health check for single monitored component.
    /// </summary>
    /// <param name="component">The component whose health should be evaluated. </param>
    /// <param name="ct">The cancellation token used to cancel the operation.</param>
    /// <returns>The health result for the requested component. </returns>
    public async Task<ComponentHealth> CheckSingleAsync(
        HealthComponent component,
        CancellationToken ct = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var checks = scope.ServiceProvider
            .GetServices<IHealthCheck>()
            .ToList();

        var check = checks.FirstOrDefault(c =>
            MapComponent(c.GetType()) == component);

        if (check is null)
        {
            return new ComponentHealth
            {
                Component = component,
                Status = DomainHealthStatus.Unhealthy,
                Description = $"No health check registered for {component}",
                CheckedAt = DateTimeOffset.UtcNow
            };
        }

        return await RunCheckAsync(check, ct);
    }

    /// <summary>
    /// Executes an individual health check and converts
    /// The result into domain health model.
    /// </summary>
    /// <remarks>
    /// Health checks are protected by an internal timeout to prevent single
    /// dependency from blocking the complete health evaluation process.
    /// </remarks>
    /// <param name="check">The health check implementation to execute. </param>
    /// <param name="externalCt">The caller cancellation token. </param>
    /// <returns>A component health result. </returns>
    private async Task<ComponentHealth> RunCheckAsync(IHealthCheck check, CancellationToken externalCt)
    {
        var mappedComponent = MapComponent(check.GetType());

        using var timeoutCts =
            CancellationTokenSource.CreateLinkedTokenSource(externalCt);

        timeoutCts.CancelAfter(_checkTimeout);

        var sw = Stopwatch.StartNew();

        try
        {
            var registration = new HealthCheckRegistration(
                check.GetType().Name,
                check,
                HealthCheckStatus.Unhealthy,
                tags: null);

            var context = new HealthCheckContext
            {
                Registration = registration
            };

            var result = await check.CheckHealthAsync(
                context,
                timeoutCts.Token);

            sw.Stop();

            var status = result.Status switch
            {
                HealthCheckStatus.Healthy =>
                    DomainHealthStatus.Healthy,

                HealthCheckStatus.Degraded =>
                    DomainHealthStatus.Degraded,

                _ => DomainHealthStatus.Unhealthy
            };

            return new ComponentHealth
            {
                Component = mappedComponent,
                Status = status,
                Description = result.Description,
                ErrorMessage = result.Exception?.Message,
                Duration = sw.Elapsed,
                CheckedAt = DateTimeOffset.UtcNow
            };
        }
        catch (OperationCanceledException)
            when (externalCt.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            sw.Stop();

            return new ComponentHealth
            {
                Component = mappedComponent,
                Status = DomainHealthStatus.Unhealthy,
                ErrorMessage = "Health check timed out",
                Duration = sw.Elapsed,
                CheckedAt = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            sw.Stop();

            return new ComponentHealth
            {
                Component = mappedComponent,
                Status = DomainHealthStatus.Unhealthy,
                ErrorMessage = $"Health check threw: {ex.Message}",
                Duration = sw.Elapsed,
                CheckedAt = DateTimeOffset.UtcNow
            };
        }
    }

    /// <summary>
    /// Resolves the domain health component represented by health check type.
    /// </summary>
    /// <param name="type">The health check implementation type. </param>
    /// <returns>The mapped domain health component. </returns>
    private DomainHealthComponent MapComponent(Type type)
    {
        return ComponentMapCache.GetOrAdd(type, static (t, log) =>
        {
            var name = t.Name
                .Replace("HealthCheck", "")
                .ToLowerInvariant();

            var mapped = name switch
            {
                "marten" => DomainHealthComponent.EventStore,
                "redis" => DomainHealthComponent.Cache,
                "ledger_integrity" or "ledgerintegrity" =>
                    DomainHealthComponent.Ledger,
                "stripe" => DomainHealthComponent.PaymentProvider,
                "system" => DomainHealthComponent.Database,
                "wolverine" => DomainHealthComponent.MessageBus,
                _ => (DomainHealthComponent?)null
            };

            if (mapped is null)
            {
                log.LogWarning(
                    "Unrecognized IHealthCheck type {CheckType}; falling back to {Fallback}. " +
                    "Add an explicit mapping to avoid misattributing its status.",
                    t.Name,
                    DomainHealthComponent.Unknown);

                return DomainHealthComponent.Unknown;
            }

            return mapped.Value;
        }, logger);
    }
}