using Ryze.Domain.Shared.Exceptions.Locks;

namespace Ryze.Application.Shared.Locking;

/// <summary>
/// Advanced distributed locking provider.
/// Implementations provide TTL-based lock acquisition with optional
/// auto-renewal, deadlock-safe multi-key acquisition, and non-blocking
/// try-to acquire semantics. Designed to abstract over in-memory.
/// </summary>
/// <remarks>
/// All acquire methods accept an expiry parameter that
/// defines the lock TTL. Once the TTL elapses without a renewal, the lock
/// is force-released. Callers with long critical sections must either call
/// <see cref="ILockHandle.StartAutoRenewal"/> or periodically check
/// <see cref="ILockHandle.IsExpired"/> to avoid losing ownership.
/// </remarks>
public interface ILockProvider
{
    /// <summary>
    /// Acquires a lock with the specified key, blocking until it becomes
    /// available or the provider's default acquisition timeout elapses.
    /// </summary>
    /// <param name="key">Resource key to lock.</param>
    /// <param name="expiry">Initial TTL for the lock.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A handle representing the acquired lock.</returns>
    /// <exception cref="LockAcquisitionFailedException">
    /// The lock could not be acquired within the configured timeout.
    /// </exception>
    Task<ILockHandle> AcquireAsync(
        string key,
        TimeSpan expiry,
        CancellationToken ct = default);

    /// <summary>
    /// Attempts to acquire a lock without blocking. Returns <c>null</c>
    /// immediately if the lock is held by another caller.
    /// </summary>
    /// <param name="key">Resource key to lock.</param>
    /// <param name="expiry">Initial TTL for the lock.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>
    /// A handle if the lock was acquired; <c>null</c> if already held.
    /// </returns>
    Task<ILockHandle?> TryAcquireAsync(
        string key,
        TimeSpan expiry,
        CancellationToken ct = default);

    /// <summary>
    /// Safely acquires multiple locks atomically (or as close to atomically
    /// as the backend allows). Prevents deadlocks by sorting keys
    /// consistently before acquisition.
    /// </summary>
    /// <param name="keys">Resource keys to lock.</param>
    /// <param name="expiry">Initial TTL applied to every lock in the batch.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A batch handle that can dispose all locks as a unit.</returns>
    /// <exception cref="LockBatchFailedException">
    /// One or more locks could not be acquired. Any partially acquired
    /// handles are rolled back before the exception propagates.
    /// </exception>
    Task<ILockBatch> AcquireMultiAsync(
        IEnumerable<string> keys,
        TimeSpan expiry,
        CancellationToken ct = default);

    /// <summary>
    /// Checks whether a key is currently locked. Best-effort only - the
    /// result may be stale the instant it is returned under concurrent access.
    /// Do not use it for check then act logic.
    /// </summary>
    Task<bool> IsLockedAsync(string key);
}
