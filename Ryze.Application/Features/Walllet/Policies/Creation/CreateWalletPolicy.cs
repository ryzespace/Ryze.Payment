using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Walllet.Mutations.Creation;
using Ryze.Domain.Features.Wallet.Entity;

namespace Ryze.Application.Features.Walllet.Policies.Creation;

/// <summary>
/// Mutation policy responsible for validating wallet creation authorization rules.
/// </summary>
/// <remarks>
/// Evaluates whether a wallet creation mutation is allowed to proceed based on
/// contextual authorization requirements and ownership constraints.
///
/// This policy does not modify wallet state. It only validates whether the
/// mutation intent satisfies application-level rules before execution.
///
/// The policy ensures that:
/// <list type="bullet">
/// <item>The mutation has an authenticated actor.</item>
/// <item>The actor creating the wallet is one of the wallet owners.</item>
/// <item>Wallet owners do not contain duplicated identities.</item>
/// </list>
/// </remarks>
public sealed class CreateWalletPolicy : IMutationPolicy<Wallet>
{
    /// <summary>
    /// Policy identifier matching the wallet creation mutation operation.
    /// </summary>
    public string Name => "wallet.create";

    /// <summary>
    /// Execution priority used by the mutation policy pipeline.
    /// </summary>
    public int Priority => 100;

    /// <summary>
    /// Human-readable description of the policy responsibility.
    /// </summary>
    public string Description => "Validates wallet creation state";

    /// <summary>
    /// Evaluates whether the wallet creation mutation is allowed.
    /// </summary>
    /// <remarks>
    /// Policies are applied before mutation execution and can reject unsafe
    /// or unauthorized operations.
    ///
    /// Non matching mutation types are automatically allowed because this
    /// policy only applies to <see cref="CreateWalletMutation"/>.
    /// </remarks>
    /// <param name="mutation">
    /// Mutation being evaluated.
    /// </param>
    /// <param name="state">
    /// Current wallet aggregate state.
    /// </param>
    /// <returns>
    /// Policy decision indicating whether execution should continue.
    /// </returns>
    public PolicyDecision Evaluate(IMutation<Wallet> mutation, Wallet state)
    {
        if (mutation is not CreateWalletMutation create) return PolicyDecision.Allow(Name);

        var ctx = create.Context;
        if (string.IsNullOrWhiteSpace(ctx.ActorId))
            return PolicyDecision.Deny("Actor is required.", Name);

        var ownerIds = create.Input.Owners.Select(o => o.OwnerId).ToList();
        if (ownerIds.Count != ownerIds.Distinct().Count())
            return PolicyDecision.Deny("Duplicate owner ids are not allowed.", Name);

        return !ownerIds.Contains(ctx.ActorId)
            ? PolicyDecision.Deny("Actor must be one of wallet owners.", Name)
            : PolicyDecision.Allow(Name, reason: $"Wallet creation allowed by policy for actor {ctx.ActorId}");
    }
}