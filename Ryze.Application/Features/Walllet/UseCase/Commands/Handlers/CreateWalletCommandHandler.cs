using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Application.Features.Walllet.DTO.Response;
using Ryze.Application.Features.Walllet.Interfaces;
using Ryze.Application.Features.Walllet.UseCase.Commands.Requests;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Application.Features.Walllet.UseCase.Commands.Handlers;

/// <summary>
/// Handles wallet creation commands.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Executes the ambient request context from <see cref="CreateWalletCommand.Request"/>.</item>
/// <item>Executes the wallet creation context from <see cref="CreateWalletCommand.Context"/>.</item>
/// <item>Delegates wallet creation to <see cref="IWriteWalletService.CreateWallet"/>.</item>
/// </list>
/// </remarks>
public class CreateWalletCommandHandler(
    IWriteWalletService walletService,
    IContextManager<RequestContext> requestContext,
    IContextManager<WalletCreationContext> walletCreationContext
)
{
    /// <summary>
    /// Processes a <see cref="CreateWalletCommand"/> and creates a new wallet.
    /// </summary>
    /// <param name="command">Command containing request and wallet-creation contexts.</param>
    /// <returns>Response DTO with created wallet details.</returns>
    public async Task<CreateWalletResponseDto> Handle(CreateWalletCommand command)
    {
        await requestContext.ExecuteInContext(
            command.Request,
            async () =>
        {
            // ContextLogger.Log(command.Request);
            await Task.CompletedTask;
        });

        var wallet = await walletCreationContext.ExecuteInContext(
            command.Context,
            async () => await walletService.CreateWallet()
        );

        return wallet;
    }
}
