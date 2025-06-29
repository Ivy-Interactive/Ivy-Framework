using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivy.Shared;

[JsonConverter(typeof(ThicknessJsonConverter))]
public readonly record struct Thickness(int Left, int Top, int Right, int Bottom)
{
    public Thickness(int uniform) : this(uniform, uniform, uniform, uniform) { }
    public Thickness(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical) { }
    public static Thickness Zero => new(0);
    public override string ToString() => $"{Left},{Top},{Right},{Bottom}";
    public static implicit operator string(Thickness thickness)
    {
        return thickness.ToString();
    }
}

public class ThicknessJsonConverter : JsonConverter<Thickness>
{
    public override Thickness Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization of ThicknessConverter not implemented.");
    }

    public override void Write(Utf8JsonWriter writer, Thickness value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}