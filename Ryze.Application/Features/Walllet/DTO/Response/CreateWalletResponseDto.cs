using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Walllet.DTO.Response;

/// <summary>
/// Response DTO returned after successful wallet creation.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Represents the canonical API payload for the create-wallet use case.</item>
/// <item>Contains immutable identity data for the newly created wallet.</item>
/// <item>Includes lifecycle metadata that clients can use for optimistic UI updates.</item>
/// </list>
/// </remarks>
public class CreateWalletResponseDto
{
    /// <summary>
    /// Unique identifier of the created wallet.
    /// </summary>
    public Guid WalletId { get; init; }

    /// <summary>
    /// Current status assigned to the created wallet.
    /// </summary>
    /// <remarks>
    /// For successful creation this is expected to be the initial active status set by the domain.
    /// </remarks>
    public WalletStatus Status { get; init; }

    /// <summary>
    /// UTC timestamp when the wallet was created.
    /// </summary>
    /// <remarks>
    /// Value is produced by the application layer and is intended for response metadata, not ordering guarantees.
    /// </remarks>
    public DateTimeOffset CreatedAt { get; init; }
}
