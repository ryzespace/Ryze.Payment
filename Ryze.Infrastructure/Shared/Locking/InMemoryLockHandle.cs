using Microsoft.Extensions.Logging;
using Ryze.Application.Shared.Locking;
using Ryze.Domain.Shared.Exceptions.Locks;

namespace Ryze.Infrastructure.Shared.Locking;

/// <summary>
/// In-memory lock handle with real, timerenforced TTL.
/// Expiry and explicit disposal share a single guarded release path
/// (<see cref="_state"/> CAS), so exactly one of them can ever release
/// the underlying semaphore — this is the single most important
/// invariant of this type.
/// </summary>
public sealed class InMemoryLockHandle : ILockHandle
{
    // 0 = active, 1 = released (terminal).
    private int _state;
    // 1 if release was triggered by the expiry timer (vs. explicit DisposeAsync).
    private int _expired;
    private long _expiresAtTicks;

    private readonly SemaphoreSlim _semaphore;
    private readonly Action<InMemoryLockHandle> _onRelease;
    private readonly ILogger? _logger;
    private readonly Timer _expiryTimer;
    private readonly Lock _autoRenewalGate = new();

    private CancellationTokenSource? _autoRenewalCts;
    private static readonly TimeSpan MaxDueTime = TimeSpan.FromMilliseconds(uint.MaxValue - 2);

    /// <inheritdoc/>
    public string Key { get; }

    /// <inheritdoc/>
    public DateTimeOffset AcquiredAt { get; }

    /// <inheritdoc/>
    public DateTimeOffset ExpiresAt => new(Interlocked.Read(ref _expiresAtTicks), TimeSpan.Zero);

