using System;
using DataAnalytics.Core.Events;
using MarketBasketAnalytics.Carts;
using Microsoft.Extensions.DependencyInjection;

namespace MarketBasketAnalytics.MarketBasketAnalysis
{
    public static class Configuration
    {
        public static IServiceCollection AddMarketBasketAnalysis(this IServiceCollection services) =>
            services
                .AddEventHandler<ShoppingCartConfirmed>((@event, ct) =>
                {
                    throw new NotImplementedException();
                })
                .AddEventHandler<CartProductItemsMatched>((@event, ct) =>
                {
                    throw new NotImplementedException();
                });
    }
}
