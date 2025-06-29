using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Code : WidgetBase<Code>
{
    public Code(string content, string language = "csharp")
    {
        Content = content;
        Language = language;
        Width = Size.Full();
        Height = Size.MaxContent().Max(Size.Px(800));
    }

    [Prop] public string Content { get; set; }
    [Prop] public string Language { get; set; }
    [Prop] public bool ShowLineNumbers { get; set; }
    [Prop] public bool ShowCopyButton { get; set; } = true;
    [Prop] public bool ShowBorder { get; set; } = true;
}

public static class CodeExtensions
{
    public static Code Content(this Code code, string content)
    {
        return code with { Content = content };
    }

    public static Code Language(this Code code, string language)
    {
        return code with { Language = language };
    }

    public static Code ShowLineNumbers(this Code code, bool showLineNumbers = true)
    {
        return code with { ShowLineNumbers = showLineNumbers };
    }

    public static Code ShowCopyButton(this Code code, bool showCopyButton = true)
    {
        return code with { ShowCopyButton = showCopyButton };
    }

    public static Code ShowBorder(this Code code, bool showBorder = true)
    {
        return code with { ShowBorder = showBorder };
    }
}