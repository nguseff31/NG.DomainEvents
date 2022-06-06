using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NG.DomainEvents.Common.Exceptions;
using NG.DomainEvents.Config;
using NG.DomainEvents.Data;

namespace NG.DomainEvents.Jobs;

public class DomainEventProcessingJob<TDbContext> : BackgroundService where TDbContext : DomainEventsDbContext<TDbContext> {
    readonly IServiceProvider _serviceProvider;
    readonly ILogger _logger;
    readonly DomainEventsMappingConfig _mappingsConfig; // todo to config
    readonly DomainEventRelayService _relayService;

    public DomainEventProcessingJob(IServiceProvider serviceProvider, ILogger<DomainEventProcessingJob<TDbContext>> logger, DomainEventsMappingConfig mappingsConfig, DomainEventRelayService relayService) {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _mappingsConfig = mappingsConfig;
        _relayService = relayService;
    }

    protected override async Task ExecuteAsync(CancellationToken ct) {
        // Получаем события
        await EnqueueRetriedEventsAsync(ct);

        // Обрабатываем очередь событий

        while (!ct.IsCancellationRequested) {
            var events = _relayService.Take();
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            foreach (var ev in events) {
                try {
                    var domainEvent = await context.Events.SingleAsync(e => e.Id == ev.eventId, ct);
                    await ProcessEventAsync(domainEvent, ev.handlerType, scope, ct);

                    var qi = new DomainEventHandlerQueueDto { EventId = ev.eventId, HandlerType = ev.handlerType };
                    context.Remove(qi);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, ""); // todo

                    var qi = new DomainEventHandlerQueueDto { EventId = ev.eventId, HandlerType = ev.handlerType };
                    var entry = context.Attach(qi);
                    entry.State = EntityState.Modified;
                    qi.Enqueued = null;
                    entry.Property(e => e.Enqueued);
                }
            }

            await context.SaveChangesAsync(ct);

            await Task.Delay(TimeSpan.FromSeconds(30), ct);
        }
    }

    async Task EnqueueRetriedEventsAsync(CancellationToken ct) {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var queueItems = await context.TakeHandlers(DateTime.UtcNow, ct);
        foreach (var qi in queueItems.GroupBy(q => q.BucketId)) {
            _relayService.Push(qi.Select(q => (q.EventId, q.HandlerType)));
        }
    }


    async Task<bool> ProcessEventAsync(DomainEventDto @event, string handlerType, IServiceScope eventScope, CancellationToken cancellationToken) {
        var eventMapping = _mappingsConfig.GetMappingOrDefault(@event.EventType);
        if (eventMapping == null) {
            throw new InvalidOperationException("");
        }

        var eventData = @event.GetEvent(eventMapping.AssemblyType);
        var handlerMapping = eventMapping.GetHandler(handlerType);

        if (eventData != null) {
            var handler = eventScope.ServiceProvider.GetRequiredService(handlerMapping.AssemblyType);

            try {
                var task = (Task)handlerMapping.AssemblyType.GetMethod("Handle")!
                    .Invoke(handler, new object[] { eventData, cancellationToken })!;
                await task;
                return true;
            }
            catch (Exception ex) {
                if (ex is ITaskResultException) {
                    // todo deal with mediatr error handling
                    return true;
                }

                _logger.LogError(ex,
                    "Unhandled Error on processing event type:`{EventType}` id:`{EntityId}` eventId: {EventId}",
                    @event.EventType,
                    eventData.EntityId,
                    eventData.Id
                );
                return false;
            }
        }

        throw new InvalidOperationException("Handler was not found");
    }
}
