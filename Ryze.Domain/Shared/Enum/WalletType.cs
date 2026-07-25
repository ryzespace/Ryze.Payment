namespace Ryze.Domain.Shared.Enum;

/// <summary>
/// Classification of wallet product type.
/// </summary>
public enum WalletType
{
    /// <summary>
    /// Wallet type is not specified.
    /// </summary>
    WALLET_TYPE_UNSPECIFIED = 0,

    /// <summary>
    /// Personal wallet owned by an individual.
    /// </summary>
    WALLET_TYPE_PERSONAL = 1,

    /// <summary>
    /// Business wallet owned by an organization.
    /// </summary>
    WALLET_TYPE_BUSINESS = 2,

    /// <summary>
    /// Savings-oriented wallet.
    /// </summary>
    WALLET_TYPE_SAVINGS = 3,

    /// <summary>
    /// Investment-oriented wallet.
    /// </summary>
    WALLET_TYPE_INVESTMENT = 4,

    /// <summary>
    /// Escrow wallet for conditional fund release.
    /// </summary>
    WALLET_TYPE_ESCROW = 5,

    /// <summary>
    /// Wallet supporting multiple currencies.
    /// </summary>
    WALLET_TYPE_MULTI_CURRENCY = 6
}
