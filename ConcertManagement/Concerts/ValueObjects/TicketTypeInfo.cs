namespace ConcertManagement.Concerts.ValueObjects;

public record TicketTypeInfo(
    string TicketType,
    decimal Price,
    int AvailableTickets
);
