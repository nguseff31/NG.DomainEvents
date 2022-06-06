using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NG.DomainEvents.Tests.Fixtures;
using NG.DomainEvents.Tests.Model;
using Xunit;

namespace NG.DomainEvents.Tests;

public class DbContextEventTests : IClassFixture<DbFixture> {
    DbFixture _fixture;

    public DbContextEventTests(DbFixture fixture) {
        _fixture = fixture;
    }

    [Fact]
    public async Task CheckCreatedEvent() {
        var articles = new ArticleDto { Published = false, Title = "atata" };

        _fixture.Db.Add(articles);
        await _fixture.Db.SaveChangesAsync();

        var events = await _fixture.Db.Events.Where(e => e.EntityId == articles.Id.ToString())
            .ToArrayAsync();
        Assert.Single(events);
    }

    [Fact]
    public async Task StateMachineTest() {

    }

    async IAsyncEnumerable<SomeData> GetSomeData() {
        foreach (var i in Enumerable.Range(0, 10)) {
            yield return new SomeData {

            };
        }
    }
    async Task<int> GetAnotherData(int data) {
        await Task.Delay(1);
        return data + data;
    }

    class SomeData {
        public int Data { get; set; }

        public Lazy<int> AnotherData { get; set; }
    }
}

class OrderCreatedEvent {
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
