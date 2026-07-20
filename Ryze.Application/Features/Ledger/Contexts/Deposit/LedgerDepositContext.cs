using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Ledger.Contexts.Deposit;

/// <summary>
/// Write context for ledger deposit operations.
/// </summary>
/// <remarks>
/// Represents immutable command intent describing a deposit into a
/// ledger account from an external funding source.
///
/// The context contains the target account, deposited amount, funding
/// information, currency, and audit metadata required to execute the
/// operation through the application layer.
/// </remarks>
public sealed class LedgerDepositContext : IContext
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Identifier of the actor initiating the deposit.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <summary>
    /// Optional business reason explaining why the deposit is performed.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Arbitrary metadata associated with the deposit request.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = [];

    /// <summary>
    /// Identifier of the ledger account receiving the deposit.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// External funding source providing the deposited funds.
    /// </summary>
    public required string FundingSource { get; init; }

    /// <summary>
    /// Amount to be deposited.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency in which the deposit is performed.
    /// </summary>
    public required string Currency { get; init; }

    public string Description { get; init; } = "Deposit from External Source";
}