
// ReSharper disable once CheckNamespace
namespace Ivy;

public record Chord
{
    public Chord(string dataKey, string? name = null)
    {
        DataKey = dataKey;
        Name = name;
    }

    internal Chord()
    {
    }

    [Prop] public string? DataKey { get; }
    [Prop] public string? Name { get; set; }
    [Prop] public Colors? Fill { get; set; } = null;
    [Prop] public Colors? Stroke { get; set; } = null;
    [Prop] public int StrokeWidth { get; set; } = 1;
    [Prop] public bool Animated { get; set; } = true;
}

public static class ChordExtensions
{
    public static Chord Fill(this Chord chord, Colors fill)
        => chord with { Fill = fill };

    public static Chord Stroke(this Chord chord, Colors stroke)
        => chord with { Stroke = stroke };

    public static Chord StrokeWidth(this Chord chord, int strokeWidth)
        => chord with { StrokeWidth = strokeWidth };

    public static Chord Animated(this Chord chord, bool animated = true)
        => chord with { Animated = animated };
}
