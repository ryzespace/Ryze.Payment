using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.Walllet.Contexts.Getters;
using Ryze.Application.Features.Walllet.DTO.Response;
using Ryze.Application.Features.Walllet.Interfaces;
using Ryze.Application.Features.Walllet.UseCase.Queries.Requests;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Application.Features.Walllet.UseCase.Queries.Handlers;

/// <summary>
/// Handles wallet read queries for a single wallet.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Executes the ambient request context from <see cref="GetWalletQueries.Request"/>.</item>
/// <item>Executes the wallet query context from <see cref="GetWalletQueries.Context"/>.</item>
/// <item>Delegates wallet retrieval to <see cref="IReadWalletService.GetWallet"/>.</item>
/// </list>
/// </remarks>
public class GetWalletQueriesHandler(
    IReadWalletService walletService,
    IContextManager<RequestContext> requestContextManager,
    IContextManager<WalletGetContext> walletGetContextManager
)
{
    /// <summary>
    /// Processes a <see cref="GetWalletQueries"/> request and returns wallet details.
    /// </summary>
    /// <param name="command">Query command containing request and wallet-get contexts.</param>
    /// <returns>Wallet response DTO for the requested wallet.</returns>
    public async Task<GetWalletResponseDto> Handle(GetWalletQueries command)
    {
        await requestContextManager.ExecuteInContext(
            command.Request,
            async () => await Task.CompletedTask
        );

        return await walletGetContextManager.ExecuteInContext(
            command.Context,
            async () => await walletService.GetWallet()
        );
    }
}
