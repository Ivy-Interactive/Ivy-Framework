---
title: UseStream
---

# UseStream

The `UseStream<T>` hook allows you to create a server-to-client stream that can push data to a frontend widget in real-time. This is particularly useful for LLM text streaming, progress updates, or any scenario where you want to continuously push fragments of data to a single widget without triggering a full state re-render for every chunk.

## API Reference

```csharp
/// <summary>
/// Creates a server-to-client stream that can push data to the frontend in real time.
/// Attach the returned stream to a widget property (e.g. `RichTextBlock.Stream`)
/// and call `Write(T data)` to send data.
/// </summary>
/// <param name="buffer">
/// When `true` (default), data written before the frontend subscribes is buffered
/// and automatically flushed once the subscription is established.
/// When `false`, data written before subscription is discarded.
/// </param>
/// <typeparam name="T">The type of data to stream (e.g. `TextRun`).</typeparam>
public IWriteStream<T> UseStream<T>(bool buffer = true)
```

## Example: Streaming Rich Text from an LLM

You can attach the stream returned by `UseStream` to the special `UseStream` widget property available on components that support streaming (like `Text.Rich()`).

```csharp
public class StreamingApp : ViewBase
{
    protected override object? Build()
    {
        // 1. Create a stream for text runs
        var stream = Context.UseStream<TextRun>();

        return Layout.Vertical(
            Text.Rich()
                .Bold("🤖 ")
                // 2. Attach the stream to the widget
                .UseStream(stream),
                
            new Button("Generate").OnClick(async () => 
            {
                var words = new[] { "Hello", "world", "from", "the", "stream!" };
                
                foreach (var word in words)
                {
                    await Task.Delay(200);
                    // 3. Write data to the stream which gets pushed to the frontend in real-time
                    stream.Write(new TextRun(word) { Word = true });
                }
            })
        );
    }
}
```

## Buffering

By default (`buffer = true`), if you start writing to the stream before the frontend component has fully rendered and subscribed via WebSockets, the data will be buffered in memory on the server. Once the client establishes the subscription, all buffered data will be flushed immediately.

If you don't care about missing early data or want to avoid memory overhead for large streams that might not be listened to, you can set `buffer = false`:

```csharp
var stream = Context.UseStream<byte[]>(buffer: false);
```
