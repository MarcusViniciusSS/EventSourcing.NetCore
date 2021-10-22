using System.Text;
using System.Text.Json;
using ECommerce.Core.Events;
using EventStore.Client;

namespace DataAnalytics.Core.Serialisation
{
    public static class EventStoreDBSerializer
    {
        public static T Deserialize<T>(this ResolvedEvent resolvedEvent) =>
            (T)Deserialize(resolvedEvent);

        public static object Deserialize(this ResolvedEvent resolvedEvent)
        {
            // get type
            var eventType = EventTypeMapper.ToType(resolvedEvent.Event.EventType);

            // deserialize event
            return JsonSerializer.Deserialize(
                resolvedEvent.Event.Data.Span,
                eventType
            )!;
        }

        public static EventData ToJsonEventData(this object @event) =>
            new(
                Uuid.NewUuid(),
                EventTypeMapper.ToName(@event.GetType()),
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)),
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { }))
            );
    }
}
