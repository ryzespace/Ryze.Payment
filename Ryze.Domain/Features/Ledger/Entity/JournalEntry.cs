using Ryze.Domain.Features.Ledger.ValueObject;

namespace Ryze.Domain.Features.Ledger.Entity;

/// <summary>
/// Represents single journal entry recorded within ledger transaction.
/// </summary>
/// <remarks>
/// A journal entry is an immutable accounting record that describes one side
/// of a double entry ledger operation.
///
/// Each entry belongs to exactly one transaction and contains account context,
/// posting information, audit metadata, and optional business metadata.
///
/// Journal entries are the atomic units used for balance calculation,
/// reconciliation, reporting, and audit verification.
/// </remarks>
public sealed record JournalEntry
{
    /// <summary>
    /// Unique identifier of the journal entry.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Sequential or externally assigned ledger entry number.
    /// </summary>
    public string? EntryNumber { get; init; }

    /// <summary>
    /// Identifier of the parent ledger transaction.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Timestamp when the journal entry was created.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Ledger account affected by this entry.
    /// </summary>
    public required LedgerAccountReference Account { get; init; }

    /// <summary>
    /// Financial posting details of the journal entry.
    /// </summary>
    public required LedgerPosting Posting { get; init; }

    /// <summary>
    /// Audit information associated with the journal entry.
    /// </summary>
    public required EntryAudit Audit { get; init; }

    /// <summary>
    /// Additional contextual metadata attached to the entry.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}