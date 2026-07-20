using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Walllet.Mutations.State;
using Ryze.Domain.Features.Wallet.Entity;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Walllet.Policies.State;

public sealed class SuspendWalletPolicy : IMutationPolicy<Wallet>
{
    public string Name => "wallet.suspend";
    public int Priority => 100;
    public string Description => "Validates whether a wallet can be suspended";

    public PolicyDecision Evaluate(IMutation<Wallet> mutation, Wallet state)
    {
        return mutation switch
        {
            not SuspendWalletMutation => PolicyDecision.Allow(Name),
            SuspendWalletMutation suspend => state.Status switch
            {
                WalletStatus.Suspended => PolicyDecision.Deny("Wallet is already suspended", Name),
                WalletStatus.Closed => PolicyDecision.DenyCritical("Closed wallet cannot be suspended", Name),
                not WalletStatus.Active => PolicyDecision.Deny("Only active wallets can be suspended", Name),
                _ => string.IsNullOrWhiteSpace(suspend.Context?.Reason)
                    ? PolicyDecision.Deny("Suspension reason is required", Name)
                    : PolicyDecision.Allow(Name)
            }
        };
    }
}
