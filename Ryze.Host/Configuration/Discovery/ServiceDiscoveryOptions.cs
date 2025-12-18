namespace Ryze.Host.Configuration.Discovery;

/// <summary>
/// Configuration options for automatic service discovery and DI registration.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Specifies which namespaces and types should be included or excluded during service scanning.</item>
/// <item>Allows controlling the lifetime of discovered services.</item>
/// <item>Can skip interfaces and exception types if not meant to be registered.</item>
/// </list>
/// </remarks>
public sealed class ServiceDiscoveryOptions
{
    /// <summary>
    /// Namespaces to include in discovery. Only types within these namespaces will be considered.
    /// </summary>
    public List<string> AllowedNamespaces { get; set; } = [];

    /// <summary>
    /// Namespaces to exclude from discovery, even if listed in <see cref="AllowedNamespaces"/>.
    /// </summary>
    public List<string> ExcludedNamespaces { get; set; } = [];

    /// <summary>
    /// Specific types to ignore during service registration.
    /// </summary>
    public List<Type> ExcludedTypes { get; set; } = [];
    
    public List<string> AllowedLayers { get; set; } = [];

    /// <summary>
    /// If <c>true</c>, interface types are ignored during discovery.
    /// </summary>
    public bool SkipInterfaces { get; set; } = true;

    /// <summary>
    /// If <c>true</c>, exception types are ignored during discovery.
    /// </summary>
    public bool SkipExceptions { get; set; } = true;

    /// <summary>
    /// If false, ServiceDiscovery will not log debug messages.
    /// </summary>
    public bool EnableLogging { get; set; } = true;
    
    /// <summary>
    /// Default service lifetime for all discovered types.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
}