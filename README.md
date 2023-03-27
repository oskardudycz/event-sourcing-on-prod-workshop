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

- **Aggregates:** `Reservation`, `ConcertTicketsAvailability`
- **Commands:** `CreateReservation`, `CancelReservation`, `ExpireReservation`
- **Events:** `ReservationCreated`, `ReservationCancelled`, `ReservationExpired`

#### Flow

1. When a concert is created, the ConcertCreated event is raised. The ConcertEventHandler listens to this event, creates a ConcertTicketsAvailability aggregate, and initializes it with the available tickets for each level.
2. When ticket levels are updated for a concert, the TicketLevelsUpdated event is raised. The ConcertEventHandler listens to this event, loads the ConcertTicketsAvailability aggregate, and updates the available tickets accordingly.
3. When a concert is canceled, the ConcertCancelled event is raised. The ConcertEventHandler listens to this event, loads the ConcertTicketsAvailability aggregate, and updates the available tickets to zero.
4. When the user confirms their cart, the system the ReserveTickets command is sent for each concert in the cart. The reservation command handler checks if there are enough available tickets for each level in the ConcertTicketsAvailability aggregate, updates the available tickets accordingly, and creates a new Reservation aggregate.
5. The reservation can be canceled or expired according to the business rules and invariants mentioned above. The Reservation aggregate is updated accordingly.

- **Business rules:**
  - Ensure tickets are available before creating a reservation.
- **Invariants:**
  - The total number of reserved tickets cannot exceed the concert's capacity for each ticket level.
  

#### Reservation

**1. Business Rules**

- A reservation can be created by a logged-in user only.
- A reservation can be canceled if it is not already canceled or expired.
- A reservation can expire if it is not already canceled or expired.

**2. Invariants:**

- The reservation cannot be created, canceled, or expired for a user that is not logged in.
- The reservation cannot be created for an unavailable concert.
- The reservation cannot be created if the number of requested tickets for each level exceeds the available tickets.

**3. Commands** 

```csharp
public record CreateReservation(string ReservationId, string ConcertId, Dictionary<string, int> TicketLevels, string UserId);
public record CancelReservation(string ReservationId);
public record ExpireReservation(string ReservationId);
````

**4. Events:**

```csharp
public record ReservationCreated(string ReservationId, string ConcertId, Dictionary<string, int> TicketLevels, string UserId);
public record ReservationCancelled(string ReservationId);
public record ReservationExpired(string ReservationId);
```

**5. Aggregate**

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
        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            throw new InvalidOperationException("User must be authenticated to create a reservation.");
        }

        Apply(new ReservationCreated(command.ReservationId, command.ConcertId, command.TicketLevels, command.UserId));
    }

    public void Cancel(CancelReservation command)
    {
        if (IsCancelled || IsExpired)
        {
            throw new InvalidOperationException("Cannot cancel an already cancelled or expired reservation.");
        }

        Apply(new ReservationCancelled(command.ReservationId));
    }

    public void Expire(ExpireReservation command)
    {
        if (IsCancelled || IsExpired)
        {
            throw new InvalidOperationException("Cannot expire an already cancelled or expired reservation.");
        }

        Apply(new ReservationExpired(command.ReservationId));
    }
}
```

**6. Command Handler**

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
        // Load the concert aggregate and reserve tickets.
        var concert = await _session.Events.AggregateStreamAsync<Concert>(command.ConcertId);
        concert.ReserveTickets(new ReserveTickets(command.ConcertId, command.TicketLevels));

        // Create the reservation.
        var reservation = new Reservation();
        reservation.Create(command);

        // Append events to both aggregates.
        _session.Events.Append(command.ConcertId, concert);
        _session.Events.Append(command.ReservationId, reservation);

        // Save the changes in the same transaction.
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

#### Concert Availability

**1. Business Rules**

- The concert tickets availability is created when a ConcertCreated event occurs.
- The concert tickets availability is updated when a TicketLevelsUpdated event occurs.
- The concert tickets availability is updated when a ConcertCancelled event occurs.
- Tickets can be reserved if there are enough available tickets for the requested ticket levels.

**2. Invariants:**

- The number of available tickets for each level should be non-negative.
- Tickets cannot be reserved if the number of requested tickets for each level exceeds the available tickets.

**3. Commands** 

None. Just using events from the concert module.

**4. Events:**

None. Just using events from the concert module and reservation aggregate.

**5. Aggregate**

```csharp
public class ConcertTicketsAvailability
{
    public string ConcertId { get; private set; }
    public Dictionary<string, int> AvailableTickets { get; private set; }

    public ConcertTicketsAvailability()
    {
    }

    private void Apply(ConcertCreated e)
    {
        ConcertId = e.ConcertId;
        AvailableTickets = e.TicketLevels;
    }

    private void Apply(TicketLevelsUpdated e)
    {
        AvailableTickets = e.TicketLevels;
    }

    private void Apply(ConcertCancelled e)
    {
        AvailableTickets = new Dictionary<string, int>();
    }

    public void ReserveTickets(ReserveTickets command)
    {
        foreach (var ticketLevel in command.TicketLevels)
        {
            if (!AvailableTickets.ContainsKey(ticketLevel.Key) || AvailableTickets[ticketLevel.Key] < ticketLevel.Value)
            {
                throw new InvalidOperationException("Not enough available tickets for the requested ticket level.");
            }

            AvailableTickets[ticketLevel.Key] -= ticketLevel.Value;
        }
    }
}

```

**6. Command Handler**

None.

**7. Event Handler**

```
public class ConcertEventHandler
{
    private readonly IDocumentSession _session;

    public ConcertEventHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task HandleAsync(ConcertCreated e)
    {
        var concertTicketsAvailability = new ConcertTicketsAvailability();
        concertTicketsAvailability.Apply(e);
        _session.Events.Append(e.ConcertId, concertTicketsAvailability);
        await _session.SaveChangesAsync();
    }

    public async Task HandleAsync(TicketLevelsUpdated e)
    {
        var concertTicketsAvailability = await _session.Events.AggregateStreamAsync<ConcertTicketsAvailability>(e.ConcertId);
        concertTicketsAvailability.Apply(e);
        _session.Events.Append(e.ConcertId, concertTicketsAvailability);
        await _session.SaveChangesAsync();
    }

    public async Task HandleAsync(ConcertCancelled e)
    {
        var concertTicketsAvailability = await _session.Events.AggregateStreamAsync<ConcertTicketsAvailability>(e.ConcertId);
        concertTicketsAvailability.Apply(e);
        _session.Events.Append(e.ConcertId, concertTicketsAvailability);
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
- **Events:** TicketDeliveryPrepared, OnlineTicketDelivered, PrintedTicketDelivered

### 6. Payment (Bounded Context: Payments)

- **Aggregates:** `Payment`
- **Commands:** : `InitiatePayment`, `ConfirmPayment`, `RefundPayment`
- **Events:** `PaymentInitiated`, `PaymentConfirmed`, `PaymentRefunded`

### 7. User Management (Bounded Context: Users)

- **Aggregates:** `User`
- **Commands:** : `RegisterUser`, `UpdateUserRole`
- **Events:** `UserRegistered`, `UserRoleUpdated`

### 8. Finance (Bounded Context: Finance)

- **Aggregates:** `Invoice`, `UserFinancialInfo`
- **Commands:** `CreateInvoice`, `UpdateInvoice`, `CreateUserFinancialInfo`, `UpdateUserFinancialInfo`
- **Events:** `InvoiceCreated`, `InvoiceUpdated`, `UserFinancialInfoCreated`, `UserFinancialInfoUpdated`
