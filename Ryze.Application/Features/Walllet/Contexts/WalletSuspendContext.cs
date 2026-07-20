using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Walllet.Contexts;

/// <summary>
/// Write context describing a wallet suspension request.
/// </summary>
/// <remarks>
/// Represents the command intent required to transition a wallet from an
/// active state into a suspended state.
///
/// The context is created at the application boundary and propagated through
/// the wallet mutation pipeline. It contains the target wallet identifier and
/// the business reason required for auditability, validation, and operational
/// diagnostics.
/// </remarks>
public sealed class WalletSuspendContext : IContext
{
    /// <summary>
    /// Unique identifier of this context instance.
    /// </summary>
    /// <remarks>
    /// Used for correlation, tracing, and diagnostics across the suspension
    /// workflow.
    /// </remarks>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Identifier of the wallet that should be suspended.
    /// </summary>
    /// <remarks>
    /// References the wallet aggregate affected by this state transition.
    /// </remarks>
    public Guid WalletId { get; init; }

    /// <summary>
    /// Business reason explaining why the wallet suspension was requested.
    /// </summary>
    /// <remarks>
    /// Required for audit trails and provides additional context for
    /// reviewing wallet lifecycle changes.
    /// </remarks>
    public string Reason { get; init; } = null!;

    /// <summary>
    /// UTC timestamp when this context instance was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}