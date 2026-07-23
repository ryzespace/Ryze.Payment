namespace Ryze.Domain.Shared;

/// <summary>
/// Defines contract for publishing domain events associated with an event stream.
/// </summary>
/// <remarks>
/// Implementations are responsible for dispatching events to the appropriate
/// event handling infrastructure while preserving the association between
/// the published event and its originating stream.
/// </remarks>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event associated with the specified event stream.
    /// </summary>
    /// <typeparam name="T">The type of the event being published.</typeparam>
    /// <param name="streamId">The unique identifier of the event stream to which the event belongs. </param>
    /// <param name="event">The event instance to publish. </param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    Task PublishAsync<T>(
        Guid streamId,
        T @event,
        CancellationToken ct = default)
        where T : class;
}