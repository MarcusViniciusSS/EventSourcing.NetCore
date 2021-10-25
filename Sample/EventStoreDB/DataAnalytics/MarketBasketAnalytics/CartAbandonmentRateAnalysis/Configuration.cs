using System;
using DataAnalytics.Core.Events;
using MarketBasketAnalytics.Carts;
using Microsoft.Extensions.DependencyInjection;

namespace MarketBasketAnalytics.CartAbandonmentRateAnalysis
{
    public static class Configuration
    {
        public static IServiceCollection AddCartAbandonmentRateAnalysis(this IServiceCollection services) =>
            services
                .AddEventHandler<ShoppingCartAbandoned>((@event, ct) =>
                {
                    throw new NotImplementedException();
                })
                .AddEventHandler<CartAbandonmentRateCalculated>((@event, ct) =>
                {
                    throw new NotImplementedException();
                })
                .AddEventHandler<ShoppingCartConfirmed>((@event, ct) =>
                {
                    throw new NotImplementedException();
                });
    }
}
