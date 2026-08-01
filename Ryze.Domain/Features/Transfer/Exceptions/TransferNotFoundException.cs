namespace Ryze.Domain.Features.Transfer.Exceptions;

/// <summary>
/// Thrown when requested transfer cannot be found.
/// </summary>
/// <remarks>
/// Used when an operation requires an existing transfer aggregate,
/// but no matching transfer identifier exists in the domain store.
/// </remarks>
public sealed class TransferNotFoundException(Guid transferId)
    : TransferException(
        $"Transfer {transferId} not found.",
        "TRANSFER_NOT_FOUND");