using Ryze.Domain.Features.Ledger.Entity;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Ledger.Mutations.Transaction;

namespace Ryze.Application.Features.Ledger.Policies.Security;

/// <summary>
/// Protects ledger transaction recording from excessively back-dated or future-dated timestamps.
/// </summary>
/// <remarks>
/// Currently enforces fixed rolling timestamp window by rejecting transactions dated more than
/// 30 days in the past or more than 5 minutes in the future.
///
/// <para>
/// The 30 day back dating restriction is temporary safeguard and does not represent the actual
/// accounting period state. It should be replaced with validation against the ledger period
/// lifecycle so that transactions are rejected only when their target accounting period is closed.
/// </para>
/// </remarks>
public sealed class LedgerPeriodSecurityPolicy : IMutationPolicy<LedgerTransaction>
{
    /// <summary>
    /// Gets the unique name of the policy.
    /// </summary>
    public string Name =>
        "ledger.security.period";

    /// <summary>
    /// Gets the priority of the policy.
    /// </summary>
    public int Priority => 50;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string Description =>
        "Protects financial periods against illegal back-dating or future-dating.";

    /// <summary>
    /// Evaluates the policy against mutation.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current state.</param>
    /// <returns>A policy decision (Allow or Deny).</returns>
    public PolicyDecision Evaluate(IMutation<LedgerTransaction> mutation, LedgerTransaction state)
    {
        if (mutation is not RecordTransactionMutation record) return PolicyDecision.Allow(Name);
        
        var tx = record.Transaction;
        
        // TODO: Replace the fixed 30 day rolling window with validation against
        // the actual accounting period state. Back dated transactions should be
        // rejected only when their target accounting period is closed.
        if (tx.Timestamp < DateTimeOffset.UtcNow.AddDays(-30))
        {
            return PolicyDecision.Deny("Transaction timestamp violates closed accounting period (too old).", Name);
        }

        // Do not allow transactions floating in the future
        return tx.Timestamp > DateTimeOffset.UtcNow.AddMinutes(5)
            ? PolicyDecision.Deny("Transaction timestamp cannot be in the future.", Name)
            : PolicyDecision.Allow(Name, "Timestamp falls within open accounting boundaries.");
    }
}
