namespace NG.DomainEvents.Common;

public interface IEntityWithDomainEvents
{
    public string GetEntityId();
    
    IReadOnlyCollection<DomainEvent> TakeAllEvents();
}