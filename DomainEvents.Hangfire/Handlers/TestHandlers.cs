using System.Diagnostics;
using DomainEvents.Hangfire.Models;
using DomainEvents.Hangfire.Models.Events;
using NG.DomainEvents.Common;
using NG.DomainEvents.Config;

namespace DomainEvents.Hangfire.Handlers;

[DomainEventHandler("test_handler_1")]
public class TestHandler1 : DomainEventHandler<TestDbContext, UserCreated, TestHandler1.Result>
{
    public override Task<Result> HandleAsync(UserCreated @event, CancellationToken cancellationToken)
    {
        Trace.WriteLine("User created whohoo");
        return Task.FromResult(new Result());
    }

    public class Result : DomainEventResult { }

    public TestHandler1(TestDbContext dbContext, ILogger<TestHandler1> logger, DomainEventsMappingConfig mappingConfig) : base(dbContext, logger, mappingConfig)
    {
    }
}

[DomainEventHandler("test_handler_2")]
public class TestHandler2 : DomainEventHandler<TestDbContext, UserCreated, TestHandler2.Result>
{
    public class Result : DomainEventResult { }

    public TestHandler2(TestDbContext dbContext, ILogger<TestHandler2> logger , DomainEventsMappingConfig mappingConfig) : base(dbContext, logger, mappingConfig)
    {
    }

    public override Task<Result> HandleAsync(UserCreated @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}