using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Sparkles, path: ["Widgets", "Inputs"], searchHints: ["picker", "icon", "lucide", "select"])]
public class IconInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Icon Input")
               | Layout.Tabs(
                   new Tab("Variants", new IconInputVariantTests()),
                   new Tab("Size Variants", new IconInputSizeVariants()),
                   new Tab("Data Binding", new IconInputDataBindings())
               ).Variant(TabsVariant.Content);
    }
}

public class IconInputVariantTests : ViewBase
{
    public override object Build()
    {
        var defaultState = UseState<Icons>(Icons.Check);
        var invalidState = UseState<Icons>(Icons.CircleAlert);
        var nullableState = UseState<Icons?>(Icons.Search);
        var disabledState = UseState<Icons>(Icons.Settings);

        return Layout.Vertical()
               | Text.H2("Variants")
               | Text.P("Demonstrate different visual states of icon inputs: default, invalid, nullable, and disabled.")
               | Layout.Vertical().Gap(6)
                  | (Layout.Horizontal().Gap(6)
                     | Text.InlineCode("Default")
                     | defaultState.ToIconInput())
                  | (Layout.Horizontal().Gap(6)
                     | Text.InlineCode("Invalid")
                     | invalidState.ToIconInput().Invalid("Please select an icon"))
                  | (Layout.Horizontal().Gap(6)
                     | Text.InlineCode("Nullable")
                     | nullableState.ToIconInput().Nullable())
                  | (Layout.Horizontal().Gap(6)
                     | Text.InlineCode("Disabled")
                     | disabledState.ToIconInput().Disabled());
    }
}

public class IconInputSizeVariants : ViewBase
{
    public override object Build()
    {
        var smallState = UseState<Icons>(Icons.Star);
        var mediumState = UseState<Icons>(Icons.Heart);
        var largeState = UseState<Icons>(Icons.Bell);

        return Layout.Vertical()
               | Text.H2("Size Variants")
               | Text.P("Icon inputs support different sizes: Small, Medium (default), and Large.")
               | Layout.Vertical().Gap(6)
                  | (Layout.Horizontal().Gap(6)
                     | Text.InlineCode("Small")
                     | smallState.ToIconInput().Scale(Scale.Small)
                     | new Icon(smallState.Value).Small())
                  | (Layout.Horizontal().Gap(6)
                     | Text.InlineCode("Medium")
                     | mediumState.ToIconInput().Scale(Scale.Medium)
                     | new Icon(mediumState.Value).Medium())
                  | (Layout.Horizontal().Gap(6)
                     | Text.InlineCode("Large")
                     | largeState.ToIconInput().Scale(Scale.Large)
                     | new Icon(largeState.Value).Large());
    }
}

public class IconInputDataBindings : ViewBase
{
    public override object Build()
    {
        var iconsState = UseState<Icons>(Icons.ChevronDown);
        var nullableIconsState = UseState<Icons?>(Icons.User);

        return Layout.Vertical()
               | Text.H2("Data Binding")
               | Text.P("Icon inputs support Icons (non-nullable) and Icons? (nullable) state types. The selected value updates in real time.")
               | Layout.Grid().Columns(3).Gap(6)
                  | Text.InlineCode("Icons")
                  | iconsState.ToIconInput().Placeholder("Pick an icon")
                  | (Layout.Horizontal().Gap(2)
                     | new Icon(iconsState.Value)
                     | Text.Block(iconsState.Value.ToString()))
                  | Text.InlineCode("Icons?")
                  | nullableIconsState.ToIconInput().Placeholder("Pick an icon (nullable)")
                  | (nullableIconsState.Value.HasValue
                     ? Layout.Horizontal().Gap(2)
                        | new Icon(nullableIconsState.Value!.Value)
                        | Text.Block(nullableIconsState.Value.ToString()!)
                     : Text.InlineCode("null"));
    }
}
