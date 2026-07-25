using Ryze.Domain.Features.Ledger.Entity;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Ledger.Mutations.Transaction;

namespace Ryze.Application.Features.Ledger.Policies.Validation;

/// <summary>
/// Validates core business rules for recording ledger transactions.
/// </summary>
/// <remarks>
/// Ensures that recorded transactions have an actor associated with them
/// for audit and accountability purposes.
/// </remarks>
public sealed class RecordTransactionPolicy : IMutationPolicy<LedgerTransaction>
{
    /// <summary>
    /// Gets the unique name of the policy.
    /// </summary>
    public string Name =>
        "ledger.record.policy";

    /// <summary>
    /// Gets the priority of the policy.
    /// </summary>
    public int Priority => 100;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string Description =>
        "Validates ledger transaction business rules";

    /// <summary>
    /// Evaluates the policy against mutation.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated. </param>
    /// <param name="state">The current ledger state. </param>
    /// <returns>Policy decision indicating whether the mutation is allowed. </returns>
    public PolicyDecision Evaluate(
        IMutation<LedgerTransaction> mutation,
        LedgerTransaction state)
    {
        if (mutation is not RecordTransactionMutation record)
        {
            return PolicyDecision.Allow(Name);
        }

        return string.IsNullOrWhiteSpace(record.Context.ActorId)
            ? PolicyDecision.Deny(
                "Actor is required for ledger audit trails.",
                Name)
            : PolicyDecision.Allow(
                Name,
                reason:
                $"Ledger transaction creation allowed for actor {record.Context.ActorId}");
    }
}
