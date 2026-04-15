using Ryze.Domain.Features.Wallet.Entity;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Domain.Features.Wallet.ValueObject;
using Ryze.Domain.Features.WalletBalance.Factory;

namespace Ryze.Application.Features.Walllet.Factory;

/// <summary>
/// Factory responsible for constructing wallet aggregates from the creation context.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Creates owner entities from <see cref="WalletCreationContext.Owners"/> entries.</item>
/// <item>Generates account and routing numbers via <see cref="WalletNumberGenerator"/>.</item>
/// <item>Initializes wallet metadata, tags, and zero balance in the requested currency.</item>
/// </list>
/// </remarks>
public static class WalletFactory
{
    /// <summary>
    /// Creates a new <see cref="Wallet"/> aggregate from wallet creation context.
    /// </summary>
    /// <param name="context">Source context describing a wallet type, currency, owners, metadata, and tags.</param>
    /// <returns>Newly constructed wallet aggregate with initialized balance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public static Wallet Create(WalletCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var owners = context.Owners
            .Select(WalletOwnerFactory.Create)
            .ToList();

        var (accountNumber, routingNumber) = WalletNumberGenerator.Generate();

        var wallet = new Wallet(
            id: Guid.NewGuid(),
            type: context.WalletType,
            currency: context.CurrencyCode,
            owners: owners,
            metadata: new Metadata(context.Metadata),
            accountNumber: accountNumber,
            routingNumber: routingNumber,
            tags: new Tags(context.Tags)
        );

        wallet.Balance = WalletBalanceFactory.CreateZeroBalance(context.CurrencyCode);
        return wallet;
    }
}
