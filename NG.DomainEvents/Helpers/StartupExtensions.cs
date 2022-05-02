﻿using System.Reflection;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NG.DomainEvents.Common;
using NG.DomainEvents.Config;
using NG.DomainEvents.Data;
using NG.DomainEvents.Jobs;

namespace NG.DomainEvents.Helpers;

public static class StartupExtensions
{
    public static void AddDomainEvents<TDbContext>(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
        where TDbContext : DomainEventsDbContext<TDbContext, DomainEventDto, DomainEventResultDto>
    {
        services.AddMediatR(assembly);
        services.Configure<DomainEventsConfig>(configuration.GetSection("DomainEvents"));
        services.AddDomainEventMappings(assembly);
        var retryJobEnabled = configuration.GetValue("DomainEvents:RetryJobEnabled", true);
        if (retryJobEnabled)
        {
            services.AddHostedService<DomainEventsRetryJob<TDbContext>>();
        }
    }
    
    public static void AddDomainEventMappings(this IServiceCollection services, Assembly assembly)
    {
        var eventTypes = assembly.GetExportedTypes()
            .Where(t => t.IsInstanceOfType(typeof(DomainEvent)));
        var config = new DomainEventsMappingConfig
        {
            Mappings = eventTypes.Select(et => new DomainEventTypeMapping
            {
                AssemblyType = et,
                EntityType = GetEntityType(et),
                Handlers = assembly.GetExportedTypes()
                    .Where(eh => eh.BaseType?.GetGenericTypeDefinition() == typeof(DomainEventHandler<,,>))
                    .Select(eh => new DomainEventHandlerMappings()
                    {
                        AssemblyType = eh,
                        HandlerType = GetHandlerType(eh)
                    })
                    .ToList()
            }).ToList()
        };
        services.AddSingleton(config);
    }
    
    
    static string GetEntityType(Type t)
    {
        var attribute = t.GetCustomAttribute(typeof(DomainEventAttribute)) as DomainEventAttribute;
        return attribute?.EventType ?? t.Name;
    }

    static string GetHandlerType(Type t)
    {
        var attribute = t.GetCustomAttribute(typeof(DomainEventHandlerAttribute)) as DomainEventHandlerAttribute;
        return attribute?.HandlerName ?? t.Name;
    }
}