using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Walllet.Contexts;

/// <summary>
/// Execution context for wallet close operations.
/// </summary>
/// <remarks>
/// Carries the information required to perform a wallet close workflow.
/// The context is created by the application layer and propagated through
///
/// The context represents a single close request and contains the target
/// wallet identifier together with the business reason that triggered the
/// operation.
/// </remarks>
public sealed class WalletCloseContext : IContext
{
    /// <summary>
    /// Unique identifier of this context instance.
    /// </summary>
    /// <remarks>
    /// Generated when the context is created and can be used for tracing,
    /// diagnostics, and correlation across the application pipeline.
    /// </remarks>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Identifier of the wallet that should be closed.
    /// </summary>
    public Guid WalletId { get; init; }

    /// <summary>
    /// Business reason explaining why the wallet close operation was requested.
    /// </summary>
    public string Reason { get; init; } = null!;

    /// <summary>
    /// UTC timestamp when this context instance was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}