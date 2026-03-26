---
searchHints:
  - usestream
  - stream
  - real-time
  - streaming
  - llm
  - text run
  - push
  - websocket
---

# UseStream

<Ingress>
Create a server-to-client stream that pushes data to the frontend in real time without triggering a full [view](../../../01_Onboarding/02_Concepts/02_Views.md) re-render for each chunk, using the UseStream [hook](../02_RulesOfHooks.md).
</Ingress>

The `UseStream<T>` [hook](../02_RulesOfHooks.md) returns an `IWriteStream<T>` that you attach to a widget property. You then call `Write(T data)` to push chunks to the client over the existing connection. Typical uses include LLM text streaming, progress updates, and live logs.

## Overview

- **Real-time push** — Data is sent to the client as you call `Write()`; no polling.
- **No re-render per chunk** — The stream is consumed by the widget on the client; the view tree is not rebuilt for every write.
- **Typical `T`** — Often `TextRun` for streaming rich text; `byte[]` is also supported (serialized as base64).

## Basic Usage

1. Create a stream with `Context.UseStream<T>()`.
2. Attach it to a widget that supports streaming (e.g. [RichTextBlock](../../../02_Widgets/01_Primitives/24_RichTextBlock.md) via `Text.Rich().UseStream(stream)`).
3. Call `stream.Write(data)` from event handlers or async code to push data in real time.

```csharp demo-below
public class StreamingApp : ViewBase
{
    public override object? Build()
    {
        var stream = Context.UseStream<TextRun>();

        return Layout.Vertical()
            | Text.Rich()
                .Bold("🤖 ")
                .UseStream(stream)
            | new Button("Generate").OnClick(async () =>
            {
                var words = new[] { "Hello", "world", "from", "the", "stream!" };
                foreach (var word in words)
                {
                    await Task.Delay(200);
                    stream.Write(new TextRun(word) { Word = true });
                }
            });
    }
}
```

## How It Works

**Hook:**

```csharp
IWriteStream<T> UseStream<T>(bool buffer = true)
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `buffer` | `bool` | `true` | When `true`, data written before the client subscribes is buffered and flushed when the subscription is established. When `false`, that data is discarded. |

**Returned interface `IWriteStream<T>`:**

| Member | Description |
|--------|-------------|
| `Id` | Unique stream identifier (used internally for client subscription). |
| `Write(T data)` | Pushes one item to the client. Serialization: JSON for most types, base64 for `byte[]`. |

## Buffering

With the default `buffer: true`, any data you write before the frontend has rendered and subscribed is buffered on the server. When the client subscribes, all buffered data is sent immediately. Use this when you want to guarantee that no chunks are lost (e.g. LLM streaming).

To avoid buffering (e.g. to reduce memory or when early data is irrelevant), pass `buffer: false`:

```csharp
var stream = Context.UseStream<TextRun>(buffer: false);
```

## When to Use

| Use UseStream for | Prefer something else when |
|-------------------|----------------------------|
| LLM or other token-by-token text output | One-shot text - normal [state](./03_UseState.md) or [UseQuery](./09_UseQuery.md) |
| Progress or log tail that updates in place | Full list updates - [UseState](./03_UseState.md) or [UseQuery](./09_UseQuery.md) |
| Pushing binary or structured chunks to a single widget | Cross-component events - [UseSignal](./10_UseSignal.md) |

## Faq

### How do I stream IChatClient responses to the UI?

Use `IChatClient.GetStreamingResponseAsync()` with `UseStream<TextRun>`:

```csharp
var chatClient = UseService<IChatClient>();
var stream = UseStream<TextRun>();

async Task Generate()
{
    stream.Clear();
    await foreach (var update in chatClient.GetStreamingResponseAsync(messages))
    {
        if (update.Text is { } text)
            stream.Write(new TextRun(text));
    }
}

Text.Rich().UseStream(stream);
```

Register `IChatClient` in Program.cs:

```csharp
builder.Services.AddSingleton<IChatClient>(sp =>
    new OpenAIClient(apiKey)
        .GetChatClient("gpt-4o-mini")
        .AsIChatClient());
```

## See Also

- [RichTextBlock — Streaming](../../../02_Widgets/01_Primitives/24_RichTextBlock.md) — Main use case: streaming `TextRun` with `Text.Rich().UseStream(stream)`
- [AutoScroll](../../../02_Widgets/01_Primitives/26_AutoScroll.md) — Optional scroll container that pins to the bottom as content grows (not used by Chat; useful for logs and custom layouts around streamed content)
- [Rules of Hooks](../02_RulesOfHooks.md) — Hook rules and best practices
- [UseState](./03_UseState.md) — For reactive state that triggers re-renders
- [UseSignal](./10_UseSignal.md) — For cross-component messaging
