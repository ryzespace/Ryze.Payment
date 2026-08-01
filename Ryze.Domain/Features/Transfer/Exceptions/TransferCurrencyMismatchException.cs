namespace Ryze.Domain.Features.Transfer.Exceptions;

/// <summary>
/// Thrown when source and destination wallets use incompatible currencies.
/// </summary>
/// <remarks>
/// Currency consistency is required before transfer can be executed.
/// This exception represents domain validation failure and prevents
/// processing transfers that would require unsupported implicit conversion.
/// </remarks>
/// <param name="from">Currency code of the source wallet. </param>
/// <param name="to">Currency code of the destination wallet. </param>
public sealed class TransferCurrencyMismatchException(
    string from,
    string to)
    : TransferException(
        $"Currency mismatch: {from} != {to}.",
        "CURRENCY_MISMATCH");