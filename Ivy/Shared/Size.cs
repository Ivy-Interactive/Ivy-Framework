using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivy.Shared;

public enum SizeType
{
    Units,
    Px,
    Rem,
    Fraction,
    FractionGap,
    Full,
    Fit,
    Screen,
    MinContent,
    MaxContent,
    Auto,
    Grow,
    Shrink
}

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
    public Size? Min { get; init; }
    public Size? Max { get; init; }

    private Size(SizeType type, float? value)
    {
        Type = type;
        Value = value;
    }

    public static Size Px(int value)
    {
        return new Size(SizeType.Px, value);
    }

    public static Size Rem(int value)
    {
        return new Size(SizeType.Rem, value);
    }

    public static Size Units(int value)
    {
        return new Size(SizeType.Units, value);
    }

    public static Size Fraction(float value)
    {
        return new Size(SizeType.Fraction, value);
    }

    public static Size Full()
    {
        return new Size(SizeType.Full, null);
    }

    public static Size Fit()
    {
        return new Size(SizeType.Fit, null);
    }

    public static Size Screen()
    {
        return new Size(SizeType.Screen, null);
    }

    public static Size MinContent()
    {
        return new Size(SizeType.MinContent, null);
    }

    public static Size MaxContent()
    {
        return new Size(SizeType.MaxContent, null);
    }

    public static Size Auto()
    {
        return new Size(SizeType.Auto, null);
    }

    public static Size Grow(int value = 1)
    {
        return new Size(SizeType.Grow, value);
    }

    public static Size Shrink(int value = 1)
    {
        return new Size(SizeType.Shrink, value);
    }

    public static Size Half()
    {
        return Fraction(0.5f);
    }

    public static Size Third()
    {
        return Fraction(0.333f);
    }

    public static Size FractionGap(float value)
    {
        return new Size(SizeType.FractionGap, value);
    }

    public static implicit operator string(Size size)
    {
        return size.ToString();
    }
}

public static class SizeExtensions
{
    public static Size Min(this Size size, Size min)
    {
        return size with { Min = min };
    }

    public static Size Max(this Size size, Size max)
    {
        return size with { Max = max };
    }

    public static Size Max(this Size size, int max)
    {
        return size.Max(Size.Units(max));
    }

    public static Size Min(this Size size, int min)
    {
        return size.Min(Size.Units(min));
    }

    public static Size Min(this Size size, float min)
    {
        return size.Min(Size.Fraction(min));
    }

    public static Size Max(this Size size, float max)
    {
        return size.Max(Size.Fraction(max));
    }
}


public class SizeJsonConverter : JsonConverter<Size>
{
    public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization of SizeJsonConverter not implemented.");
    }

    public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}