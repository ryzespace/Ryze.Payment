using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Ledger.Contexts.Fee;

/// <summary>
/// Write context for ledger fee operations.
/// </summary>
/// <remarks>
/// Represents immutable command intent describing a fee charged against
/// a ledger account.
///
/// The context specifies the source account, fee amount, currency,
/// fee classification, and audit metadata required to execute the
/// operation through the application layer.
/// </remarks>
public sealed class LedgerFeeContext : IContext
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Identifier of the actor initiating the fee operation.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <summary>
    /// Optional business reason explaining why the fee is being applied.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Arbitrary metadata associated with the fee request.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = [];

    /// <summary>
    /// Identifier of the ledger account from which the fee is charged.
    /// </summary>
    public required string FromAccountId { get; init; }

    /// <summary>
    /// Business classification of the fee.
    /// </summary>
    /// <remarks>
    /// Examples include transaction fees, monthly maintenance fees,
    /// foreign exchange fees, or processing fees.
    /// </remarks>
    public required string FeeType { get; init; }

    /// <summary>
    /// Fee amount to be charged.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency in which the fee is charged.
    /// </summary>
    public required string Currency { get; init; }
    
    public string Description { get; init; } = "Platform Fee";
}