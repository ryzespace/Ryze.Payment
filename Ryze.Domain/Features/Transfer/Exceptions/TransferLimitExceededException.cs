namespace Ryze.Domain.Features.Transfer.Exceptions;

/// <summary>
/// Thrown when transfer amount exceeds the configured transaction limit.
/// </summary>
/// <remarks>
/// Used to enforce transfer risk constraints before the operation enters
/// processing state.
/// </remarks>
public sealed class TransferLimitExceededException(
    decimal amount,
    decimal maxAmount)
    : TransferException(
        $"Transfer amount {amount} exceeds limit {maxAmount}.",
        "LIMIT_EXCEEDED");