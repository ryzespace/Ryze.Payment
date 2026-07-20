using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Domain.Features.Ledger.ValueObject;

/// <summary>
/// Represents reference to ledger account used by journal entry.
/// </summary>
/// <remarks>
/// This value object provides a lightweight immutable reference to an account
/// within the Chart of Accounts. It avoids coupling journal entries directly
/// to the full <see cref="LedgerAccount"/> entity while preserving the
/// information required for posting and auditing.
///
/// The account reference is stored as part of the immutable accounting record
/// and should not change after a journal entry has been created.
/// </remarks>
public sealed record LedgerAccountReference
{
    /// <summary>
    /// Gets the unique identifier of the ledger account.
    /// </summary>
    /// <remarks>
    /// Usually corresponds to an identifier from the Chart of Accounts,
    /// for example wallet account, treasury account, escrow account,
    /// or revenue account.
    /// </remarks>
    public required string AccountId { get; init; }

    /// <summary>
    /// Gets the logical classification of the account.
    /// </summary>
    /// <remarks>
    /// This value is used for operational routing, reporting,
    /// reconciliation, and integration boundaries.
    /// </remarks>
    public required string AccountType { get; init; }

    /// <summary>
    /// Creates new immutable ledger account reference.
    /// </summary>
    /// <param name="accountId"> Unique ledger account identifier.</param>
    /// <param name="accountType"> Logical account classification.</param>
    public LedgerAccountReference(
        string accountId,
        string accountType)
    {
        AccountId = accountId;
        AccountType = accountType;
    }
}