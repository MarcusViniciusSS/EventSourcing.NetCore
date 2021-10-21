﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MarketBasketAnalytics.MarketBasketAnalysis
{
    public record ProductRelationshipsInBaskets(
        IReadOnlyList<Guid> RelatedProducts,
        int BasketWithProductsCount
    );

    public record MarketBasketModelSummaryCalculated(
        Guid ProductId,
        IReadOnlyList<ProductRelationshipsInBaskets> Relationships
    )
    {
        public static async Task<MarketBasketModelSummaryCalculated> Handle(
            Func<Guid, CancellationToken, Task<MarketBasketModelSummaryCalculated>> getCurrentSummary,
            CartProductItemsMatched @event,
            CancellationToken ct
        )
        {
            var result = new List<ProductRelationshipsInBaskets>();

            var currentSummary = await getCurrentSummary(@event.ProductId, ct);
            var relatedProducts = Expand(@event.RelatedProducts).ToList();

            foreach (var currentRel in currentSummary.Relationships)
            {
                var relationship = relatedProducts
                    .SingleOrDefault(rp => rp.SequenceEqual(currentRel.RelatedProducts));

                if (relationship == null)
                {
                    result.Add(currentRel);
                    continue;
                }

                result.Add(new ProductRelationshipsInBaskets(
                    relationship,
                    currentRel.BasketWithProductsCount + 1
                ));
                relatedProducts.Remove(relationship);
            }

            result.AddRange(
                relatedProducts
                    .Select(relationship =>
                        new ProductRelationshipsInBaskets(relationship, 1)
                    ).ToList()
            );

            return currentSummary with { Relationships = result };
        }

        private static IReadOnlyList<IReadOnlyList<Guid>> Expand(IReadOnlyList<Guid> relatedProducts)
            => relatedProducts
                .SelectMany(
                    (relatedProduct, index) =>
                        Expand(new[] { relatedProduct }, relatedProducts.Skip(index + 1).ToList())
                )
                .ToList();

        private static IEnumerable<IReadOnlyList<Guid>> Expand
        (
            IReadOnlyList<Guid> accumulator,
            IReadOnlyList<Guid> relatedProducts
        )
        {
            if (!relatedProducts.Any())
                return new[] { accumulator };

            var aggregates = relatedProducts
                .Select(relatedProduct => accumulator.Union(new[] { relatedProduct }).ToList())
                .ToList();

            return aggregates.Union(
                aggregates.SelectMany((acc, i) => Expand(acc, relatedProducts.Skip(i + 1).ToList()))
                    .ToList()
            );
        }
    }
}
