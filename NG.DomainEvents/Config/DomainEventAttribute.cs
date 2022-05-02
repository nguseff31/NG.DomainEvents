namespace NG.DomainEvents.Config;

[AttributeUsage(AttributeTargets.Class)]
public class DomainEventAttribute : Attribute
{
    public DomainEventAttribute(string eventType)
    {
        EventType = eventType;
    }

    public string EventType { get; }
}