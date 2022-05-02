using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NG.DomainEvents.Config;
using NG.DomainEvents.Data;

namespace NG.DomainEvents.Tests.Fixtures;

public class UnitTestingDbContext : DomainEventsDbContext<UnitTestingDbContext, DomainEventDto, DomainEventResultDto>
{
    public UnitTestingDbContext(DbContextOptions<UnitTestingDbContext> options, DomainEventsMappingConfig mappingConfig, IOptionsSnapshot<DomainEventsConfig> domainEventsConfig) : base(options, mappingConfig, domainEventsConfig)
    {
    }
}