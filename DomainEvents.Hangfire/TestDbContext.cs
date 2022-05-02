using DomainEvents.Hangfire.Models;
using DomainEvents.Hangfire.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NG.DomainEvents.Config;
using NG.DomainEvents.Data;

namespace DomainEvents.Hangfire;

public class TestDbContext : DomainEventsDbContext<TestDbContext, DomainEventDto, DomainEventResultDto>
{
    public DbSet<UserDto> Users { get; set; }

    
    public TestDbContext(DbContextOptions<TestDbContext> options, DomainEventsMappingConfig mappingConfig, IOptionsSnapshot<DomainEventsConfig> domainEventsConfig) : base(options, mappingConfig, domainEventsConfig)
    {
    }
}