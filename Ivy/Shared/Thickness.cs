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
    /// <summary>Creates uniform thickness with the same value on all sides.</summary>
    /// <param name="uniform">The thickness value to apply to all sides.</param>
    public Thickness(int uniform) : this(uniform, uniform, uniform, uniform) { }

    /// <summary>Creates thickness with separate horizontal and vertical values.</summary>
    /// <param name="horizontal">The thickness value for left and right sides.</param>
    /// <param name="vertical">The thickness value for top and bottom sides.</param>
    public Thickness(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical) { }

    /// <summary>Gets a thickness with zero values on all sides.</summary>
    public static Thickness Zero => new(0);

    /// <summary>Returns a string representation in "Left,Top,Right,Bottom" format.</summary>
    public override string ToString() => $"{Left},{Top},{Right},{Bottom}";

    /// <summary>Implicitly converts thickness to string representation.</summary>
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
    /// <summary>Reads Thickness from JSON (not implemented).</summary>
    public override Thickness Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization of ThicknessConverter not implemented.");
    }

    /// <summary>Writes Thickness to JSON as string value.</summary>
    public override void Write(Utf8JsonWriter writer, Thickness value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}