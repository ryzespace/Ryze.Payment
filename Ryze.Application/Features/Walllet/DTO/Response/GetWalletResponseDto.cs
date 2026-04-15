namespace Ryze.Application.Features.Walllet.DTO.Response;

/// <summary>
/// Response DTO for single-wallet retrieval.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Represents a read-model response for a single wallet identifier.</item>
/// <item>Encapsulates projection decisions performed upstream.</item>
/// <item>Keeps response contract stable by wrapping the wallet payload in a dedicated envelope DTO.</item>
/// </list>
/// </remarks>
public sealed class GetWalletResponseDto
{
    /// <summary>
    /// Wallet details returned by the get-wallet query.
    /// </summary>
    /// <remarks>
    /// The shape of this object depends on requested projection flags in the corresponding query context.
    /// </remarks>
    public WalletDto Wallet { get; init; } = null!;
}
