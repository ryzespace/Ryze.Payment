using Mapster;
using Payment.Common.Grpc;
using Ryze.Application.Features.Walllet.Contexts.Getters;
using Ryze.Application.Features.Walllet.DTO.Response;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

public static class WalletListMapping
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<ListWalletsRequest, WalletListContext>
            .NewConfig()
            .Map(dest => dest.PageSize, src => src.PageSize > 0 ? src.PageSize : 50)
            .Map(dest => dest.PageToken, src => string.IsNullOrWhiteSpace(src.PageToken) ? null : src.PageToken)
            .Map(dest => dest.OwnerId, src => string.IsNullOrWhiteSpace(src.OwnerId) ? null : src.OwnerId)
            .Map(dest => dest.Status, src => src.Status == WalletStatus.Unspecified ? (WalletStatus?)null : src.Status)
            .Map(dest => dest.WalletType, src => src.WalletType == WalletType.Unspecified ? (WalletType?)null : src.WalletType)
            .Map(dest => dest.Tags, src => src.Tags)
            .Map(dest => dest.CreatedAfter,
                src => src.CreatedAfter.ToDateTimeOffset())
            .Map(dest => dest.CreatedBefore,
                src => src.CreatedBefore.ToDateTimeOffset());

        TypeAdapterConfig<ListWalletsResponseDto, ListWalletsResponse>
                 .NewConfig()
                 .AfterMapping((src, dest) =>
                 {
                     if (src.Wallets.Count > 0)
                     {
                         dest.Wallets.AddRange(
                             src.Wallets.Adapt<RyzeSpace.Wallet.Contracts.V1.Wallet[]>()
                         );
                     }

                     dest.NextPageToken = src.NextPageToken ?? string.Empty;
                     dest.TotalCount = src.TotalCount;
                 });
    }
}
