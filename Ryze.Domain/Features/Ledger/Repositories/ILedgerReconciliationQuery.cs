using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Domain.Features.Ledger.DTO
{
    /// <summary>
    /// Represents aggregated totals for a ledger period.
    /// </summary>
    public record PeriodTotals(decimal TotalDebits, decimal TotalCredits, int EntryCount);

    /// <summary>
    /// Represents aggregated totals for a specific currency.
    /// </summary>
    public record CurrencyTotals(Ryze.Domain.Shared.Enum.Currency Currency, decimal TotalDebits, decimal TotalCredits, int EntryCount);
}

namespace Ryze.Domain.Features.Ledger.Repositories
{
    using Ryze.Domain.Features.Ledger.DTO;
    using Ryze.Domain.Features.Ledger.Enum;
    using Ryze.Domain.Shared.Enum;

    /// <summary>
    /// Defines high performance aggregation queries for ledger reconciliation and period closing.
    /// </summary>
    /// <remarks>
    /// These queries are designed to execute directly in the database to avoid
    /// transferring large volumes of journal entries to the application.
    /// </remarks>
    public interface ILedgerReconciliationQuery
    {
        /// <summary>
        /// Gets aggregated debit and credit totals for a specific account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="periodEnd">Optional upper bound for the entry timestamp.</param>
        /// <param name="status">Optional status filter (usually <see cref="EntryStatus.Cleared"/>).</param>
        /// <param name="ct">Cancellation token.</param>
        Task<PeriodTotals> GetPeriodTotalsAsync(
            string accountId,
            DateTimeOffset? periodEnd = null,
            EntryStatus? status = EntryStatus.Cleared,
            CancellationToken ct = default);

        /// <summary>
        /// Gets global aggregated debit and credit totals across all accounts.
        /// </summary>
        /// <param name="periodEnd">Optional upper bound for the entry timestamp.</param>
        /// <param name="status">Optional status filter (usually <see cref="EntryStatus.Cleared"/>).</param>
        /// <param name="ct">Cancellation token.</param>
        Task<PeriodTotals> GetGlobalPeriodTotalsAsync(
            DateTimeOffset? periodEnd = null,
            EntryStatus? status = EntryStatus.Cleared,
            CancellationToken ct = default);

        /// <summary>
        /// Gets global aggregated debit and credit totals grouped by currency.
        /// </summary>
        /// <param name="periodEnd">Optional upper bound for the entry timestamp.</param>
        /// <param name="status">Optional status filter (usually <see cref="EntryStatus.Cleared"/>).</param>
        /// <param name="ct">Cancellation token.</param>
        Task<IReadOnlyList<CurrencyTotals>> GetGlobalTotalsPerCurrencyAsync(
            DateTimeOffset? periodEnd = null,
            EntryStatus? status = EntryStatus.Cleared,
            CancellationToken ct = default);
    }
}
