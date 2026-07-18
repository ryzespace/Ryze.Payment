namespace Ryze.Domain.Shared.Exceptions.Locks;

/// <summary>
/// Thrown when a lock could not be acquired within the configured
/// acquisition timeout (or the caller-supplied <see cref="System.Threading.CancellationToken"/>
/// fired before the lock became available). Distinct from cancellation
/// requested by the caller — see <see cref="OperationCanceledException"/>
/// for that case, which providers should let propagate as-is rather than
/// wrapping.
/// </summary>
public sealed class LockAcquisitionFailedException(string lockKey, string message, TimeSpan? timeout = null)
    : LockException(message)
{
    public TimeSpan? Timeout { get; } = timeout;
}