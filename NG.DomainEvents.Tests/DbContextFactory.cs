using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NG.DomainEvents.Data;
using NG.DomainEvents.Helpers;
using NG.DomainEvents.Tests.Fixtures;

namespace NG.DomainEvents.Tests;

public class DbContextFactory : IDesignTimeDbContextFactory<UnitTestingDbContext> {
    public UnitTestingDbContext CreateDbContext(string[] args) {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        var connectionString = config.GetConnectionString("Default") ?? args[0];
        var dbContextOptions = new DbContextOptionsBuilder<UnitTestingDbContext>()
            .UseNpgsql(connectionString, opts => {
            })
            .Options;
        var relayService = DomainEventRelayService.GetInstance();
        return new UnitTestingDbContext(dbContextOptions, StartupExtensions.GetMappingConfig(GetType().Assembly), relayService);
    }
}
