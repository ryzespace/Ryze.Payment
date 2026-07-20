using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Ledger.Contexts.Adjustment;

/// <summary>
/// Write context for ledger adjustment operations.
/// </summary>
/// <remarks>
/// Represents immutable command intent describing a manual or system-driven
/// balance adjustment applied to a ledger account.
///
/// The context contains the adjustment amount, target account, currency,
/// adjustment classification, and audit information required to execute the
/// operation through the application layer.
/// </remarks>
public sealed class LedgerAdjustmentContext : IContext
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Identifier of the actor initiating the adjustment.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <summary>
    /// Optional business reason explaining why the adjustment is performed.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Arbitrary metadata associated with the adjustment request.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = [];

    /// <summary>
    /// Identifier of the ledger account being adjusted.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Adjustment amount applied to the account.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency in which the adjustment is performed.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Business classification of the adjustment.
    /// </summary>
    /// <remarks>
    /// Examples include manual correction, reconciliation, write-off,
    /// or operational adjustment.
    /// </remarks>
    public required string AdjustmentType { get; init; }

    /// <summary>
    /// Indicates whether the adjustment affects encumbered funds rather than
    /// the available balance.
    /// </summary>
    public bool IsEncumbrance { get; init; }
}