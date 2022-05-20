using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NG.DomainEvents.Common;
using NG.DomainEvents.Config;

namespace NG.DomainEvents.Data {
    public abstract class DomainEventsDbContext<TContext, TEventDto, TResultDto> : DbContext where TContext : DbContext
        where TEventDto : DomainEventDto
        where TResultDto : DomainEventResultDto
    {
        protected DomainEventsMappingConfig MappingConfig;
        
        protected DomainEventsDbContext(DbContextOptions<TContext> options,
            DomainEventsMappingConfig mappingConfig) : base(options)
        {
            MappingConfig = mappingConfig;
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
            await foreach (var @event in AddDomainEventsToDb(entities, cancellationToken)) {
                //todo fire events                    
            }
            var domainEventResult = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            return saveResult + domainEventResult;
        }
        
        async IAsyncEnumerable<DomainEvent> AddDomainEventsToDb(
            IEnumerable<(EntityEntry<IEntityWithDomainEvents> Entry,
            IReadOnlyCollection<DomainEvent> DomainEvents)> entities,
            CancellationToken cancellationToken)
        {
            foreach (var item in entities)
            {
                var order = 0;
                foreach (var domainEvent in item.DomainEvents)
                {
                    domainEvent.SetEntity(item.Entry.Entity);
                    var mappings = MappingConfig.GetMapping(domainEvent.GetType());
                    var domainEventEntity = new DomainEventDto
                    {
                        EntityTableName = item.Entry.Metadata.GetTableName()!,
                        EntityId = item.Entry.IsKeySet ? item.Entry.Entity.GetEntityId() : null,
                        EventType = mappings?.EntityType,
                        ShouldExecute = true,
                        Order = order++
                    };
                    domainEventEntity.SetEvent(domainEvent);
                    await AddAsync(domainEventEntity, cancellationToken);
                    yield return domainEvent;
                }
            }
        }
    }
}
