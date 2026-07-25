using Marten;
using Ryze.Application.Features.Ledger.Interfaces.Queries;
using Ryze.Domain.Features.Ledger.ReadModels;

namespace Ryze.Infrastructure.Features.Ledger.Services;

/// <summary>
/// Analytics query operations for ledger dimensions.
/// </summary>
/// <remarks>
/// Use Marten <see cref="IQuerySession"/> to retrieve pre-calculated analytical
/// balance projections without loading or aggregating the underlying ledger entries.
/// </remarks>
/// <param name="querySession">The Marten query session used to read analytics projections.</param>
public sealed class LedgerAnalyticsQueryService(IQuerySession querySession)
    : ILedgerAnalyticsQuery
{
    /// <summary>
    /// Returns the calculated balance for the specified analytics dimension.
    /// </summary>
    /// <param name="dimensionType">The type of the analytics dimension.</param>
    /// <param name="dimensionValue">The value identifying the analytics dimension.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns> The dimension balance projection, or null when no matching projection exists. </returns>
    public async Task<LedgerAnalyticsDimensionBalance?> GetDimensionBalanceAsync(
        string dimensionType,
        string dimensionValue,
        CancellationToken ct = default)
    {
        var docId = $"Tag:{dimensionType}:{dimensionValue}";

        return await querySession.LoadAsync<LedgerAnalyticsDimensionBalance>(
            docId,
            ct);
    }

    /// <summary>
    /// Returns all calculated balance projections for the specified dimension type.
    /// </summary>
    /// <param name="dimensionType">The type of the analytics dimension.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>Collection containing all analytics balance projections matching the dimension type. </returns>
    public async Task<IReadOnlyList<LedgerAnalyticsDimensionBalance>> GetAllByDimensionTypeAsync(
        string dimensionType,
        CancellationToken ct = default)
    {
        var results = await querySession
            .Query<LedgerAnalyticsDimensionBalance>()
            .Where(x => x.DimensionType == dimensionType)
            .ToListAsync(ct);

        return results;
    }
}