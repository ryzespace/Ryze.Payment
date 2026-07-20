using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Domain.Features.Ledger.Entity.ChartsAccounts;

public static partial class ChartOfAccounts
{
    /// <summary>
    /// Revenue account used to record income generated from platform transaction fees.
    /// </summary>
    /// <remarks>
    /// Represents revenue collected from users or external parties for provided
    /// platform services, including payment processing fees, service charges,
    /// and operational commissions.
    ///
    /// Revenue accounts increase through credit postings and are used for
    /// financial reporting and profitability analysis.
    /// </remarks>
    public static readonly LedgerAccount RevenueTransactionFees = new()
    {
        Id = "revenue:platform:fees",
        Name = "Revenue - Transaction Fees",
        AccountType = "platform_revenue",
        Category = AccountCategory.Revenue
    };

    /// <summary>
    /// Revenue account used to track income generated from foreign exchange
    /// spread margins.
    /// </summary>
    /// <remarks>
    /// Represents gains earned when currency conversion operations include
    /// platform exchange margin between the customer rate and internal
    /// settlement rate.
    ///
    /// This account is separated from operational fees to provide transparent
    /// reporting of FX related revenue streams.
    /// </remarks>
    public static readonly LedgerAccount RevenueFxSpread = new()
    {
        Id = "revenue:platform:fx_spread",
        Name = "Revenue - FX Spread Gain",
        AccountType = "platform_revenue",
        Category = AccountCategory.Revenue
    };
}