namespace NG.DomainEvents.Config;

public class DomainEventsMappingConfig
{
    public List<DomainEventTypeMapping> Mappings { get; set; } = new(); //todo to dictionary
    
    public DomainEventTypeMapping? GetMapping(Type type)
    {
        return Mappings.FirstOrDefault(m => m.AssemblyType == type);
    }
}