using Microsoft.EntityFrameworkCore;
using NG.DomainEvents.Config;
using NG.DomainEvents.Data;
using NG.DomainEvents.Tests.Model;

namespace NG.DomainEvents.Tests.Fixtures;

public class UnitTestingDbContext : DomainEventsDbContext<UnitTestingDbContext> {
    public DbSet<ArticleDto> Articles { get; set; }

    public UnitTestingDbContext(DbContextOptions<UnitTestingDbContext> options, DomainEventsMappingConfig mappingConfig,
                                DomainEventRelayService relayService)
        : base(options, mappingConfig, relayService) {
    }
}
