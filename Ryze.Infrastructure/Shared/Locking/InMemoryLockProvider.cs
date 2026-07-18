using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Ryze.Application.Shared.Locking;
using Ryze.Domain.Shared.Exceptions.Locks;

namespace Ryze.Infrastructure.Shared.Locking;

/// <summary>
/// In-memory implementation of <see cref="ILockProvider"/> with real,
/// timer-enforced TTL and safe, ref-counted key cleanup.
/// </summary>
/// <remarks>
/// Single process only - intended for local dev/test or single-instance
/// deployments. Use a distributed provider (e.g., Redis) for multi-instance
/// production environments.
///
/// <para>
/// TTL semantics intentionally mirror distributed lock providers: once
/// <c>expiry</c> elapses without a <see cref="ILockHandle.RenewAsync"/> call,
/// the lock is force-released even if the original owner is still "using" it.
/// Callers doing long critical sections should either call
/// <see cref="ILockHandle.StartAutoRenewal"/> or periodically check
/// <see cref="ILockHandle.IsExpired"/>.
/// </para>
///
/// <para>
/// Keys are stored as <see cref="LockEntry"/> in a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> and serialised via the
/// entry's own monitor lock during acquire/release. This eliminates the
/// TOCTOU race present in simpler dictionary-based implementations and
/// guarantees deterministic cleanup of unused entries.
/// </para>
/// </remarks>
public sealed class InMemoryLockProvider(
    TimeSpan? defaultAcquisitionTimeout = null,
    ILogger<InMemoryLockProvider>? logger = null) : ILockProvider
{
    private readonly ConcurrentDictionary<string, LockEntry> _locks = new();
    private readonly TimeSpan _defaultAcquisitionTimeout = defaultAcquisitionTimeout ?? TimeSpan.FromSeconds(10);

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null or empty.</exception>
    /// <exception cref="LockAcquisitionFailedException">
    /// The lock could not be acquired within <see cref="_defaultAcquisitionTimeout"/>.
    /// </exception>
    public async Task<ILockHandle> AcquireAsync(string key, TimeSpan expiry, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var entry = AcquireEntry(key);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_defaultAcquisitionTimeout);

        try
        {
            await entry.Semaphore.WaitAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            ReleaseEntry(key, entry);
            throw new LockAcquisitionFailedException(key, $"Timed out acquiring lock '{key}' after {_defaultAcquisitionTimeout}.");
        }
        catch
        {
            ReleaseEntry(key, entry);
            throw;
        }

        return CreateHandle(key, entry, expiry);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null or empty.</exception>
    public async Task<ILockHandle?> TryAcquireAsync(string key, TimeSpan expiry, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var entry = AcquireEntry(key);

        bool acquired;
        try
        {
            acquired = await entry.Semaphore.WaitAsync(TimeSpan.Zero, ct).ConfigureAwait(false);
        }
        catch
        {
            ReleaseEntry(key, entry);
            throw;
        }

        if (!acquired)
        {
            ReleaseEntry(key, entry);
            return null;
        }

        return CreateHandle(key, entry, expiry);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Keys are sorted before acquisition to guarantee a consistent global
    /// acquisition order across all callers, which prevents deadlocks between
    /// concurrent multi key acquisitions. On any failure, all handles acquired
    /// so far are released in parallel before the exception propagates.
    ///
    /// <para>
    /// The acquisition timeout applies per key, so in a worst-case,
    /// highly contested scenario the overall call can take up to
    /// N × <c>defaultAcquisitionTimeout</c>. Pass an external <paramref name="ct"/>
    /// with an overall deadline if that's not acceptable for your use case.
    /// </para>
    /// </remarks>
    public async Task<ILockBatch> AcquireMultiAsync(IEnumerable<string> keys, TimeSpan expiry, CancellationToken ct = default)
    {
        var sortedKeys = keys.Distinct(StringComparer.Ordinal)
                              .OrderBy(k => k, StringComparer.Ordinal)
                              .ToList();

        if (sortedKeys.Count == 0)
            return new InMemoryLockBatch([]);

        var acquired = new List<ILockHandle>(sortedKeys.Count);
        try
        {
            foreach (var key in sortedKeys)
            {
                acquired.Add(await AcquireAsync(key, expiry, ct).ConfigureAwait(false));
            }

            return new InMemoryLockBatch(acquired);
        }
        catch (Exception ex)
        {
            if (acquired.Count > 0)
            {
                var rollback = new Task[acquired.Count];
                for (var i = 0; i < acquired.Count; i++)
                    rollback[i] = acquired[i].DisposeAsync().AsTask();

                await Task.WhenAll(rollback).ConfigureAwait(false);
            }

            var failedKeys = sortedKeys.Skip(acquired.Count).ToList();
            throw new LockBatchFailedException(failedKeys,
                $"Failed to acquire locks for keys: {string.Join(", ", failedKeys)}", ex);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Best-effort / informational only: the result can be stale the instant
    /// after it's returned under concurrent access. Do not use it for
    /// check then act logic use <see cref="TryAcquireAsync"/> instead.
    /// </remarks>
    public Task<bool> IsLockedAsync(string key)
        => Task.FromResult(_locks.TryGetValue(key, out var entry) && entry.Semaphore.CurrentCount == 0);

    /// <summary>
    /// Creates a handle for acquired semaphore. The handle owns the
    /// expiry timer and ultimately drives <see cref="ReleaseEntry"/> via
    /// its <c>onRelease</c> callback.
    /// </summary>
    private InMemoryLockHandle CreateHandle(string key, LockEntry entry, TimeSpan expiry)
        => new(key, entry.Semaphore, expiry, _ => ReleaseEntry(key, entry), logger);

    /// <summary>
    /// Retrieves (or creates) the <see cref="LockEntry"/> for a key and pins it
    /// by incrementing its ref-count, all under the entry's own monitor lock.
    /// This is what makes the pattern race-free: increment and the
    /// "is this entry still the one registered for this key" check happen
    /// atomically with respect to <see cref="ReleaseEntry"/>'s
    /// decrement and maybe remove, because both sides serialize on the exact
    /// same object monitor.
    /// </summary>
    private LockEntry AcquireEntry(string key)
    {
        while (true)
        {
            var entry = _locks.GetOrAdd(key, static _ => new LockEntry());

            lock (entry)
            {
                if (entry.Retired)
                    continue; // Stale reference — another thread just retired it; retry with a fresh lookup.

                entry.RefCount++;
                return entry;
            }
        }
    }

    /// <summary>
    /// Unpins a previously acquired entry. If this was the last reference,
    /// retires and removes it from the dictionary under the same monitor lock
    /// used by <see cref="AcquireEntry"/>, so no concurrent acquirer can ever
    /// observe half-removed entry.
    /// </summary>
    private void ReleaseEntry(string key, LockEntry entry)
    {
        lock (entry)
        {
            entry.RefCount--;
            if (entry.RefCount == 0)
            {
                entry.Retired = true;
                _locks.TryRemove(key, out _);
            }
        }
    }
}
