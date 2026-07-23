using Marten;
using Ryze.Domain.Shared;

namespace Ryze.Infrastructure.Shared;

/// <summary>
/// Publishes domain events to a Marten event store.
/// </summary>
/// <param name="session">
/// The Marten document session used to append events and persist changes.
/// </param>
public sealed class MartenEventPublisher(IDocumentSession session) : IEventPublisher
{
    /// <summary>
    /// Appends an event to the specified event stream and persists the changes.
    /// </summary>
    /// <typeparam name="T">The type of the event being published.</typeparam>
    /// <param name="streamId">The unique identifier of the event stream to which the event is appended.</param>
    /// <param name="event">The event instance to append to the event stream. </param>
    /// <param name="ct">Token that can be used to cancel the asynchronous persistence operation.</param>
    /// <returns>
    /// A task representing the asynchronous event publishing operation.
    /// </returns>
    public async Task PublishAsync<T>(
        Guid streamId,
        T @event,
        CancellationToken ct = default)
        where T : class
    {
        session.Events.Append(streamId, @event);
        await session.SaveChangesAsync(ct);
    }
}