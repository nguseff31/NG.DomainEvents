using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using NG.DomainEvents.Common;
using NG.DomainEvents.Config;

namespace NG.DomainEvents.Data {
    public abstract class DomainEventsDbContext<TContext, TEventDto, TResultDto> : DbContext where TContext : DbContext
        where TEventDto : DomainEventDto
        where TResultDto : DomainEventResultDto
    {
        protected DomainEventsMappingConfig MappingConfig;
        protected IOptionsSnapshot<DomainEventsConfig> DomainEventsConfig;
        
        protected DomainEventsDbContext(DbContextOptions<TContext> options, DomainEventsMappingConfig mappingConfig, IOptionsSnapshot<DomainEventsConfig> domainEventsConfig) : base(options)
        {
            MappingConfig = mappingConfig;
            DomainEventsConfig = domainEventsConfig;
        }

        public DbSet<TEventDto> Events { get; set; }
        
        public DbSet<TResultDto> EventResults { get; set; }

        [Obsolete("Use async version")]
        public override int SaveChanges(bool acceptAllChangesOnSuccess) {
            return SaveChangesAndEventsAsync(acceptAllChangesOnSuccess, CancellationToken.None).Result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) {
            return await SaveChangesAndEventsAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        async Task<int> SaveChangesAndEventsAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) {
            var entities = ChangeTracker.Entries<IEntityWithDomainEvents>()
                .Where(e => e.State == EntityState.Added)
                .Select(x => (
                    Entry: x,
                    DomainEvents: x.Entity.TakeAllEvents()
                ))
                .ToArray();

            if (!entities.Any()) {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            
            // save entities to fill auto generated ids
            var saveResult = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            var newEventsCount = await AddDomainEventsToDb(entities, cancellationToken);
            if (newEventsCount > 0) {
                saveResult += await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            
            return saveResult;
        }
        
        async Task<int> AddDomainEventsToDb(
            IEnumerable<(EntityEntry<IEntityWithDomainEvents> Entry,
            IReadOnlyCollection<DomainEvent> DomainEvents)> entities,
            CancellationToken cancellationToken)
        {
            var eventCount = 0;
            foreach (var item in entities)
            foreach (var domainEvent in item.DomainEvents) {
                domainEvent.SetEntity(item.Entry.Entity);
                var mappings = MappingConfig.GetMapping(domainEvent.GetType());
                var domainEventEntity = new DomainEventDto {
                    EntityTableName = item.Entry.Metadata.GetTableName()!,
                    EntityId = item.Entry.IsKeySet ? item.Entry.Entity.GetEntityId() : null,
                    EventType = mappings?.EntityType,
                    ShouldExecute = true // todo move to config 
                };
                await AddAsync(domainEventEntity, cancellationToken);
                domainEventEntity.SetEvent(domainEvent);
                eventCount++;
            }

            return eventCount;
        }
    }
}
