namespace Ryze.Domain.Features.Ledger.Events;

/// <summary>
/// Represents technical audit event recording critical financial state change.
/// </summary>
/// <remarks>
/// Captures the action performed, additional details, initiating actor,
/// occurrence timestamp, and an optional resource identifier associated
/// with the audited operation.
/// </remarks>
public sealed record LedgerAuditEvent(
    Guid Id,
    string Action,
    string Details,
    string InitiatedBy,
    DateTimeOffset Timestamp,
    string? ResourceId = null
);
