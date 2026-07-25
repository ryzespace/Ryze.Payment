using System.Diagnostics.CodeAnalysis;
using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Domain.Features.Ledger.ValueObject;

/// <summary>
/// Represents audit information attached to ledger entry.
/// </summary>
/// <remarks>
/// Provides traceability metadata for accounting operations,
/// including the type of transaction that produced the entry,
/// the actor responsible for initiating the operation,
/// context, and idempotency tracking.
/// </remarks>
public sealed record EntryAudit
{
    /// <summary>
    /// Gets transaction category that created this entry.
    /// </summary>
    /// <remarks>
    /// Examples include wallet topup, withdrawal, payment,
    /// refund, fee, escrow operation, or correction.
    /// </remarks>
    public required TransactionType TransactionType { get; init; }

    /// <summary>
    /// Gets the identifier of user, service, or system component
    /// that initiated the transaction.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <summary>
    /// Gets the optional business reason or explanation
    /// associated with the transaction.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Gets the optional idempotency key associated with the operation.
    /// </summary>
    /// <remarks>
    /// Allows external callers and distributed systems to safely retry
    /// operations without creating duplicate ledger transactions.
    /// </remarks>
    public string? IdempotencyKey { get; init; }

    /// <summary>
    /// Creates immutable audit information for a ledger entry.
    /// </summary>
    /// <param name="transactionType">Type of accounting transaction. </param>
    /// <param name="initiatedBy">Actor responsible for initiating the operation. </param>
    /// <param name="reason">Optional business explanation. </param>
    /// <param name="idempotencyKey">Optional idempotency identifier.</param>
    [SetsRequiredMembers]
    public EntryAudit(
        TransactionType transactionType,
        string initiatedBy,
        string? reason,
        string? idempotencyKey)
    {
        TransactionType = transactionType;
        InitiatedBy = initiatedBy;
        Reason = reason;
        IdempotencyKey = idempotencyKey;
    }
}