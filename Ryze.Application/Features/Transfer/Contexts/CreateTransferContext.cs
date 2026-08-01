using Ryze.Domain.Features.Transfer.Enum;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Transfer.Contexts;

/// <summary>
/// Represents execution context for creating new transfer.
/// </summary>
/// <remarks>
/// Contains all data required by the transfer creation workflow, including
/// source and destination wallets, transfer amount, currency, transfer type,
/// and optional external metadata.
///
/// The context is consumed by the mutation pipeline and provides stable
/// application boundary between incoming transfer requests and domain
/// execution.
///
/// Instances are immutable after initialization to ensure that the transfer
/// creation parameters cannot change during processing.
/// </remarks>
public sealed class CreateTransferContext : ITransferContext
{
    /// <summary>
    /// Gets the unique identifier of the operation context.
    /// </summary>
    /// <remarks>
    /// Generated automatically and used for correlation, tracing, and audit
    /// purposes throughout the transfer lifecycle.
    /// </remarks>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the timestamp when this context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the wallet identifier from which funds are transferred.
    /// </summary>
    public required Guid SourceWalletId { get; init; }

    /// <summary>
    /// Gets the wallet identifier receiving transferred funds.
    /// </summary>
    public required Guid DestinationWalletId { get; init; }

    /// <summary>
    /// Gets the amount to transfer.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Gets the currency used for the transfer.
    /// </summary>
    public required Currency Currency { get; init; }

    /// <summary>
    /// Gets the transfer execution type.
    /// </summary>
    public required TransferType TransferType { get; init; }

    /// <summary>
    /// Gets an optional business reference assigned to the transfer.
    /// </summary>
    public string? Reference { get; init; }

    /// <summary>
    /// Gets an optional transfer description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the idempotency key associated with this transfer request.
    /// </summary>
    /// <remarks>
    /// Used to safely retry transfer creation without creating duplicate
    /// operations.
    /// </remarks>
    public string? IdempotencyKey { get; init; }

    /// <summary>
    /// Gets additional metadata associated with the transfer.
    /// </summary>
    /// <remarks>
    /// Metadata is intended for non domain information required by external
    /// integrations, auditing, or operational workflows.
    /// </remarks>
    public Dictionary<string, string> Metadata { get; init; } = new();

    /// <summary>
    /// Gets tags assigned to the transfer.
    /// </summary>
    /// <remarks>
    /// Tags can be used for categorization, filtering, and operational
    /// analysis.
    /// </remarks>
    public List<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets an optional external system reference.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Gets value indicating whether the transfer may exceed the available
    /// wallet balance.
    /// </summary>
    /// <remarks>
    /// Overdraft support should only be enabled for workflows explicitly
    /// allowing negative balances or credit backed transfers.
    /// </remarks>
    public bool AllowOverdraft { get; init; }

    /// <summary>
    /// Returns readable representation of the transfer creation context.
    /// </summary>
    public override string ToString()
        => $"CreateTransferContext {{ Id = {Id}, SourceWalletId = {SourceWalletId}, DestinationWalletId = {DestinationWalletId}, Amount = {Amount} {Currency}, Type = {TransferType} }}";
}