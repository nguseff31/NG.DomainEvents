using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NG.DomainEvents.Common.Exceptions;
using NG.DomainEvents.Config;
using NG.DomainEvents.Data;

namespace NG.DomainEvents.Jobs;

public class DomainEventsRetryJob<TDbContext> : BackgroundService
    where TDbContext : DomainEventsDbContext<TDbContext, DomainEventDto, DomainEventResultDto>
{
    private ILogger _logger;
    private DomainEventsMappingConfig _eventsMappingConfig;
    private IServiceProvider _serviceProvider;
    private IOptionsMonitor<DomainEventsConfig> _eventsConfig;

    public DomainEventsRetryJob(
        ILogger<DomainEventsRetryJob<TDbContext>> logger,
        DomainEventsMappingConfig eventsMappingConfig,
        IServiceProvider serviceProvider,
        IOptionsMonitor<DomainEventsConfig> eventsConfig)
    {
        _logger = logger;
        _eventsMappingConfig = eventsMappingConfig;
        _serviceProvider = serviceProvider;
        _eventsConfig = eventsConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteInteration(cancellationToken);
            } catch(Exception e)
            {
                _logger.LogError(e, "Failed on retry domain events");
            }
            await Task.Delay(_eventsConfig.CurrentValue.RetryInterval, cancellationToken);
        }
    }

    private async Task ExecuteInteration(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var eventsToExecute = await dbContext.Events
            .Where(e => e.ShouldExecute)
            .ToListAsync(cancellationToken);

        
        foreach (var @event in eventsToExecute)
        {
            using var eventScope = _serviceProvider.CreateScope();
            if (@event.Retries >= _eventsConfig.CurrentValue.MaxRetries)
            {
                @event.ShouldExecute = false;
                continue; // todo move retries to handlers
            }
            var eventMapping = _eventsMappingConfig.Mappings.FirstOrDefault(e => e.EntityType == @event.EventType);
            if (eventMapping != null)
            {
                var eventData = @event.GetEvent(eventMapping.AssemblyType);
                if (eventData != null)
                {
                    var mediator = eventScope.ServiceProvider.GetRequiredService<IMediator>();
                    try
                    {
                        @event.Retries++;
                        await mediator.Publish(eventData, cancellationToken);
                        @event.ShouldExecute = false;
                    }
                    catch (Exception ex)
                    {
                        if (ex is ITaskResultException)
                        {
                            @event.ShouldExecute = false;                            
                        }
                    }
                }
            }
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}