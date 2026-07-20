using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Application.Features.Walllet.Interfaces;
using Ryze.Application.Features.Walllet.UseCase.Commands.Requests;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Application.Features.Walllet.UseCase.Commands.Handlers;

/// <summary>
/// Handles wallet suspension commands.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Executes the ambient request context from <see cref="SuspendWalletCommand.Request"/>.</item>
/// <item>Executes the wallet suspension context from <see cref="SuspendWalletCommand.SuspendContext"/>.</item>
/// <item>Delegates wallet suspension to <see cref="IWriteWalletService.SuspendWallet"/>.</item>
/// </list>
/// </remarks>
public class SuspendWalletCommandHandler(
    IWriteWalletService walletService,
    IContextManager<RequestContext> requestContext,
    IContextManager<WalletSuspendContext> suspendContext
)
{
    /// <summary>
    /// Processes a <see cref="SuspendWalletCommand"/> and suspends the target wallet.
    /// </summary>
    /// <param name="command">Command containing request and wallet-suspension contexts.</param>
    /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
    public async ValueTask Handle(SuspendWalletCommand command, CancellationToken cancellationToken)
    {
        await requestContext.ExecuteInContext(
            command.Request,
            async () =>
        {
            await Task.CompletedTask;
        });

        await suspendContext.ExecuteInContext(
            command.SuspendContext, async () => await walletService.SuspendWallet());
    }
}
