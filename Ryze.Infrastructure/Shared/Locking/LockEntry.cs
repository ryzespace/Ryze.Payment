namespace Ryze.Infrastructure.Shared.Locking;

/// <summary>
/// Reference counted entry backing a single logical lock key.
/// Lifetime is managed exclusively through <see cref="RefCount"/> under
/// the entry's own monitor lock, which enables safe removal from
/// the owning <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/>
/// without TOCTOU window between "check if unused" and "remove".
/// </summary>
internal sealed class LockEntry
{
    /// <summary>Semaphore serializing access to the underlying resource.</summary>
    public readonly SemaphoreSlim Semaphore = new(1, 1);

    /// <summary>
    /// Number of callers currently holding or waiting to acquire this key.
    /// Guarded by <c>lock(entry)</c> — never read or written outside it.
    /// </summary>
    public int RefCount;

    /// <summary>
    /// Set (under lock) the moment <see cref="RefCount"/> hits zero and the entry
    /// is removed from the dictionary. Any acquirer that grabbed a reference to
    /// this exact object before removal but observes <c>Retired == true</c> must
    /// discard it and retry via <c>GetOrAdd</c>, since a fresh entry is (or will
    /// be) registered instead.
    /// </summary>
    public bool Retired;
}
