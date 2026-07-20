using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Domain.Features.Ledger.Entity.ChartsAccounts;

public static partial class ChartOfAccounts
{
    /// <summary>
    /// Creates dedicated ledger account for user wallet.
    /// </summary>
    /// <remarks>
    /// User wallet accounts represent the platform financial liability
    /// towards users.
    ///
    /// Each wallet account is isolated by wallet identifier and currency,
    /// allowing independent balance tracking for multi-currency wallets.
    /// </remarks>
    /// <param name="walletId">
    /// Unique identifier of the wallet owner account.
    /// </param>
    /// <param name="currency">
    /// ISO currency code assigned to this ledger account.
    /// Example: PLN, EUR, USD.
    /// </param>
    /// <returns>
    /// A ledger account representing the user's wallet liability.
    /// </returns>
    public static LedgerAccount UserWallet(Guid walletId, string currency) => new()
    {
        Id = $"wallet:{walletId:N}:{currency}",
        Name = $"User Wallet ({walletId}, {currency})",
        AccountType = "user_wallet",
        Category = AccountCategory.Liability,
        Currency = currency
    };
}