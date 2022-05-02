namespace NG.DomainEvents.Config;

[AttributeUsage(AttributeTargets.Class)]
public class DomainEventHandlerAttribute : Attribute
{
    public DomainEventHandlerAttribute(string handlerName)
    {
        HandlerName = handlerName;
    }

    public string HandlerName { get; }
}