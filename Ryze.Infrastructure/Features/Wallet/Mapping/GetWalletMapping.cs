using Mapster;
using Ryze.Application.Features.Walllet.Contexts.Getters;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

/// <summary>
/// Configures Mapster mappings for wallet retrieval operations.
/// </summary>
/// <remarks>
/// Maps <see cref="GetWalletRequest"/> into <see cref="WalletGetContext"/>,
/// ensuring proper identifier parsing and flag propagation
/// for optional wallet data loading.
/// </remarks>
public static class GetWalletMapping
{
    /// <summary>
    /// Registers Mapster configuration for mapping
    /// <see cref="GetWalletRequest"/> to <see cref="WalletGetContext"/>.
    /// </summary>
    /// <remarks>
    /// - Parses <c>WalletId</c> from string to <see cref="Guid"/>.<br/>
    /// - Copies optional inclusion flags for balance, owners, and limits.
    /// </remarks>
    /// <exception cref="FormatException">
    /// Thrown when <c>WalletId</c> is not a valid <see cref="Guid"/>.
    /// </exception>
    public static void RegisterMappings()
    {
        TypeAdapterConfig<GetWalletRequest, WalletGetContext>
            .NewConfig()
            .Map(dest => dest.WalletId, src => Guid.Parse(src.WalletId))
            .Map(dest => dest.IncludeBalance, src => src.IncludeBalance)
            .Map(dest => dest.IncludeOwners, src => src.IncludeOwners)
            .Map(dest => dest.IncludeLimits, src => src.IncludeLimits);
    }
}