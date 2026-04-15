using System.Security.Cryptography;

namespace Ryze.Application.Features.Walllet;

/// <summary>
/// Generates deterministic-format wallet identifiers using a cryptographically secure RNG.
/// </summary>
/// <remarks>
/// This generator produces:
/// <list type="bullet">
/// <item><description>Account number: 12-digit numeric string</description></item>
/// <item><description>Routing number: 9-digit numeric string</description></item>
/// </list>
/// Values are generated using <see cref="RandomNumberGenerator"/> to ensure cryptographic randomness.
/// </remarks>
public static class WalletNumberGenerator
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    /// <summary>
    /// Generates a new wallet account number and routing number pair.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description><c>accountNumber</c> - 12-digit string</description></item>
    /// <item><description><c>routingNumber</c> - 9-digit string</description></item>
    /// </list>
    /// </returns>
    public static (string accountNumber, string routingNumber) Generate()
    {
        var accountNumber = GenerateDigits(12);
        var routingNumber = GenerateDigits(9);

        return (accountNumber, routingNumber);
    }

    /// <summary>
    /// Generates a numeric string of the specified length using cryptographically secure random bytes.
    /// </summary>
    /// <param name="length">Desired length of the numeric string.</param>
    /// <returns>String consisting only of digits <c>0-9</c>.</returns>
    /// <remarks>
    /// Each byte is mapped to a digit using modulo 10, ensuring uniform digit distribution.
    /// </remarks>
    private static string GenerateDigits(int length)
    {
        var bytes = new byte[length];
        Rng.GetBytes(bytes);

        var digits = new char[length];

        for (var i = 0; i < length; i++)
        {
            digits[i] = (char)('0' + (bytes[i] % 10));
        }

        return new string(digits);
    }
}