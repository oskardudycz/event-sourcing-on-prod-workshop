using Core.Aggregates;

namespace ConcertManagement.Concerts;

public class Concert: Aggregate
{
    public string ConcertId { get; private set; } = default!;
    public string ConcertName { get; private set; } = default!;
    public string Artist { get; private set; } = default!;
    public bool IsCancelled { get; private set; } = default!;
    public Dictionary<string, int> TicketLevels { get; private set; } = default!;
    public Dictionary<string, decimal> Pricing { get; private set; } = default!;

    public static Concert New(CreateConcert command)
    {
        var concert = new Concert();
        concert.ApplyAndEnqueue(
            new ConcertCreated(
                command.ConcertId,
                command.ConcertName,
                command.Artist,
                command.ConcertDate,
                command.Location,
                command.TicketTypes
            )
        );
        return concert;
    }


    public void Update(UpdateConcert command)
    {
        if (IsCancelled)
        {
            throw new InvalidOperationException("Cannot update ticket levels for a cancelled concert.");
        }

        ApplyAndEnqueue(
            new ConcertUpdated(
                command.ConcertId,
                command.ConcertName,
                command.Artist,
                command.ConcertDate,
                command.Location
            )
        );
    }

    public void UpdateTicketLevels(UpdateTicketTypes command)
    {
        if (IsCancelled)
        {
            throw new InvalidOperationException("Cannot update ticket levels for a cancelled concert.");
        }

        ApplyAndEnqueue(new TicketTypesUpdated(command.ConcertId, command.TicketTypes));
    }

    public void Cancel(CancelConcert command)
    {
        if (IsCancelled)
        {
            throw new InvalidOperationException("Cannot cancel an already cancelled concert.");
        }

        ApplyAndEnqueue(new ConcertCancelled(command.ConcertId));
    }

    private void Apply(ConcertCreated e)
    {
        ConcertId = e.ConcertId;
        Artist = e.Artist;
        TicketLevels = e.TicketTypes.ToDictionary(t => t.TicketType, t => t.AvailableTickets);
        Pricing = e.TicketTypes.ToDictionary(t => t.TicketType, t => t.Price);
    }

    private void Apply(TicketTypesUpdated e)
    {
        TicketLevels = e.TicketTypes.ToDictionary(t => t.TicketType, t => t.AvailableTickets);
        Pricing = e.TicketTypes.ToDictionary(t => t.TicketType, t => t.Price);
    }

    private void Apply(ConcertCancelled e)
    {
        IsCancelled = true;
    }
}
