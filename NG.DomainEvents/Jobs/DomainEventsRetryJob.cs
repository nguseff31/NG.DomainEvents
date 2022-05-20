using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NG.DomainEvents.Common.Exceptions;
using NG.DomainEvents.Config;
using NG.DomainEvents.Data;

namespace NG.DomainEvents.Jobs;

public class DomainEventsRetryJob<TDbContext> : BackgroundService
    where TDbContext : DomainEventsDbContext<TDbContext, DomainEventDto, DomainEventResultDto> {
    private readonly ILogger _logger;
    private readonly DomainEventsMappingConfig _eventsMappingConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<DomainEventsConfig> _eventsConfig;

    public DomainEventsRetryJob(
        ILogger<DomainEventsRetryJob<TDbContext>> logger,
        DomainEventsMappingConfig eventsMappingConfig,
        IServiceProvider serviceProvider,
        IOptionsMonitor<DomainEventsConfig> eventsConfig) {
        _logger = logger;
        _eventsMappingConfig = eventsMappingConfig;
        _serviceProvider = serviceProvider;
        _eventsConfig = eventsConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            try {
                await ExecuteInteration(cancellationToken);
            } catch (Exception e) {
                _logger.LogError(e, "Failed on retry domain events");
            }

            await Task.Delay(_eventsConfig.CurrentValue.RetryInterval, cancellationToken);
        }
    }

    private async Task ExecuteInteration(CancellationToken cancellationToken) {
        using var scope = _serviceProvider.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var eGroups = await db.Events
            .Where(e => e.ShouldExecute)
            .GroupBy(e => new { e.EntityId, e.EventType })
            .ToListAsync(cancellationToken);

        var tasks = new List<Task>(eGroups.Count);
        foreach (var eGroup in eGroups) {
            var egTask = Task.Run(async () => {
                using var eventScope = _serviceProvider.CreateScope();
                foreach (var @event in eGroup.OrderBy(e => e.Order)) {
                    cancellationToken.ThrowIfCancellationRequested();
                    await ProcessEventAsync(@event, eventScope, cancellationToken);
                }
            }, cancellationToken);
            tasks.Add(egTask);
        }

        try {
            await Task.WhenAll(tasks);
        } finally {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ProcessEventAsync(DomainEventDto @event, IServiceScope eventScope,
        CancellationToken cancellationToken) {
        if (@event.Retries >= _eventsConfig.CurrentValue.MaxRetries) {
            @event.ShouldExecute = false;
            return;
        }

        var eventMapping = _eventsMappingConfig.GetMapping(@event.EventType);
        if (eventMapping == null) {
            @event.ShouldExecute = false; // todo true or false?
            _logger.LogError("Event mapping was not found");
            return;
        }

        var eventData = @event.GetEvent(eventMapping.AssemblyType);
        if (eventData != null) {
            var mediator = eventScope.ServiceProvider.GetRequiredService<IMediator>();
            try {
                @event.Retries++;
                await mediator.Publish(eventData, cancellationToken);
                @event.ShouldExecute = false;
            } catch (Exception ex) {
                if (ex is ITaskResultException) {
                    @event.ShouldExecute = false;
                } else {
                    _logger.LogError(ex,
                        "Unhandled Error on processing event type:`{EventType}` id:`{EntityId}` eventId: {EventId}",
                        @event.EventType,
                        eventData.EntityId,
                        eventData.Id
                    );
                }
            }
        }
    }
}
