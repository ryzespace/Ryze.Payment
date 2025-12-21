using ModularityKit.Context.Abstractions;

namespace Ryze.Domain.Features.WalletBalance.Contexts;

/// <summary>
/// Represents the context of a single wallet balance request.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks request metadata such as <see cref="TenantId"/>, <see cref="UserId"/>, and <see cref="CorrelationId"/>.</item>
/// <item>Allows specifying the type of request via <see cref="RequestType"/> (e.g., "TopUp", "Transfer").</item>
/// <item>Manages feature flags for the current request through <see cref="EnableFeature(string, string?)"/>.</item>
/// <item>Provides a factory method <see cref="Create"/> for generating contexts with unique IDs.</item>
/// <item>Supports fluent chaining for setting request type and enabling features.</item>
/// </list>
/// </remarks>
public sealed class RequestContext : IContext
{
    /// <summary>
    /// Gets the unique identifier of the request context.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the timestamp when the request context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the identifier of the tenant associated with the request.
    /// </summary>
    public Guid TenantId { get; }

    /// <summary>
    /// Gets the identifier of the user performing the request.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the correlation ID used for tracking the request across distributed systems.
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Gets the type of the request, e.g., "TopUp" or "Transfer".
    /// </summary>
    public string? RequestType { get; private set; }

    /// <summary>
    /// Stores feature flags enabled for this request.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Keys represent feature names.</item>
    /// <item>Values represent optional parameters for the feature (default is "true").</item>
    /// <item>Used internally to enable/disable conditional behavior during request processing.</item>
    /// </list>
    /// </remarks>
    private Dictionary<string, string> FeatureFlags { get; } = new();
    
    private RequestContext(
        string id,
        Guid tenantId,
        Guid userId,
        string correlationId)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        CorrelationId = correlationId;
    }

    public static RequestContext Create(
        Guid tenantId,
        Guid userId,
        string correlationId)
    {
        return new RequestContext(
            Guid.NewGuid().ToString("N"),
            tenantId,
            userId,
            correlationId
        );
    }
    
    /// <summary>
    /// Sets the type of the request.
    /// </summary>
    /// <param name="requestType">Request type, e.g., "TopUp" or "Transfer".</param>
    /// <returns>The same <see cref="RequestContext"/> instance for fluent chaining.</returns>
    public RequestContext WithRequestType(string requestType)
    {
        RequestType = requestType;
        return this;
    }
    
    /// <summary>
    /// Enables a feature flag for this request.
    /// </summary>
    /// <param name="featureKey">The key of the feature to enable.</param>
    /// <param name="value">Optional value of the feature; defaults to "true".</param>
    /// <returns>The same <see cref="RequestContext"/> instance for fluent chaining.</returns>
    public RequestContext EnableFeature(string featureKey, string? value = null)
    {
        FeatureFlags[featureKey] = value ?? "true";
        return this;
    }
}