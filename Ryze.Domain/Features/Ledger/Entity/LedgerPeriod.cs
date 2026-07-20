using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Exceptions;

namespace Ryze.Domain.Features.Ledger.Entity;

/// <summary>
/// Represents fiscal accounting period used for controlling ledger activity.
/// </summary>
/// <remarks>
/// A ledger period defines bounded time interval during which journal entries
/// may be recorded.
///
/// Period lifecycle management ensures financial consistency by controlling
/// when transactions can be posted, when reconciliation is required,
/// and when accounting data becomes permanently immutable.
/// </remarks>
public sealed record LedgerPeriod
{
    /// <summary>
    /// Unique identifier of the accounting period.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display label of the accounting period.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Inclusive UTC timestamp marking the beginning of the period.
    /// </summary>
    public required DateTimeOffset StartsAt { get; init; }

    /// <summary>
    /// Exclusive UTC timestamp marking the end of the period.
    /// </summary>
    /// <remarks>
    /// The period interval follows the mathematical range:
    /// <c>[StartsAt, EndsAt)</c>.
    /// </remarks>
    public required DateTimeOffset EndsAt { get; init; }

    /// <summary>
    /// Current lifecycle state of the accounting period.
    /// </summary>
    /// <remarks>
    /// Determines whether new journal entries are accepted
    /// and what level of authorization is required.
    /// </remarks>
    public PeriodStatus Status { get; init; } = PeriodStatus.Open;

    /// <summary>
    /// Timestamp when the period was closed.
    /// </summary>
    /// <remarks>
    /// Remains null while the period has not reached a closed state.
    /// </remarks>
    public DateTimeOffset? ClosedAt { get; init; }

    /// <summary>
    /// Timestamp when the period was permanently locked.
    /// </summary>
    /// <remarks>
    /// Locked periods cannot receive any further ledger modifications.
    /// </remarks>
    public DateTimeOffset? LockedAt { get; init; }

    /// <summary>
    /// Identifier of the actor who performed the close or lock operation.
    /// </summary>
    public string? ClosedBy { get; init; }

    /// <summary>
    /// Determines whether this period accepts new journal entries.
    /// </summary>
    /// <remarks>
    /// Open periods accept normal ledger postings.
    ///
    /// Closing periods require an explicit override, typically for
    /// approved reconciliation adjustments.
    ///
    /// Closed and Locked periods reject new entries.
    /// </remarks>
    /// <param name="hasOverride">Indicates whether the caller has permission to bypass normal restrictions. </param>
    public bool AcceptsEntries(bool hasOverride = false)
    {
        return Status switch
        {
            PeriodStatus.Open => true,
            PeriodStatus.Closing => hasOverride,
            _ => false
        };
    }

    /// <summary>
    /// Performs lifecycle transition to another accounting period state.
    /// </summary>
    /// <remarks>
    /// Only approved state transitions are allowed.
    ///
    /// Supported transitions include:
    /// <list type="bullet">
    /// <item><description>Open → Closing</description></item>
    /// <item><description>Closing → Open</description></item>
    /// <item><description>Closing → Closed</description></item>
    /// <item><description>Closed → Open</description></item>
    /// <item><description>Closed → Locked</description></item>
    /// </list>
    ///
    /// Invalid transitions throw a ledger validation exception.
    /// </remarks>
    /// <param name="newStatus">Target lifecycle state.</param>
    /// <param name="initiatedBy">Actor initiating the transition.</param>
    public LedgerPeriod TransitionTo(
        PeriodStatus newStatus,
        string initiatedBy)
    {
        var valid = (Status, newStatus) switch
        {
            (PeriodStatus.Open, PeriodStatus.Closing) => true,
            (PeriodStatus.Closing, PeriodStatus.Open) => true,
            (PeriodStatus.Closing, PeriodStatus.Closed) => true,
            (PeriodStatus.Closed, PeriodStatus.Open) => true,
            (PeriodStatus.Closed, PeriodStatus.Locked) => true,
            _ => false
        };

        if (!valid)
            throw new LedgerValidationException(
                $"Cannot transition period '{Id}' from {Status} to {newStatus}.");

        var now = DateTimeOffset.UtcNow;

        return this with
        {
            Status = newStatus,
            ClosedBy = initiatedBy,
            ClosedAt = newStatus is PeriodStatus.Closed or PeriodStatus.Locked
                ? now
                : ClosedAt,
            LockedAt = newStatus == PeriodStatus.Locked
                ? now
                : LockedAt
        };
    }

    /// <summary>
    /// Determines whether timestamp belongs to this accounting period.
    /// </summary>
    /// <remarks>
    /// Uses an inclusive start and exclusive end interval:
    /// <c>[StartsAt, EndsAt)</c>.
    /// </remarks>
    /// <param name="timestamp">Timestamp to evaluate.</param>
    public bool Contains(DateTimeOffset timestamp)
        => timestamp >= StartsAt && timestamp < EndsAt;
}