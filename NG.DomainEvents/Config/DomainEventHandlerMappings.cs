using System.ComponentModel.DataAnnotations;

namespace NG.DomainEvents.Config;

public class DomainEventHandlerMappings {
    public string HandlerType { get; set; }

    public Type AssemblyType { get; set; }

    public TimeSpan Timeout { get; set; }

    public TimeSpan RetryAfter { get; set; }
}
