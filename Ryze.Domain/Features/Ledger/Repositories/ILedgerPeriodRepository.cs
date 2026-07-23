using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Domain.Features.Ledger.Repositories;

/// <summary>
/// Defines the persistence contract for managing ledger accounting periods.
/// </summary>
/// <remarks>
/// Provides access to fiscal period records used by the ledger domain to
/// determine period availability, resolve accounting periods by identifier,
/// persist lifecycle changes, and retrieve the complete period history.
/// </remarks>
public interface ILedgerPeriodRepository
{
    /// <summary>
    /// Retrieves the accounting period containing the specified timestamp.
    /// </summary>
    /// <param name="timestamp">The timestamp used to determine the applicable accounting period. </param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation. </param>
    Task<LedgerPeriod?> GetPeriodForDateAsync(
        DateTimeOffset timestamp,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves an accounting period by its unique identifier.
    /// </summary>
    /// <param name="periodId">The unique period identifier,</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation. </param>
    Task<LedgerPeriod?> GetByIdAsync(
        string periodId,
        CancellationToken ct = default);

    /// <summary>
    /// Persists a new accounting period or updates an existing one.
    /// </summary>
    /// <param name="period">The accounting period to persist. </param>
    /// <param name="ct">token that can be used to cancel the asynchronous operation. </param>
    Task SaveAsync(
        LedgerPeriod period,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all accounting periods ordered by their start timestamp
    /// in ascending order.
    /// </summary>
    Task<IReadOnlyList<LedgerPeriod>> GetAllAsync(
        CancellationToken ct = default);
}