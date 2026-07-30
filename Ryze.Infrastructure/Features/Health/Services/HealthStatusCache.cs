using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ryze.Application.Features.Health.Interfaces;
using Ryze.Domain.Features.Health.Enum;
using Ryze.Domain.Features.Health.ReadModels;
using Ryze.Infrastructure.Features.Health.Options;

namespace Ryze.Infrastructure.Features.Health.Services;

/// <summary>
/// Provides cached health report collection using stale-while-revalidate semantics.
/// </summary>
public sealed class HealthStatusCache(
    IHealthCheckCollector inner,
    IOptions<HealthStatusCacheOptions> options,
    IHealthCacheMetrics metrics,
    ILogger<HealthStatusCache> logger)
    : IHealthCheckCollector, IAsyncDisposable
{
    private readonly HealthStatusCacheOptions _options = options.Value;

    private volatile SystemHealthReport? _cached;

    /// <summary>
    /// Monotonic timestamp when the current cache entry was created.
    /// </summary>
    private long _cachedAtTicks;

    /// <summary>
    /// Monotonic timestamp after which the entry becomes stale.
    /// </summary>
    private long _softExpiryTicks = long.MinValue;

    /// <summary>
    /// Monotonic timestamp after which the entry can no longer normally be served.
    /// </summary>
    private long _hardExpiryTicks = long.MinValue;

    private readonly SemaphoreSlim _lock = new(1, 1);

    private int _backgroundRefreshInFlight;
    private Task? _backgroundRefreshTask;

    private readonly Lock _backgroundTaskGate = new();

    private readonly CancellationTokenSource _shutdownCts = new();

    private volatile bool _disposed;

    /// <summary>
    /// Retrieves system health report using cached stale while revalidate behavior.
    /// </summary>
    /// <remarks>
    /// Fresh reports are returned immediately. Stale reports trigger asynchronous
    /// refresh while remaining available to callers. Fully expired reports require
    /// synchronous refresh before returning.
    /// </remarks>
    /// <param name="ct">The cancellation token used to cancel the operation.</param>
    /// <returns>The current system health report.</returns>
    public async Task<SystemHealthReport> CollectAsync(
        CancellationToken ct = default)
    {
        ThrowIfDisposed();

        var now = Environment.TickCount64;

        if (_cached is { } fresh && now < _softExpiryTicks)
        {
            metrics.RecordHit();
            return fresh;
        }

        if (_cached is { } stale && now < _hardExpiryTicks)
        {
            metrics.RecordStaleHit();
            TriggerBackgroundRefresh();

            return stale;
        }

        metrics.RecordMiss();

        await _lock.WaitAsync(ct);

        try
        {
            now = Environment.TickCount64;

            if (_cached is { } snapshot)
            {
                if (now < _softExpiryTicks)
                    return snapshot;

                if (now < _hardExpiryTicks)
                {
                    TriggerBackgroundRefresh();
                    return snapshot;
                }
            }

            try
            {
                var report = await inner.CollectAsync(ct);

                SetCache(report);

                return report;
            }
            catch (Exception ex)
                when (CanFailOpen(now, out var staleReport))
            {
                metrics.RecordFailOpen();

                logger.LogWarning(
                    ex,
                    "Health check refresh failed; serving fail-open stale report (age: {AgeMs}ms)",
                    now - _cachedAtTicks);

                return staleReport!;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Collects a health report, persists it, and updates the cache.
    /// </summary>
    /// <remarks>
    /// Unlike normal collection, this operation always executes the underlying
    /// collector because persistence represents an explicit state-changing action.
    /// </remarks>
    public async Task<SystemHealthReport> CollectAndPersistAsync(
        CancellationToken ct = default)
    {
        ThrowIfDisposed();
        await _lock.WaitAsync(ct);

        try
        {
            var report = await inner.CollectAndPersistAsync(ct);
            SetCache(report);
            return report;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Executes a health check for a single component without using the aggregate cache.
    /// </summary>
    public Task<ComponentHealth> CheckSingleAsync(
        HealthComponent component,
        CancellationToken ct = default)
    {
        ThrowIfDisposed();
        return inner.CheckSingleAsync(component, ct);
    }

    /// <summary>
    /// Determines whether stale cache data can be returned after refresh failure.
    /// </summary>
    /// <remarks>
    /// Must be called while holding <c>_lock</c> because cached state is accessed
    /// without additional synchronization.
    /// </remarks>
    private bool CanFailOpen(
        long now,
        out SystemHealthReport? staleReport)
    {
        staleReport = null;

        if (!_options.FailOpen)
            return false;

        if (_cached is not { } cached)
            return false;

        var age = now - _cachedAtTicks;

        if (age > _options.FailOpenMaxAge.TotalMilliseconds)
            return false;

        staleReport = cached;

        return true;
    }

    /// <summary>
    /// Starts a background cache refresh if one is not already running.
    /// </summary>
    /// <remarks>
    /// Ensures only a single refresh operation executes concurrently to avoid
    /// duplicated health checks during stale cache usage.
    /// </remarks>
    private void TriggerBackgroundRefresh()
    {
        if (_disposed)
            return;

        if (Interlocked.CompareExchange(
                ref _backgroundRefreshInFlight,
                1,
                0) != 0)
        {
            return;
        }

        var shutdownToken = _shutdownCts.Token;

        var task = Task.Run(async () =>
        {
            try
            {
                await _lock.WaitAsync(shutdownToken);

                try
                {
                    if (Environment.TickCount64 >= _softExpiryTicks)
                    {
                        var report = await inner.CollectAsync(shutdownToken);

                        SetCache(report);
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (OperationCanceledException)
                when (shutdownToken.IsCancellationRequested)
            {
                // Expected during shutdown.
            }
            catch (Exception ex)
            {
                metrics.RecordBackgroundRefreshFailure(ex);

                logger.LogWarning(
                    ex,
                    "Background health cache refresh failed; serving stale value until next hard expiry");
            }
            finally
            {
                Volatile.Write(ref _backgroundRefreshInFlight, 0);

                lock (_backgroundTaskGate)
                {
                    _backgroundRefreshTask = null;
                }
            }
        }, CancellationToken.None);

        lock (_backgroundTaskGate)
        {
            _backgroundRefreshTask = task;
        }
    }

    /// <summary>
    /// Updates the cached health report and calculates new expiration thresholds.
    /// </summary>
    private void SetCache(SystemHealthReport report)
    {
        var now = Environment.TickCount64;

        var jitterMs = (int)_options.JitterRange.TotalMilliseconds;

        var jitter = jitterMs > 0
            ? Random.Shared.Next(-jitterMs, jitterMs)
            : 0;

        _cached = report;
        _cachedAtTicks = now;

        _softExpiryTicks =
            now + (long)_options.Ttl.TotalMilliseconds + jitter;

        _hardExpiryTicks =
            now + (long)(_options.Ttl + _options.StaleWindow)
                .TotalMilliseconds + jitter;
    }

    /// <summary>
    /// Throws when the cache instance has already been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HealthStatusCache));
    }

    /// <summary>
    /// Stops background refresh operations and releases resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        await _shutdownCts.CancelAsync();

        Task? pending;

        lock (_backgroundTaskGate)
        {
            pending = _backgroundRefreshTask;
        }

        if (pending is not null)
        {
            try
            {
                await pending;
            }
            catch
            {
                // Exceptions are already logged inside background refresh task.
            }
        }

        _shutdownCts.Dispose();
        _lock.Dispose();
    }
}