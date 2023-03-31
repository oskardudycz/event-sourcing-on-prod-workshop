using ConcertManagement.Concerts.ValueObjects;
using Core.Projections;

namespace ConcertManagement.Concerts.Projections;

public record ConcertDetails(
    string ConcertId,
    string ConcertName,
    string Artist,
    DateTimeOffset ConcertDate,
    string Location,
    IReadOnlyList<TicketTypeInfo> TicketTypes
);

public class ConcertDetailsProjection: Projection<ConcertDetails>
{
    public ConcertDetailsProjection()
    {
        Projects<ConcertCreated>(ev => ev.ConcertId, Apply);
        Projects<ConcertUpdated>(ev => ev.ConcertId, Apply);
        Projects<TicketTypesUpdated>(ev => ev.ConcertId, Apply);
        // Projects<TicketReserved>(ev => ev.ConcertId, Apply);
        // Projects<TicketReservationCancelled>(ev => ev.ConcertId, Apply);
    }

    private ConcertDetails Apply(ConcertDetails view, ConcertCreated @event) =>
        new ConcertDetails(
            @event.ConcertId,
            @event.ConcertName,
            @event.Artist,
            @event.ConcertDate,
            @event.Location,
            @event.TicketTypes
        );

    private ConcertDetails Apply(ConcertDetails view, ConcertUpdated @event) =>
        view with
        {
            ConcertName = @event.ConcertName,
            ConcertDate = @event.ConcertDate,
            Artist = @event.Artist,
            Location = @event.Location
        };

    private ConcertDetails Apply(ConcertDetails view, TicketTypesUpdated @event) =>
        view with { TicketTypes = @event.TicketTypes };

    // public ConcertDetails Apply(ConcertDetails view, TicketReserved @event)
    // {
    //     var updatedTicketTypes = view.TicketTypes.Select(tt => tt.TicketType == @event.TicketType
    //         ? tt with { AvailableTickets = tt.AvailableTickets - 1 }
    //         : tt).ToList();
    //
    //     return view with { TicketTypes = updatedTicketTypes };
    // }
    //
    // public ConcertDetails Apply(ConcertDetails view, TicketReservationCancelled @event)
    // {
    //     var updatedTicketTypes = view.TicketTypes.Select(tt => tt.TicketType == @event.TicketType
    //         ? tt with { AvailableTickets = tt.AvailableTickets + 1 }
    //         : tt).ToList();
    //
    //     return view with { TicketTypes = updatedTicketTypes };
    // }
}
