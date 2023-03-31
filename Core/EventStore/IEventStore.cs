using Core.Aggregates;

namespace Core.EventStore;

public interface IEventStore
{
    void Append(string streamId, object[] events);
    Task SaveChangesAsync();
    Task<T> AggregateStreamAsync<T>(string streamId);
}


public static class EventStoreExtensionMethods
{
    public static void Append<T>(this IEventStore eventStore, string streamId, T aggregate) where T : Aggregate =>
        eventStore.Append(streamId, aggregate.DequeueUncommittedEvents());
}
