#pragma warning disable IVYHOOK001

using System.ComponentModel;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.LassoSelect, path: ["Widgets", "Inputs"], searchHints: ["dropdown", "picker", "options", "choice", "select", "menu"])]
public class SelectInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Basic", new SelectInputBasicExample()),
            new Tab("Sizes", new SelectInputSizesExample()),
            new Tab("Variants", new SelectInputVariantsExample()),
            new Tab("Nullable & Edge Cases", new SelectInputAdvancedExample())
        ).Variant(TabsVariant.Content);
    }
}

public class SelectInputBasicExample : ViewBase
{
    public override object? Build()
    {
        var defaultBehavior = UseState("Allowed");
        var notificationTypes = UseState<string[]>([]);

        return Layout.Vertical()
            | Text.H3("Basic Usage")
            | Layout.Vertical().Gap(6)
                | defaultBehavior.ToSelectInput(["Refused", "Allowed", "Ignored"])
                | notificationTypes
                    .ToSelectInput().Options(["Email", "SMS", "Push", "In-App"])
                    .Variant(SelectInputs.List)
                    .Placeholder("Select notification types...")
                    .WithField()
                    .Label("Notification types");
    }
}

public class SelectInputSizesExample : ViewBase
{
    private enum Colors { Red, Green, Blue, Yellow }

    public override object? Build()
    {
        var colorStateSelect = UseState<Colors[]>([]);
        var colorStateList = UseState<Colors[]>([]);
        var colorStateToggle = UseState<Colors[]>([]);
        var colorState = UseState(Colors.Red);
        var colorStateSelectList = UseState(Colors.Red);
        var colorOptions = typeof(Colors).ToOptions();

        var sizesGrid = Layout.Grid().Columns(4)
            | Text.InlineCode("Description")
            | Text.InlineCode("Small")
            | Text.InlineCode("Medium")
            | Text.InlineCode("Large")

            | Text.InlineCode("SelectInputs")
            | colorState.ToSelectInput(colorOptions).Small()
            | colorState.ToSelectInput(colorOptions)
            | colorState.ToSelectInput(colorOptions).Large()

            | Text.InlineCode("SelectInputs.List")
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputs.List).Small()
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputs.List)
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputs.List).Large()

            | Text.InlineCode("SelectInputs.Select")
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputs.Select).Small()
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputs.Select)
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputs.Select).Large()

            | Text.InlineCode("SelectInputs.List (multi)")
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputs.List).Small()
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputs.List)
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputs.List).Large()

            | Text.InlineCode("SelectInputs.Toggle")
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle).Small()
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle)
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle).Large();

        return Layout.Vertical()
            | Text.H3("SelectInput Sizes")
            | sizesGrid;
    }
}

public class SelectInputVariantsExample : ViewBase
{
    private enum Colors { Red, Green, Blue, Yellow }

    private static readonly IAnyOption[] IconOptions =
    [
        new Option<string>(null, "bold", icon: Icons.Bold),
        new Option<string>(null, "italic", icon: Icons.Italic),
        new Option<string>(null, "underline", icon: Icons.Underline)
    ];

