# Event Schema Versioning

As time flow, the events' definition may change. Our business is changing, and we need to add more information. Sometimes we have to fix a bug or modify the definition for a better developer experience. Migrations are never easy, even in relational databases. You always have to think on:
- what caused the change?
- what are the possible solutions?
- is the change breaking?
- what to do with old data?

We should always try to perform the change in a non-breaking manner. I explained that in [Let's take care of ourselves! Thoughts on compatibility](https://event-driven.io/en/lets_take_care_of_ourselves_thoughts_about_comptibility/) article.

The same "issues" happens for event data model. Greg Young wrote a book about it: https://leanpub.com/esversioning/read. I recommend you to read it.

This sample shows how to do basic Event Schema versioning. Those patterns can be applied to any event store.

## Simple mapping

There are some simple mappings that we could handle on the code structure or serialisation level. I'm using `System.Text.Json` in samples, other serialises may be smarter, but the patterns will be similar. 

Having event defined as such:

```csharp
public record ShoppingCartInitialized(
    Guid ShoppingCartId,
    Guid ClientId
);
```

### New not required property

If we'd like to add a new not required property, e.g. `IntializedAt`, we can add it just as a new nullable property. The essential fact to decide if that's the right strategy is if we're good with not having it defined. It can be handled as:

```csharp
public record ShoppingCartInitialized(
    Guid ShoppingCartId,
    Guid ClientId,
    // Adding new not required property as nullable
    DateTime? IntializedAt
);
```

See full sample: [NewNotRequiredProperty.cs](./EventsVersioning.Tests/SimpleMappings/NewNotRequiredProperty.cs).


### New required property

If we'd like to add a new required property and make it non-breaking, we must define a default value. It's the same as you'd add a new column to the relational table. 

For instance, we decide that we'd like to add a validation step when the shopping cart is open (e.g. for fraud or spam detection), and our shopping cart can be opened with a pending state. We could solve that by adding the new property with the status information and setting it to `Initialized`, assuming that all old events were appended using the older logic.

```csharp
public enum ShoppingCartStatus
{
    Pending = 1,
    Initialized = 2,
    Confirmed = 3,
    Cancelled = 4
}

public record ShoppingCartInitialized(
    Guid ShoppingCartId,
    Guid ClientId,
    // Adding new not required property as nullable
    ShoppingCartStatus Status = ShoppingCartStatus.Initialized
);
```

See full sample: [NewRequiredProperty.cs](./EventsVersioning.Tests/SimpleMappings/NewRequiredProperty.cs).

### Renamed property

Renaming property is also a breaking change. Still, we can do it in a non-breaking manner. We could keep the same name in the JSON but map it during (de)serialisation.

Let's assume that we concluded that keeping `ShoppingCart` prefix in the `ShoppingCartId` is redundant and decided to change it to `CartId`, as we see in the event name, what cart we have in mind.

We could do it as:

```csharp
public class ShoppingCartInitialized
{
    [JsonPropertyName("ShoppingCartId")]
    public Guid CartId { get; init; }
    public Guid ClientId { get; init; }

    public ShoppingCartInitialized(
        Guid cartId,
        Guid clientId
    )
    {
        CartId = cartId;
        ClientId = clientId;
    }
}
```
See full sample: [NewRequiredProperty.cs](./EventsVersioning.Tests/SimpleMappings/NewRequiredProperty.cs).

## Upcasting

Sometimes we want to make more significant changes or be more flexible in the event mapping. We'd like to use a new structure in our code, not polluted by the custom mappings.

We can use an upcasting pattern for that. We can plug a middleware between the deserialisation and application logic. Having that, we can either grab raw JSON or deserialised object of the old structure and transform it to the new schema. 

### Changed Structure

For instance, we decide to send also other information about the client, instead of just their id. We'd like to have a nested object instead of the flattened list of fields. We could model new event structure as:

```csharp
public record Client(
        Guid Id,
        string Name = "Unknown"
    );

    public record ShoppingCartInitialized(
        Guid ShoppingCartId,
        Client Client
    );
```

We can define upcaster as a function that'll later plug in the deserialisation process. 

We can define the transformation of the object of the old structure as:

```csharp
public static ShoppingCartInitialized Upcast(
    V1.ShoppingCartInitialized oldEvent
)
{
    return new ShoppingCartInitialized(
        oldEvent.ShoppingCartId,
        new Client(oldEvent.ClientId)
    );
}
```

Or we can map it from JSON

```csharp
public static ShoppingCartInitialized Upcast(
    V1.ShoppingCartInitialized oldEvent
)
{
    return new ShoppingCartInitialized(
        oldEvent.ShoppingCartId,
        new Client(oldEvent.ClientId)
    );
}
```

See full sample: [ChangedStructure.cs](./EventsVersioning.Tests/Upcasters/ChangedStructure.cs).

### New required property

We can also solve the same cases as simple mappings, but we have more handling options.

Let's say that we forget to add information about who initialised the shopping cart (user id). We cannot retroactively guess what the user was, but if we were lucky enough to decide to track such information in user metadata (e.g. for tracing), then we can try to map it.

```csharp
public record EventMetadata(
    Guid UserId
);

public record ShoppingCartInitialized(
    Guid ShoppingCartId,
    Guid ClientId,
    Guid InitializedBy
);
```

Upcaster from old object to the new one can look like:

```csharp
public static ShoppingCartInitialized Upcast(
    V1.ShoppingCartInitialized oldEvent,
    EventMetadata eventMetadata
)
{
    return new ShoppingCartInitialized(
        oldEvent.ShoppingCartId,
        oldEvent.ClientId,
        eventMetadata.UserId
    );
}
```

From JSON to the object:

```csharp
public static ShoppingCartInitialized Upcast(
    string oldEventJson,
    string eventMetadataJson
)
{
    var oldEvent = JsonDocument.Parse(oldEventJson);
    var eventMetadata = JsonDocument.Parse(eventMetadataJson);

    return new ShoppingCartInitialized(
        oldEvent.RootElement.GetProperty("ShoppingCartId").GetGuid(),
        oldEvent.RootElement.GetProperty("ClientId").GetGuid(),
        eventMetadata.RootElement.GetProperty("UserId").GetGuid()
    );
}
```

See full sample: [NewRequiredPropertyFromMetadata.cs](./EventsVersioning.Tests/Upcasters/NewRequiredPropertyFromMetadata.cs).

## Downcasters

In the same way, as described above, we can downcast the events from the new structure to the old one (if we have the old reader/listener or for some reason want to keep the old format).

From the new object to the old one:

```csharp
public static V1.ShoppingCartInitialized Downcast(
    ShoppingCartInitialized newEvent
)
{
    return new V1.ShoppingCartInitialized(
        newEvent.ShoppingCartId,
        newEvent.Client.Id
    );
}
```

From new JSON format to the old object:

```csharp
public static V1.ShoppingCartInitialized Downcast(
    string newEventJson
)
{
    var newEvent = JsonDocument.Parse(newEventJson).RootElement;

    return new V1.ShoppingCartInitialized(
        newEvent.GetProperty("ShoppingCartId").GetGuid(),
        newEvent.GetProperty("Client").GetProperty("Id").GetGuid()
    );
}
```

See full sample: [ChangedStructure.cs](./EventsVersioning.Tests/Upcasters/ChangedStructure.cs).