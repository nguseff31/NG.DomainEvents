using System.Diagnostics;
using System.Runtime.ExceptionServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NG.DomainEvents.Common.Exceptions;
using NG.DomainEvents.Config;
using NG.DomainEvents.Data;

namespace NG.DomainEvents.Common;

public abstract class DomainEventHandler<TDbContext, TEvent, TResult> : INotificationHandler<TEvent>
    where TEvent : DomainEvent
    where TDbContext : DomainEventsDbContext<TDbContext>
    where TResult : DomainEventResult, new() {
    protected readonly TDbContext DbContext;
    protected readonly ILogger Logger;
    protected readonly DomainEventsMappingConfig MappingConfig;

    protected DomainEventHandler(
        TDbContext dbContext,
        ILogger<INotificationHandler<TEvent>> logger,
        DomainEventsMappingConfig mappingConfig) {
        DbContext = dbContext;
        Logger = logger;
        MappingConfig = mappingConfig;
    }

    public async Task Handle(TEvent domainEvent, CancellationToken cancellationToken) {
        var handlerMap = MappingConfig.Mappings
            .SelectMany(e => e.Handlers)
            .FirstOrDefault(h => h.AssemblyType == GetType());
        if (handlerMap == null) {
            throw new ArgumentException("Handler map was not found for type {HandlerType}", GetType().FullName);
        }

        var isCompleted = await DbContext.Set<DomainEventResultDto>()
            .AnyAsync(x =>
                x.DomainEventId == domainEvent.Id
                && x.Handler == handlerMap.HandlerType
                && x.Completed, cancellationToken);

        if (isCompleted) {
            Logger.LogTrace("Handler `{handler}` already completed for domain event `{eventId}`", handlerMap.HandlerType, domainEvent.Id);
            return;
        }

        var watch = new Stopwatch();
        watch.Start();

        var result = await GetResult(domainEvent, cancellationToken);
        watch.Stop();
        result.Elapsed = watch.Elapsed;
        result.Handler = handlerMap.HandlerType;
        try {
            var domainEventResult = new DomainEventResultDto {
                DomainEventId = domainEvent.Id,
                Completed = !result.NeedRetry,
                Elapsed = result.Elapsed,
                Handler = result.Handler
            };
            domainEventResult.SetResult(result);
            await DbContext.AddAsync(domainEventResult, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e) {
            Logger.LogCritical(e, "Failed to save result of handling domain event `{id}`", domainEvent.Id);
            if (result.NeedRetry && result.ExceptionInstance != null) {
                throw new AggregateException(e, result.ExceptionInstance);
            }

            throw;
        }

        if (result.NeedRetry) {
            throw new NeedRetryException(result.ExceptionInstance);
        }

        if (result.ExceptionInstance != null) {
            ExceptionDispatchInfo.Capture(result.ExceptionInstance).Throw();
        }
    }

    async Task<TResult> GetResult(TEvent domainEvent, CancellationToken cancellationToken) {
        try {
            return await HandleAsync(domainEvent, cancellationToken);
        }
        catch (Exception e) {
            LogException(domainEvent, e);
            return e switch {
                NotNeededException => new TResult { NotNeeded = true },
                AlreadyDoneException => new TResult { AlreadyDone = true },
                _ => new TResult {
                    ExceptionInstance = e,
                    NeedRetry = e is not ITaskResultException
                }
            };
        }
    }

    private void LogException(TEvent domainEvent, Exception e) {
        var level = GetLogLevel(e);
        if (level == LogLevel.Error) {
            Logger.LogError(e, "Failed to handle domain event `{id}`", domainEvent.Id);
        }
        else {
            Logger.Log(level, "Failed to handle domain event `{id}`: {error}", domainEvent.Id, e.Message);
        }
    }

    private LogLevel GetLogLevel(Exception exception) {
        if (exception is not ITaskResultException) {
            return LogLevel.Error;
        }

        return LogLevel.Information;
    }

    public abstract Task<TResult> HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
