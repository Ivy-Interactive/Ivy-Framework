---
searchHints:
  - communication
  - events
  - pubsub
  - messaging
  - broadcast
  - cross-component
---

# Signals

<Ingress>
Signals enable inter-component communication in Ivy [applications](./15_Apps.md), allowing components to send and receive messages across the component tree.
They follow a publisher-subscriber pattern where components can send messages through signals and other components can listen for and respond to those messages.
</Ingress>

## Basic Usage

First, define a signal by creating a class that inherits from `AbstractSignal<TInput, TOutput>`:

```csharp demo-below
public class CounterSignal : AbstractSignal<int, string> { }

public class SignalExample : ViewBase
{
    public override object? Build()
    {
        var signal = UseSignal<CounterSignal, int, string>();
        var output = UseState("");

        async ValueTask OnClick(Event<Button> _)
        {
            var results = await signal.Send(1);
            output.Set(string.Join(", ", results));
        }

        return Layout.Vertical(
            new Button("Send Signal", OnClick),
            new ChildReceiver(),
            output
        );
    }
}

public class ChildReceiver : ViewBase
{
    public override object? Build()
    {
        var signal = UseSignal<CounterSignal, int, string>();
        var counter = UseState(0);

        UseEffect(() => signal.Receive(input =>
        {
            counter.Set(counter.Value + input);
            return $"Child received: {input}, total: {counter.Value}";
        }));

        return new Card($"Counter: {counter.Value}");
    }
}
```

## Signal Communication Patterns

### One-to-Many Communication

This example demonstrates the one-to-many pattern where one sender broadcasts a message that multiple receivers all receive simultaneously.

```csharp demo-tabs
public class BroadcastSignal : AbstractSignal<string, Unit> { }

public class OneToManyDemo : ViewBase
{
    public override object? Build()
    {
        var signal = UseSignal<BroadcastSignal, string, Unit>();
        var message = UseState("");
        var receiver1Message = UseState("");
        var receiver2Message = UseState("");
        var receiver3Message = UseState("");

        async ValueTask BroadcastMessage(Event<Button> _)
        {
            if (!string.IsNullOrWhiteSpace(message.Value))
            {
                await signal.Send(message.Value);
                message.Set("");
            }
        }

        // Process incoming messages
        UseEffect(() => signal.Receive(msg =>
        {
            // Each receiver processes the same message differently
            receiver1Message.Set($"Logged: {msg}");
            receiver2Message.Set($"Analyzed: {msg.Length} characters");
            receiver3Message.Set($"Stats: {msg.Split(' ').Length} words");
            return new Unit();
        }));

        return Layout.Vertical(
            Layout.Horizontal(
                message.ToTextInput("Broadcast Message"),
                new Button("Send", BroadcastMessage)
            ),
            Layout.Horizontal(
                new Card(Text.Block(receiver1Message.Value)),
                new Card(Text.Block(receiver2Message.Value)),
                new Card(Text.Block(receiver3Message.Value))
            )
        );
    }
}
```

### Request-Response Pattern

This example demonstrates the request-response pattern where a requester sends a query and receives specific responses from providers. Unlike one-to-many broadcasting, this pattern expects specific data back from each provider.

