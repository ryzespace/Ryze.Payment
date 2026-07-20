using Mapster;
using Ryze.Application.Features.Walllet.DTO;
using AddressGrpc = RyzeSpace.Wallet.Contracts.V1.Address;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

/// <summary>
/// Configures Mapster mappings for core wallet shared types.
/// </summary>
/// <remarks>
/// Provides base-level type conversions used across the Wallet module,
/// including date/time normalization and address transformation between:
/// - gRPC contracts<br/>
/// - application DTOs<br/><br/>
///
/// These mappings are considered foundational and are reused by higher-level
/// wallet mapping profiles.
/// </remarks>
public static class WalletMapping
{
    /// <summary>
    /// Registers core Mapster configurations for wallet-related shared types.
    /// </summary>
    /// <remarks>
    /// Includes:
    /// - Conversion from <see cref="DateTimeOffset"/> to <see cref="DateTime"/> using UTC normalization<br/>
    /// - Mapping between <see cref="AddressGrpc"/> and <see cref="AddressDto"/><br/><br/>
    ///
    /// These mappings are global and affect all Mapster transformations
    /// within the application lifetime once registered.
    /// </remarks>
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