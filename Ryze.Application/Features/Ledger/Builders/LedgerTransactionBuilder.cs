using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Application.Features.Ledger.Builders;

/// <summary>
/// Provides low level fluent builder for constructing granular
/// double entry ledger transactions.
/// </summary>
/// <remarks>
/// Responsible for composing raw ledger transactions from individual
/// journal entries while preserving accounting structure and audit metadata.
///
/// The builder handles transaction identity, idempotency information,
/// timestamps, initiator context, reasons, and final aggregate validation.
///
/// This component should be used for low level ledger composition.
/// Higher level business workflows such as transfers, deposits, fees,
/// withdrawals, and escrow operations should prefer
/// <see cref="LedgerOperationBuilder"/>.
/// </remarks>
public sealed partial class LedgerTransactionBuilder
{
    private Guid? _customId;
    private readonly DateTimeOffset _timestamp = DateTimeOffset.UtcNow;
    private readonly List<JournalEntry> _entries = [];

    private readonly TransactionType _type;
    private string _idempotencyKey = string.Empty;
    private readonly string _initiatedBy;
    private string? _reason;

    private LedgerTransactionBuilder(TransactionType type, string initiatedBy)
    {
        _type = type;
        _initiatedBy = initiatedBy;
    }

    /// <summary>
    /// Creates a new ledger transaction builder instance.
    /// </summary>
    /// <param name="type">Ledger transaction type.</param>
    /// <param name="initiatedBy">Identifier of the actor initiating the transaction.</param>
    /// <returns>A new ledger transaction builder instance.</returns>
    public static LedgerTransactionBuilder Create(
        TransactionType type,
        string initiatedBy)
        => new(type, initiatedBy);

    /// <summary>
    /// Sets the idempotency key for the transaction.
    /// </summary>
    /// <remarks>
    /// The idempotency key is used to guarantee that the same logical
    /// operation cannot be committed multiple times.
    /// </remarks>
    /// <param name="key">Unique idempotency identifier.</param>
    /// <returns>The current builder instance.</returns>
    public LedgerTransactionBuilder WithIdempotencyKey(string key)
    {
        _idempotencyKey = key;
        return this;
    }

    /// <summary>
    /// Sets the business reason associated with the transaction.
    /// </summary>
    /// <param name="reason">Human-readable transaction reason.</param>
    /// <returns>The current builder instance.</returns>
    public LedgerTransactionBuilder WithReason(string reason)
    {
        _reason = reason;
        return this;
    }

    /// <summary>
    /// Sets a custom identifier for the transaction.
    /// </summary>
    /// <remarks>
    /// Allows deterministic transaction identifiers for scenarios such as
    /// testing, migration, replay, or controlled reconstruction.
    /// </remarks>
    /// <param name="id">Custom transaction identifier.</param>
    /// <returns>The current builder instance.</returns>
    public LedgerTransactionBuilder WithId(Guid id)
    {
        _customId = id;
        return this;
    }

    private Guid TransactionId => _customId ?? Guid.NewGuid();

    /// <summary>
    /// Creates a journal entry from the provided accounting information.
    /// </summary>
    /// <remarks>
    /// Initializes a ledger entry with transaction metadata, audit fields,
    /// account information, lifecycle status, and accounting details.
    /// </remarks>
    /// <param name="account">Ledger account affected by the entry.</param>
    /// <param name="entryType">Entry direction such as debit or credit.</param>
    /// <param name="amount">Positive monetary amount.</param>
    /// <param name="currency">Currency code associated with the entry.</param>
    /// <param name="description">Human-readable entry description.</param>
    /// <param name="metadata">Additional structured metadata.</param>
    /// <param name="status">Initial ledger entry status.</param>
    /// <returns>A newly created journal entry.</returns>
    private JournalEntry CreateEntry(
        LedgerAccount account,
        EntryType entryType,
        decimal amount,
        string currency,
        string description,
        Dictionary<string, string>? metadata,
        EntryStatus status)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = TransactionId,
            Timestamp = _timestamp,
            AccountId = account.Id,
            AccountType = account.AccountType,
            Type = entryType,
            Amount = amount,
            Currency = currency,
            RunningBalance = 0,
            Description = description,
            TransactionType = _type,
            InitiatedBy = _initiatedBy,
            Reason = _reason,
            IdempotencyKey = _idempotencyKey,
            Metadata = metadata,
            Status = status
        };
    }

    /// <summary>
    /// Builds and validates the ledger transaction.
    /// </summary>
    /// <remarks>
    /// Creates the final ledger transaction aggregate from configured
    /// journal entries.
    ///
    /// Before returning, the transaction validation process is executed
    /// to ensure that double-entry accounting rules and structural
    /// constraints are satisfied.
    /// </remarks>
    /// <returns>
    /// Fully constructed and validated ledger transaction.
    /// </returns>
    public LedgerTransaction Build()
    {
        if (string.IsNullOrEmpty(_idempotencyKey))
            throw new InvalidOperationException("IdempotencyKey is required.");

        var tx = new LedgerTransaction
        {
            Id = TransactionId,
            Timestamp = _timestamp,
            Type = _type,
            IdempotencyKey = _idempotencyKey,
            InitiatedBy = _initiatedBy,
            Reason = _reason,
            Entries = [.. _entries]
        };

        tx.Validate();
        return tx;
    }

    /// <summary>
    /// Infers the accounting category from a ledger account identifier.
    /// </summary>
    /// <remarks>
    /// Uses account naming conventions to determine the default accounting
    /// category required for balance calculation logic.
    /// </remarks>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <returns>Resolved account category.</returns>
    private static AccountCategory InferCategory(string accountId)
    {
        if (accountId.StartsWith("asset:", StringComparison.OrdinalIgnoreCase))
            return AccountCategory.Asset;

        if (accountId.StartsWith("liability:", StringComparison.OrdinalIgnoreCase))
            return AccountCategory.Liability;

        if (accountId.StartsWith("equity:", StringComparison.OrdinalIgnoreCase))
            return AccountCategory.Equity;

        if (accountId.StartsWith("revenue:", StringComparison.OrdinalIgnoreCase))
            return AccountCategory.Revenue;

        return accountId.StartsWith("expense:", StringComparison.OrdinalIgnoreCase)
            ? AccountCategory.Expense
            : AccountCategory.Liability;
    }
}