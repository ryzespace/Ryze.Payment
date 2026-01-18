using Mapster;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Domain.Features.Shared.Enum;
using RyzeSpace.Wallet.Contracts.V1;
using WalletTypeDomain = Ryze.Domain.Features.Shared.Enum.WalletType;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

public static class WalletRequestMapping
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<CreateWalletRequest, WalletCreationContext>
            .NewConfig()
            .Map(dest => dest.CurrencyCode, src => (Currency)src.Currency)
            .Map(dest => dest.WalletType, src => Enum.Parse<WalletTypeDomain>(src.WalletType, true))
            .Map(dest => dest.Owners, src => src.Owners.Adapt<List<WalletOwnerContext>>())
            .Map(dest => dest.Metadata, src => src.Metadata)
            .Map(dest => dest.Tags, src => src.Tags);
    }
}
