using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.WalletBalance.Interfaces;
using Ryze.Application.Features.Walllet.Contexts.Getters;
using Ryze.Application.Features.Walllet.DTO;
using Ryze.Application.Features.Walllet.DTO.Response;
using Ryze.Application.Features.Walllet.Helpers;
using Ryze.Application.Features.Walllet.Interfaces;
using Ryze.Application.Features.Walllet.Mapping;
using Ryze.Application.Shared.Pagination;
using Ryze.Domain.Features.Wallet.Entity;
using Ryze.Domain.Features.Wallet.Repositories;

namespace Ryze.Application.Features.Walllet.Service;

/// <summary>
/// Read-only application service providing wallet query operations.
/// Request specific query contexts are obtained from
/// <see cref="IContextAccessor{T}"/> and are expected to be established
/// by the caller before invoking any method.
/// </summary>
/// <remarks>
/// This service is responsible for retrieving wallet aggregates,
/// projecting them into API DTOs, enriching them with balance data
/// when requested, and applying filtering and cursor-based pagination
/// for collection queries.
/// </remarks>
public sealed class ReadWalletService(
    IWalletRepository walletRepository,
    IPageTokenPagination pagination,
    IWalletBalanceProjectionReader balanceReader,
    IContextAccessor<WalletGetContext> getContextAccessor,
    IContextAccessor<WalletListContext> listContextAccessor) : IReadWalletService
{
    /// <summary>
    /// Retrieves a single wallet using the active wallet query context.
    /// </summary>
    /// <remarks>
    /// The wallet identifier and projection options are resolved from
    /// <see cref="WalletGetContext"/>.
    /// 
    /// The returned DTO may include additional data such as the current balance
    /// depending on the requested context flags. Balance information is loaded
    /// separately through <see cref="IWalletBalanceProjectionReader"/> to keep
    /// wallet aggregate loading independent of external projections.
    /// </remarks>
    /// <returns>
    /// A response containing the requested wallet representation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no <see cref="WalletGetContext"/> is active or when the wallet
    /// cannot be found.
    ///</exception>
    public async Task<GetWalletResponseDto> GetWallet()
    {
        var context = getContextAccessor.Current
            ?? throw new InvalidOperationException("WalletGetContext is not active.");

        var wallet = await GetWalletOrThrow(context.WalletId);

        var dto = WalletQueryMapping.ToDto(wallet, context);

        if (context.IncludeBalance)
            dto.Balance = await balanceReader.GetBalanceAsync(wallet.Id);

        return new GetWalletResponseDto { Wallet = dto };
    }

    /// <summary>
    /// Retrieves a paginated collection of wallets matching the active list query context.
    /// </summary>
    /// <remarks>
    /// The operation loads available wallets, applies context based filtering,
    /// projects domain entities into DTO representations, enriches results with
    /// current balances, and finally applies cursor-based pagination.
    ///
    /// Pagination is performed after filtering and projection to ensure the
    /// returned page contains stable results matching the requested criteria.
    /// </remarks>
    /// <returns>
    /// A paginated wallet collection together with the next page cursor and total count.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no <see cref="WalletListContext"/> is active.
    /// </exception>
    public async Task<ListWalletsResponseDto> ListWallets()
    {
        var context = listContextAccessor.Current
            ?? throw new InvalidOperationException("WalletListContext is not active.");

        var wallets = await walletRepository.ListAsync();
        var filteredWallets = wallets.Where(w => WalletListFilterHelper.Matches(w, context)).ToList();
        var projected = new List<WalletDto>(filteredWallets.Count);
        foreach (var wallet in filteredWallets)
        {
            var dto = WalletQueryMapping.ToDto(wallet, includeOwners: true, includeBalance: true);
            dto.Balance = await balanceReader.GetBalanceAsync(wallet.Id);
            projected.Add(dto);
        }

        var page = pagination.Apply(projected, context.PageSize, context.PageToken);
        return new ListWalletsResponseDto
        {
            Wallets = [.. page.Items],
            NextPageToken = page.NextPageToken ?? string.Empty,
            TotalCount = page.TotalCount
        };
    }

    private async Task<Wallet> GetWalletOrThrow(Guid walletId) =>
        await walletRepository.GetByIdAsync(walletId) ??
            throw new InvalidOperationException($"Wallet with ID {walletId} not found.");
}
