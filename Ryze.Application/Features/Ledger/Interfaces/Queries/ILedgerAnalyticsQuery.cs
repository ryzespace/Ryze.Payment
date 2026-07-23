using Ryze.Domain.Features.Ledger.ReadModels;

namespace Ryze.Application.Features.Ledger.Interfaces.Queries;

/// <summary>
/// Defines read only operations for retrieving ledger analytics data.
/// </summary>
/// <remarks>
/// Provides queries for accessing pre computed
/// analytical dimensions and aggregated ledger data optimized for
/// reporting, analytics, and OLAP style workloads.
/// </remarks>
public interface ILedgerAnalyticsQuery
{
    /// <summary>
    /// Retrieves an aggregated ledger analytics balance for dimension value.
    /// </summary>
    /// <param name="dimensionType">Analytics dimension type.</param>
    /// <param name="dimensionValue">Analytics dimension value.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<LedgerAnalyticsDimensionBalance?> GetDimensionBalanceAsync(
        string dimensionType,
        string dimensionValue,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all analytics balances for specific dimension type.
    /// </summary>
    /// <param name="dimensionType">Analytics dimension type.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<LedgerAnalyticsDimensionBalance>> GetAllByDimensionTypeAsync(
        string dimensionType,
        CancellationToken ct = default);
}
