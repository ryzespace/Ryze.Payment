namespace Ryze.Application.Features.Ledger.Builders;

/// <summary>
/// Provides fluent builder for constructing ledger entry metadata.
/// </summary>
/// <remarks>
/// Encapsulates metadata creation for ledger journal entries by providing
/// a consistent way to attach additional contextual information.
///
/// Metadata can contain external references, correlation identifiers,
/// tax information, or other audit related attributes required for
/// tracing and reconciliation purposes.
/// </remarks>
public sealed class MetadataBuilder
{
    private readonly Dictionary<string, string> _metadata = [];

    /// <summary>
    /// Adds or updates a metadata key-value pair.
    /// </summary>
    /// <param name="key">Metadata key identifier.</param>
    /// <param name="value">Metadata value.</param>
    /// <returns>The current metadata builder instance.</returns>
    public MetadataBuilder With(string key, string value)
    {
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Adds an external reference identifier to the metadata.
    /// </summary>
    /// <param name="reference">External system reference identifier.</param>
    /// <returns>The current metadata builder instance.</returns>
    public MetadataBuilder WithExternalRef(string reference)
        => With("external_ref", reference);

    /// <summary>
    /// Adds correlation identifier to the metadata.
    /// </summary>
    /// <remarks>
    /// Correlation identifiers allow tracing a ledger operation across
    /// distributed services and asynchronous workflows.
    /// </remarks>
    /// <param name="correlationId">Correlation identifier.</param>
    /// <returns>The current metadata builder instance.</returns>
    public MetadataBuilder WithCorrelationId(string correlationId)
        => With("correlation_id", correlationId);

    /// <summary>
    /// Adds tax identifier to the metadata.
    /// </summary>
    /// <param name="taxId">Tax identifier associated with the entry.</param>
    /// <returns>The current metadata builder instance.</returns>
    public MetadataBuilder WithTaxId(string taxId)
        => With("tax_id", taxId);

    /// <summary>
    /// Builds the metadata collection.
    /// </summary>
    /// <remarks>
    /// Returns the accumulated metadata entries configured through the builder.
    /// </remarks>
    /// <returns>
    /// Dictionary containing ledger entry metadata.
    /// </returns>
    public Dictionary<string, string> Build()
        => _metadata;
}