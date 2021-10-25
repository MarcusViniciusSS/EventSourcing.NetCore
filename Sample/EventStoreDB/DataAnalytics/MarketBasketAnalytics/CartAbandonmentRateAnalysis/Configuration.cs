using System;
using System.Threading;
using System.Threading.Tasks;
using DataAnalytics.Core.Entities;
using DataAnalytics.Core.Events;
using EventStore.Client;
using MarketBasketAnalytics.Carts;
using Microsoft.Extensions.DependencyInjection;

namespace MarketBasketAnalytics.CartAbandonmentRateAnalysis
{
    public static class Configuration
    {
        public static IServiceCollection AddCartAbandonmentRateAnalysis(this IServiceCollection services) =>
            services
                .AddEventHandler<ShoppingCartAbandoned>(async (sp, shoppingCartAbandoned, ct) =>
                {
                    var eventStore = sp.GetRequiredService<EventStoreClient>();

                    var @event = await CartAbandonmentRate.Handle(
                        eventStore.AggregateStream,
                        shoppingCartAbandoned,
                        ct
                    );

                    await eventStore.AppendToNewStream(
                        CartAbandonmentRate.ToStreamId(shoppingCartAbandoned.ShoppingCartId),
                        @event,
                        ct
                    );
                })
                .AddEventHandler<CartAbandonmentRateCalculated>(async (sp, shoppingCartAbandoned, ct) =>
                {
                    var eventStore = sp.GetRequiredService<EventStoreClient>();

                    var streamId = CartAbandonmentRatesSummary.StreamId;

                    var @event = await CartAbandonmentRatesSummary.Handle(
                        token => eventStore.ReadLastEvent<CartAbandonmentRatesSummary>(streamId, token),
                        shoppingCartAbandoned,
                        ct
                    );

                    await eventStore.AppendToStreamWithSingleEvent(
                        streamId,
                        @event,
                        ct
                    );
                })
                .AddEventHandler<ShoppingCartConfirmed>(async (sp, shoppingCartConfirmed, ct) =>
                {
                    var eventStore = sp.GetRequiredService<EventStoreClient>();

                    var streamId = CartAbandonmentRatesSummary.StreamId;

                    var @event = await CartAbandonmentRatesSummary.Handle(
                        token => eventStore.ReadLastEvent<CartAbandonmentRatesSummary>(streamId, token),
                        shoppingCartConfirmed,
                        ct
                    );

                    await eventStore.AppendToStreamWithSingleEvent(
                        streamId,
                        @event,
                        ct
                    );
                });

    }
}
