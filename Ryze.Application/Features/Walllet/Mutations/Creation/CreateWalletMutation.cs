using ModularityKit.Context.Abstractions;
using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Application.Features.Walllet.Factory;
using Ryze.Domain.Features.Wallet.Entity;

namespace Ryze.Application.Features.Walllet.Mutations.Creation;

/// <summary>
/// Mutation responsible for creating a new wallet aggregate.
/// </summary>
/// <remarks>
/// Executes the wallet creation workflow through the mutation pipeline.
///
/// The mutation builds the initial wallet state from
/// <see cref="WalletCreationContext"/>, validates creation requirements,
/// and produces the corresponding ledger transaction required to initialize
/// the wallet accounting state.
///
/// This mutation is not reversible because wallet creation represents the
/// beginning of an aggregate lifecycle.
/// </remarks>
public class CreateWalletMutation(
    IContextAccessor<WalletCreationContext> contextAccessor,
    MutationContext mutationContext) : IMutation<Wallet>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "wallet.create",
        Category = "domain.wallet",
        Description = "Create a new wallet",
        RiskLevel = MutationRiskLevel.High,
        IsReversible = false,
        Tags = new HashSet<string> { "wallet", "create" }
    };
    
    public MutationContext Context { get; } = mutationContext;

    public WalletCreationContext Input => contextAccessor.Current
        ?? throw new InvalidOperationException("WalletCreationContext is not active.");
    
    public MutationResult<Wallet> Apply(Wallet state)
    {
        if (state != null)
            throw new InvalidOperationException("Wallet already exists.");

        var wallet = WalletFactory.Create(Input);
      
        var changes = ChangeSet.Single(
            StateChange.Added("wallet", wallet)
        );

        return MutationResult<Wallet>.Success(wallet, changes);
    }

    public ValidationResult Validate(Wallet state)
    {
        var result = ValidationResult.Success();

        if (Input.Owners.Count == 0)
        {
            result.AddError(
                path: "owners",
                message: "At least one owner is required.",
                code: "MISSING_OWNERS");
        }
        return result;
    }

    public MutationResult<Wallet> Simulate(Wallet state)
    {
        if (state != null)
            throw new InvalidOperationException("Wallet already exists.");

        var wallet = WalletFactory.Create(Input);
        var changes = ChangeSet.Single(StateChange.Added("wallet", wallet));

        return MutationResult<Wallet>.Success(wallet, changes);
    }
}
