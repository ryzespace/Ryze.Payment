using System.Globalization;
using Microsoft.Extensions.Logging;
using Ryze.Application.Features.Ledger.Interfaces.Helpers;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Events;
using Ryze.Domain.Features.Ledger.Exceptions;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Application.Features.Ledger.Helpers;

/// <summary>
/// Provides accounting period lifecycle management and enforces period based
/// journal entry admission rules.
/// </summary>
/// <remarks>
/// Resolves accounting periods for transaction timestamps, automatically creates
/// missing calendar periods, validates whether entries may be recorded, and
/// manages accounting period status transitions.
/// </remarks>
public sealed class LedgerPeriod(
    ILedgerPeriodRepository periodRepo,
    ILogger<LedgerPeriod> logger) : ILedgerPeriod
{
    /// <summary>
    /// Ensures the transaction timestamp falls within an open period.
    /// Throws <see cref="Ryze.Domain.Features.Ledger.Exceptions.LedgerPeriodClosedException"/>
    /// if the period is Closed or Locked.
    /// </summary>
    /// <param name="transactionTimestamp">The timestamp of the transaction.</param>
    /// <param name="hasOverride">If true, allows entry into some restricted periods (e.g., Closing).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task EnsurePeriodOpenAsync(
        DateTimeOffset transactionTimestamp, 
        bool hasOverride = false, 
        CancellationToken ct = default)
    {
        // No period defined yet auto create an Open period for this month
        var period = await periodRepo.GetPeriodForDateAsync(transactionTimestamp, ct) ?? await GetOrCreatePeriodAsync(transactionTimestamp, ct);

        if (!period.AcceptsEntries(hasOverride))
        {
            logger.LogWarning(
                "Rejected entry for period {PeriodId} (Status: {Status}). Override: {HasOverride}",
                period.Id, period.Status, hasOverride);

            throw new LedgerPeriodClosedException(period.Id, period.Status);
        }

        logger.LogDebug("Period {PeriodId} is {Status} — entry allowed", period.Id, period.Status);
    }

    /// <summary>
    /// Transitions period to the given status.
    /// </summary>
    /// <param name="periodId">The ID of the period.</param>
    /// <param name="newStatus">The new status to transition to.</param>
    /// <param name="initiatedBy">The actor who initiated the transition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated ledger period.</returns>
    public async Task<Domain.Features.Ledger.Entity.LedgerPeriod> TransitionPeriodAsync(
        string periodId, 
        PeriodStatus newStatus, 
        string initiatedBy, 
        CancellationToken ct = default)
    {
        var period = await periodRepo.GetByIdAsync(periodId, ct)
            ?? throw new LedgerValidationException($"Period '{periodId}' not found.");

        var oldStatus = period.Status;
        var updated = period.TransitionTo(newStatus, initiatedBy);
        await periodRepo.SaveAsync(updated, ct);

        // Record Audit Event for period transition
        new LedgerAuditEvent(
            Guid.NewGuid(),
            "PeriodTransition",
            $"Period {periodId} transitioned from {oldStatus} to {newStatus}",
            initiatedBy,
            DateTimeOffset.UtcNow,
            periodId
        );
        // TODO: Assuming ledgerRepo or periodRepo can handle general events if we had a dedicated AuditRepository
        // For now, logging and standard repo update is the baseline.

        logger.LogInformation(
            "Period {PeriodId} transitioned from {OldStatus} to {NewStatus} by {InitiatedBy}",
            periodId, oldStatus, newStatus, initiatedBy);

        return updated;
    }

    /// <summary>
    /// Returns the current period for given timestamp, auto-creating if necessary.
    /// </summary>
    /// <param name="timestamp">The timestamp to find or create a period for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The found or created ledger period.</returns>
    public async Task<Domain.Features.Ledger.Entity.LedgerPeriod> GetOrCreatePeriodAsync(
        DateTimeOffset timestamp, 
        CancellationToken ct = default)
    {
        var periodId = timestamp.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        var existing = await periodRepo.GetByIdAsync(periodId, ct);

        if (existing is not null)
            return existing;

        // Auto create new Open period for the calendar month
        var startsAt = new DateTimeOffset(timestamp.Year, timestamp.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var endsAt = startsAt.AddMonths(1);
        var label = timestamp.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        var period = new Domain.Features.Ledger.Entity.LedgerPeriod
        {
            Id = periodId,
            Label = label,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Status = PeriodStatus.Open
        };

        await periodRepo.SaveAsync(period, ct);
        logger.LogInformation("Auto-created accounting period {PeriodId} ({Label})", periodId, label);

        return period;
    }

    /// <summary>
    /// Returns all accounting periods.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of all ledger periods.</returns>
    public async Task<IReadOnlyList<Domain.Features.Ledger.Entity.LedgerPeriod>> GetAllPeriodsAsync(CancellationToken ct = default) =>
        await periodRepo.GetAllAsync(ct);
}
