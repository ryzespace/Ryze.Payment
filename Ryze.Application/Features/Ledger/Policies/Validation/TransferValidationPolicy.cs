using Ryze.Domain.Features.Ledger.Entity;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Ledger.Mutations.Operations;

namespace Ryze.Application.Features.Ledger.Policies.Validation;

/// <summary>
/// Validates business rules for ledger transfer mutations.
/// </summary>
/// <remarks>
/// Ensures the source and destination accounts are different
/// and that the transfer amount is positive.
/// </remarks>
public sealed class TransferValidationPolicy : IMutationPolicy<LedgerTransaction>
{
    /// <summary>
    /// Gets the unique name of the policy.
    /// </summary>
    public string Name =>
        "ledger.validation.transfer";

    /// <summary>
    /// Gets the priority of the policy.
    /// </summary>
    public int Priority => 110;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string Description =>
        "Validates business rules for ledger transfers";

    /// <summary>
    /// Evaluates the transfer mutation against ledger transfer rules.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current ledger transaction state.</param>
    /// <returns>
    /// A policy decision that denies invalid transfers; otherwise, an allow decision.
    /// </returns>
    public PolicyDecision Evaluate(IMutation<LedgerTransaction> mutation, LedgerTransaction state)
    {
        if (mutation is not TransferMutation transfer) return PolicyDecision.Allow(Name);
        
        var input = transfer.Input;

        if (input.FromAccountId == input.ToAccountId)
            return PolicyDecision.Deny("Source and destination accounts must be different.", Name);

        if (input.Amount <= 0)
            return PolicyDecision.Deny("Transfer amount must be positive.", Name);

        return PolicyDecision.Allow(Name);
    }
}
