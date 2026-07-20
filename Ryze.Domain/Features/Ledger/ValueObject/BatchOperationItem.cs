namespace Ryze.Domain.Features.Ledger.ValueObject;

/// <summary>
/// Represents single item within batch ledger operation.
/// </summary>
/// <remarks>
/// A batch operation item describes one wallet financial action
/// that will be processed together with other items as part of grouped
/// ledger operation.
/// </remarks>
public sealed record BatchOperationItem
{
    /// <summary>
    /// Identifier of the wallet affected by this operation item.
    /// </summary>
    public required Guid WalletId { get; init; }

    /// <summary>
    /// Monetary amount associated with this operation item.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency code of the operation item.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Direction of the ledger movement.
    /// </summary>
    public required string Direction { get; init; }
}
