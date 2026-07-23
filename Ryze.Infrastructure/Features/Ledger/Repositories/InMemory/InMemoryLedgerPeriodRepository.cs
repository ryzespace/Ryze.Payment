using System.Collections.Concurrent;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Infrastructure.Features.Ledger.Repositories.InMemory;

/// <summary>
/// Provides an in memory implementation of <see cref="ILedgerPeriodRepository"/>
/// for ledger accounting period persistence.
/// </summary>
/// <remarks>
/// Stores ledger periods in thread safe concurrent dictionary and is intended
/// primarily for development, testing, and single process scenarios.
/// Data is not persisted across application restarts.
/// </remarks>
public sealed class InMemoryLedgerPeriodRepository : ILedgerPeriodRepository
{
    private readonly ConcurrentDictionary<string, LedgerPeriod> _periods = new();

    /// <summary>
    /// Retrieves the accounting period that contains the specified timestamp.
    /// </summary>
    /// <param name="timestamp">The timestamp used to locate the containing accounting period.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The accounting period containing the specified timestamp, or
    /// <see langword="null"/> if no matching period exists.
    /// </returns>
    public Task<LedgerPeriod?> GetPeriodForDateAsync(DateTimeOffset timestamp, CancellationToken ct = default)
    {
        var match = _periods.Values.FirstOrDefault(p => p.Contains(timestamp));
        return Task.FromResult(match);
    }

    /// <summary>
    /// Retrieves an accounting period by its unique identifier.
    /// </summary>
    /// <param name="periodId">The unique identifier of the accounting period.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching accounting period, or <see langword="null"/> if no period
    /// with the specified identifier exists.
    /// </returns>
    public Task<LedgerPeriod?> GetByIdAsync(string periodId, CancellationToken ct = default)
    {
        _periods.TryGetValue(periodId, out var period);
        return Task.FromResult(period);
    }

    /// <summary>
    /// Persists a new or updated accounting period in memory.
    /// </summary>
    /// <param name="period">The accounting period to persist.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A completed task representing the persistence operation.</returns>
    public Task SaveAsync(LedgerPeriod period, CancellationToken ct = default)
    {
        _periods[period.Id] = period;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves all accounting periods ordered by their start timestamp.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection containing all stored accounting periods ordered
    /// by their <see cref="LedgerPeriod.StartsAt"/> value.
    /// </returns>
    public Task<IReadOnlyList<LedgerPeriod>> GetAllAsync(CancellationToken ct = default)
    {
        var sorted = _periods.Values.OrderBy(p => p.StartsAt).ToList();
        return Task.FromResult<IReadOnlyList<LedgerPeriod>>(sorted);
    }
}