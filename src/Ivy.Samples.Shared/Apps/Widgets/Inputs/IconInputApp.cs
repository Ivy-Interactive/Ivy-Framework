using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Sparkles, path: ["Widgets", "Inputs"], searchHints: ["picker", "icon", "lucide", "select"])]
public class IconInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H2("Size Variants")
               | new IconInputSizeVariants()
               | Text.H1("IconInput")
               | Text.H2("Variants")
               | new IconInputVariants()
               | Text.H2("Data Binding")
               | new IconInputDataBindings()
            ;
    }
}

public class IconInputSizeVariants : ViewBase
{
    public override object Build()
    {
        var smallState = UseState<Icons>(Icons.Star);
        var mediumState = UseState<Icons>(Icons.Heart);
        var largeState = UseState<Icons>(Icons.Bell);

        return Layout.Grid().Columns(3)
            | Text.InlineCode("Size")
            | Text.InlineCode("IconInput")
            | Text.InlineCode("Selected")

            | Text.InlineCode("Small")
            | smallState.ToIconInput().Scale(Scale.Small)
            | Layout.Horizontal() | new Icon(smallState.Value)

            | Text.InlineCode("Medium")
            | mediumState.ToIconInput().Scale(Scale.Medium)
            | Layout.Horizontal() | new Icon(mediumState.Value)

            | Text.InlineCode("Large")
            | largeState.ToIconInput().Scale(Scale.Large)
            | Layout.Horizontal() | new Icon(largeState.Value);
    }
}

public class IconInputVariants : ViewBase
{
    public override object Build()
    {
        var defaultState = UseState<Icons>(Icons.Check);
        var nullableState = UseState<Icons?>(Icons.Search);
        var disabledState = UseState<Icons>(Icons.Settings);
        var invalidState = UseState<Icons>(Icons.CircleAlert);

        return Layout.Grid().Columns(4)
            | Text.InlineCode("")
            | Text.InlineCode("Default")
            | Text.InlineCode("Nullable")
            | Text.InlineCode("Disabled / Invalid")

            | Text.InlineCode("IconInput")
            | defaultState.ToIconInput()
            | nullableState.ToIconInput().Nullable()
            | Layout.Vertical()
                | disabledState.ToIconInput().Disabled()
                | invalidState.ToIconInput().Invalid("Please select an icon");
    }
}

public class IconInputDataBindings : ViewBase
{
    public override object Build()
    {
        var iconsState = UseState<Icons>(Icons.ChevronDown);
        var nullableIconsState = UseState<Icons?>(Icons.User);

        object livePreview = nullableIconsState.Value.HasValue
            ? Layout.Horizontal().Gap(2) | new Icon(nullableIconsState.Value!.Value) | Text.Block(nullableIconsState.Value.ToString()!)
            : Text.InlineCode("null");

        return Layout.Vertical()
            | Layout.Grid().Columns(3)
                | Text.InlineCode("Type")
                | Text.InlineCode("Input")
                | Text.InlineCode("Live Preview")

                | Text.InlineCode("Icons")
                | iconsState.ToIconInput().Placeholder("Pick an icon")
                | Layout.Horizontal().Gap(2) | new Icon(iconsState.Value) | Text.Block(iconsState.Value.ToString())

                | Text.InlineCode("Icons?")
                | nullableIconsState.ToIconInput().Placeholder("Pick an icon (nullable)")
                | livePreview
            ;
    }
}
