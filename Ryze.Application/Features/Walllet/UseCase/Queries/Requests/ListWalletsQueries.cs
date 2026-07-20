using Ryze.Application.Features.Walllet.Contexts.Getters;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Application.Features.Walllet.UseCase.Queries.Requests;

/// <summary>
/// Query representing a request to list wallets.
/// </summary>
/// <param name="Request">Ambient request metadata and correlation context.</param>
/// <param name="Context">Domain context containing wallet list filters and pagination.</param>
public sealed record ListWalletsQueries(
    RequestContext Request,
    WalletListContext Context
);
