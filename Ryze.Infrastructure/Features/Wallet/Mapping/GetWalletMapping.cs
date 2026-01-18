using Mapster;
using Ryze.Application.Features.Walllet.Contexts.Getters;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

/// <summary>
/// Configures Mapster mappings for <see cref="GetWalletRequest"/> to <see cref="WalletGetContext"/>.
/// </summary>
/// <remarks>
/// Ensures proper conversion of gRPC request types into domain context objects.
/// </remarks>
public static class GetWalletMapping
{
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
