namespace NG.DomainEvents.Config;

public class DomainEventsConfig {
    public bool RetryJobEnabled { get; set; } = true;

    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(30);

    public int MaxRetries { get; set; } = 10;

    public TimeSpan EventExpiredAfter { get; set; } = TimeSpan.FromMinutes(5);
}
