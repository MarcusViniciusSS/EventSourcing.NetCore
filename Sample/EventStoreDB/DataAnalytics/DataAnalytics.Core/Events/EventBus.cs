using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace DataAnalytics.Core.Events
{
    public interface IEventBus
    {
        Task Publish(ResolvedEvent @event, CancellationToken ct);
    }

    public class EventBus: IEventBus
    {
        private readonly IServiceProvider serviceProvider;
        private readonly AsyncRetryPolicy retryPolicy;
        private readonly IEnumerable<Func<ResolvedEvent, CancellationToken, Task>> eventHandlers;

        public EventBus(
            IServiceProvider serviceProvider,
            AsyncRetryPolicy retryPolicy,
            IEnumerable<Func<ResolvedEvent, CancellationToken, Task>> eventHandlers
        )
        {
            this.serviceProvider = serviceProvider;
            this.retryPolicy = retryPolicy;
            this.eventHandlers = eventHandlers;
        }

        public async Task Publish(ResolvedEvent @event, CancellationToken ct)
        {
            using var scope = serviceProvider.CreateScope();

            foreach (var handle in eventHandlers)
            {
                await retryPolicy.ExecuteAsync(async token =>
                {
                    await handle(@event, token);
                }, ct);
            }
        }
    }

    public static class EventBusExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services) =>
            services.AddSingleton<IEventBus, EventBus>(sp =>
                new EventBus(
                    sp,
                    Policy.Handle<Exception>().RetryAsync(3),
                    sp.GetServices<Func<ResolvedEvent, CancellationToken, Task>>()
                )
            );
    }
}
