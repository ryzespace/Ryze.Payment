namespace Ryze.Domain.Shared.Exceptions.Locks;

/// <summary>
/// Thrown when a caller attempts to operate on a lock handle
/// after it has already expired or been disposed.
/// </summary>
/// <remarks>
/// Note: <c>RenewAsync</c> returning <c>false</c> is the expected, non-exceptional
/// signal for "renewal failed because the lock is gone" — this exception is for
/// call sites where silent failure would be dangerous (e.g., a caller assuming
/// exclusive ownership and proceeding to mutate shared state without checking
/// the boolean result first).
/// </remarks>
public sealed class LockExpiredException(string lockKey, DateTimeOffset expiresAt)
    : LockException($"Lock '{lockKey}' expired at {expiresAt:o} and is no longer held.")
{
    public DateTimeOffset ExpiresAt { get; } = expiresAt;
}