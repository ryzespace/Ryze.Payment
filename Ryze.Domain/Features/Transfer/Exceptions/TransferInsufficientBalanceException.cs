namespace Ryze.Domain.Features.Transfer.Exceptions;

/// <summary>
/// Exception thrown when transfer cannot be completed because the source wallet
/// does not contain sufficient funds.
/// </summary>
/// <remarks>
/// This exception represents domain validation failure during transfer execution.
/// It is raised before any state mutation is committed to prevent creating
/// transfers that exceed the available wallet balance.
/// </remarks>
public sealed class TransferInsufficientBalanceException(
    Guid walletId,
    decimal requested,
    decimal available)
    : TransferException(
        $"Wallet {walletId} has insufficient balance. Requested: {requested}, Available: {available}.",
        "INSUFFICIENT_BALANCE")
{
    /// <summary>
    /// Identifier of the wallet that did not have enough funds.
    /// </summary>
    public Guid WalletId { get; } = walletId;

    /// <summary>
    /// Amount requested by the transfer operation.
    /// </summary>
    public decimal Requested { get; } = requested;

    /// <summary>
    /// Available balance at the time of validation.
    /// </summary>
    public decimal Available { get; } = available;
}