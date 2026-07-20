using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.Walllet.DTO.Response;

namespace Ryze.Application.Features.Walllet.Interfaces;

/// <summary>
/// Read-only application service exposing wallet query operations.
/// The execution context (tenant, identity, permissions, etc.) is
/// resolved from <see cref="IContextAccessor{TContext}"/> and is expected to
/// be established by the caller before invoking any method.
/// </summary>
public interface IReadWalletService
{
    /// <summary>
    /// Retrieves a single wallet identified by the current
    /// <c>WalletGetContext</c>.
    /// </summary>
    /// <remarks>
    /// Depending on the active query context, the returned wallet may
    /// include its current balance together with associated metadata.
    /// Throws if the requested wallet cannot be found or the caller
    /// is not authorized to access it.
    /// </remarks>
    /// <returns>
    /// A wallet representation containing the requested wallet data.
    /// </returns>
    Task<GetWalletResponseDto> GetWallet();

    /// <summary>
    /// Returns a paginated collection of wallets matching the current
    /// <c>WalletListContext</c>.
    /// </summary>
    /// <remarks>
    /// Supports optional filtering by wallet status, type, owner,
    /// tags, and creation date range. Pagination is cursor-based to
    /// provide stable iteration over large datasets.
    /// </remarks>
    /// <returns>
    /// A page of wallets together with pagination metadata.
    /// </returns>
    Task<ListWalletsResponseDto> ListWallets();
}