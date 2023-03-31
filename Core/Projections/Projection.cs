namespace Core.Projections;

public abstract class Projection<TReadModel>
{
    public Func<string, Task<TReadModel?>> GetReadModel { private get; init; } = default!;
    public Func<string, TReadModel, Task> SaveReadModel { private get; init; } = default!;

    private record ProjectEvent(
        Func<object, string> GetId,
        Func<TReadModel, object, TReadModel> Apply
    );

    private readonly Dictionary<Type, ProjectEvent> projectors = new();

    private void Project(Func<object, object> func, object apply)
    {
        throw new NotImplementedException();
    }

    protected void Projects<TEvent>(
        Func<TEvent, string> getId,
        Func<TReadModel, TEvent, TReadModel> apply
    )
    {
        projectors.Add(
            typeof(TEvent),
            new ProjectEvent(
                @event => getId((TEvent)@event),
                (document, @event) => apply(document, (TEvent)@event)
            )
        );
    }

    public async Task HandleAsync(object @event, CancellationToken ct)
    {
        if (!projectors.ContainsKey(@event.GetType()))
            return;

        var (getId, apply) = projectors[@event.GetType()];

        var readModelId = getId(@event);

        var readModel = await GetReadModel(readModelId);

        var updated = apply(readModel ?? (TReadModel)Activator.CreateInstance(typeof(TReadModel), false)!, @event);

        await SaveReadModel(readModelId, updated);
    }
}
