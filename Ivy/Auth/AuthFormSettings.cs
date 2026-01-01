using Ivy.Shared;

namespace Ivy.Auth;

public class AuthFormSettings
{
    // Branding
    internal object? Logo { get; private set; }
    internal bool ShowLogo { get; private set; } = true;

    // Text
    internal string? Title { get; private set; }
    internal string? Subtitle { get; private set; }
    internal string? UserLabel { get; private set; }
    internal string? PasswordLabel { get; private set; }
    internal string? ButtonText { get; private set; }

    // Styling
    internal Size? CardWidth { get; private set; }
    internal Size? CardHeight { get; private set; }
    internal int? CardPadding { get; private set; }
    internal int? Gap { get; private set; }
    internal object? Background { get; private set; }
    internal object? Footer { get; private set; }

    // Fluent API: Branding

    public AuthFormSettings WithLogo(object logo)
    {
        Logo = logo;
        return this;
    }

    public AuthFormSettings HideLogo()
    {
        ShowLogo = false;
        return this;
    }

    // Fluent API: Text

    public AuthFormSettings WithTitle(string title)
    {
        Title = title;
        return this;
    }

    public AuthFormSettings WithSubtitle(string subtitle)
    {
        Subtitle = subtitle;
        return this;
    }

    public AuthFormSettings WithUserLabel(string label)
    {
        UserLabel = label;
        return this;
    }

    public AuthFormSettings WithPasswordLabel(string label)
    {
        PasswordLabel = label;
        return this;
    }

    public AuthFormSettings WithButtonText(string text)
    {
        ButtonText = text;
        return this;
    }

    // Fluent API: Styling

    public AuthFormSettings WithCardWidth(Size width)
    {
        CardWidth = width;
        return this;
    }

    public AuthFormSettings WithCardHeight(Size height)
    {
        CardHeight = height;
        return this;
    }

    public AuthFormSettings WithCardPadding(int padding)
    {
        CardPadding = padding;
        return this;
    }

    public AuthFormSettings WithGap(int gap)
    {
        Gap = gap;
        return this;
    }

    public AuthFormSettings WithBackground(object background)
    {
        Background = background;
        return this;
    }

    public AuthFormSettings WithFooter(object footer)
    {
        Footer = footer;
        return this;
    }
}
