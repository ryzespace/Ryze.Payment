using Mapster;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Application.Features.Walllet.DTO;
using Ryze.Domain.Shared.Enum;
using WalletOwnerGrpc = RyzeSpace.Wallet.Contracts.V1.WalletOwner;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

/// <summary>
/// Configures Mapster mappings for wallet owner gRPC contracts.
/// </summary>
/// <remarks>
/// Maps <see cref="WalletOwnerGrpc"/> into <see cref="WalletOwnerContext"/>,
/// handling enum conversion, nested address mapping,
/// Protobuf date conversion, and metadata dictionary transformation.
/// </remarks>
public static class WalletOwnerMapping
{
    /// <summary>
    /// Registers Mapster configuration for mapping
    /// <see cref="WalletOwnerGrpc"/> to <see cref="WalletOwnerContext"/>.
    /// </summary>
    /// <remarks>
    /// - Converts gRPC role enum to domain <see cref="OwnerRole"/>.<br/>
    /// - Maps nested Address using Mapster adaptation.<br/>
    /// - Converts Protobuf <c>DateOfBirth</c> to <see cref="DateTime"/>.<br/>
    /// - Transforms a metadata map into a standard dictionary.
    /// </remarks>
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