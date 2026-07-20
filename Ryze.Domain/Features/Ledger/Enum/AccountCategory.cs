namespace Ryze.Domain.Features.Ledger.Enum;

/// <summary>
/// Defines the accounting classification of ledger account.
/// </summary>
/// <remarks>
/// Represents the standard accounting categories used to classify
/// ledger accounts according to double-entry bookkeeping principles.
///
/// Each category defines the natural balance direction of the account:
/// assets and expenses normally increase on the debit side,
/// while liabilities, equity, and revenue normally increase on the credit side.
/// </remarks>
public enum AccountCategory
{
    /// <summary>
    /// Represents assets owned or controlled by the organization.
    /// </summary>
    /// <remarks>
    /// Examples include cash accounts, bank balances, and receivables.
    /// The normal balance direction is Debit.
    /// </remarks>
    Asset,

    /// <summary>
    /// Represents obligations owed to external parties.
    /// </summary>
    /// <remarks>
    /// Examples include loans, customer liabilities, and supplier obligations.
    /// The normal balance direction is Credit.
    /// </remarks>
    Liability,

    /// <summary>
    /// Represents ownership value within the accounting structure.
    /// </summary>
    /// <remarks>
    /// Examples include retained earnings and contributed capital.
    /// The normal balance direction is Credit.
    /// </remarks>
    Equity,

    /// <summary>
    /// Represents income generated from business activities.
    /// </summary>
    /// <remarks>
    /// Examples include transaction fees, service revenue, and other income sources.
    /// The normal balance direction is Credit.
    /// </remarks>
    Revenue,

    /// <summary>
    /// Represents costs incurred during business operations.
    /// </summary>
    /// <remarks>
    /// Examples include operational expenses and processing costs.
    /// The normal balance direction is Debit.
    /// </remarks>
    Expense
}