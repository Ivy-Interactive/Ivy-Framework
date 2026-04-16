using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace Ivy;

[JsonConverter(typeof(ResponsiveJsonConverterFactory))]
public record Responsive<T>
{
    public T? Default { get; init; }
    public T? Mobile { get; init; }
    public T? Tablet { get; init; }
    public T? Desktop { get; init; }
    public T? Wide { get; init; }

    public static implicit operator Responsive<T>(T value)
        => new() { Default = value };
}

public static class ResponsiveExtensions
{
    public static Responsive<T>? ToResponsive<T>(this T? value) where T : class
        => value is not null ? new Responsive<T> { Default = value } : null;

    public static Responsive<Density?>? ToResponsiveDensity(this Density? value)
        => value.HasValue ? new Responsive<Density?> { Default = value } : null;

    // Reference type overloads (Size, etc.)
    public static Responsive<T> At<T>(this T value, Breakpoint bp) where T : class => bp switch
    {
        Breakpoint.Mobile => new Responsive<T> { Mobile = value },
        Breakpoint.Tablet => new Responsive<T> { Tablet = value },
        Breakpoint.Desktop => new Responsive<T> { Desktop = value },
        Breakpoint.Wide => new Responsive<T> { Wide = value },
        _ => throw new ArgumentOutOfRangeException(nameof(bp))
    };

    public static Responsive<T> And<T>(this Responsive<T> r, Breakpoint bp, T value) where T : class => bp switch
    {
        Breakpoint.Mobile => r with { Mobile = value },
        Breakpoint.Tablet => r with { Tablet = value },
        Breakpoint.Desktop => r with { Desktop = value },
        Breakpoint.Wide => r with { Wide = value },
        _ => throw new ArgumentOutOfRangeException(nameof(bp))
    };

    // Private generic helpers for value types — the switch logic lives here once.
    private static Responsive<T?> AtCore<T>(T value, Breakpoint bp) where T : struct => bp switch
    {
        Breakpoint.Mobile => new Responsive<T?> { Mobile = value },
        Breakpoint.Tablet => new Responsive<T?> { Tablet = value },
        Breakpoint.Desktop => new Responsive<T?> { Desktop = value },
        Breakpoint.Wide => new Responsive<T?> { Wide = value },
        _ => throw new ArgumentOutOfRangeException(nameof(bp))
    };

    private static Responsive<T?> AndCore<T>(Responsive<T?> r, Breakpoint bp, T value) where T : struct => bp switch
    {
        Breakpoint.Mobile => r with { Mobile = value },
        Breakpoint.Tablet => r with { Tablet = value },
        Breakpoint.Desktop => r with { Desktop = value },
        Breakpoint.Wide => r with { Wide = value },
        _ => throw new ArgumentOutOfRangeException(nameof(bp))
    };

    // To add a new value type, add an At and And overload that delegates to AtCore/AndCore.

    // int overloads (for gap, columns, etc.)
    public static Responsive<int?> At(this int value, Breakpoint bp) => AtCore(value, bp);
    public static Responsive<int?> And(this Responsive<int?> r, Breakpoint bp, int value) => AndCore(r, bp, value);

    // Orientation overloads
    public static Responsive<Orientation?> At(this Orientation value, Breakpoint bp) => AtCore(value, bp);
    public static Responsive<Orientation?> And(this Responsive<Orientation?> r, Breakpoint bp, Orientation value) => AndCore(r, bp, value);

    // Density overloads
    public static Responsive<Density?> At(this Density value, Breakpoint bp) => AtCore(value, bp);
    public static Responsive<Density?> And(this Responsive<Density?> r, Breakpoint bp, Density value) => AndCore(r, bp, value);

    // bool overloads (for visibility)
    public static Responsive<bool?> At(this bool value, Breakpoint bp) => AtCore(value, bp);
    public static Responsive<bool?> And(this Responsive<bool?> r, Breakpoint bp, bool value) => AndCore(r, bp, value);

    // Thickness overloads (for padding, margin)
    public static Responsive<Thickness?> At(this Thickness value, Breakpoint bp) => AtCore(value, bp);
    public static Responsive<Thickness?> And(this Responsive<Thickness?> r, Breakpoint bp, Thickness value) => AndCore(r, bp, value);
}

public class ResponsiveJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(Responsive<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var innerType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(ResponsiveJsonConverter<>).MakeGenericType(innerType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class ResponsiveJsonConverter<T> : JsonConverter<Responsive<T>>
{
    public override Responsive<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization of Responsive<T> not implemented.");
    }

    public override void Write(Utf8JsonWriter writer, Responsive<T> value, JsonSerializerOptions options)
    {
        var hasBreakpoint = !IsDefault(value.Mobile) || !IsDefault(value.Tablet) ||
                            !IsDefault(value.Desktop) || !IsDefault(value.Wide);

        if (!hasBreakpoint)
        {
            // Backward compatible: serialize as plain T
            JsonSerializer.Serialize(writer, value.Default, options);
            return;
        }

        writer.WriteStartObject();

        if (!IsDefault(value.Default))
        {
            writer.WritePropertyName("default");
            JsonSerializer.Serialize(writer, value.Default, options);
        }

        if (!IsDefault(value.Mobile))
        {
            writer.WritePropertyName("mobile");
            JsonSerializer.Serialize(writer, value.Mobile, options);
        }

        if (!IsDefault(value.Tablet))
        {
            writer.WritePropertyName("tablet");
            JsonSerializer.Serialize(writer, value.Tablet, options);
        }

        if (!IsDefault(value.Desktop))
        {
            writer.WritePropertyName("desktop");
            JsonSerializer.Serialize(writer, value.Desktop, options);
        }

        if (!IsDefault(value.Wide))
        {
            writer.WritePropertyName("wide");
            JsonSerializer.Serialize(writer, value.Wide, options);
        }

        writer.WriteEndObject();
    }

    private static bool IsDefault(T? value)
    {
        if (value is null) return true;
        return EqualityComparer<T>.Default.Equals(value, default);
    }
}
