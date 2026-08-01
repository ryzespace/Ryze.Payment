namespace Ryze.Domain.Features.Transfer.Entity;

/// <summary>
/// Represents single failure encountered while processing transfer batch.
/// </summary>
/// <remarks>
/// Associates an error with the original batch item index and, when available,
/// the identifier of the transfer involved. Intended for reporting partial
/// failures during bulk transfer execution.
/// </remarks>
public sealed record BatchError
{
    /// <summary>
    /// Zero based index of the failed batch item.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// Human-readable description of the failure.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Identifier of the associated transfer, when one was created before
    /// the failure occurred.
    /// </summary>
    public Guid? TransferId { get; init; }
}