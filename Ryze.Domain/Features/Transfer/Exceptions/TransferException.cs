namespace Ryze.Domain.Features.Transfer.Exceptions;

/// <summary>
/// Base exception for transfer domain failures.
/// </summary>
/// <remarks>
/// Represents business rule violations that occur during transfer processing.
/// Unlike infrastructure or system exceptions, transfer exceptions are expected
/// domain failures and contain stable error code.
/// </remarks>
/// <param name="message">Description of the failure. </param>
/// <param name="code">Stable machine-readable error code used for integration contracts. </param>
public abstract class TransferException(
    string message,
    string code) : Exception(message)
{
    /// <summary>
    /// Gets the stable domain error code associated with this failure.
    /// </summary>
    /// <remarks>
    /// The code should remain backward compatible because external consumers
    /// may depend on it for error handling and localization.
    /// </remarks>
    public string Code { get; } = code;
}