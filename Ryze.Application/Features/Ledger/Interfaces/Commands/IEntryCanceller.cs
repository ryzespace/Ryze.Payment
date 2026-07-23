using Ryze.Application.Features.Ledger.DTO.Entry;

namespace Ryze.Application.Features.Ledger.Interfaces.Commands;

/// <summary>
/// Defines operations for cancelling pending ledger entries.
/// </summary>
public interface IEntryCanceller
{
    /// <summary>
    /// Cancels all pending entries for the specified account.
    /// </summary>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of the cancellation operation.</returns>
    Task<CommitResultDto> CancelAllPendingAsync(
        string accountId,
        CancellationToken ct = default);

    /// <summary>
    /// Cancels the specified pending ledger entries.
    /// </summary>
    /// <param name="entryIds">Identifiers of the entries to cancel.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of the cancellation operation.</returns>
    Task<CommitResultDto> CancelEntriesAsync(
        IReadOnlyList<Guid> entryIds,
        CancellationToken ct = default);
}
