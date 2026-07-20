using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Domain.Features.Ledger.Entity.ChartsAccounts;

/// <summary>
/// Provides predefined stable ledger accounts used by the system wide Chart of Accounts.
/// </summary>
/// <remarks>
/// These accounts represent global financial control accounts required by the ledger
/// to maintain double entry consistency across platform operations.
///
/// Chart of Accounts entries are immutable references once used by journal entries.
/// Changing identifiers may break historical accounting records, reconciliation,
/// and audit traceability.
/// </remarks>
public static partial class ChartOfAccounts
{
    /// <summary>
    /// Platform treasury account representing actual funds held by the platform.
    /// </summary>
    /// <remarks>
    /// Represents the primary asset account used to track external bank balances,
    /// payment processor balances, or other custody locations.
    /// </remarks>
    public static readonly LedgerAccount PlatformTreasury = new()
    {
        Id = "asset:platform:treasury",
        Name = "Platform Treasury (Main Bank Account)",
        AccountType = "platform_asset",
        Category = AccountCategory.Asset
    };

    /// <summary>
    /// Aggregate liability account representing the platform obligation
    /// towards users.
    /// </summary>
    /// <remarks>
    /// Tracks the total amount owed by the platform to wallet holders.
    /// This account provides the liability counterpart for user balances.
    /// </remarks>
    public static readonly LedgerAccount PlatformLiabilities = new()
    {
        Id = "liability:platform:aggregates",
        Name = "Platform Liabilities (User Fund Backing)",
        AccountType = "platform_liability",
        Category = AccountCategory.Liability
    };

    /// <summary>
    /// Escrow holding account for temporarily reserved user funds.
    /// </summary>
    /// <remarks>
    /// Used during settlement workflows, payment authorization,
    /// disputes, and delayed releases.
    ///
    /// Funds recorded here remain controlled by the platform
    /// until the corresponding business process is completed.
    /// </remarks>
    public static readonly LedgerAccount EscrowFunds = new()
    {
        Id = "liability:platform:escrow",
        Name = "Escrow Funds (Held during settlement)",
        AccountType = "platform_liability",
        Category = AccountCategory.Liability
    };

    /// <summary>
    /// Technical balancing account used for foreign exchange adjustments.
    /// </summary>
    /// <remarks>
    /// Handles differences created during currency conversion operations
    /// where source and destination currencies have different values.
    /// </remarks>
    public static readonly LedgerAccount CurrencyExchangeGainLoss = new()
    {
        Id = "equity:currency:exchange",
        Name = "Currency Exchange Gain/Loss",
        AccountType = "fx_control",
        Category = AccountCategory.Equity
    };

    /// <summary>
    /// Temporary holding account for unresolved ledger postings.
    /// </summary>
    /// <remarks>
    /// Used when the system cannot immediately determine the correct target
    /// account or requires manual reconciliation.
    /// </remarks>
    public static readonly LedgerAccount SuspenseAccount = new()
    {
        Id = "system:suspense",
        Name = "Suspense Account (Unidentified Postings)",
        AccountType = "system_control",
        Category = AccountCategory.Liability
    };
}