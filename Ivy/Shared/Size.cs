using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivy.Shared;

/// <summary>
/// Specifies the type of size measurement for width and height values.
/// </summary>
public enum SizeType
{
    /// <summary>Framework-specific units that scale with the design system.</summary>
    Units,
    /// <summary>Absolute pixel values for precise sizing.</summary>
    Px,
    /// <summary>Rem units relative to root font size.</summary>
    Rem,
    /// <summary>Fractional values (0.0 to 1.0) for percentage-based sizing.</summary>
    Fraction,
    /// <summary>Fractional values with gap consideration for layout spacing.</summary>
    FractionGap,
    /// <summary>Takes up the full available space.</summary>
    Full,
    /// <summary>Sizes to fit the content.</summary>
    Fit,
    /// <summary>Sizes relative to the screen dimensions.</summary>
    Screen,
    /// <summary>Minimum content size based on intrinsic content width.</summary>
    MinContent,
    /// <summary>Maximum content size based on intrinsic content width.</summary>
    MaxContent,
    /// <summary>Automatic sizing based on content and context.</summary>
    Auto,
    /// <summary>Flexible grow behavior in layout containers.</summary>
    Grow,
    /// <summary>Flexible shrink behavior in layout containers.</summary>
    Shrink
}

/// <summary>
/// Represents width or height values with support for various measurement types and constraints.
/// Provides a comprehensive sizing system for responsive layouts and precise dimensional control.
/// </summary>
[JsonConverter(typeof(SizeJsonConverter))]
public record Size
{
    public override string ToString()
    {
        string Format(Size size)
            => $"{size.Type.ToString()}{(size.Value.HasValue ? $":{size.Value.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}" : "")}";

        var sb = new StringBuilder();
        sb.Append(Format(this));
        sb.Append(",");
        if (Min != null)
        {
            sb.Append(Format(Min!));
        }
        sb.Append(",");
        if (Max != null)
        {
            sb.Append(Format(Max!));
        }

        return sb.ToString().TrimEnd(',');
    }

    private SizeType Type { get; set; }
    private float? Value { get; set; }

    /// <summary>Gets the minimum size constraint.</summary>
    public Size? Min { get; init; }

    /// <summary>Gets the maximum size constraint.</summary>
    public Size? Max { get; init; }

    private Size(SizeType type, float? value)
    {
        Type = type;
        Value = value;
    }

    /// <summary>Creates a size with absolute pixel value.</summary>
    public static Size Px(int value)
    {
        return new Size(SizeType.Px, value);
    }

    /// <summary>Creates a size with rem units relative to root font size.</summary>
    public static Size Rem(int value)
    {
        return new Size(SizeType.Rem, value);
    }

    /// <summary>Creates a size with framework-specific units.</summary>
    public static Size Units(int value)
    {
        return new Size(SizeType.Units, value);
    }

    /// <summary>Creates a fractional size (0.0 to 1.0) for percentage-based sizing.</summary>
    public static Size Fraction(float value)
    {
        return new Size(SizeType.Fraction, value);
    }

    /// <summary>Creates a size that takes up the full available space.</summary>
    public static Size Full()
    {
        return new Size(SizeType.Full, null);
    }

    /// <summary>Creates a size that fits the content.</summary>
    public static Size Fit()
    {
        return new Size(SizeType.Fit, null);
    }

    /// <summary>Creates a size relative to screen dimensions.</summary>
    public static Size Screen()
    {
        return new Size(SizeType.Screen, null);
    }

    /// <summary>Creates a size based on minimum intrinsic content width.</summary>
    public static Size MinContent()
    {
        return new Size(SizeType.MinContent, null);
    }

    /// <summary>Creates a size based on maximum intrinsic content width.</summary>
    public static Size MaxContent()
    {
        return new Size(SizeType.MaxContent, null);
    }

    /// <summary>Creates an automatic size based on content and context.</summary>
    public static Size Auto()
    {
        return new Size(SizeType.Auto, null);
    }

    /// <summary>Creates a flexible grow size for layout containers.</summary>
    public static Size Grow(int value = 1)
    {
        return new Size(SizeType.Grow, value);
    }

    /// <summary>Creates a flexible shrink size for layout containers.</summary>
    public static Size Shrink(int value = 1)
    {
        return new Size(SizeType.Shrink, value);
    }

    /// <summary>Creates a half-width fractional size (0.5).</summary>
    public static Size Half()
    {
        return Fraction(0.5f);
    }

    /// <summary>Creates a third-width fractional size (0.333).</summary>
    public static Size Third()
    {
        return Fraction(0.333f);
    }

    /// <summary>Creates a fractional size with gap consideration for layout spacing.</summary>
    public static Size FractionGap(float value)
    {
        return new Size(SizeType.FractionGap, value);
    }

    public static implicit operator string(Size size)
    {
        return size.ToString();
    }
}

/// <summary>
/// Extension methods for adding size constraints to Size objects.
/// </summary>
public static class SizeExtensions
{
    /// <summary>Sets the minimum size constraint.</summary>
    public static Size Min(this Size size, Size min)
    {
        return size with { Min = min };
    }

    /// <summary>Sets the maximum size constraint.</summary>
    public static Size Max(this Size size, Size max)
    {
        return size with { Max = max };
    }

    /// <summary>Sets the maximum size constraint using framework units.</summary>
    public static Size Max(this Size size, int max)
    {
        return size.Max(Size.Units(max));
    }

    /// <summary>Sets the minimum size constraint using framework units.</summary>
    public static Size Min(this Size size, int min)
    {
        return size.Min(Size.Units(min));
    }

    /// <summary>Sets the minimum size constraint using fractional value.</summary>
    public static Size Min(this Size size, float min)
    {
        return size.Min(Size.Fraction(min));
    }

    /// <summary>Sets the maximum size constraint using fractional value.</summary>
    public static Size Max(this Size size, float max)
    {
        return size.Max(Size.Fraction(max));
    }
}


/// <summary>
/// JSON converter for Size objects that serializes them as string representations.
/// </summary>
public class SizeJsonConverter : JsonConverter<Size>
{
    /// <summary>Reads Size from JSON (not implemented).</summary>
    public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization of SizeJsonConverter not implemented.");
    }

    /// <summary>Writes Size to JSON as string value.</summary>
    public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}