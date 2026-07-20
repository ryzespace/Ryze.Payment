using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Application.Features.Ledger.Builders;

/// <summary>
/// Provides low level fluent builder for constructing ledger transactions
/// using double-entry accounting principles.
/// </summary>
/// <remarks>
/// Responsible for composing granular journal entries that form a valid
/// ledger transaction aggregate.
///
/// The builder manages transaction metadata, idempotency information,
/// audit context, entry lifecycle status, and structural validation before
/// producing the final <see cref="LedgerTransaction"/> instance.
///
/// This component is intended for technical transaction composition.
/// Business-level operations such as transfers, deposits, withdrawals,
/// fees, refunds, and escrow workflows should use
/// <see cref="LedgerOperationBuilder"/> as the higher-level abstraction.
/// </remarks>
public sealed partial class LedgerTransactionBuilder
{
    /// <summary>
    /// Adds a debit journal entry using an account identifier.
    /// </summary>
    /// <remarks>
    /// Creates a ledger account from the provided identifier and infers
    /// its accounting category automatically before adding the entry.
    /// </remarks>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="amount">Positive debit amount.</param>
    /// <param name="currency">Currency code associated with the entry.</param>
    /// <param name="description">Human-readable entry description.</param>
    /// <param name="accountType">Logical account type.</param>
    /// <param name="metadata">Additional entry metadata.</param>
    /// <param name="status">Initial lifecycle status of the entry.</param>
    /// <returns>The current transaction builder instance.</returns>
    public LedgerTransactionBuilder AddDebit(
        string accountId,
        decimal amount,
        string currency,
        string description,
        string accountType = "unknown",
        Dictionary<string, string>? metadata = null,
        EntryStatus status = EntryStatus.Cleared)
        => AddDebit(
            new LedgerAccount
            {
                Id = accountId,
                Name = accountId,
                AccountType = accountType,
                Category = InferCategory(accountId)
            },
            amount,
            currency,
            description,
            metadata,
            status);

    /// <summary>
    /// Adds a credit journal entry using an account identifier.
    /// </summary>
    /// <remarks>
    /// Creates a ledger account from the provided identifier and infers
    /// its accounting category automatically before adding the entry.
    /// </remarks>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="amount">Positive credit amount.</param>
    /// <param name="currency">Currency code associated with the entry.</param>
    /// <param name="description">Human-readable entry description.</param>
    /// <param name="accountType">Logical account type.</param>
    /// <param name="metadata">Additional entry metadata.</param>
    /// <param name="status">Initial lifecycle status of the entry.</param>
    /// <returns>The current transaction builder instance.</returns>
    public LedgerTransactionBuilder AddCredit(
        string accountId,
        decimal amount,
        string currency,
        string description,
        string accountType = "unknown",
        Dictionary<string, string>? metadata = null,
        EntryStatus status = EntryStatus.Cleared)
        => AddCredit(
            new LedgerAccount
            {
                Id = accountId,
                Name = accountId,
                AccountType = accountType,
                Category = InferCategory(accountId)
            },
            amount,
            currency,
            description,
            metadata,
            status);

    /// <summary>
    /// Adds a debit journal entry using a predefined ledger account.
    /// </summary>
    /// <param name="account">Ledger account affected by the debit entry.</param>
    /// <param name="amount">Positive debit amount.</param>
    /// <param name="currency">Currency code associated with the entry.</param>
    /// <param name="description">Human-readable entry description.</param>
    /// <param name="metadata">Additional entry metadata.</param>
    /// <param name="status">Initial lifecycle status of the entry.</param>
    /// <returns>The current transaction builder instance.</returns>
    public LedgerTransactionBuilder AddDebit(
        LedgerAccount account,
        decimal amount,
        string currency,
        string description,
        Dictionary<string, string>? metadata = null,
        EntryStatus status = EntryStatus.Cleared)
    {
        _entries.Add(
            CreateEntry(
                account,
                EntryType.Debit,
                amount,
                currency,
                description,
                metadata,
                status));

        return this;
    }

