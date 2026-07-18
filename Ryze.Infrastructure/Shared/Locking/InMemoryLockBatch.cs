using Ryze.Application.Shared.Locking;
using Ryze.Domain.Shared.Exceptions.Locks;

namespace Ryze.Infrastructure.Shared.Locking;

/// <summary>
/// A collection of memory-based lock handles treated as a single unit.
/// Disposal is parallelised for minimum contention.
/// </summary>
public sealed class InMemoryLockBatch(IReadOnlyList<ILockHandle> handles) : ILockBatch
{
    /// <inheritdoc/>
    public IReadOnlyList<ILockHandle> Handles { get; } = handles;

    /// <inheritdoc/>
    /// <remarks>
    /// Renews all handles in parallel. Not atomic: if one handle's renewal
    /// fails (already expired/disposed), others already renewed remain so —
    /// this throws <see cref="LockBatchFailedException"/> with the keys of
    /// the handles that failed, but does NOT roll back already-renewed handles.
    /// Callers should treat a thrown exception as "not all locks are guaranteed
    /// held" and either retry or release everything.
    /// </remarks>
    public async Task<bool> RenewAllAsync(TimeSpan extraTime)
    {
        var count = Handles.Count;
        if (count == 0) return true;
        if (count == 1)
        {
            var ok = await Handles[0].RenewAsync(extraTime).ConfigureAwait(false);
            if (!ok)
                throw new LockBatchFailedException([Handles[0].Key],
                    $"Renewal failed for lock '{Handles[0].Key}'.");
            return true;
        }

        var tasks = new Task<bool>[count];
        for (var i = 0; i < count; i++)
            tasks[i] = Handles[i].RenewAsync(extraTime);

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        var failedKeys = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            if (!results[i])
                failedKeys.Add(Handles[i].Key);
        }

        if (failedKeys.Count > 0)
            throw new LockBatchFailedException(failedKeys,
                $"Renewal failed for {failedKeys.Count} of {count} locks: {string.Join(", ", failedKeys)}.");

        return true;
    }

    /// <inheritdoc/>
    public void StartAutoRenewal(TimeSpan interval, TimeSpan extensionTime)
    {
        for (var i = 0; i < Handles.Count; i++)
            Handles[i].StartAutoRenewal(interval, extensionTime);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Disposes all handles in parallel. This reduces total wall-clock time
    /// compared to sequential disposal, at the cost of slightly more
    /// scheduler overhead — a worthwhile trade-off for batches that may
    /// contain dozens of locks.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        var count = Handles.Count;
        if (count == 0) return;
        if (count == 1)
        {
            await Handles[0].DisposeAsync().ConfigureAwait(false);
            return;
        }

        var tasks = new Task[count];
        for (var i = 0; i < count; i++)
            tasks[i] = Handles[i].DisposeAsync().AsTask();

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
