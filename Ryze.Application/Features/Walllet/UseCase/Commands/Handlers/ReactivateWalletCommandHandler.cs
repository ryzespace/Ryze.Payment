using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Application.Features.Walllet.Interfaces;
using Ryze.Application.Features.Walllet.UseCase.Commands.Requests;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Application.Features.Walllet.UseCase.Commands.Handlers;

/// <summary>
/// Handles wallet reactivation commands.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Executes the ambient request context from <see cref="ReactivateWalletCommand.Request"/>.</item>
/// <item>Executes the wallet reactivation context from <see cref="ReactivateWalletCommand.ReactivateContext"/>.</item>
/// <item>Delegates wallet reactivation to <see cref="IWriteWalletService.ReactivateWallet"/>.</item>
/// </list>
/// </remarks>
public class ReactivateWalletCommandHandler(
    IWriteWalletService walletService,
    IContextManager<RequestContext> requestContext,
    IContextManager<WalletReactivateContext> reactivateContext)
{
    /// <summary>
    /// Processes a <see cref="ReactivateWalletCommand"/> and reactivates the target wallet.
    /// </summary>
    /// <param name="command">Command containing request and wallet-reactivation contexts.</param>
    /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
    public async Task Handle(ReactivateWalletCommand command, CancellationToken cancellationToken)
    {
        await requestContext.ExecuteInContext(
            command.Request,
            async () =>
            {
                Console.WriteLine("ReactivateWalletCommand");
                await Task.CompletedTask;
            });

        await reactivateContext.ExecuteInContext(
            command.ReactivateContext, async () => await walletService.ReactivateWallet());
    }
}
