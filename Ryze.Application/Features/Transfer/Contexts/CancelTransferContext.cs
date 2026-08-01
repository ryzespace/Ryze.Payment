namespace Ryze.Application.Features.Transfer.Contexts;

/// <summary>
/// Represents execution context for a transfer cancellation operation.
/// </summary>
/// <remarks>
/// Contains the data required by the mutation pipeline to cancel an existing
/// transfer.
///
/// The context provides operation metadata used for tracing and auditing while
/// carrying the target transfer identifier and cancellation reason required by
/// the business workflow.
///
/// Instances are immutable after initialization to ensure consistent state
/// throughout the mutation execution lifecycle.
/// </remarks>
public sealed class CancelTransferContext : ITransferContext
{
    /// <summary>
    /// Gets the unique identifier of the operation context.
    /// </summary>
    /// <remarks>
    /// Generated automatically and used as correlation identifier for
    /// tracing, logging, and audit records.
    /// </remarks>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the timestamp when this context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the identifier of the transfer being canceled.
    /// </summary>
    public required Guid TransferId { get; init; }

    /// <summary>
    /// Gets the business reason for cancelling the transfer.
    /// </summary>
    /// <remarks>
    /// The reason is preserved for auditability and operational diagnostics.
    /// </remarks>
    public required string Reason { get; init; }

    /// <summary>
    /// Returns readable representation of the cancellation context.
    /// </summary>
    public override string ToString()
        => $"CancelTransferContext {{ Id = {Id}, TransferId = {TransferId}, Reason = {Reason} }}";
}