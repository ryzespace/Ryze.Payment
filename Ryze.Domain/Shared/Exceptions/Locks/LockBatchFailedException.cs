namespace Ryze.Domain.Shared.Exceptions.Locks;

/// <summary>
/// Thrown by batch operations
/// when one or more locks within the batch could not be acquired or renewed,
/// after any partial acquisitions have already been rolled back.
/// </summary>
public sealed class LockBatchFailedException(
    IReadOnlyList<string> failedKeys,
    string message,
    Exception? innerException = null)
    : LockException(message, innerException ?? new InvalidOperationException(message))
{
    /// <summary>Keys that failed within the batch operation.</summary>
    public IReadOnlyList<string> FailedKeys { get; } = failedKeys;
}