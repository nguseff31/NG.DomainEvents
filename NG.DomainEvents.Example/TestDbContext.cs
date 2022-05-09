using NG.DomainEvents.Example.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NG.DomainEvents.Config;
using NG.DomainEvents.Data;
using NG.DomainEvents.Example.Models.Domain;

namespace NG.DomainEvents.Example;

public class TestDbContext : DomainEventsDbContext<TestDbContext, DomainEventDto, DomainEventResultDto>
{
    public DbSet<UserDto> Users { get; set; }
    
    public TestDbContext(DbContextOptions<TestDbContext> options,
        DomainEventsMappingConfig mappingConfig,
        IOptionsSnapshot<DomainEventsConfig> domainEventsConfig,
        IServiceProvider serviceProvider) : base(options, mappingConfig, domainEventsConfig, serviceProvider)
    {
    }
}