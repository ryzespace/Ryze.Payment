using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.Walllet.Contexts.Getters;
using Ryze.Application.Features.Walllet.DTO.Response;
using Ryze.Application.Features.Walllet.Interfaces;
using Ryze.Application.Features.Walllet.UseCase.Queries.Requests;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Application.Features.Walllet.UseCase.Queries.Handlers;

/// <summary>
/// Handles wallet listing queries.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Executes the ambient request context from <see cref="ListWalletsQueries.Request"/>.</item>
/// <item>Executes the wallet list context from <see cref="ListWalletsQueries.Context"/>.</item>
/// <item>Delegates wallet listing to <see cref="IReadWalletService.ListWallets"/>.</item>
/// </list>
/// </remarks>
public class ListWalletQueriesHandler(
    IReadWalletService walletService,
    IContextManager<RequestContext> requestContextManager,
    IContextManager<WalletListContext> walletListContextManager
)
{
    /// <summary>
    /// Processes a <see cref="ListWalletsQueries"/> request and returns paged wallets.
    /// </summary>
    /// <param name="command">Query command containing request and wallet-list contexts.</param>
    /// <returns>Paged wallet response DTO.</returns>
    public async Task<ListWalletsResponseDto> Handle(ListWalletsQueries command)
    {
        await requestContextManager.ExecuteInContext(
            command.Request,
            async () => await Task.CompletedTask
        );

        return await walletListContextManager.ExecuteInContext(
            command.Context,
            async () => await walletService.ListWallets()
        );
    }
}
