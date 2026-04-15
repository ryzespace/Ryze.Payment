namespace Ryze.Application.Features.Walllet.DTO;

/// <summary>
/// Data Transfer Object representing postal address.
/// </summary>
/// <remarks>
/// Certain fields are optional (e.g., second street line and state/region).
/// Values should generally be trimmed; no specific casing is enforced.
/// When supplying a country code, prefer ISO 3166-1 alpha-2 (e.g., "US", "GB") where applicable.
/// </para>
/// </remarks>
public class AddressDto
{
    /// <summary>
    /// Required first line of the street address.
    /// </summary>
    public required string Street { get; init; }

    /// <summary>
    /// Optional second line of the street address (e.g., apartment, suite, unit).
    /// </summary>
    public string? StreetTwo { get; init; } = null;

    /// <summary>
    /// Required city or locality.
    /// </summary>
    public required string City { get; init; }

    /// <summary>
    /// Optional state, province, or region.
    /// </summary>
    public string? State { get; init; }

    /// <summary>
    /// Required postal or ZIP code.
    /// </summary>
    public required string PostalCode { get; init; }

    /// <summary>
    /// Required country or region. Prefer ISO 3166-1 alpha-2 codes when available.
    /// </summary>
    public required string Country { get; init; }
}
