using System.Reflection;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public abstract record WidgetBase<T> : AbstractWidget where T : WidgetBase<T>
{
    protected WidgetBase(params object[] children) : base(children)
    {
    }

    [Prop] public Size? Width { get; set; }

    [Prop] public Size? Height { get; set; }

    [Prop] public Scale? Scale { get; set; }

    [Prop] public bool Visible { get; set; } = true;

    [Prop] public string? TestId { get; set; }
}

public static class WidgetBaseExtensions
{
    extension<T>(T widget) where T : WidgetBase<T>
    {
        public T Width(Size? width) => widget with { Width = width };
        public T Width(int units) => widget with { Width = Shared.Size.Units(units) };
        public T Width(float units) => widget with { Width = Shared.Size.Fraction(units) };
        public T Width(double units) => widget with { Width = Shared.Size.Fraction(Convert.ToSingle(units)) };

        public T Width(string percent)
        {
            if (percent.EndsWith("%"))
            {
                if (float.TryParse(percent[..^1], out var value))
                    return widget with { Width = Shared.Size.Fraction(value / 100) };
            }
            throw new ArgumentException("Invalid percentage value.");
        }

        public T Height(Size? height) => widget with { Height = height };
        public T Height(int units) => widget with { Height = Shared.Size.Units(units) };
        public T Height(float units) => widget with { Height = Shared.Size.Fraction(units) };
        public T Height(double units) => widget with { Height = Shared.Size.Fraction(Convert.ToSingle(units)) };

        public T Height(string percent)
        {
            if (!percent.EndsWith("%")) throw new ArgumentException("Invalid percentage value.");
            if (float.TryParse(percent[..^1], out var value))
                return widget with { Height = Shared.Size.Fraction(value / 100) };
            throw new ArgumentException("Invalid percentage value.");
        }

        public T Size(Size? size) => widget.Width(size).Height(size);
        public T Size(int units) => widget.Width(units).Height(units);
        public T Size(float units) => widget.Width(units).Height(units);
        public T Size(double units) => widget.Width(units).Height(units);

        public T Size(string percent)
        {
            if (!percent.EndsWith("%")) throw new ArgumentException("Invalid percentage value.");
            if (!float.TryParse(percent[..^1], out var value)) throw new ArgumentException("Invalid percentage value.");
            var val = Shared.Size.Fraction(value / 100);
            return widget with { Width = val, Height = val };
        }

        public T Scale(Scale scale) => widget with { Scale = scale };
        public T Small() => widget with { Scale = Shared.Scale.Small };
        public T Medium() => widget with { Scale = Shared.Scale.Medium };
        public T Large() => widget with { Scale = Shared.Scale.Large };
        public T Visible(bool visible = true) => widget with { Visible = visible };
        public T Show() => widget with { Visible = true };
        public T Hide() => widget with { Visible = false };
        public T TestId(string testId) => widget with { TestId = testId };
    }

    internal static void SetScaleViaReflection(object input, Scale? scale)
    {
        var type = input.GetType();
        var prop = type.GetProperty(
            nameof(Scale),
            BindingFlags.Instance | BindingFlags.Public
        );

        if (prop is null) return;
        if (!prop.CanWrite) return;
        if (!prop.PropertyType.IsAssignableFrom(typeof(Scale))) return;

        prop.SetValue(input, scale);
    }
}