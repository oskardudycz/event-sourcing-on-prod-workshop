# Event Sourcing on Production Workshop

## Bounded Conexts, Aggregates, Commands and Events

1. Concert Management (Bounded Context: Concerts)

- Aggregates: Concert
- Commands: CreateConcert, UpdateConcert, CancelConcert, UpdateTicketLevels
- Events: ConcertCreated, ConcertUpdated, ConcertCancelled, TicketLevelsUpdated
- Flow:

        Admin creates, updates, or cancels a concert.
        Admin updates ticket levels (e.g., regular, golden circle).
        Business rules:
            Only admins can create, update, or cancel concerts.
            Ensure valid data for concerts and ticket levels.
        Invariants:
            A concert cannot have negative or duplicate ticket levels or capacity.

2. Shopping Cart (Bounded Context: ShoppingCarts)

- Aggregates: ShoppingCart
- Commands: CreateCart, AddTicketToCart, RemoveTicketFromCart, ClearCart, ConfirmCart
- Events: CartCreated, TicketAddedToCart, TicketRemovedFromCart, CartCleared, CartConfirmed
- Flow: 

        A user (unauthenticated or logged in) visits the website, and the system creates a new shopping cart.
        The user adds or removes tickets from the cart without being logged in.
        Users can clear their cart.
        When the user logs in and confirms their selection, the cart information is transferred to their account.
        Business rules:
            Ensure valid tickets are added to the cart.
        Invariants:
            A user cannot have multiple confirmed carts at the same time.
            A cart cannot have negative or duplicate ticket quantities.

3. Reservation (Bounded Context: Reservations)

- Aggregates: Reservation
- Commands: CreateReservation, CancelReservation, ExpireReservation
- Events: ReservationCreated, ReservationCancelled, ReservationExpired
- Flow: 

        When the user confirms their cart, the system checks if there are enough tickets available at the requested level for each concert in the cart.
        If there are enough tickets, the system creates reservations for each concert.
        Users can cancel the reservation or let it expire.
        Business rules:
            Ensure tickets are available before creating a reservation.
        Invariants:
            The total number of reserved tickets cannot exceed the concert's capacity for each ticket level.

4. Purchases (Bounded Context: Orders)

- Aggregates: TicketOrder
- Commands: CreateOrder, CompleteOrder, CancelOrder, CompensateOrder
- Events: OrderCreated, OrderCompleted, OrderCancelled
- Flow: 

        When the user confirms their cart and initiates the purchase process, the system creates orders for each concert.
        Upon successful payment, the system marks the order as complete.
        If the payment fails, the system reverts the ticket reservation and any associated actions.
        Business rules:
            Ensure valid payment information and process payments securely.
        Invariants:
            An order cannot be marked as complete without successful payment.

5. Ticket Delivery (Bounded Context: TicketDelivery)

- Aggregates: TicketDelivery
- Commands: PrepareTicketDelivery, DeliverOnlineTicket, DeliverPrintedTicket
- Events: TicketDeliveryPrepared, OnlineTicketDelivered, PrintedTicketDelivered
- Flow: 

        When an order is completed, the system prepares ticket delivery for each concert.
        Based on the ticket type, the system delivers online tickets or printed tickets.
        Business rules:
            Ensure the correct ticket delivery method is used based on the ticket type.
        Invariants:
            A ticket cannot be delivered using an incorrect delivery method.

6. Payment (Bounded Context: Payments)

- Aggregates: Payment
- Commands: InitiatePayment, ConfirmPayment, RefundPayment
- Events: PaymentInitiated, PaymentConfirmed, PaymentRefunded
- Flow: 

        When the user initiates the purchase process, the system calculates the total amount and creates a payment.
        Upon successful payment, the system confirms the payment.
        If the payment fails, the system processes a refund.
        Business rules:
            Ensure secure handling of payment information.
        Invariants:
            Payment status must accurately reflect the success or failure of the transaction.

7. User Management (Bounded Context: Users)

- Aggregates: User
- Commands: RegisterUser, UpdateUserRole
- Events: UserRegistered, UserRoleUpdated
- Flow: 

        Users register and can be assigned different roles (unauthenticated, logged in, or admin).
        Business rules:
            Ensure secure handling of user information and credentials.
        Invariants:
            A user cannot have multiple roles simultaneously.

8. Finance (Bounded Context: Finance)

- Aggregates: Invoice, UserFinancialInfo
- Commands: CreateInvoice, UpdateInvoice, CreateUserFinancialInfo, UpdateUserFinancialInfo
- Events: InvoiceCreated, InvoiceUpdated, UserFinancialInfoCreated, UserFinancialInfoUpdated
- Flow: 

        When the user initiates the purchase process, the system creates an invoice for the entire purchase.
        After successful payment, the system updates the invoice with payment information.
        When a user registers, the system creates a UserFinancialInfo record.
        After each successful payment, the system updates the user's financial information, including ongoing and completed payments.
        Business rules:
            Ensure accurate calculation and recording of financial transactions.
        Invariants:
            Invoices must accurately reflect the user's purchase, including ticket quantities and amounts.
            User financial information must accurately reflect the user's transaction history.
