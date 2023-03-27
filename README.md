# Event Sourcing on Production Workshop

## Bounded Conexts, Aggregates, Commands and Events

1. Concert Management (Bounded Context: Concerts)

Aggregates: Concert
Commands: CreateConcert, UpdateConcert, CancelConcert, UpdateTicketLevels
Events: ConcertCreated, ConcertUpdated, ConcertCancelled, TicketLevelsUpdated

2. Shopping Cart (Bounded Context: ShoppingCarts)

Aggregates: ShoppingCart
Commands: CreateCart, AddTicketToCart, RemoveTicketFromCart, ClearCart, ConfirmCart
Events: CartCreated, TicketAddedToCart, TicketRemovedFromCart, CartCleared, CartConfirmed

3. Reservation (Bounded Context: Reservations)

Aggregates: Reservation
Commands: CreateReservation, CancelReservation, ExpireReservation
Events: ReservationCreated, ReservationCancelled, ReservationExpired

4. Purchases (Bounded Context: Orders)

Aggregates: TicketOrder
Commands: CreateOrder, CompleteOrder, CancelOrder, CompensateOrder
Events: OrderCreated, OrderCompleted, OrderCancelled

5. Ticket Delivery (Bounded Context: TicketDelivery)

Aggregates: TicketDelivery
Commands: PrepareTicketDelivery, DeliverOnlineTicket, DeliverPrintedTicket
Events: TicketDeliveryPrepared, OnlineTicketDelivered, PrintedTicketDelivered

6. Payment (Bounded Context: Payments)

Aggregates: Payment
Commands: InitiatePayment, ConfirmPayment, RefundPayment
Events: PaymentInitiated, PaymentConfirmed, PaymentRefunded

7. User Management (Bounded Context: Users)

Aggregates: User
Commands: RegisterUser, UpdateUserRole
Events: UserRegistered, UserRoleUpdated

8. Finance (Bounded Context: Finance)

Aggregates: Invoice, UserFinancialInfo
Commands: CreateInvoice, UpdateInvoice, CreateUserFinancialInfo, UpdateUserFinancialInfo
Events: InvoiceCreated, InvoiceUpdated, UserFinancialInfoCreated, UserFinancialInfoUpdated
