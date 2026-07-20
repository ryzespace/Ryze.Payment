using Ryze.Domain.Shared.Exceptions.Locks;

namespace Ryze.Application.Shared.Locking;

/// <summary>
/// A collection of lock handles acquired as a batch. Disposing the batch
/// releases all contained locks, typically in parallel.
/// </summary>
/// <remarks>
/// Acquired via <see cref="ILockProvider.AcquireMultiAsync"/>, which
/// guarantees deadlock-safe acquisition order and rollback on partial
/// failure. Renewal is <b>not</b> atomic if one handle's renewal fails,
/// others already renewed remain so, and the caller must decide how to
/// recover.
/// </remarks>
public interface ILockBatch : IAsyncDisposable
{
    /// <summary>Locks held by this batch, in acquisition order.</summary>
    IReadOnlyList<ILockHandle> Handles { get; }

    /// <summary>
    /// Renews all locks in the batch in parallel.
    /// </summary>
    /// <param name="extraTime">Additional TTL to add to every lock.</param>
    /// <returns><c>true</c> if all locks were renewed successfully.</returns>
    /// <exception cref="LockBatchFailedException">
    /// One or more locks failed to renew. Already-renewed handles are
    /// <b>not</b> rolled back — they remain held. The exception carries
    /// the keys of the failed handles so the caller can inspect or release
    /// them individually.
    /// </exception>
    Task<bool> RenewAllAsync(TimeSpan extraTime);

    /// <summary>
    /// Starts background auto-renewal for every handle in the batch.
    /// See <see cref="ILockHandle.StartAutoRenewal"/> for details.
    /// </summary>
    void StartAutoRenewal(TimeSpan interval, TimeSpan extensionTime);
}
