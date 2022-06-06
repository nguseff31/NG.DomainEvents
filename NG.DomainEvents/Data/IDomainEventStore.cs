using Microsoft.EntityFrameworkCore;
using NG.DomainEvents.Config;

namespace NG.DomainEvents.Data;

/// <summary>
/// Workng with domain events tables in database
/// </summary>

public class DomainEventStore<TDbContext>
 where TDbContext : DomainEventsDbContext<TDbContext> {
    readonly TDbContext _dbContext; // todo cirular dependency
    readonly DomainEventsMappingConfig _mappingConfig;

    public DomainEventStore(TDbContext dbContext, DomainEventsMappingConfig mappingConfig) {
        _dbContext = dbContext;
        _mappingConfig = mappingConfig;
    }

    public async Task<List<DomainEventHandlerQueueDto>> Store(IEnumerable<DomainEventDto> domainEvents, DateTime utcNow, bool enqueue, CancellationToken cancellationToken) {
        var eventArray = domainEvents.ToArray();
        await _dbContext.AddRangeAsync(eventArray, cancellationToken);
        if (enqueue) {
            var queuedHandlers = GetQueuedHandlers(eventArray, utcNow).ToList();
            await _dbContext.AddRangeAsync(queuedHandlers, cancellationToken);
            return queuedHandlers;
        }
        return new List<DomainEventHandlerQueueDto>();
    }

    public async Task<IEnumerable<DomainEventHandlerQueueDto>> TakeHandlers(DateTime utcNow, CancellationToken cancellationToken) {
        var sql = @"
update domain_event_handler_queue as q set ""Enqueued"" = {0}
from domain_event as e
where (q.""Enqueued"" is null or q.""Enqueued"" < {1}) and q.""EventId"" = e.""Id"" and q.""Created"" > {2}
returning q.*";

        var queueItems = await _dbContext.EventHandlerQueue.FromSqlRaw(sql,
            utcNow,
            utcNow - TimeSpan.FromSeconds(30),
            utcNow - TimeSpan.FromMinutes(5)
        ).ToArrayAsync(cancellationToken);
        return queueItems;
    }

    IEnumerable<DomainEventHandlerQueueDto> GetQueuedHandlers(IEnumerable<DomainEventDto> entitiesWithEvents, DateTime utcNow) {
        return entitiesWithEvents
            .Select(e => new { @event = e, mapping = _mappingConfig.GetMapping(e.EventType) })
            .SelectMany(e => e.mapping.Handlers.Select(h => new DomainEventHandlerQueueDto {
                EventId = e.@event.Id,
                HandlerType = h.HandlerType,
                Order = e.@event.Order,
                BucketId = e.@event.EntityId,
                Enqueued = utcNow
            }));
    }
}
