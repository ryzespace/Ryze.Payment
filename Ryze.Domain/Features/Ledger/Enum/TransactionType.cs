namespace Ryze.Domain.Features.Ledger.Enum;

/// <summary>
/// Defines the type of financial transaction recorded in the ledger.
/// </summary>
/// <remarks>
/// Represents business level transaction categories used to classify
/// ledger movements.
///
/// Transaction types provide semantic meaning above individual journal entries
/// and are used for reporting, auditing, reconciliation, and transaction
/// lifecycle management.
///
/// Each transaction type may produce one or more debit and credit entries
/// while preserving double-entry accounting consistency.
/// </remarks>
public enum TransactionType
{
    /// <summary>
    /// Represents adding funds to wallet balance.
    /// </summary>
    /// <remarks>
    /// Typically created when user deposits funds into their wallet
    /// through an external payment source.
    /// </remarks>
    WalletTopUp,

    /// <summary>
    /// Represents withdrawing funds from a wallet.
    /// </summary>
    /// <remarks>
    /// Used when funds leave the user's wallet and are transferred
    /// to an external destination.
    /// </remarks>
    WalletWithdrawal,

    /// <summary>
    /// Represents payment transaction.
    /// </summary>
    /// <remarks>
    /// Used for business payments such as compute usage,
    /// service consumption, or platform-provided resources.
    /// </remarks>
    Payment,

    /// <summary>
    /// Represents reversal of previous payment or transaction.
    /// </summary>
    /// <remarks>
    /// Refunds preserve audit history by creating compensating ledger entries
    /// instead of modifying the original transaction.
    /// </remarks>
    Refund,

    /// <summary>
    /// Represents a platform fee transaction.
    /// </summary>
    /// <remarks>
    /// Used to record commissions, processing fees,
    /// and other revenue-generating charges.
    /// </remarks>
    Fee,

    /// <summary>
    /// Represents a transaction that moves funds into escrow.
    /// </summary>
    /// <remarks>
    /// Used to reserve funds while keeping them unavailable
    /// for normal spending until a later decision.
    /// </remarks>
    Escrow,

    /// <summary>
    /// Represents releasing funds previously held in escrow.
    /// </summary>
    /// <remarks>
    /// Used when reserved funds are returned to an account
    /// or transferred to another destination.
    /// </remarks>
    EscrowRelease,

    /// <summary>
    /// Represents a manual or automated ledger correction.
    /// </summary>
    /// <remarks>
    /// Used to fix accounting discrepancies while maintaining
    /// a complete audit trail.
    /// </remarks>
    Correction,

    /// <summary>
    /// Represents creation of a wallet ledger account.
    /// </summary>
    /// <remarks>
    /// Used during wallet initialization workflows
    /// where an initial ledger state may be established.
    /// </remarks>
    WalletCreation,

    /// <summary>
    /// Represents an internal transfer between ledger accounts.
    /// </summary>
    /// <remarks>
    /// Transfers move value between accounts without changing
    /// the total ledger value.
    /// </remarks>
    Transfer,

    /// <summary>
    /// Represents an incoming deposit operation.
    /// </summary>
    /// <remarks>
    /// Used for recording funds received from external systems,
    /// gateways, or payment providers.
    /// </remarks>
    Deposit,

    /// <summary>
    /// Represents an outgoing withdrawal operation.
    /// </summary>
    /// <remarks>
    /// Used when funds are sent from the ledger to an external account,
    /// payout destination, or financial provider.
    /// </remarks>
    Withdrawal
}