namespace NG.DomainEvents.Config;

public class DomainEventsMappingConfig {
    public List<DomainEventTypeMapping> Mappings { get; set; } = new();

    public DomainEventTypeMapping GetMapping(Type type) {
        return Mappings.Single(m => m.AssemblyType == type);
    }

    public DomainEventTypeMapping GetMapping(string entityType) {
        return Mappings.Single(m => m.EntityType == entityType);
    }

    public DomainEventTypeMapping? GetMappingOrDefault(string entityType) {
        return Mappings.SingleOrDefault(m => m.EntityType == entityType);
    }
}
