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
}
