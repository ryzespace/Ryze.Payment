namespace Ryze.Domain.Features.Ledger.Enum;

/// <summary>
/// Defines the direction of journal entry in double entry bookkeeping.
/// </summary>
/// <remarks>
/// Every ledger transaction consists of one or more debit and credit entries.
/// The total value of debit entries must always equal the total value
/// of credit entries to preserve accounting consistency.
/// </remarks>
public enum EntryType
{
    /// <summary>
    /// Represents a debit side journal entry.
    /// </summary>
    /// <remarks>
    /// Typically increases asset and expense accounts
    /// while decreasing liability, equity, and revenue accounts.
    /// </remarks>
    Debit,

    /// <summary>
    /// Represents a credit side journal entry.
    /// </summary>
    /// <remarks>
    /// Typically increases liability, equity, and revenue accounts
    /// while decreasing asset and expense accounts.
    /// </remarks>
    Credit
}