namespace Ryze.Domain.Shared.Exceptions.Locks;

/// <summary>
/// Base type for all exceptions raised by the distributed/in-memory locking
/// subsystem. Catch this to handle any locking failure generically without
/// coupling to a specific provider's failure modes.
/// </summary>
public abstract class LockException : Exception
{
    protected LockException(string message) : base(message) { }

    protected LockException(string message, Exception innerException) : base(message, innerException) { }
}