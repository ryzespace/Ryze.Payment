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
/// Mutation responsible for permanently closing a wallet aggregate.
/// </summary>
/// <remarks>
/// Executes a wallet lifecycle transition from an operational state into
/// a closed state.
///
/// The mutation delegates the actual state transition to the wallet aggregate
/// and records the resulting changes as a mutation change set for auditing,
/// persistence, and downstream processing.
///
/// Closing a wallet is treated as a high-risk, non-reversible operation.
/// A business reason is required to provide explicit audit intent for
/// the lifecycle change.
/// </remarks>
public class CloseWalletMutation(
    IContextAccessor<WalletCloseContext> contextAccessor,
    MutationContext mutationContext) : IMutation<Wallet>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "wallet.close",
        Category = "domain.wallet",
        Description = "Close wallet permanently",
        RiskLevel = MutationRiskLevel.High,
        IsReversible = false,
        Tags = new HashSet<string> { "wallet", "close" }
    };

    public MutationContext Context { get; } = mutationContext;

    private WalletCloseContext Input => contextAccessor.Current
        ?? throw new InvalidOperationException("WalletCloseContext is not active.");

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

        state.Close(Input.Reason);

        var changes = ChangeSet.FromChanges(
            StateChange.Modified(
                path: "status",
                oldValue: beforeStatus,
                newValue: state.Status
            ),
            StateChange.Modified(
                path: "closed_at",
                oldValue: null,
                newValue: state.ClosedAt
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
