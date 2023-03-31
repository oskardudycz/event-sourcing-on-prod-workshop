namespace Core.Aggregates;

public abstract class Aggregate
{
    public string Id { get; protected set; } = default!;

    public int Version { get; protected set; }

    [NonSerialized] private readonly Queue<object> uncommittedEvents = new();

    public void Evolve<T>(T @event) where T : notnull =>
        Apply((dynamic)@event);

    public void Apply(object @event) =>
        throw new ArgumentOutOfRangeException(nameof(@event), @event, null);

    public object[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void ApplyAndEnqueue(object @event)
    {
        Evolve(@event);
        uncommittedEvents.Enqueue(@event);
    }
}
