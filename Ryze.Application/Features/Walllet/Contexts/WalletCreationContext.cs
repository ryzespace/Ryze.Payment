using ModularityKit.Context.Abstractions;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Walllet.Contexts;

/// <summary>
/// Write context describing a wallet creation request.
/// </summary>
/// <remarks>
/// Represents the command intent required to create a new wallet.
/// The context contains all initial aggregate data required during the
/// creation workflow, including wallet classification, currency,
/// ownership information, metadata, and tags.
/// </remarks>
public sealed class WalletCreationContext : IContext
{
    /// <summary>
    /// Unique identifier of this context instance.
    /// </summary>
    /// <remarks>
    /// Used for correlation, diagnostics, and tracing across the
    /// wallet creation pipeline.
    /// </remarks>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// UTC timestamp when this creation context was initialized.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Defines the category and behavior model of the wallet being created.
    /// </summary>
    public required WalletType WalletType { get; init; }

    /// <summary>
    /// Currency in which the wallet operates.
    /// </summary>
    public required Currency CurrencyCode { get; init; }

    /// <summary>
    /// Owners assigned during wallet initialization.
    /// </summary>
    /// <remarks>
    /// Ownership information is used to establish the initial wallet
    /// ownership model before the aggregate becomes available for use.
    /// </remarks>
    public List<WalletOwnerContext> Owners { get; init; } = [];

    /// <summary>
    /// Additional key-value metadata attached to the wallet.
    /// </summary>
    /// <remarks>
    /// Metadata is application-defined and does not affect core wallet
    /// invariants unless explicitly handled by domain rules.
    /// </remarks>
    public Dictionary<string, string> Metadata { get; init; } = new();

    /// <summary>
    /// Classification tags assigned to the wallet.
    /// </summary>
    /// <remarks>
    /// Tags can be used for searching, grouping, and filtering wallets.
    /// </remarks>
    public List<string> Tags { get; init; } = [];

    /// <summary>
    /// Returns a concise textual representation of the creation context.
    /// </summary>
    /// <returns>
    /// A string containing the context identifier and wallet currency.
    /// </returns>
    public override string ToString()
        => $"WalletCreationContext {{ Id = {Id}, CurrencyCode = {CurrencyCode} }}";
}