using Ryze.Application.Features.Walllet.DTO;
using Ryze.Domain.Features.Wallet.Entity;

namespace Ryze.Application.Features.Walllet.Factory;

/// <summary>
/// Factory responsible for translating <see cref="AddressDto"/> into domain <see cref="Address"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Acts as an anti-corruption layer between transport DTOs and domain value objects.</item>
/// <item>Centralizes address construction rules used by wallet-owner creation flows.</item>
/// <item>Delegates final object creation to <see cref="Address.Create"/>.</item>
/// </list>
/// </remarks>
public static class AddressFactory
{
    /// <summary>
    /// Creates a domain <see cref="Address"/> instance from a DTO payload.
    /// </summary>
    /// <param name="dto">Source address DTO containing raw request values.</param>
    /// <returns>Fully initialized domain address instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="dto"/> is <see langword="null"/>.
    /// </exception>
    public static Address Create(AddressDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return Address.Create(
            street: dto.Street,
            streetTwo: dto.StreetTwo!,
            city: dto.City,
            postalCode: dto.PostalCode,
            country: dto.Country,
            state: dto.State!);
    }
}
