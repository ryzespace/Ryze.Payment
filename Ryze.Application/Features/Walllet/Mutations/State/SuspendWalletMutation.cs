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
/// Mutation responsible for suspending an active wallet aggregate.
/// </summary>
/// <remarks>
/// Executes a wallet lifecycle transition from an active state into a
/// suspended state.
///
/// The mutation delegates the actual state change to the wallet aggregate
/// and produces a change set describing the lifecycle transition.
///
/// Suspension is considered a high-risk but reversible operation because the
/// wallet can later be restored through a reactivation mutation.
///
/// A suspension reason is required to provide an explicit operational and
/// audit context for the state transition.
/// </remarks>
public class SuspendWalletMutation(
    IContextAccessor<WalletSuspendContext> contextAccessor,
    MutationContext mutationContext) : IMutation<Wallet>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "wallet.suspend",
        Category = "domain.wallet",
        Description = "Suspend wallet",
        RiskLevel = MutationRiskLevel.High,
        IsReversible = true,
        Tags = new HashSet<string> { "wallet", "suspension" }
    };

    public MutationContext Context { get; } = mutationContext;

    private WalletSuspendContext Input => contextAccessor.Current
       ?? throw new InvalidOperationException("WalletSuspendContext is not active.");

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

        state.Suspend(Input.Reason);

        var changes = ChangeSet.FromChanges(
            StateChange.Modified(
                path: "status",
                oldValue: beforeStatus,
                newValue: state.Status
            ),
            StateChange.Modified(
                path: "suspension.reason",
                oldValue: null,
                newValue: Input.Reason
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
