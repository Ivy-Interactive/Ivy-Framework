using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IStream
{
    string Id { get; }
}

public static class StreamRegistry
{
    private static readonly ConcurrentDictionary<string, WeakReference<WriteStream>> Streams = new();

    internal static void Register(string streamId, WriteStream stream)
    {
        Streams[streamId] = new WeakReference<WriteStream>(stream);
    }

    internal static void Unregister(string streamId)
    {
        Streams.TryRemove(streamId, out _);
    }

    public static void NotifySubscribed(string streamId)
    {
        if (Streams.TryGetValue(streamId, out var weakRef) && weakRef.TryGetTarget(out var stream))
        {
            stream.OnSubscribed();
        }
    }
}

internal abstract class WriteStream
{
    public abstract void OnSubscribed();
}

[JsonConverter(typeof(WriteStreamJsonConverter))]
public interface IWriteStream<in T> : IStream
{
    void Write(T data);
}

internal class WriteStream<T> : WriteStream, IWriteStream<T>, IDisposable
{
    private readonly IClientSender _sender;
    private readonly List<T>? _buffer;
    private readonly object _lock = new();
    private readonly bool _bufferEnabled;
    private bool _subscribed;
    private bool _disposed;

    public string Id { get; }

    public WriteStream(string id, IClientSender sender, bool buffer = true)
    {
        Id = id;
        _sender = sender;
        _bufferEnabled = buffer;
        if (buffer)
        {
            _buffer = new List<T>();
            StreamRegistry.Register(id, this);
        }
    }

    public void Write(T data)
    {
        if (_disposed) return;

        // Explicitly base64 encode byte arrays to ensure proper serialization
        object serializedData = data is byte[] bytes
            ? Convert.ToBase64String(bytes)
            : JsonSerializer.SerializeToNode(data, WidgetSerializer.SerializerOptions)!;

        if (!_bufferEnabled)
        {
            _sender.Send("StreamData", new { streamId = Id, data = serializedData });
            return;
        }

        lock (_lock)
        {
            if (_subscribed)
            {
                _sender.Send("StreamData", new { streamId = Id, data = serializedData });
            }
            else
            {
                _buffer!.Add(data);
            }
        }
    }

    public override void OnSubscribed()
    {
        if (!_bufferEnabled || _buffer == null) return;

        lock (_lock)
        {
            if (_subscribed) return;
            _subscribed = true;

            // Flush buffered data
            foreach (var data in _buffer)
            {
                // Serialize identically to Write() to ensure consistent casing/format
                object serializedData = data is byte[] bytes
                    ? Convert.ToBase64String(bytes)
                    : JsonSerializer.SerializeToNode(data, WidgetSerializer.SerializerOptions)!;
                _sender.Send("StreamData", new { streamId = Id, data = serializedData });
            }
            _buffer.Clear();
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _disposed = true;
            if (_bufferEnabled)
            {
                _buffer!.Clear();
                StreamRegistry.Unregister(Id);
            }
        }
    }
}

public class WriteStreamJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsInterface && !typeToConvert.IsClass)
            return false;

        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(IWriteStream<>) ||
               typeToConvert.GetInterfaces().Any(i =>
                   i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IWriteStream<>));
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new WriteStreamJsonConverterInner();
    }

    private class WriteStreamJsonConverterInner : JsonConverter<IStream>
    {
        public override IStream? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Deserializing IWriteStream is not supported");
        }

        public override void Write(Utf8JsonWriter writer, IStream value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("id", value.Id);
            writer.WriteEndObject();
        }
    }
}

public static class UseStreamExtensions
{
    /// <summary>
    /// Creates a server-to-client stream that can push data to the frontend in real time.
    /// Attach the returned stream to a widget property (e.g. <see cref="RichTextBlock.Stream"/>)
    /// and call <see cref="IWriteStream{T}.Write"/> to send data.
    /// </summary>
    /// <param name="context">The view context (typically accessed via <c>Context</c>).</param>
    /// <param name="buffer">
    /// When <c>true</c> (default), data written before the frontend subscribes is buffered
    /// and automatically flushed once the subscription is established.
    /// When <c>false</c>, data written before subscription is discarded.
    /// </param>
    /// <typeparam name="T">The type of data to stream (e.g. <see cref="TextRun"/>).</typeparam>
    public static IWriteStream<T> UseStream<T>(this IViewContext context, bool buffer = true)
    {
        var streamId = context.UseState(() => Guid.NewGuid().ToString(), false);
        var clientProvider = context.UseService<IClientProvider>();

        var stream = context.UseRef(() => new WriteStream<T>(streamId.Value, clientProvider.Sender, buffer));

        context.UseEffect(() => stream.Value, [EffectTrigger.OnMount()]);

        return stream.Value;
    }
}
