﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NG.DomainEvents.Config;
using NG.DomainEvents.Helpers;

namespace NG.DomainEvents.Tests.Fixtures;

public class DbFixture : IDisposable {
    public UnitTestingDbContext Db { get; private set; }

    public DbFixture() {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        var dbContextOptions = new DbContextOptionsBuilder<UnitTestingDbContext>()
            .UseNpgsql(config.GetConnectionString("Default"), opts => {
            })
            .Options;
        Db = new UnitTestingDbContext(dbContextOptions, StartupExtensions.GetMappingConfig(typeof(DbFixture).Assembly));
        Db.Database.Migrate();
    }

    public void Dispose() {
        Db.Dispose();
    }
}
