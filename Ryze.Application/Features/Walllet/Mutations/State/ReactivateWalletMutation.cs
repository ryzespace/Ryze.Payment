using ModularityKit.Context.Abstractions;
using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Application.Features.Walllet.Mutations.Helpers;
using Ryze.Domain.Features.Wallet.Entity;

namespace Ryze.Application.Features.Walllet.Mutations.State;

/// <summary>
/// Mutation responsible for reactivating a suspended wallet aggregate.
/// </summary>
/// <remarks>
/// Executes a wallet lifecycle transition from a suspended state back into
/// an active state.
///
/// The mutation delegates the state transition to the wallet aggregate and
/// records the resulting changes as a mutation change set for auditing,
/// persistence tracking, and downstream processing.
///
/// Unlike wallet closure, reactivation is considered reversible because the
/// wallet lifecycle can transition back to a suspended state when required.
/// A business reason is required to provide explicit audit intent.
/// </remarks>
public class ReactivateWalletMutation(
    IContextAccessor<WalletReactivateContext> contextAccessor,
    MutationContext mutationContext) : IMutation<Wallet>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "wallet.reactivate",
        Category = "domain.wallet",
        Description = "reactivate wallet",
        RiskLevel = MutationRiskLevel.High,
        IsReversible = true,
        Tags = new HashSet<string> { "wallet", "reactivate" }
    };

    public MutationContext Context { get; } = mutationContext;

    private WalletReactivateContext Input => contextAccessor.Current
        ?? throw new InvalidOperationException("WalletReactivateContext is not active.");

    public ValidationResult Validate(Wallet state)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(Input.Reason))
        {
            result.AddError(
                path: "reason",
                message: "Reason is required.",
                code: "MISSING_REASON");
        }

        return result;
    }

    public MutationResult<Wallet> Apply(Wallet state)
    {
        var beforeStatus = state.Status;

        state.Reactivate(Input.Reason);

        var changes = ChangeSet.FromChanges(
            StateChange.Modified(
                path: "status",
                oldValue: beforeStatus,
                newValue: state.Status
            ),
            StateChange.Modified(
                path: "suspension.reason",
                oldValue: Input.Reason,
                newValue: null
            )
        );

        return MutationResult<Wallet>.Success(
            newState: state,
            changes: changes
        );
    }

    public MutationResult<Wallet> Simulate(Wallet state)
    {
        var simulatedState = WalletSimulationClone.ShallowClone(state);
        return Apply(simulatedState);
    }
}
