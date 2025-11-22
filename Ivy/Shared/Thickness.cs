using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivy.Shared;

/// <summary>
/// Represents spacing values for the four sides of a rectangular area.
/// Used for padding, margins, borders, and offsets in widgets and layouts.
/// </summary>
/// <param name="Left">The thickness value for the left side.</param>
/// <param name="Top">The thickness value for the top side.</param>
/// <param name="Right">The thickness value for the right side.</param>
/// <param name="Bottom">The thickness value for the bottom side.</param>
[JsonConverter(typeof(ThicknessJsonConverter))]
public readonly record struct Thickness(int Left, int Top, int Right, int Bottom)
{
    /// <param name="uniform">The thickness value to apply to all sides.</param>
    public Thickness(int uniform) : this(uniform, uniform, uniform, uniform) { }

    /// <param name="horizontal">The thickness value for left and right sides.</param>
    /// <param name="vertical">The thickness value for top and bottom sides.</param>
    public Thickness(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical) { }

    public static Thickness Zero => new(0);

    public override string ToString() => $"{Left},{Top},{Right},{Bottom}";

    public static implicit operator string(Thickness thickness)
    {
        return thickness.ToString();
    }
}

/// <summary>
/// JSON converter for Thickness objects that serializes them as string representations.
/// </summary>
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