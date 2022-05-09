# NG.DomainEvents

This project implements persisent domain events with retry logic.
You can inherit your DbContext from DomainEventsDbContext and add events to your entities.
Events can be retried with background job on failure. Fired events and handler execution results are persisted in database.

It's already working, but need some improvements.
