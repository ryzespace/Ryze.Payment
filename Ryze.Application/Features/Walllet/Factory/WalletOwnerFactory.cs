using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Domain.Features.Wallet.Entity;

namespace Ryze.Application.Features.Walllet.Factory;

/// <summary>
/// Factory responsible for creating domain <see cref="WalletOwner"/> entities
/// from application <see cref="WalletOwnerContext"/> inputs.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Converts raw context values into domain-friendly primitives and value objects.</item>
/// <item>Builds nested aggregates such as <see cref="Address"/> through dedicated factories.</item>
/// <item>Applies default metadata such as owner creation timestamp.</item>
/// </list>
/// </remarks>
public static class WalletOwnerFactory
{
    /// <summary>
    /// Creates a <see cref="WalletOwner"/> from wallet-owner context data.
    /// </summary>
    /// <param name="context">Source context containing owner identity and profile details.</param>
    /// <returns>Initialized wallet-owner entity ready to be attached to a wallet aggregate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public static WalletOwner Create(WalletOwnerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new()
        {
            OwnerId = context.OwnerId,
            DisplayName = context.DisplayName,
            Role = context.Role,
            Address = AddressFactory.Create(context.Address),
            DateOfBirth = context.DateOfBirth,
            NationalId = context.NationalId!,
            AddedAt = DateTime.UtcNow,
            Metadata = new(context.Metadata)
        };
    }
}
