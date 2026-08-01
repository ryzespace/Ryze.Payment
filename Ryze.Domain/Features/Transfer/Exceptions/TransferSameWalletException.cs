namespace Ryze.Domain.Features.Transfer.Exceptions;

/// <summary>
/// Thrown when transfer attempts to use the same wallet as both source and destination.
/// </summary>
/// <remarks>
/// Transfer between the same wallet has no valid financial meaning and would
/// create an invalid ledger operation. This exception protects domain invariants
/// before any balance validation, fee calculation, or transaction execution occurs.
/// </remarks>
/// <param name="walletId">Identifier of the wallet used as both transfer source and destination. </param>
public sealed class TransferSameWalletException(Guid walletId)
    : TransferException(
        $"Cannot transfer to the same wallet {walletId}.",
        "SAME_WALLET");