using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAnalytics.Core.Serialisation;
using EventStore.Client;

namespace DataAnalytics.Core.Entities
{
    public static class EventStoreDBRepository
    {
        public static async Task<TEntity> AggregateStream<TEntity>(
            this EventStoreClient eventStore,
            Func<TEntity?, object, TEntity> when,
            string id,
            CancellationToken cancellationToken
        )
        {
            var readResult = eventStore.ReadStreamAsync(
                Direction.Forwards,
                id,
                StreamPosition.Start,
                cancellationToken: cancellationToken
            );

            return (await readResult
                .Select(@event => @event.DeserializeData())
                .AggregateAsync(
                    default,
                    when,
                    cancellationToken
                ))!;
        }

        public static async Task AppendToNewStream(
            this EventStoreClient eventStore,
            string id,
            object @event,
            CancellationToken cancellationToken
        )

        {
            await eventStore.AppendToStreamAsync(
                id,
                StreamState.NoStream,
                new[] { @event.ToJsonEventData() },
                cancellationToken: cancellationToken
            );
        }


        public static async Task AppendToExisting(
            this EventStoreClient eventStore,
            string id,
            object @event,
            uint version,
            CancellationToken cancellationToken
        )
        {
            await eventStore.AppendToStreamAsync(
                id,
                StreamRevision.FromInt64(version),
                new[] { @event.ToJsonEventData() },
                cancellationToken: cancellationToken
            );
        }
    }
}
