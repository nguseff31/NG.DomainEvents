namespace NG.DomainEvents.Config
{
     public class DomainEventTypeMapping
     {
          public string EntityType { get; set; }
          
          public Type AssemblyType { get; set; }

          public List<DomainEventHandlerMappings> Handlers = new();
     }
}