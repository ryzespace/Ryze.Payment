namespace Ryze.Domain.Features.Ledger.ReadModels;

/// <summary>
/// Represents pre aggregated analytics projection of ledger activity grouped
/// by a business dimension.
/// </summary>
/// <remarks>
/// This read model provides optimized access to financial metrics aggregated
/// by custom dimensions such as campaigns, cost centers, products,
/// or other reporting categories.
///
/// It is derived from ledger transactions and maintained separately from
/// the transactional write model. Updates should be performed through
/// projection handlers or domain event processing.
///
/// The model is intended for reporting, analytics, and dashboard scenarios
/// where calculating aggregations directly from journal entries would be
/// inefficient.
/// </remarks>
public sealed class LedgerAnalyticsDimensionBalance
{
    /// <summary>
    /// Unique identifier of the analytics dimension projection.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Type of business dimension used for aggregation.
    /// </summary>
    public required string DimensionType { get; set; }

    /// <summary>
    /// Specific value within the dimension category.
    /// </summary>
    public required string DimensionValue { get; set; }

    /// <summary>
    /// Total recognized revenue associated with this dimension.
    /// </summary>
    /// <remarks>
    /// Calculated from credited revenue accounts.
    /// </remarks>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Total expenses associated with this dimension.
    /// </summary>
    /// <remarks>
    /// Calculated from debited expense accounts.
    /// </remarks>
    public decimal TotalExpenses { get; set; }

    /// <summary>
    /// Number of ledger transactions contributing to this aggregation.
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Timestamp of the last projection update.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}