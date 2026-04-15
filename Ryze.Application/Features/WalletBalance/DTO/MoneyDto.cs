using Ryze.Domain.Features.Shared.Enum;

namespace Ryze.Application.Features.WalletBalance.DTO;

/// <summary>
/// Data transfer object representing a monetary value.
/// </summary>
/// <remarks>
/// Combines a numeric <see cref="Amount"/> with its <see cref="Currency"/> and the number of fractional digits
/// specified by <see cref="Precision"/>.
/// </remarks>
public sealed class MoneyDto
{
    /// <summary>
    /// The numeric amount for the money value.
    /// </summary>
    /// <remarks>
    /// Interpreted together with <see cref="Precision"/>; for example, an amount of 10.50 with a precision of 2
    /// represents ten currency units and fifty minor units.
    /// </remarks>
    public decimal Amount { get; init; }

    /// <summary>
    /// The currency associated with the <see cref="Amount"/>.
    /// </summary>
    public Currency Currency { get; init; }

    /// <summary>
    /// The number of fractional digits used to represent the <see cref="Amount"/> for the specified <see cref="Currency"/>.
    /// </summary>
    /// <example>
    /// USD typically uses a precision of 2, while JPY uses 0.
    /// </example>
    public int Precision { get; init; }
}