using Ryze.Application.Features.Walllet.Contexts.Getters;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Application.Features.Walllet.UseCase.Queries.Requests;

/// <summary>
/// Query representing a request to retrieve a single wallet.
/// </summary>
/// <param name="Request">Ambient request metadata and correlation context.</param>
/// <param name="Context">Domain context containing wallet retrieval criteria.</param>
public sealed record GetWalletQueries (
    RequestContext Request,
    WalletGetContext Context
);
