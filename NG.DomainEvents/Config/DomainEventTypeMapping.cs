namespace NG.DomainEvents.Config {
    public class DomainEventTypeMapping {
        public string EntityType { get; set; }

        public Type AssemblyType { get; set; }

        public List<DomainEventHandlerMappings> Handlers = new();

        public DomainEventHandlerMappings GetHandler(string handlerType) {
            return Handlers.Single(h => h.HandlerType == handlerType);
        }
    }
}
