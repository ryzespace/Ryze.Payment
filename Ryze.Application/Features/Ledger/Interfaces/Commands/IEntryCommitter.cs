using Ryze.Application.Features.Ledger.DTO.Entry;

namespace Ryze.Application.Features.Ledger.Interfaces.Commands;

/// <summary>
/// Defines operations for committing pending ledger entries.
/// </summary>
public interface IEntryCommitter
{
    /// <summary>
    /// Commits all pending entries for the specified account.
    /// </summary>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of the commit operation.</returns>
    Task<CommitResultDto> CommitAllPendingAsync(
        string accountId,
        CancellationToken ct = default);

    /// <summary>
    /// Commits the specified pending ledger entries.
    /// </summary>
    /// <param name="entryIds">Identifiers of the entries to commit.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of the commit operation.</returns>
    Task<CommitResultDto> CommitEntriesAsync(
        IReadOnlyList<Guid> entryIds,
        CancellationToken ct = default);
}