    /// <summary>
    /// Adds a credit journal entry using a predefined ledger account.
    /// </summary>
    /// <param name="account">Ledger account affected by the credit entry.</param>
    /// <param name="amount">Positive credit amount.</param>
    /// <param name="currency">Currency code associated with the entry.</param>
    /// <param name="description">Human-readable entry description.</param>
    /// <param name="metadata">Additional entry metadata.</param>
    /// <param name="status">Initial lifecycle status of the entry.</param>
    /// <returns>The current transaction builder instance.</returns>
    public LedgerTransactionBuilder AddCredit(
        LedgerAccount account,
        decimal amount,
        string currency,
        string description,
        Dictionary<string, string>? metadata = null,
        EntryStatus status = EntryStatus.Cleared)
    {
        _entries.Add(
            CreateEntry(
                account,
                EntryType.Credit,
                amount,
                currency,
                description,
                metadata,
                status));

        return this;
    }

    /// <summary>
    /// Adds a debit journal entry with a specific account type and metadata builder.
    /// </summary>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="accountType">Logical account type.</param>
    /// <param name="amount">Positive debit amount.</param>
    /// <param name="currency">Currency code associated with the entry.</param>
    /// <param name="description">Human-readable entry description.</param>
    /// <param name="metadataConfig">Metadata configuration callback.</param>
    /// <returns>The current transaction builder instance.</returns>
    public LedgerTransactionBuilder AddDebit(
        string accountId,
        string accountType,
        decimal amount,
        string currency,
        string description,
        Action<MetadataBuilder> metadataConfig)
    {
        var mb = new MetadataBuilder();
        metadataConfig(mb);

        return AddDebit(
            accountId,
            amount,
            currency,
            description,
            accountType,
            mb.Build());
    }

    /// <summary>
    /// Adds a credit journal entry with a specific account type and metadata builder.
    /// </summary>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="accountType">Logical account type.</param>
    /// <param name="amount">Positive credit amount.</param>
    /// <param name="currency">Currency code associated with the entry.</param>
    /// <param name="description">Human-readable entry description.</param>
    /// <param name="metadataConfig">Metadata configuration callback.</param>
    /// <returns>The current transaction builder instance.</returns>
    public LedgerTransactionBuilder AddCredit(
        string accountId,
        string accountType,
        decimal amount,
        string currency,
        string description,
        Action<MetadataBuilder> metadataConfig)
    {
        var mb = new MetadataBuilder();
        metadataConfig(mb);

        return AddCredit(
            accountId,
            amount,
            currency,
            description,
            accountType,
            mb.Build());
    }

    /// <summary>
    /// Adds a debit journal entry using a predefined account and fluent metadata builder.
    /// </summary>
    /// <param name="account">Ledger account affected by the entry.</param>
    /// <param name="amount">Positive debit amount.</param>
    /// <param name="currency">Currency code associated with the entry.</param>
    /// <param name="description">Human-readable entry description.</param>
    /// <param name="metadataConfig">Metadata configuration callback.</param>
    /// <returns>The current transaction builder instance.</returns>
    public LedgerTransactionBuilder AddDebit(
        LedgerAccount account,
        decimal amount,
        string currency,
        string description,
        Action<MetadataBuilder> metadataConfig)
    {
        var mb = new MetadataBuilder();
        metadataConfig(mb);

        return AddDebit(
            account,
            amount,
            currency,
            description,
            mb.Build());
    }

    /// <summary>
    /// Adds a credit journal entry using a predefined account and fluent metadata builder.
    /// </summary>
    /// <param name="account">Ledger account affected by the entry.</param>
    /// <param name="amount">Positive credit amount.</param>
    /// <param name="currency">Currency code associated with the entry.</param>
    /// <param name="description">Human-readable entry description.</param>
    /// <param name="metadataConfig">Metadata configuration callback.</param>
    /// <returns>The current transaction builder instance.</returns>
    public LedgerTransactionBuilder AddCredit(
        LedgerAccount account,
        decimal amount,
        string currency,
        string description,
        Action<MetadataBuilder> metadataConfig)
    {
        var mb = new MetadataBuilder();
        metadataConfig(mb);

        return AddCredit(
            account,
            amount,
            currency,
            description,
            mb.Build());
    }
}
