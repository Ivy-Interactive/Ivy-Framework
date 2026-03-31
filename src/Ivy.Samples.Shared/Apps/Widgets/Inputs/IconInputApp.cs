
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Sparkles, group: ["Widgets", "Inputs"], searchHints: ["picker", "icon", "lucide", "select"])]
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
                     | Text.Monospaced("Default")
                     | defaultState.ToIconInput())
                  | (Layout.Horizontal().Gap(6)
                     | Text.Monospaced("Invalid")
                     | invalidState.ToIconInput().Invalid("Please select an icon"))
                  | (Layout.Horizontal().Gap(6)
                     | Text.Monospaced("Nullable")
                     | nullableState.ToIconInput().Nullable())
                  | (Layout.Horizontal().Gap(6)
                     | Text.Monospaced("Disabled")
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
                     | Text.Monospaced("Small")
                     | smallState.ToIconInput().Density(Density.Small)
                     | new Icon(smallState.Value).Small())
                  | (Layout.Horizontal().Gap(6)
                     | Text.Monospaced("Medium")
                     | mediumState.ToIconInput().Density(Density.Medium)
                     | new Icon(mediumState.Value).Medium())
                  | (Layout.Horizontal().Gap(6)
                     | Text.Monospaced("Large")
                     | largeState.ToIconInput().Density(Density.Large)
                     | new Icon(largeState.Value).Large());
    }
}

public class IconInputDataBindings : ViewBase
{
    public override object Build()
    {
        var iconsState = UseState<Icons>(Icons.ChevronDown);
        var nullableIconsState = UseState<Icons?>(Icons.User);

        var onBlurState = UseState<Icons>(Icons.Check);
        var onBlurLabel = UseState("");
        var onFocusState = UseState<Icons>(Icons.Check);
        var onFocusLabel = UseState("");

        return Layout.Vertical()
               | Text.H2("Data Binding")
               | Text.P("Icon inputs support Icons (non-nullable) and Icons? (nullable) state types. The selected value updates in real time.")
               | Layout.Grid().Columns(3).Gap(6)
                  | Text.Monospaced("Icons")
                  | iconsState.ToIconInput().Placeholder("Pick an icon")
                  | (Layout.Horizontal().Gap(2)
                     | new Icon(iconsState.Value)
                     | Text.Block(iconsState.Value.ToString()))
                  | Text.Monospaced("Icons?")
                  | nullableIconsState.ToIconInput().Placeholder("Pick an icon (nullable)")
                  | (nullableIconsState.Value.HasValue
                     ? Layout.Horizontal().Gap(2)
                        | new Icon(nullableIconsState.Value!.Value)
                        | Text.Block(nullableIconsState.Value.ToString()!)
                     : Text.Monospaced("null"))
                | Text.H2("Events")
                | (Layout.Vertical()
                   | new Card(
                       Layout.Vertical().Gap(2)
                           | Text.P("The blur event fires when the input loses focus.").Small()
                           | onBlurState.ToIconInput().OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
                           | (onBlurLabel.Value != ""
                               ? Callout.Success(onBlurLabel.Value)
                               : Callout.Info("Interact then click away to see blur events"))
                   ).Title("OnBlur Handler")
                   | new Card(
                       Layout.Vertical().Gap(2)
                           | Text.P("The focus event fires when you click on or tab into the input.").Small()
                           | onFocusState.ToIconInput().OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
                           | (onFocusLabel.Value != ""
                               ? Callout.Success(onFocusLabel.Value)
                               : Callout.Info("Click or tab into the input to see focus events"))
                   ).Title("OnFocus Handler")
                );
    }
}
