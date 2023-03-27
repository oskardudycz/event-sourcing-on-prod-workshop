# Event Sourcing on Production Workshop

## Bounded Contexts, Aggregates, Commands, and Events

### 1. Concert Management (Bounded Context: `Concerts`)

#### Summary

- **Aggregates:** `Concert`
- **Commands:** `CreateConcert`, `UpdateConcert`, `CancelConcert`, `UpdateTicketLevels`
- **Events:** `ConcertCreated`, `ConcertUpdated`, `ConcertCancelled`, `TicketLevelsUpdated`

#### Flow

1. Admin creates, updates, or cancels a concert.
2. Admin updates ticket levels (e.g., regular, golden circle).

- **Business rules:**
  - Only admins can create, update, or cancel concerts.
  - Ensure valid data for concerts and ticket levels.
- **Invariants:**
  - A concert cannot have negative or duplicate ticket levels or capacity.

### 2. Shopping Cart (Bounded Context: `ShoppingCarts`)

#### Summary

- **Aggregates:** `ShoppingCart`
- **Commands:** `CreateCart`, `AddTicketToCart`, `RemoveTicketFromCart`, `ClearCart`, `ConfirmCart`
- **Events:** `CartCreated`, `TicketAddedToCart`, `TicketRemovedFromCart`, `CartCleared`, `CartConfirmed`

#### Flow

1. A user (unauthenticated or logged in) visits the website, and the system creates a new shopping cart.
2. The user adds or removes tickets from the cart without being logged in.
3. Users can clear their cart.
4. When the user logs in and confirms their selection, the cart information is transferred to their account.

- **Business rules:**
  - Ensure valid tickets are added to the cart.
- **Invariants:**
  - A user cannot have multiple confirmed carts at the same time.
  - A cart cannot have negative or duplicate ticket quantities.

### 3. Reservation (Bounded Context: `Reservations`)

#### Summary

- **Aggregates:** `Reservation`
- **Commands:** `CreateReservation`, `CancelReservation`, `ExpireReservation`
- **Events:** `ReservationCreated`, `ReservationCancelled`, `ReservationExpired`

#### Flow

1. When the user confirms their cart, the system checks if there are enough tickets available at the requested level for each concert in the cart.
2. If there are enough tickets, the system creates reservations for each concert.
3. Users can cancel the reservation or let it expire.

- **Business rules:**
  - Ensure tickets are available before creating a reservation.
- **Invariants:**
  - The total number of reserved tickets cannot exceed the concert's capacity for each ticket level.
  

#### Commands:

```csharp
public record CreateReservation(string ReservationId, string ConcertId, Dictionary<string, int> TicketLevels, string UserId);
public record CancelReservation(string ReservationId);
public record ExpireReservation(string ReservationId);
````

#### Events:

```csharp
public record ReservationCreated(string ReservationId, string ConcertId, Dictionary<string, int> TicketLevels, string UserId);
public record ReservationCancelled(string ReservationId);
public record ReservationExpired(string ReservationId);
```

#### Aggregates

```csharp
public class Reservation
{
    public string ReservationId { get; private set; }
    public string ConcertId { get; private set; }
    public Dictionary<string, int> TicketLevels { get; private set; }
    public string UserId { get; private set; }
    public bool IsCancelled { get; private set; }
    public bool IsExpired { get; private set; }

    private void Apply(ReservationCreated e)
    {
        ReservationId = e.ReservationId;
        ConcertId = e.ConcertId;
        TicketLevels = e.TicketLevels;
        UserId = e.UserId;
    }

    private void Apply(ReservationCancelled e)
    {
        IsCancelled = true;
    }

    private void Apply(ReservationExpired e)
    {
        IsExpired = true;
    }

    public void Create(CreateReservation command)
    {
        // Add logic for checking ticket availability, user authentication, etc.
        Apply(new ReservationCreated(command.ReservationId, command.ConcertId, command.TicketLevels, command.UserId));
    }

    public void Cancel(CancelReservation command)
    {
        // Add logic for checking if the reservation can be cancelled
        Apply(new ReservationCancelled(command.ReservationId));
    }

    public void Expire(ExpireReservation command)
    {
        // Add logic for checking if the reservation can be expired
        Apply(new ReservationExpired(command.ReservationId));
    }
}
```

#### Command Handlers

```csharp
public class ReservationCommandHandler
{
    private readonly IDocumentSession _session;

    public ReservationCommandHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task HandleAsync(CreateReservation command)
    {
        var reservation = new Reservation();
        reservation.Create(command);
        _session.Events.Append(command.ReservationId, reservation);
        await _session.SaveChangesAsync();
    }

    public async Task HandleAsync(CancelReservation command)
    {
        var reservation = await _session.Events.AggregateStreamAsync<Reservation>(command.ReservationId);
        reservation.Cancel(command);
        _session.Events.Append(command.ReservationId, reservation);
        await _session.SaveChangesAsync();
    }

    public async Task HandleAsync(ExpireReservation command)
    {
        var reservation = await _session.Events.AggregateStreamAsync<Reservation>(command.ReservationId);
        reservation.Expire(command);
        _session.Events.Append(command.ReservationId, reservation);
        await _session.SaveChangesAsync();
    }
}

```

### 4. Purchases (Bounded Context: `Orders`)

#### Summary

- **Aggregates:** `TicketOrder`
- **Commands:** `CreateOrder`, `CompleteOrder`, `CancelOrder`, `CompensateOrder`
- **Events:** `OrderCreated`, `OrderCompleted`, `OrderCancelled`

#### Flow

1. When the user confirms their cart and initiates the purchase process, the system creates orders for each concert.
2. Upon successful payment, the system marks the order as complete.
3. If the payment fails, the system reverts the ticket reservation and any associated actions.

- **Business rules:**
  - Ensure valid payment information and process payments securely.
- **Invariants:**
  - An order cannot be marked as complete without successful payment.

### 5. Ticket Delivery (Bounded Context: `TicketDelivery`)

#### Summary

- **Aggregates:** `TicketDelivery`
- **Commands:** `PrepareTicketDelivery`, `DeliverOnlineTicket`, `DeliverPrintedTicket`
