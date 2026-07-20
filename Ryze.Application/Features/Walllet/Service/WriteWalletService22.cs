using Microsoft.Extensions.Logging;
using ModularityKit.Context.Abstractions;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Ledger.Interfaces;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Application.Features.Walllet.DTO.Response;
using Ryze.Application.Features.Walllet.Interfaces;
using Ryze.Application.Features.Walllet.Mutations.Creation;
using Ryze.Application.Features.Walllet.Mutations.State;
using Ryze.Application.Shared.Helpers;
using Ryze.Domain.Features.Wallet.Entity;
using Ryze.Domain.Features.Wallet.Repositories;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Walllet.Service;

/// <summary>
/// Application service responsible for wallet state mutations.
/// </summary>
/// <remarks>
/// All write operations are executed through the mutation engine pipeline,
/// which provides validation, state transitions, and mutation lifecycle handling.
///
/// The service does not directly modify the wallet state. Instead, it creates
/// domain mutations and commits their results through the appropriate
/// persistence services.
///
/// Operation context is resolved from <see cref="IContextAccessor{T}"/> and
/// must be established by the caller before invoking any method using
/// <c>contextManager.ExecuteInContext(ctx, ...)</c>.
///
/// Wallet creation additionally coordinates with the ledger subsystem to ensure
/// that the initial wallet state and its corresponding accounting transaction are
/// persisted together.
/// </remarks>
public sealed class WriteWalletService22(
    IMutationEngine engine,
    IWalletRepository walletRepository,
    ILedgerCommandService ledgerService,
    IContextAccessor<WalletCreationContext> creationContextAccessor,
    IContextAccessor<WalletSuspendContext> suspendContextAccessor,
    IContextAccessor<WalletReactivateContext> reactivateContextAccessor,
    ILogger<WriteWalletService> logger) : IWriteWalletService
{
    private const MutationMode Mode = MutationMode.Commit;

    /// <summary>
    /// Creates a new wallet using the active wallet creation context.
    /// </summary>
    /// <remarks>
    /// Executes <see cref="CreateWalletMutation"/> through the mutation engine,
    /// validates the generated state, and commits the wallet aggregate.
    ///
    /// When the mutation produces an initial ledger transaction, it is recorded
    /// through <see cref="ILedgerCommandService"/> as part of the commit workflow.
    /// This keeps wallet creation and accounting state synchronized.
    /// </remarks>
    /// <returns>
    /// Information about the newly created wallet, including its identifier,
    /// initial status, and creation timestamp.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the creation context is missing, the mutation fails validation,
    /// or the mutation engine does not return a resulting wallet state.
    /// </exception>
    public async Task<CreateWalletResponseDto> CreateWallet()
    {
        var context = creationContextAccessor.Current
            ?? throw new InvalidOperationException("WalletCreationContext is not active.");

        logger.LogDebug("Starting CreateWallet process for context: {ContextId}", context.Id);
        var actor = context.Owners.FirstOrDefault();

        var userId = actor?.OwnerId ?? "anonymous";
        var reason = "User requested wallet creation";

        var mutationContext = MutationContext.User(
            userId: userId,
            userName: actor?.DisplayName ?? "anonymous",
            reason: reason
        ) with { Mode = Mode };

        var mutation = new CreateWalletMutation(creationContextAccessor, mutationContext);

        var result = await engine.ExecuteAsync(mutation, null!);
        EnsureMutationSucceeded(result, "wallet.create");

        var wallet = result.NewState;
        if (wallet is null)
            throw new InvalidOperationException("Mutation engine returned empty wallet state.");

        await MutationCommitHelper.PersistIfCommit(mutation, async () =>
        {
            await walletRepository.AddAsync(wallet);

            if (mutation.LedgerTransaction is { } ledgerTx)
                await ledgerService.RecordTransactionAsync(ledgerTx);
        });

        var response = new CreateWalletResponseDto
        {
            WalletId = wallet.Id,
            Status = WalletStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        return response;
    }

    /// <summary>
    /// Reactivates a suspended wallet.
    /// </summary>
    /// <remarks>
    /// Loads the wallet aggregate identified by the active
    /// <see cref="WalletReactivateContext"/>, executes
    /// <see cref="ReactivateWalletMutation"/>, and persists the resulting state
    /// after successful mutation validation.
    ///
    /// The operation is executed as a system mutation using the reason supplied
    /// by the current context.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the context is missing, the wallet does not exist,
    /// or the mutation validation fails.
    /// </exception>
    public async Task ReactivateWallet()
    {
        var context = reactivateContextAccessor.Current
            ?? throw new InvalidOperationException("WalletReactivateContext is not active.");

        var wallet = await GetWalletOrThrow(context.WalletId);

        var mutationContext = MutationContext.System(reason: context.Reason);
        var mutation = new ReactivateWalletMutation(reactivateContextAccessor, mutationContext);
        var result = await engine.ExecuteAsync(mutation, wallet);
        EnsureMutationSucceeded(result, "wallet.reactivate");

        await MutationCommitHelper.PersistIfCommit(mutation, () => walletRepository.UpdateAsync(wallet));
    }

    /// <summary>
    /// Suspends an active wallet.
    /// </summary>
    /// <remarks>
    /// Loads the wallet aggregate identified by the active
    /// <see cref="WalletSuspendContext"/> and applies
    /// <see cref="SuspendWalletMutation"/> through the mutation engine.
    ///
    /// A suspended wallet remains persisted with its historical state intact,
    /// but cannot participate in operations restricted to active wallets.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the context is missing, the wallet does not exist,
    /// or the mutation validation fails.
    /// </exception>
    public async Task SuspendWallet()
    {
        var context = suspendContextAccessor.Current
            ?? throw new InvalidOperationException("WalletSuspendContext is not active.");

        var wallet = await GetWalletOrThrow(context.WalletId);

        var mutationContext = MutationContext.System(reason: context.Reason);
        var mutation = new SuspendWalletMutation(suspendContextAccessor, mutationContext);
        var result = await engine.ExecuteAsync(mutation, wallet);
        EnsureMutationSucceeded(result, "wallet.suspend");

        await MutationCommitHelper.PersistIfCommit(mutation, () => walletRepository.UpdateAsync(wallet));
    }

    /// <summary>
    /// Loads a wallet entity by its identifier, throwing when the wallet does not exist.
    /// </summary>
    private async Task<Wallet> GetWalletOrThrow(Guid walletId) =>
        await walletRepository.GetByIdAsync(walletId) ??
            throw new InvalidOperationException($"Wallet with ID {walletId} not found.");

    /// <summary>
    /// Inspects the engine result and throws on failure, collecting validation messages
    /// into a single error string.
    /// </summary>
    private static void EnsureMutationSucceeded<TState>(MutationResult<TState> result, string operation)
    {
        if (result.IsSuccess)
            return;

        var errors = result.ValidationResult.Errors.Select(e => e.Message).Where(m => !string.IsNullOrWhiteSpace(m)).ToArray();
        var message = errors.Length > 0 ? string.Join(", ", errors) : "Mutation failed.";

        throw new InvalidOperationException($"{operation} failed: {message}");
    }
}
