using Core.Projections;

namespace ConcertManagement.Concerts.Projections;

public record AvailableConcert(
    string ConcertId,
    string ConcertName,
    DateTimeOffset ConcertDate,
    string Location
);

public record AvailableConcerts(
    IReadOnlyList<AvailableConcert> Concerts
);

public class AvailableConcertsProjection : Projection<AvailableConcerts>
{
    public AvailableConcertsProjection()
    {
        Projects<ConcertCreated>(ev => ev.ConcertId, Apply);
        Projects<ConcertUpdated>(ev => ev.ConcertId, Apply);
        Projects<ConcertCancelled>(ev => ev.ConcertId, Apply);
    }

    private AvailableConcerts Apply(AvailableConcerts view, ConcertCreated @event)
    {
        var newConcert = new AvailableConcert(@event.ConcertId, @event.ConcertName, @event.ConcertDate, @event.Location);
        return view with { Concerts = view.Concerts.Append(newConcert).ToList() };
    }

    private AvailableConcerts Apply(AvailableConcerts view, ConcertUpdated @event)
    {
        var updatedConcerts = view.Concerts.Select(c => c.ConcertId == @event.ConcertId
            ? c with { ConcertName = @event.ConcertName, ConcertDate = @event.ConcertDate, Location = @event.Location }
            : c).ToList();

        return view with { Concerts = updatedConcerts };
    }

    private AvailableConcerts Apply(AvailableConcerts view, ConcertCancelled @event)
    {
        var updatedConcerts = view.Concerts.Where(c => c.ConcertId != @event.ConcertId).ToList();
        return view with { Concerts = updatedConcerts };
    }
}
