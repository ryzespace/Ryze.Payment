namespace Ryze.Application.Features.Transfer.Contexts;

/// <summary>
/// Represents execution context for transfer operation mutations.
/// </summary>
/// <remarks>
/// Contains normalized transfer data required during transfer processing,
/// including source and destination wallets, financial values, currency,
/// and initiating actor.
///
/// This context is passed through the mutation pipeline and is used by
/// transfer policies, validators, and handlers. It does not contain business
/// behavior and should only represent the operation input state.
/// </remarks>
public sealed class TransferOperationContext : ITransferContext
{
    /// <summary>
    /// Unique identifier of this mutation context instance.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Timestamp when the context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Source wallet from which funds are transferred.
    /// </summary>
    public required Guid FromWalletId { get; init; }

    /// <summary>
    /// Destination wallet receiving transferred funds.
    /// </summary>
    public required Guid ToWalletId { get; init; }

    /// <summary>
    /// Gross transfer amount before fees.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Fee charged for executing the transfer.
    /// </summary>
    public required decimal Fee { get; init; }

    /// <summary>
    /// Final amount transferred after applying fees.
    /// </summary>
    public required decimal NetAmount { get; init; }

    /// <summary>
    /// Currency used for the transfer operation.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Optional human-readable transfer description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Identifier of the actor initiating the transfer.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <inheritdoc />
    public override string ToString()
        => $"TransferOperationContext {{ Id = {Id}, FromWalletId = {FromWalletId}, ToWalletId = {ToWalletId}, Amount = {Amount}, Fee = {Fee}, NetAmount = {NetAmount}, Currency = {Currency} }}";
}