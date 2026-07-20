using Ryze.Domain.Shared.Exceptions.Locks;

namespace Ryze.Application.Shared.Locking;

/// <summary>
/// Handle for an acquired distributed lock. Exposes metadata about the
/// lock (key, acquisition time, expiry) and methods to release, renew,
/// or start background auto-renewal.
/// </summary>
/// <remarks>
/// Lock handles are <see cref="IAsyncDisposable"/> — always dispose (or
/// call <see cref="ReleaseAsync"/>) to release the underlying resource.
/// If the TTL elapses without renewal, the lock is force-released even
/// without an explicit disposal call.
/// </remarks>
public interface ILockHandle : IAsyncDisposable
{
    /// <summary>Resource key this lock protects.</summary>
    string Key { get; }

    /// <summary>Moment the lock was acquired (UTC).</summary>
    DateTimeOffset AcquiredAt { get; }

    /// <summary>
    /// Moment the lock will expire (UTC) if not renewed. Updated by
    /// <see cref="RenewAsync"/> and <see cref="StartAutoRenewal"/>.
    /// </summary>
    DateTimeOffset ExpiresAt { get; }

    /// <summary>
    /// Returns <c>true</c> when the lock TTL has elapsed and the lock
    /// may have been force-released by the provider. Check this before
    /// entering a long critical section if you are not using auto-renewal.
    /// </summary>
    bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    /// <summary>
    /// Explicitly releases the lock. Idempotent safe to call multiple
    /// times or after the lock has already expired.
    /// </summary>
    ValueTask ReleaseAsync();

    /// <summary>
    /// Extends the lock TTL by <paramref name="extraTime"/>. Both the
    /// reported <see cref="ExpiresAt"/> and the provider's internal expiry
    /// clock are updated.
    /// </summary>
    /// <param name="extraTime">Additional duration to extend the lock.</param>
    /// <returns>
    /// <c>true</c> if the renewal succeeded; <c>false</c> if the lock has
    /// already been released (by expiry or explicit dispose).
    /// </returns>
    /// <exception cref="LockExpiredException">
    /// The lock was force-released by the expiry timer before this call.
    /// Unlike a deliberate disposal, timer expiry means the TTL was
    /// inadequate - this is thrown so the caller cannot silently proceed
    /// without holding the lock.
    /// </exception>
    Task<bool> RenewAsync(TimeSpan extraTime);

    /// <summary>
    /// Starts a background loop that calls <see cref="RenewAsync"/> every
    /// <paramref name="interval"/> until the handle is disposed.
    /// </summary>
    /// <param name="interval">
    /// How often to renew. A typical choice is one-third to one-quarter
    /// of the original expiry duration.
    /// </param>
    /// <param name="extensionTime">
    /// How much TTL to add on each renewal cycle.
    /// </param>
    /// <remarks>
    /// Safe to call multiple times - only the first call takes effect.
    /// Autorenewal stops automatically when the handle is disposed or
    /// when a renewal attempt fails (e.g., the lock expired).
    /// </remarks>
    void StartAutoRenewal(TimeSpan interval, TimeSpan extensionTime);
}
