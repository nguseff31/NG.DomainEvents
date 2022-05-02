using NG.DomainEvents.Common;

namespace DomainEvents.Hangfire.Models;

public interface IEntityWithDomainEvents
{
    public string GetEntityId();
    
    IReadOnlyCollection<DomainEvent> TakeAllEvents();
}