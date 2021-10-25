using System;
using System.Threading;
using System.Threading.Tasks;
using DataAnalytics.Core.Serialisation;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace DataAnalytics.Core.Events
{
    public static class EventHandler
    {
        public static IServiceCollection AddEventHandler(
            this IServiceCollection services,
            Func<ResolvedEvent, CancellationToken, Task> handler,
            string? eventType = null
        )
        {
            services.AddScoped<Func<ResolvedEvent, CancellationToken, Task>>(
                _ => (resolvedEvent, ct) =>
                {
                    if (eventType != null && resolvedEvent.Event.EventType != eventType)
                        return Task.CompletedTask;

                    return handler(@resolvedEvent, ct);
                }
            );

            return services;
        }

        public static IServiceCollection AddEventHandler<TEvent>(
            this IServiceCollection services,
            Func<TEvent, CancellationToken, Task> handler
        )
        {
            var eventType = EventTypeMapper.ToName<TEvent>();

            services.AddScoped<Func<ResolvedEvent, CancellationToken, Task>>(
                _ => (resolvedEvent, ct) =>
                {
                    if (resolvedEvent.Event.EventType != eventType)
                        return Task.CompletedTask;

                    var @event = resolvedEvent.DeserializeData<TEvent>();

                    return handler(@event, ct);
                }
            );

            return services;
        }

        public static IServiceCollection AddEventHandler<TEvent, TEventMetadata>(
            this IServiceCollection services,
            Func<TEvent, TEventMetadata, CancellationToken, Task> handler
        )
        {
            var eventType = EventTypeMapper.ToName<TEvent>();

            services.AddScoped<Func<ResolvedEvent, CancellationToken, Task>>(
                _ => (resolvedEvent, ct) =>
                {
                    if (resolvedEvent.Event.EventType != eventType)
                        return Task.CompletedTask;

                    var @event = resolvedEvent.DeserializeData<TEvent>();
                    var metadata = resolvedEvent.DeserializeMetadata<TEventMetadata>();

                    return handler(@event, metadata, ct);
                }
            );

            return services;
        }
    }
}
