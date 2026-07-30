namespace Ryze.Domain.Features.Health.Enum;

/// <summary>
/// Represents logical infrastructure or application component that participates
/// in health monitoring and health report generation.
/// </summary>
public enum HealthComponent
{
    /// <summary>
    /// The primary database used by the application.
    /// </summary>
    Database = 0,

    /// <summary>
    /// The event store responsible for persisting domain events.
    /// </summary>
    EventStore = 1,

    /// <summary>
    /// The distributed or in-memory cache layer.
    /// </summary>
    Cache = 2,

    /// <summary>
    /// The message bus used for asynchronous messaging and event delivery.
    /// </summary>
    MessageBus = 3,

    /// <summary>
    /// The ledger subsystem responsible for financial integrity and reconciliation.
    /// </summary>
    Ledger = 4,

    /// <summary>
    /// The external payment provider integration.
    /// </summary>
    PaymentProvider = 5,

    /// <summary>
    /// A monitored gRPC endpoint or service dependency.
    /// </summary>
    GrpcEndpoint = 6,

    /// <summary>
    /// Unrecognised or unmapped health check component.
    /// </summary>
    Unknown = 7
}