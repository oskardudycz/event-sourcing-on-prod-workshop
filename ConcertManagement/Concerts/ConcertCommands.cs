using ConcertManagement.Concerts.ValueObjects;

namespace ConcertManagement.Concerts;

public record CreateConcert(
    string ConcertId,
    string ConcertName,
    string Artist,
    DateTimeOffset ConcertDate,
    string Location,
    IReadOnlyList<TicketTypeInfo> TicketTypes
);

public record UpdateConcert(
    string ConcertId,
    string ConcertName,
    string Artist,
    DateTimeOffset ConcertDate,
    string Location
);

public record UpdateTicketTypes(
    string ConcertId,
    IReadOnlyList<TicketTypeInfo> TicketTypes
);

public record CancelConcert(
    string ConcertId
);
