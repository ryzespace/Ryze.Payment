using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Application.Features.Ledger.DTO.Entry;

/// <summary>
/// Represents ledger entry data transfer object.
/// </summary>
/// <remarks>
/// Provides read only representation of journal entry returned by
/// ledger queries.
///
/// Contains transaction metadata, account information, monetary details,
/// lifecycle status, and audit information required for displaying and
/// processing ledger history.
/// </remarks>
/// <param name="Id">Unique identifier of the ledger entry.</param>
/// <param name="TransactionId">Identifier of the transaction containing the entry.</param>
/// <param name="Timestamp">Timestamp when the ledger entry was recorded.</param>
/// <param name="AccountId">Ledger account identifier associated with the entry.</param>
/// <param name="Direction">Entry direction, such as debit or credit.</param>
/// <param name="Amount">Monetary amount represented by the ledger entry.</param>
/// <param name="Currency">Currency code associated with the entry amount.</param>
/// <param name="Status">Current lifecycle status of the ledger entry.</param>
/// <param name="TransactionType">Type of transaction that generated the entry.</param>
/// <param name="Description">Human-readable description of the ledger entry.</param>
/// <param name="InitiatedBy">Optional identifier of the actor who initiated the operation.</param>
public sealed record EntryItemDto(
    Guid Id,
    Guid TransactionId,
    DateTimeOffset Timestamp,
    string AccountId,
    string Direction,
    decimal Amount,
    string Currency,
    string Status,
    string TransactionType,
    string Description,
    string? InitiatedBy
);
