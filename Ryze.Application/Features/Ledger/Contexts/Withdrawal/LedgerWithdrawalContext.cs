using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Ledger.Contexts.Withdrawal;

/// <summary>
/// Write context for ledger withdrawal operations.
/// </summary>
/// <remarks>
/// Represents immutable command intent describing a withdrawal from a
/// ledger account to an external destination.
///
/// The context contains the source account, withdrawal amount, destination,
/// currency, and audit metadata required to execute the operation through
/// the application layer.
/// </remarks>
public sealed class LedgerWithdrawalContext : IContext
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Identifier of the actor initiating the withdrawal.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <summary>
    /// Reason explaining why the withdrawal is performed.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Arbitrary metadata associated with the withdrawal request.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = [];

    /// <summary>
    /// Identifier of the ledger account from which funds are withdrawn.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// External destination receiving the withdrawn funds.
    /// </summary>
    /// <remarks>
    /// Depending on the payment rail, this may represent a bank account,
    /// payment instrument, wallet identifier, or another supported destination.
    /// </remarks>
    public required string Destination { get; init; }

    /// <summary>
    /// Amount to withdraw from the ledger account.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency in which the withdrawal is performed.
    /// </summary>
    public required string Currency { get; init; }
    
    public string Description { get; init; } = "Withdrawal to External Destination";
}