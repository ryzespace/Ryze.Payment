using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Domain.Features.Ledger.Entity;

/// <summary>
/// Represents an account defined in the Chart of Accounts within the ledger system.
/// </summary>
/// <remarks>
/// A ledger account is fundamental accounting entity used to classify,
/// group, and track financial movements.
///
/// Each account belongs to standard accounting category that determines
/// its natural balance direction and how debit and credit postings affect it.
///
/// Ledger accounts are used as targets for journal entries and provide the
/// foundation for balance calculations, reporting, reconciliation,
/// and financial auditing.
/// </remarks>
public sealed record LedgerAccount
{
    /// <summary>
    /// Unique system identifier of the ledger account.
    /// </summary>
    /// <remarks>
    /// Identifiers are typically represented using domain-specific formats,
    /// for example:
    /// <c>wallet:1234</c> for user wallet accounts or
    /// <c>platform:liabilities</c> for platform accounting accounts.
    /// </remarks>
    public required string Id { get; init; }

    /// <summary>
    /// Name of the account.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Operational classification of the account.
    /// </summary>
    public required string AccountType { get; init; }

    /// <summary>
    /// Accounting classification of the account.
    /// </summary>
    public required AccountCategory Category { get; init; }

    /// <summary>
    /// Gets the natural balance direction for the account.
    /// </summary>
    /// <remarks>
    /// Asset and Expense accounts normally increase through Debit postings.
    ///
    /// Liability, Equity, and Revenue accounts normally increase through
    /// Credit postings.
    /// </remarks>
    public EntryType NormalBalance => Category switch
    {
        AccountCategory.Asset or AccountCategory.Expense => EntryType.Debit,
        _ => EntryType.Credit
    };

    /// <summary>
    /// Currency assigned to the account.
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Indicates whether the account is available for new ledger postings.
    /// </summary>
    public bool IsActive { get; init; } = true;
}