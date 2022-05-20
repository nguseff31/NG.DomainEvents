using Microsoft.EntityFrameworkCore;
using NG.DomainEvents.Config;
using NG.DomainEvents.Data;
using NG.DomainEvents.Example.Models.Domain;
#pragma warning disable CS8618

namespace NG.DomainEvents.Example;

public class TestDbContext : DomainEventsDbContext<TestDbContext, DomainEventDto, DomainEventResultDto> {
    public DbSet<UserDto> Users { get; set; }

    public TestDbContext(DbContextOptions<TestDbContext> options,
                         DomainEventsMappingConfig mappingConfig) : base(options, mappingConfig) {
    }
}
