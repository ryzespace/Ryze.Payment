namespace Ryze.Domain.Features.Ledger.Repositories;

/// <summary>
/// Defines contract for generating sequential journal entry numbers.
/// </summary>
/// <remarks>
/// Implementations are responsible for generating unique entry numbers
/// used to identify journal entries within the ledger.
/// </remarks>
public interface ILedgerEntryNumberGenerator
{
    /// <summary>
    /// Generates the next sequential journal entry number.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation that returns the next journal entry number.</returns>
    Task<string> GetNextEntryNumberAsync(CancellationToken ct = default);
}