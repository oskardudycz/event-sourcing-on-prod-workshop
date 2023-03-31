using Core.EventStore;

namespace ConcertManagement.Concerts;

public class ConcertCommandHandler
{
    private readonly IEventStore eventStore;

    public ConcertCommandHandler(IEventStore eventStore)
    {
        this.eventStore = eventStore;
    }

    public async Task HandleAsync(CreateConcert command)
    {
        var concert = Concert.New(command);

        eventStore.Append(command.ConcertId, concert);
        await eventStore.SaveChangesAsync();
    }

    public async Task HandleAsync(UpdateTicketTypes command)
    {
        var concert = await eventStore.AggregateStreamAsync<Concert>(command.ConcertId);
        concert.UpdateTicketLevels(command);

        eventStore.Append(command.ConcertId, concert);
        await eventStore.SaveChangesAsync();
    }

    public async Task HandleAsync(CancelConcert command)
    {
        var concert = await eventStore.AggregateStreamAsync<Concert>(command.ConcertId);
        concert.Cancel(command);

        eventStore.Append(command.ConcertId, concert);
        await eventStore.SaveChangesAsync();
    }
}