    /// <summary>
    /// Constructs a handle for already acquired semaphore.
    /// </summary>
    /// <param name="key">Resource key being locked.</param>
    /// <param name="semaphore">Semaphore that was successfully acquired.</param>
    /// <param name="expiry">Initial TTL for this lock.</param>
    /// <param name="onRelease">
    /// Callback invoked exactly once, after the semaphore has been released,
    /// so the owning provider can decrement the backing <see cref="LockEntry"/>
    /// ref count and retire it if unused. Must be inexpensive and non-throwing.
    /// </param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    internal InMemoryLockHandle(
        string key,
        SemaphoreSlim semaphore,
        TimeSpan expiry,
        Action<InMemoryLockHandle> onRelease,
        ILogger? logger)
    {
        Key = key;
        _semaphore = semaphore;
        _onRelease = onRelease;
        _logger = logger;

        AcquiredAt = DateTimeOffset.UtcNow;
        Interlocked.Exchange(ref _expiresAtTicks, AcquiredAt.Add(expiry).UtcTicks);

        // Twostep timer init: create dormant, then arm it. This guarantees the
        // _expiryTimer field is fully assigned before the callback can possibly
        // fire, even for near zero expiry.
        _expiryTimer = new Timer(OnExpired, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _expiryTimer.Change(Clamp(expiry), Timeout.InfiniteTimeSpan);
    }

    /// <summary>Clamp timer due time to the maximum supported by the runtime.</summary>
    private static TimeSpan Clamp(TimeSpan due)
    {
        if (due < TimeSpan.Zero) return TimeSpan.Zero;
        return due > MaxDueTime ? MaxDueTime : due;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Extends both the reported <see cref="ExpiresAt"/> and the physical timer
    /// that actually releases the semaphore.
    /// </remarks>
    /// <exception cref="LockExpiredException">
    /// The lock was force-released by the expiry timer before this call.
    /// Unlike explicit <see cref="DisposeAsync"/> (which is a deliberate
    /// release), timer expiry means the TTL was not adequate for the
    /// critical section - this is signaled as an exception so the caller
    /// cannot silently proceed without holding the lock.
    /// </exception>
    public Task<bool> RenewAsync(TimeSpan extraTime)
    {
        if (Volatile.Read(ref _state) != 0)
        {
            if (Volatile.Read(ref _expired) != 0)
                throw new LockExpiredException(Key, ExpiresAt);
            return Task.FromResult(false);
        }

        long current, updated;
        do
        {
            current = Interlocked.Read(ref _expiresAtTicks);
            updated = new DateTimeOffset(current, TimeSpan.Zero).Add(extraTime).UtcTicks;
        } while (Interlocked.CompareExchange(ref _expiresAtTicks, updated, current) != current);

        var due = new DateTimeOffset(updated, TimeSpan.Zero) - DateTimeOffset.UtcNow;

        try
        {
            _expiryTimer.Change(Clamp(due), Timeout.InfiniteTimeSpan);
        }
        catch (ObjectDisposedException)
        {
            // Expired/disposed concurrently between the state check above and here.
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Starts a background loop that calls <see cref="RenewAsync"/> every
    /// <paramref name="interval"/> until the lock is released or disposed.
    /// Safe to call multiple times — only the first call has an effect.
    /// </remarks>
    public void StartAutoRenewal(TimeSpan interval, TimeSpan extensionTime)
    {
        lock (_autoRenewalGate)
        {
            if (Volatile.Read(ref _state) != 0 || _autoRenewalCts is not null)
                return;

            _autoRenewalCts = new CancellationTokenSource();
            _ = RunAutoRenewalAsync(interval, extensionTime, _autoRenewalCts.Token);
        }
    }

    /// <summary>Background loop that periodically extends the lock TTL.</summary>
    private async Task RunAutoRenewalAsync(TimeSpan interval, TimeSpan extensionTime, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(interval, token).ConfigureAwait(false);

                if (!await RenewAsync(extensionTime).ConfigureAwait(false))
                {
                    _logger?.LogDebug("Stopping auto-renewal for lock {Key}: renewal failed (expired or disposed).", Key);
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown via Dispose/StopAutoRenewal.
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Auto-renewal loop faulted for lock {Key}.", Key);
        }
    }

    /// <summary>
    /// Timer callback. Fires when TTL elapses without a renewal.
    /// Competes with <see cref="DisposeAsync"/> on <see cref="_state"/>
    /// only the first path to set it releases the semaphore.
    /// </summary>
    private void OnExpired(object? _)
    {
        if (Interlocked.CompareExchange(ref _state, 1, 0) != 0)
            return; // Already disposed explicitly; expiry lost the race, which is fine.

        Volatile.Write(ref _expired, 1);
        StopAutoRenewal();
        _expiryTimer.Dispose();
        _semaphore.Release();

        InvokeOnRelease();
        _logger?.LogDebug("Lock {Key} expired at {ExpiresAt:o} and was auto-released.", Key, ExpiresAt);
    }

    /// <inheritdoc/>
    public async ValueTask ReleaseAsync() => await DisposeAsync().ConfigureAwait(false);

    /// <inheritdoc/>
    /// <remarks>
    /// Releases the semaphore and stops the expiry timer and auto-renewal loop.
    /// Idempotent safe to call multiple times.
    /// </remarks>
    public ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _state, 1, 0) != 0)
            return ValueTask.CompletedTask; // Already released (explicitly or by expiry).

        StopAutoRenewal();
        _expiryTimer.Dispose();
        _semaphore.Release();

        InvokeOnRelease();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Stops the auto-renewal loop under the protection of <see cref="_autoRenewalGate"/>.
    /// </summary>
    private void StopAutoRenewal()
    {
        lock (_autoRenewalGate)
        {
            _autoRenewalCts?.Cancel();
            _autoRenewalCts?.Dispose();
            _autoRenewalCts = null;
        }
    }

    /// <summary>
    /// Invokes the release callback, catching any exceptions so the
    /// release path never throws (critical for timer threads).
    /// </summary>
    private void InvokeOnRelease()
    {
        try
        {
            _onRelease(this);
        }
        catch (Exception ex)
        {
            // Must never throw out of timer callback or bubble past Dispose.
            _logger?.LogWarning(ex, "Release callback failed for lock {Key}.", Key);
        }
    }
}
