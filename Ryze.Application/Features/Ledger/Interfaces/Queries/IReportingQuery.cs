using Ryze.Application.Features.Ledger.DTO.Balance;

namespace Ryze.Application.Features.Ledger.Interfaces.Queries;

/// <summary>
/// Defines read only operations for ledger reporting and account summaries.
/// </summary>
/// <remarks>
/// Provides queries for generating reporting views,
/// account balance summaries, and accounting reports derived from
/// ledger data.
/// </remarks>
public interface IReportingQuery
{
    /// <summary>
    /// Retrieves projected balance summaries for all ledger accounts.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<LedgerAccountSummaryDto>> GetAllAccountSummariesAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a summary view for the specified ledger account.
    /// </summary>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<LedgerAccountSummaryDto?> GetAccountSummaryAsync(
        string accountId,
        CancellationToken ct = default);
    
}
