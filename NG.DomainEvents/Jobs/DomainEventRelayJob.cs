using Microsoft.Extensions.Hosting;
using NG.DomainEvents.Data;

namespace NG.DomainEvents.Jobs;

public class DomainEventRelayJob<TDbContext> : BackgroundService where TDbContext : DomainEventsDbContext<TDbContext> {
    readonly IServiceProvider _serviceProvider;

    public DomainEventRelayJob(IServiceProvider serviceProvider) { _serviceProvider = serviceProvider; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {

            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }
    }
}
