using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Hooks;

public interface IStream
{
    string Id { get; }
}

[JsonConverter(typeof(WriteStreamJsonConverter))]
public interface IWriteStream<in T> : IStream
{
    void Write(T data);
}

internal class WriteStream<T> : IWriteStream<T>, IDisposable
{
    private readonly IClientSender _sender;
    private bool _disposed;

    public string Id { get; }

    public WriteStream(string id, IClientSender sender)
    {
        Id = id;
        _sender = sender;
    }

    public void Write(T data)
    {
        if (_disposed) return;
        _sender.Send("StreamData", new { streamId = Id, data });
    }

    public void Dispose()
    {
        _disposed = true;
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
    public static IWriteStream<T> UseStream<T>(this IViewContext context)
    {
        var streamId = context.UseState(() => Guid.NewGuid().ToString(), false);
        var clientProvider = context.UseService<IClientProvider>();

        var stream = context.UseRef(() => new WriteStream<T>(streamId.Value, clientProvider.Sender));

        context.UseEffect(() => stream.Value, [EffectTrigger.OnMount()]);

        return stream.Value;
    }
}
