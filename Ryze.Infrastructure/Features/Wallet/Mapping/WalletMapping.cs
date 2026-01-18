using Mapster;
using Ryze.Application.Features.Shared.DTO;
using Ryze.Application.Features.Walllet.DTO;
using AddressGrpc = RyzeSpace.Wallet.Contracts.V1.Address;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

public static class WalletMapping
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<DateTimeOffset, DateTime>.NewConfig()
            .MapWith(src => src.UtcDateTime);

        TypeAdapterConfig<AddressGrpc, AddressDto>
            .NewConfig()
            .Map(dest => dest.Street, src => src.StreetLine1)
            .Map(dest => dest.StreetTwo, src => src.StreetLine2)
            .Map(dest => dest.City, src => src.City)
            .Map(dest => dest.State, src => src.StateProvince)
            .Map(dest => dest.PostalCode, src => src.PostalCode)
            .Map(dest => dest.Country, src => src.CountryCode);
    }
}