    public override object? Build()
    {
        var colorState = UseState(Colors.Red);
        var colorArrayState = UseState(Array.Empty<Colors>());
        var nullableColorArrayState = UseState<Colors[]?>(() => null);
        var colorOptions = typeof(Colors).ToOptions();
        var iconsState = UseState<string>("bold");
        var nullableIconsState = UseState<string?>();

        return Layout.Vertical()
            | Text.H3("Variants")
            | Text.P("Different visual states: default, disabled, invalid, with placeholder, nullable.")
            | Layout.Vertical().Gap(6)
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("SelectInputs.Select")
                    | colorState.ToSelectInput(colorOptions)
                    | colorState.ToSelectInput(colorOptions).Disabled()
                    | colorState.ToSelectInput(colorOptions).Invalid("Invalid")
                    | colorState.ToSelectInput(colorOptions).Placeholder("Select a color")
                    | nullableColorArrayState.ToSelectInput(colorOptions)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("SelectInputs.List")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.List)
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.List).Disabled()
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.List).Invalid("Invalid")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.List).Placeholder("Select colors")
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.List)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.List).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("SelectInputs.Toggle")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle)
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle).Disabled()
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle).Invalid("Invalid")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle).Placeholder("Select a color")
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("Toggle with Icons")
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputs.Toggle)
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputs.Toggle).Disabled()
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputs.Toggle).Invalid("Invalid")
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputs.Toggle).Placeholder("Select a color")
                    | nullableIconsState.ToSelectInput(IconOptions).Variant(SelectInputs.Toggle)
                    | nullableIconsState.ToSelectInput(IconOptions).Variant(SelectInputs.Toggle).Invalid("Invalid"))
            | Text.H3("Multi-Select Variants")
            | CreateMultiSelectVariants();
    }

    private object CreateMultiSelectVariants()
    {
        var colorStateSelect = UseState<Colors[]>([]);
        var colorStateList = UseState<Colors[]>([]);
        var colorStateToggle = UseState<Colors[]>([]);
        var colorOptions = typeof(Colors).ToOptions();

        return Layout.Vertical().Gap(6)
            | (Layout.Horizontal().Gap(6)
                | Text.InlineCode("SelectInputs.Select")
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputs.Select)
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputs.Select).Disabled()
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputs.Select).Invalid("Invalid")
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputs.Select).Placeholder("Select colors")
                | Text.InlineCode($"[{string.Join(", ", colorStateSelect.Value)}]"))
            | (Layout.Horizontal().Gap(6)
                | Text.InlineCode("SelectInputs.List")
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputs.List)
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputs.List).Disabled()
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputs.List).Invalid("Invalid")
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputs.List).Placeholder("Select colors")
                | Text.InlineCode($"[{string.Join(", ", colorStateList.Value)}]"))
            | (Layout.Horizontal().Gap(6)
                | Text.InlineCode("SelectInputs.Toggle")
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle)
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle).Disabled()
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle).Invalid("Invalid")
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle).Placeholder("Select colors")
                | Text.InlineCode($"[{string.Join(", ", colorStateToggle.Value)}]"));
    }
}

public class SelectInputAdvancedExample : ViewBase
{
    private enum Colors { Red, Green, Blue, Yellow }

    private enum DatabaseNamingConvention
    {
        [Description("PascalCase")] PascalCase,
        [Description("camelCase")] CamelCase,
        [Description("snake_case")] SnakeCase,
    }

    private object CreateNullableTestSection()
    {
        var nullableColorState = UseState((Colors?)null);
        var nonNullableColorState = UseState(Colors.Red);
        var colorOptions = typeof(Colors).ToOptions();

        var nullableGrid = Layout.Grid().Columns(4)
            | Text.InlineCode("Type")
            | Text.InlineCode("Select")
            | Text.InlineCode("List")
            | Text.InlineCode("Toggle")

            | Text.InlineCode("Nullable")
            | nullableColorState.ToSelectInput(colorOptions).Nullable()
            | nullableColorState.ToSelectInput(colorOptions).Variant(SelectInputs.List).Nullable()
            | nullableColorState.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle).Nullable()

            | Text.InlineCode("Non-Nullable")
            | nonNullableColorState.ToSelectInput(colorOptions)
            | nonNullableColorState.ToSelectInput(colorOptions).Variant(SelectInputs.List)
            | nonNullableColorState.ToSelectInput(colorOptions).Variant(SelectInputs.Toggle);

        return Layout.Vertical()
            | Text.H3("Nullable vs Non-Nullable")
            | nullableGrid;
    }

    private object CreateLabelValueEdgeCasesSection()
    {
        var namingConventionOptions = typeof(DatabaseNamingConvention).ToOptions();
        var singleSelectState = UseState(DatabaseNamingConvention.PascalCase);
        var multiSelectState = UseState<DatabaseNamingConvention[]>([DatabaseNamingConvention.PascalCase, DatabaseNamingConvention.SnakeCase]);

        var edgeCasesGrid = Layout.Grid().Columns(4)
            | Text.InlineCode("Type")
            | Text.InlineCode("Select")
            | Text.InlineCode("List")
            | Text.InlineCode("Toggle")

            | Text.InlineCode("Single Select")
            | singleSelectState.ToSelectInput(namingConventionOptions)
            | singleSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputs.List)
            | singleSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputs.Toggle)

            | Text.InlineCode("Multi Select")
            | multiSelectState.ToSelectInput(namingConventionOptions)
            | multiSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputs.List)
            | multiSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputs.Toggle);

        return Layout.Vertical()
            | Text.H3("Label/Value Edge Cases")
            | Text.P("Enums with [Description] show labels but store enum values.")
            | edgeCasesGrid;
    }

    public override object? Build()
    {
        return Layout.Vertical()
            | CreateNullableTestSection()
            | CreateLabelValueEdgeCasesSection();
    }
}

#pragma warning restore IVYHOOK001
