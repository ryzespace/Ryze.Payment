namespace Ryze.Domain.Shared.Enum;

/// <summary>
/// Lifecycle status of wallet.
/// </summary>
public enum WalletStatus
{
    /// <summary>
    /// Wallet status is not specified.
    /// </summary>
    Unspecified,

    /// <summary>
    /// Wallet exists but has not completed verification.
    /// </summary>
    Unverified,

    /// <summary>
    /// Wallet is active and can process operations.
    /// </summary>
    Active,

    /// <summary>
    /// Wallet is temporarily suspended.
    /// </summary>
    Suspended,

    /// <summary>
    /// Wallet is permanently closed.
    /// </summary>
    Closed,

    /// <summary>
    /// Wallet is frozen due to risk/compliance constraints.
    /// </summary>
    Frozen,

    /// <summary>
    /// Wallet is scheduled for closure.
    /// </summary>
    PendingClosure
}
