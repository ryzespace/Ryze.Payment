using Marten;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Infrastructure.Features.Ledger.Repositories.Marten;

/// <summary>
/// Generates sequential journal entry numbers using PostgreSQL sequence managed by Marten.
/// </summary>
/// <param name="session">The Marten document session providing access to the database connection.</param>
public sealed class MartenLedgerEntryNumberGenerator(IDocumentSession session) : ILedgerEntryNumberGenerator
{
    /// <summary>
    /// Generates the next sequential journal entry number.
    /// </summary>
    /// <remarks>
    /// The generated number is based on the <c>ledger_entry_seq</c> PostgreSQL sequence
    /// and is formatted using the current UTC year in the form <c>JE-yyyy-######</c>.
    /// </remarks>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The next unique formatted journal entry number.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Marten session does not have an active database connection. </exception>
    public async Task<string> GetNextEntryNumberAsync(CancellationToken ct = default)
    {
        var conn = session.Connection
                   ?? throw new InvalidOperationException("No active database connection on the session.");

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT nextval('ledger_entry_seq')";

        var result = await cmd.ExecuteScalarAsync(ct);
        var seq = Convert.ToInt64(result);
        var year = DateTimeOffset.UtcNow.Year;

        return $"JE-{year}-{seq:D6}";
    }
}