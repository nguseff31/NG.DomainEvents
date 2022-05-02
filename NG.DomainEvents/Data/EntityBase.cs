using NG.DomainEvents.Common;

namespace NG.DomainEvents.Data;

public abstract class EntityBase : IEntityWithDomainEvents
{
    private List<DomainEvent> _events = new();

    private object _lockEvents = new();
    public abstract string GetEntityId();

    public IReadOnlyCollection<DomainEvent> TakeAllEvents()
    {
        lock (_lockEvents)
        {
            var events = _events.ToArray();
            _events.Clear();
            return events;
        }
    }

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        lock (_lockEvents)
        {
            _events.Add(domainEvent);
        }
    }
}