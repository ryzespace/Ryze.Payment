using Marten;
using Marten.Events.Projections;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Events.Transaction;
using Ryze.Domain.Features.Ledger.ReadModels;

namespace Ryze.Infrastructure.Features.Ledger.Projections;

/// <summary>
/// Projects recorded ledger transactions into OLAP-style analytical dimension balances.
/// </summary>
/// <remarks>
/// Processes metadata dimensions attached to journal entries and maintains
/// materialized analytical views for each unique dimension type and value pair.
/// Each dimension slice aggregates transaction amounts, transaction counts,
/// and the timestamp of the most recent update.
/// </remarks>
public sealed class OlapAnalyticsProjection : EventProjection
{
    /// <summary>
    /// Projects recorded ledger transaction into analytical dimension balances.
    /// </summary>
    /// <remarks>
    /// Each journal entry may contribute to multiple analytical dimensions.
    /// Dimension slices are identified by a composite key consisting of the
    /// dimension type and dimension value. Debit entries are accumulated as
    /// revenue and credit entries as expenses according to the current
    /// projection model.
    /// </remarks>
    /// <param name="event">The ledger transaction recorded event containing the journal entries to project.</param>
    /// <param name="ops">The Marten document operations used to load and persist analytical read models.</param>
    /// <returns>A task representing the asynchronous projection operation.</returns>
    public async Task Project(LedgerTransactionRecorded @event, IDocumentOperations ops)
    {
        foreach (var entry in @event.Entries)
        {
            if (entry.Metadata == null || entry.Metadata.Count == 0)
                continue;

            foreach (var kvp in entry.Metadata)
            {
                var dimensionType = kvp.Key;
                var dimensionValue = kvp.Value;

                // Composite ID to prevent key collisions between different tag types
                var docId = $"Tag:{dimensionType}:{dimensionValue}";

                // Load existing slice or initialize
                var analyticsDoc = await ops.LoadAsync<LedgerAnalyticsDimensionBalance>(docId) ?? new LedgerAnalyticsDimensionBalance
                {
                    Id = docId,
                    DimensionType = dimensionType,
                    DimensionValue = dimensionValue
                };

                // Allocate revenue or expense
                if (entry.Posting.Type == EntryType.Debit)
                {
                    analyticsDoc.TotalRevenue += entry.Posting.Amount;
                }
                else
                {
                    analyticsDoc.TotalExpenses += entry.Posting.Amount;
                }

                analyticsDoc.TransactionCount++;
                analyticsDoc.LastUpdatedAt = @event.Timestamp;

                ops.Store(analyticsDoc);
            }
        }
    }
}