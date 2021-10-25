using System;
using System.Threading;
using System.Threading.Tasks;
using MarketBasketAnalytics.Carts;

namespace MarketBasketAnalytics.CartAbandonmentRateAnalysis
{
    // See pt 5 of https://www.bolt.com/resources/ecommerce-metrics/#
    public record CartAbandonmentRateCalculated(
        Guid Id,
        Guid ClientId,
        int ProductItemsCount,
        decimal TotalAmount,
        DateTime InitializedAt,
        DateTime AbandonedAt,
        TimeSpan TotalTime
    );

    public class CartAbandonmentRate
    {
        public static Task<CartAbandonmentRateCalculated> Handle(
            Func<Func<CartAbandonmentRateCalculated?, object, CartAbandonmentRateCalculated>, string, CancellationToken,
                Task<CartAbandonmentRateCalculated?>> aggregate,
            ShoppingCartAbandoned @event,
            CancellationToken ct
        ) =>
            aggregate(When, ShoppingCart.ToStreamId(@event.ShoppingCartId), ct)!;

        public static CartAbandonmentRateCalculated When(CartAbandonmentRateCalculated? entity, object @event) =>
            @event switch
            {
                ShoppingCartInitialized (var cartId, var clientId, var initializedAt) =>
                    new CartAbandonmentRateCalculated(cartId, clientId, 0, 0, initializedAt, default, default),

                ProductItemAddedToShoppingCart (_, var productItem) =>
                    entity! with
                    {
                        ProductItemsCount = entity.ProductItemsCount + 1,
                        TotalAmount = entity.TotalAmount + productItem.TotalPrice
                    },

                ProductItemRemovedFromShoppingCart (_, var productItem) =>
                    entity! with
                    {
                        ProductItemsCount = entity.ProductItemsCount - 1,
                        TotalAmount = entity.TotalAmount - productItem.TotalPrice
                    },

                ShoppingCartAbandoned (_, var abandonedAt) =>
                    entity! with { AbandonedAt = abandonedAt, TotalTime = abandonedAt - entity.InitializedAt },
                _ => entity!
            };

        public static string ToStreamId(Guid shoppingCartId) =>
            $"cart_abandonment_rate-{shoppingCartId}";
    }
}
