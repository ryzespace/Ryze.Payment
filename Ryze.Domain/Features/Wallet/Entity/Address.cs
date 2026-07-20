using Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.AdressVo;

namespace Ryze.Domain.Features.Wallet.Entity;

/// <summary>
/// Domain entity representing postal address information assigned to a wallet owner.
/// </summary>
/// <remarks>
/// Address values are stored as strongly-typed value objects to preserve domain invariants.
/// </remarks>
public sealed class Address
{
    /// <summary>
    /// Primary street line.
    /// </summary>
    public Street Street { get; private set; } = null!;

    /// <summary>
    /// City or locality.
    /// </summary>
    public City City { get; private set; } = null!;

    /// <summary>
    /// Postal or ZIP code.
    /// </summary>
    public PostalCode PostalCode { get; private set; } = null!;

    /// <summary>
    /// Country designation.
    /// </summary>
    public Country Country { get; private set; } = null!;

    /// <summary>
    /// Optional state/region/province.
    /// </summary>
    public State? State { get; private set; }

    /// <summary>
    /// Optional second street line.
    /// </summary>
    public StreetTwo? StreetTwo { get; private set; }

    /// <summary>
    /// Creates a new address entity from strongly-typed value objects.
    /// </summary>
    /// <param name="street">Primary street line.</param>
    /// <param name="streetTwo">Optional second street line.</param>
    /// <param name="city">City or locality.</param>
    /// <param name="postalCode">Postal or ZIP code.</param>
    /// <param name="country">Country designation.</param>
    /// <param name="state">Optional state/region/province.</param>
    /// <returns>New initialized address entity.</returns>
    public static Address Create(
        Street street,
        StreetTwo? streetTwo,
        City city,
        PostalCode postalCode,
        Country country,
        State? state)
    {
        return new Address
        {
            Street = street,
            StreetTwo = streetTwo,
            City = city,
            PostalCode = postalCode,
            Country = country,
            State = state
        };
    }
}