```csharp demo-tabs
public class DataRequestSignal : AbstractSignal<string, string[]> { }

public class RequestResponseDemo : ViewBase
{
    public override object? Build()
    {
        var signal = UseSignal<DataRequestSignal, string, string[]>();
        var query = UseState<string>("");
        var results = UseState<string[]>(() => Array.Empty<string>());
        var isSearching = UseState<bool>(false);

        async ValueTask SearchData(Event<Button> _)
        {
            if (!string.IsNullOrWhiteSpace(query.Value))
            {
                isSearching.Set(true);

                // Send request via signal and get responses from all providers
                var responses = await signal.Send(query.Value);
                var allResults = responses.SelectMany(r => r).ToArray();

                results.Set(allResults);
                query.Set("");
                isSearching.Set(false);
            }
        }

        return Layout.Vertical(
            Text.Block("Try searching for: John, Jane, Laptop, Smartphone, Tablet"),
            Layout.Horizontal(
                query.ToTextInput("Search"),
                new Button("Search", SearchData)
            ),
            Text.Block(isSearching.Value ? "Searching..." : $"Found {results.Value.Length} results"),
            results.Value.Select(r => Text.Block(r)),
            Layout.Horizontal(
                new DataProvider("User Database", new[] { "John Doe", "Jane Smith", "Bob Johnson" }),
                new DataProvider("Product Catalog", new[] { "Laptop", "Smartphone", "Tablet" })
            )
        );
    }
}

public class DataProvider : ViewBase
{
    private readonly string _providerName;
    private readonly string[] _dataSource;

    public DataProvider(string providerName, string[] dataSource)
    {
        _providerName = providerName;
        _dataSource = dataSource;
    }

    public override object? Build()
    {
        var signal = UseSignal<DataRequestSignal, string, string[]>();
        var processedQueries = UseState<int>(0);
        var lastQuery = UseState<string>("");

        UseEffect(() => signal.Receive(query =>
        {
            processedQueries.Set(processedQueries.Value + 1);
            lastQuery.Set(query);

            // Process the query and return results
            var matchingResults = _dataSource
                .Where(item => item.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(item => $"[{_providerName}] {item}")
                .ToArray();

            return matchingResults;
        }));

        return new Card(
            Layout.Vertical(
                Text.Block(_providerName),
                Text.Block($"Data source: {string.Join(", ", _dataSource)}"),
                Text.Block($"Processed: {processedQueries.Value} queries"),
                Text.Block($"Last query: {lastQuery.Value}")
            )
        );
    }
}
```

## API Reference

### Required Imports

Add `using Ivy.Hooks;` to access signal functionality:

```csharp
using Ivy.Hooks;
```

### Key Types

- **`AbstractSignal<TInput, TOutput>`** - Base class for signals
- **`Unit`** - Void return type for notifications without responses
- **`ISignal<TInput, TOutput>`** - Interface for sending and receiving messages
- **`UseSignal<TSignal, TInput, TOutput>()`** - Gets the signal for sending and receiving

### Signal Operations

**Getting a signal** (for sending or receiving):

```csharp
var signal = UseSignal<CounterSignal, int, string>();
```

**Sending messages**:

```csharp
var signal = UseSignal<CounterSignal, int, string>();
var results = await signal.Send(42); // Returns TOutput[] from all subscribers
```

**Receiving messages**:

```csharp
var signal = UseSignal<CounterSignal, int, string>();
UseEffect(() => signal.Receive(input => {
    // Handle message and return response
    return $"Processed: {input}";
}));
```

**Important**: Always use `UseEffect()` to manage signal subscriptions for proper [lifecycle handling](./02_Views.md).

### Signal Types

- `TInput` - Data sent to subscribers
- `TOutput` - Response type from each subscriber (aggregated into array)

Use `Unit` when no response is needed, otherwise return meaningful data.

## Broadcast Types

### Session vs Broadcast Signals

By default, signals are **session-scoped** - all components in the same session automatically share the same signal instance by type. For cross-session communication, add the `[Signal]` attribute:

```csharp
// Session-scoped - all components in the same session share this signal
public class LocalSignal : AbstractSignal<string, Unit> { }

// Broadcast - sends to multiple sessions based on BroadcastType
[Signal(BroadcastType.App)]
public class AppSignal : AbstractSignal<string, Unit> { }
```

### Available Broadcast Types

- **`App`** - All sessions running the same application
- **`Server`** - All active sessions on the server
- **`User`** - All sessions on the same physical machine
- **`Chrome`** - Parent [Chrome](./16_Chrome.md) session (for embedded apps)

```csharp
[Signal(BroadcastType.App)]
public class AppNotificationSignal : AbstractSignal<string, Unit> { }
```

```mermaid
graph TB
    Sender[Signal Sender]
    App[App-Level]
    Server[Server-Level]
    User[User-Level]

    Sender --> App
    Sender --> Server
    Sender --> User
```

## See Also

- [State Management](../../03_Hooks/Core/03_State.md)
- [Effects](../../03_Hooks/Core/04_Effect.md)
