using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NG.DomainEvents.Common;
using NG.DomainEvents.Config;
#pragma warning disable CS8618

namespace NG.DomainEvents.Data {
    public abstract class DomainEventsDbContext<TContext> : DbContext where TContext : DomainEventsDbContext<TContext> {
        readonly DomainEventsMappingConfig _mappingConfig;
        readonly DomainEventRelayService _relayService;

        protected DomainEventsDbContext(DbContextOptions<TContext> options, DomainEventsMappingConfig mappingConfig, DomainEventRelayService relayService) : base(options) {
            _mappingConfig = mappingConfig;
            _relayService = relayService;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<DomainEventDto> Events { get; set; }

        public DbSet<DomainEventResultDto> EventResults { get; set; }

        public DbSet<DomainEventHandlerQueueDto> EventHandlerQueue { get; set; }

        [Obsolete("Use async version")]
        public override int SaveChanges(bool acceptAllChangesOnSuccess) {
            return SaveChangesAndEventsAsync(acceptAllChangesOnSuccess, CancellationToken.None).Result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) {
            return await SaveChangesAndEventsAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public async Task<List<DomainEventHandlerQueueDto>> Store(IEnumerable<DomainEventDto> domainEvents, DateTime utcNow, bool enqueue, CancellationToken cancellationToken) {
            var eventArray = domainEvents.ToArray();
            await AddRangeAsync(eventArray, cancellationToken);
            if (enqueue) {
                var queuedHandlers = GetQueuedHandlers(eventArray, utcNow).ToList();
                await AddRangeAsync(queuedHandlers, cancellationToken);
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

            var queueItems = await EventHandlerQueue.FromSqlRaw(sql,
                utcNow,
                utcNow - TimeSpan.FromSeconds(30),
                utcNow - TimeSpan.FromMinutes(5)
            ).ToArrayAsync(cancellationToken);
            return queueItems;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<DomainEventHandlerQueueDto>()
                .HasKey("HandlerType", "EventId");
        }

        IEnumerable<DomainEventHandlerQueueDto> GetQueuedHandlers(IEnumerable<DomainEventDto> entitiesWithEvents, DateTime utcNow) {
            return entitiesWithEvents
                .Select(e => new { @event = e, mapping = _mappingConfig.GetMapping(e.EventType) })
                .SelectMany(e => e.mapping.Handlers.Select(h => new DomainEventHandlerQueueDto {
                    Event = e.@event,
                    HandlerType = h.HandlerType,
                    Order = e.@event.Order,
                    BucketId = e.@event.EntityId,
                    Enqueued = utcNow
                }));
        }
        async Task<int> SaveChangesAndEventsAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) {
            var events = ChangeTracker.Entries<IEntityWithDomainEvents>()
                .Where(e => e.State == EntityState.Added)
                .Select(x => (
                    Entry: x,
                    DomainEvents: x.Entity.TakeAllEvents()
                ));
            var eventsWithoutEntities = ChangeTracker.Entries<DomainEventDto>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToArray();
            var savedResult = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            var eventsWithEntities = GetEventEntities(events).ToArray();

            if (eventsWithEntities.Any()) {
                var now = DateTime.UtcNow;
                var queued = await Store(eventsWithEntities, now, true, cancellationToken);
                var queued2 = await Store(eventsWithoutEntities, now, true, cancellationToken);

                var domainEventResult = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                _relayService.Push(queued.Select(e => (e.EventId, e.HandlerType)));
                _relayService.Push(queued2.Select(e => (e.EventId, e.HandlerType)));

                return savedResult + domainEventResult;
            }

            return savedResult;
        }

        IEnumerable<DomainEventDto> GetEventEntities(IEnumerable<(EntityEntry<IEntityWithDomainEvents> Entry, IReadOnlyCollection<DomainEvent> DomainEvents)> entities) {
            foreach (var item in entities) {
                var order = 0;
                foreach (var domainEvent in item.DomainEvents) {
                    domainEvent.SetEntity(item.Entry.Entity);
                    var mappings = _mappingConfig.GetMapping(domainEvent.GetType());
                    var domainEventEntity = new DomainEventDto {
                        EntityId = item.Entry.Entity.GetEntityId(),
                        EventType = mappings.EntityType,
                        // todo correlationId
                        Order = order++
                    };
                    domainEventEntity.SetEvent(domainEvent);
                    yield return domainEventEntity;
                }
            }
        }
    }
}
