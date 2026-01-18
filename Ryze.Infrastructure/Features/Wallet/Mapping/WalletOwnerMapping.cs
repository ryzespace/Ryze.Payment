using Mapster;
using Ryze.Application.Features.Shared.DTO;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Application.Features.Walllet.DTO;
using Ryze.Domain.Features.Shared.Enum;
using RyzeSpace.Wallet.Contracts.V1;
using WalletOwnerGrpc = RyzeSpace.Wallet.Contracts.V1.WalletOwner;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

public static class WalletOwnerMapping
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<WalletOwnerGrpc, WalletOwnerContext>
            .NewConfig()
            .Map(dest => dest.OwnerId, src => src.OwnerId)
            .Map(dest => dest.Role, src => (OwnerRole)src.Role)
            .Map(dest => dest.Address, src => src.Address.Adapt<AddressDto>())
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth.ToDateTime())
            .Map(dest => dest.NationalId, src => src.NationalId)
            .Map(dest => dest.Metadata, src => src.Metadata.ToDictionary(kv => kv.Key, kv => kv.Value));
    }
}
