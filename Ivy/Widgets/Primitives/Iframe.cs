using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Iframe : WidgetBase<Iframe>
{
    public Iframe(string src, long? refreshToken = null)
    {
        Src = src;
        Width = Size.Full();
        Height = Size.Full();
        RefreshToken = refreshToken;
    }

    [Prop] public string Src { get; set; }

    [Prop] public long? RefreshToken { get; }
}