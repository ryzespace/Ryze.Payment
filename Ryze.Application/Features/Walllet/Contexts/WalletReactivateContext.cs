using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Walllet.Contexts;

/// <summary>
/// Write context describing a wallet reactivation request.
/// </summary>
/// <remarks>
/// Represents the command intent required to transition a wallet from
/// a suspended state back into an active state.
///
/// The context is created at the application boundary and propagated
/// through the wallet mutation pipeline. It provides the target wallet
/// identifier together with the business reason required for auditability
/// and domain decision handling.
/// </remarks>
public sealed class WalletReactivateContext : IContext
{
    /// <summary>
    /// Unique identifier of this context instance.
    /// </summary>
    /// <remarks>
    /// Used for correlation, diagnostics, and tracing throughout the
    /// wallet reactivation workflow.
    /// </remarks>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Identifier of the wallet that should be reactivated.
    /// </summary>
    /// <remarks>
    /// Points to the wallet aggregate affected by this state transition.
    /// </remarks>
    public Guid WalletId { get; init; }

    /// <summary>
    /// Business reason explaining why the wallet reactivation was requested.
    /// </summary>
    /// <remarks>
    /// Preserved for audit trails and operational diagnostics.
    /// </remarks>
    public string Reason { get; init; }

    /// <summary>
    /// UTC timestamp when this context instance was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}