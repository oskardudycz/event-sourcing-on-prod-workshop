using ConcertManagement.Concerts.ValueObjects;

namespace ConcertManagement.Concerts;

public record ConcertCreated(
    string ConcertId,
    string ConcertName,
    string Artist,
    DateTimeOffset ConcertDate,
    string Location,
    IReadOnlyList<TicketTypeInfo> TicketTypes
);

public record ConcertUpdated(
    string ConcertId,
    string ConcertName,
    string Artist,
    DateTimeOffset ConcertDate,
    string Location
);

public record TicketTypesUpdated(
    string ConcertId,
    IReadOnlyList<TicketTypeInfo> TicketTypes
);

public record ConcertCancelled(
    string ConcertId
);
