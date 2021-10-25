using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketBasketAnalytics.Carts;
using MarketBasketAnalytics.Carts.ProductItems;

namespace MarketBasketAnalytics.MarketBasketAnalysis
{
    public record CartProductItemsMatched(
        Guid ProductId,
        IReadOnlyList<Guid> RelatedProducts
    );

    public static class CartProductItemsMatching
    {
        public record ShoppingCart(
            IDictionary<Guid, int> ProductItems
        )
        {
            public static ShoppingCart Default() => new (new Dictionary<Guid, int>());
        }

        public static async Task<IReadOnlyList<CartProductItemsMatched>> Handle(
            Func<Func<ShoppingCart?, object, ShoppingCart>, string, CancellationToken, Task<ShoppingCart?>> aggregate,
            ShoppingCartConfirmed @event,
            CancellationToken ct
        )
        {
            var shoppingCard = await aggregate(
                When,
                MarketBasketAnalytics.Carts.ShoppingCart.ToStreamId(@event.ShoppingCartId),
                ct
            ) ?? ShoppingCart.Default();

            var productIds = shoppingCard.ProductItems.Keys;

            return productIds
                .Select(productId =>
                    new CartProductItemsMatched(
                        productId,
                        productIds.Where(pid => pid != productId).ToList()
                    )
                )
                .ToList();
        }

        public static ShoppingCart When(ShoppingCart? entity, object @event) =>
            @event switch
            {
                ShoppingCartInitialized =>
                    new ShoppingCart(new Dictionary<Guid, int>()),

                ProductItemAddedToShoppingCart (_, var productItem) =>
                    entity! with
                    {
                        ProductItems = Add(
                            entity.ProductItems,
                            productItem
                        )
                    },

                ProductItemRemovedFromShoppingCart (_, var productItem) =>
                    entity! with
                    {
                        ProductItems = Subtract(
                            entity.ProductItems,
                            productItem
                        )
                    },
                _ => entity!
            };

        private static IDictionary<Guid, int> Add(IDictionary<Guid, int> productItems, PricedProductItem productItem)
        {
            var productId = productItem.ProductId;
            var quantity = productItem.Quantity;

            var result = new Dictionary<Guid, int>(productItems);
            if (!productItems.ContainsKey(productId))
            {
                result.Add(productId, quantity);
                return result;
            }

            result[productId] += quantity;

            return result;
        }

        private static IDictionary<Guid, int> Subtract(IDictionary<Guid, int> productItems, PricedProductItem productItem)
        {
            var productId = productItem.ProductId;
            var quantity = productItem.Quantity;

            var result = new Dictionary<Guid, int>(productItems);

            result[productId] -= quantity;

            if (result[productId] == 0)
                result.Remove(productId);

            return result;
        }

        public static string ToStreamId(Guid shoppingCartId) =>
            $"cart_product_items_matching-{shoppingCartId}";
    }
}
