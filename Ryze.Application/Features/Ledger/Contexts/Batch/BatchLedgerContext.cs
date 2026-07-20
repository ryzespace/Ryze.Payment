using ModularityKit.Context.Abstractions;
using Ryze.Domain.Features.Ledger.ValueObject;

namespace Ryze.Application.Features.Ledger.Contexts.Batch;

/// <summary>
/// Write context for batch ledger operations.
/// </summary>
/// <remarks>
/// Represents immutable command intent describing a collection of
/// ledger operations that should be processed as a single logical batch.
///
/// The context carries the operation set together with execution metadata,
/// including the initiating actor, an optional business description, and
/// an optional idempotency key override for the entire batch.
/// </remarks>
public sealed class BatchLedgerContext : IContext
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Collection of ledger operations included in the batch.
    /// </summary>
    /// <remarks>
    /// The execution order of items is preserved and may be significant
    /// depending on the batch processing strategy.
    /// </remarks>
    public required IReadOnlyList<BatchOperationItem> Items { get; init; }

    /// <summary>
    /// Identifier of the actor initiating the batch.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <summary>
    /// business description explaining the purpose of the batch.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional idempotency key applied to the entire batch operation.
    /// </summary>
    public string? IdempotencyKeyOverride { get; init; }
}