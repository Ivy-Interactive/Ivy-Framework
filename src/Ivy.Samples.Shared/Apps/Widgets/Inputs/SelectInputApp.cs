#pragma warning disable IVYHOOK001

using System.ComponentModel;

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
            new Tab("Disabled Options", new SelectInputDisabledOptionsExample()),
            new Tab("Nullable & Edge Cases", new SelectInputAdvancedExample()),
            new Tab("Advanced Props", new SelectInputAdvancedPropsExample()),
            new Tab("Ghost", new SelectInputGhostExample())
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
                    .Variant(SelectInputVariant.List)
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

            | Text.InlineCode("SelectInputVariant")
            | colorState.ToSelectInput(colorOptions).Small()
            | colorState.ToSelectInput(colorOptions)
            | colorState.ToSelectInput(colorOptions).Large()

            | Text.InlineCode("SelectInputVariant.List")
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Small()
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Large()

            | Text.InlineCode("SelectInputVariant.Select")
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select).Small()
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select)
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select).Large()

            | Text.InlineCode("SelectInputVariant.List (multi)")
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Small()
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Large()

            | Text.InlineCode("SelectInputVariant.Toggle")
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Small()
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle)
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Large();

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
        var iconsState = UseState<string>("bold");
        var nullableIconsState = UseState<string?>();
        var colorOptions = typeof(Colors).ToOptions();

        return Layout.Vertical()
            | Text.H3("Variants")
            | Text.P("Different visual states: default, disabled, invalid, with placeholder, nullable.")
            | Layout.Vertical().Gap(6)
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("SelectInputVariant.Select")
                    | colorState.ToSelectInput(colorOptions)
                    | colorState.ToSelectInput(colorOptions).Disabled()
                    | colorState.ToSelectInput(colorOptions).Invalid("Invalid")
                    | colorState.ToSelectInput(colorOptions).Placeholder("Select a color")
                    | nullableColorArrayState.ToSelectInput(colorOptions)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("SelectInputVariant.List")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Disabled()
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Invalid("Invalid")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Placeholder("Select colors")
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("SelectInputVariant.Toggle")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle)
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Disabled()
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Invalid("Invalid")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Placeholder("Select a color")
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("Toggle with Icons")
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle)
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle).Disabled()
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle).Invalid("Invalid")
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle).Placeholder("Select a color")
                    | nullableIconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle)
                    | nullableIconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle).Invalid("Invalid"))
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
                | Text.InlineCode("SelectInputVariant.Select")
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select)
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select).Disabled()
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select).Invalid("Invalid")
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select).Placeholder("Select colors")
                | Text.InlineCode($"[{string.Join(", ", colorStateSelect.Value)}]"))
            | (Layout.Horizontal().Gap(6)
                | Text.InlineCode("SelectInputVariant.List")
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Disabled()
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Invalid("Invalid")
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Placeholder("Select colors")
                | Text.InlineCode($"[{string.Join(", ", colorStateList.Value)}]"))
            | (Layout.Horizontal().Gap(6)
                | Text.InlineCode("SelectInputVariant.Toggle")
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle)
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Disabled()
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Invalid("Invalid")
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Placeholder("Select colors")
                | Text.InlineCode($"[{string.Join(", ", colorStateToggle.Value)}]"));
    }
}

public class SelectInputDisabledOptionsExample : ViewBase
{
    public override object? Build()
    {
        var fruitState = UseState("apple");
        var colorState = UseState<string[]>([]);

        var fruitOptions = new IAnyOption[]
        {
            new Option<string>("Apple", "apple"),
            new Option<string>("Orange", "orange"),
            new Option<string>("Grape (Out of Stock)", "grape").Disabled(),
            new Option<string>("Banana", "banana"),
            new Option<string>("Mango (Coming Soon)", "mango").Disabled(),
        };

        var colorOptions = new IAnyOption[]
        {
            new Option<string>("Red", "red"),
            new Option<string>("Green", "green"),
            new Option<string>("Blue (Premium)", "blue").Disabled(),
            new Option<string>("Yellow", "yellow"),
            new Option<string>("Purple (Unavailable)", "purple").Disabled(),
        };

        return Layout.Vertical()
            | Text.H3("Disabled Options")
            | Text.P("Individual options can be disabled using the fluent .Disabled() method. Disabled options appear greyed out and cannot be selected.")
            | Layout.Grid().Columns(3).Gap(6)
                | (Layout.Vertical().Gap(2)
                    | Text.InlineCode("Select Variant")
                    | fruitState.ToSelectInput(fruitOptions)
                        .Placeholder("Select a fruit..."))
                | (Layout.Vertical().Gap(2)
                    | Text.InlineCode("List Variant")
                    | colorState.ToSelectInput(colorOptions)
                        .Variant(SelectInputVariant.List))
                | (Layout.Vertical().Gap(2)
                    | Text.InlineCode("Toggle Variant")
                    | colorState.ToSelectInput(colorOptions)
                        .Variant(SelectInputVariant.Toggle));
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
            | nullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Nullable()
            | nullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Nullable()

            | Text.InlineCode("Non-Nullable")
            | nonNullableColorState.ToSelectInput(colorOptions)
            | nonNullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
            | nonNullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle);

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
            | singleSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputVariant.List)
            | singleSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputVariant.Toggle)

            | Text.InlineCode("Multi Select")
            | multiSelectState.ToSelectInput(namingConventionOptions)
            | multiSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputVariant.List)
            | multiSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputVariant.Toggle);

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

public class SelectInputAdvancedPropsExample : ViewBase
{
    private enum Frameworks { React, Angular, Vue, Svelte, Ember, Backbone, Preact, Lit, Solid, Alpine }

    public override object? Build()
    {
        var fwSingle = UseState(Frameworks.React);
        var fwMultiList = UseState<Frameworks[]>([Frameworks.React, Frameworks.Vue]);
        var fwMultiToggle = UseState<Frameworks[]>([Frameworks.React, Frameworks.Vue]);
        var fwMultiSelect = UseState<Frameworks[]>([Frameworks.React, Frameworks.Vue]);

        var fwNullableSingle = UseState((Frameworks?)null);
        var fwNullableMultiList = UseState<Frameworks[]?>(() => null);
        var fwNullableMultiToggle = UseState<Frameworks[]?>(() => null);
        var fwNullableMultiSelect = UseState<Frameworks[]?>(() => null);
        var isLoading = UseState(false);
        var isSearchable = UseState(true);

        var options = typeof(Frameworks).ToOptions();

        return Layout.Vertical()
            | Text.H3("Advanced properties")
            | (Layout.Horizontal()
                | isLoading.ToSwitchInput().Label("Loading State")
                | isSearchable.ToSwitchInput().Label("Searchable"))
            | Layout.Grid().Columns(2)
                | (Layout.Vertical()
                    | Text.H4("Select (Single)")
                    | (Layout.Horizontal()
                        | fwSingle.ToSelectInput(options).Variant(SelectInputVariant.Select)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).EmptyMessage("No frameworks found").SearchMode(SearchMode.Fuzzy).Width(Size.Grow())
                        | fwNullableSingle.ToSelectInput(options).Variant(SelectInputVariant.Select)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).EmptyMessage("No frameworks found").SearchMode(SearchMode.Fuzzy).Width(Size.Grow()).Nullable(true)))
                | (Layout.Vertical()
                    | Text.H4("Select (Multi, Min=1, Max=3)")
                    | (Layout.Horizontal()
                        | fwMultiSelect.ToSelectInput(options).Variant(SelectInputVariant.Select)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("No frameworks found").Width(Size.Grow())
                        | fwNullableMultiSelect.ToSelectInput(options).Variant(SelectInputVariant.Select)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("No frameworks found").Width(Size.Grow()).Nullable(true)))
                | (Layout.Vertical()
                    | Text.H4("List (Multi, Min=1, Max=3)")
                    | (Layout.Horizontal()
                        | fwMultiList.ToSelectInput(options).Variant(SelectInputVariant.List)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("No frameworks found").Width(Size.Grow())
                        | fwNullableMultiList.ToSelectInput(options).Variant(SelectInputVariant.List)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("No frameworks found").Width(Size.Grow()).Nullable(true)))
                | (Layout.Vertical()
                    | Text.H4("Toggle (Multi, Min=1, Max=3)")
                    | (Layout.Horizontal()
                        | fwMultiToggle.ToSelectInput(options).Variant(SelectInputVariant.Toggle)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("Nothing here").Width(Size.Grow())
                        | fwNullableMultiToggle.ToSelectInput(options).Variant(SelectInputVariant.Toggle)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("Nothing here").Width(Size.Grow()).Nullable(true)));
    }
}

public class SelectInputGhostExample : ViewBase
{
    private enum Colors { Red, Green, Blue, Yellow }

    public override object? Build()
    {
        var colorState = UseState(Colors.Red);
        var colorArrayState = UseState<Colors[]>([Colors.Red, Colors.Blue]);
        var colorOptions = typeof(Colors).ToOptions();

        return Layout.Vertical()
            | Text.H3("Ghost Styling")
            | Text.P("Ghost styling removes borders and background fill, making the select blend into its surroundings.")
            | Layout.Grid().Columns(2).Gap(6)
                | (Layout.Vertical().Gap(2)
                    | Text.InlineCode("Normal")
                    | colorState.ToSelectInput(colorOptions))
                | (Layout.Vertical().Gap(2)
                    | Text.InlineCode("Ghost")
                    | colorState.ToSelectInput(colorOptions).Ghost())
                | (Layout.Vertical().Gap(2)
                    | Text.InlineCode("Normal (List)")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List))
                | (Layout.Vertical().Gap(2)
                    | Text.InlineCode("Ghost (List)")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Ghost())
                | (Layout.Vertical().Gap(2)
                    | Text.InlineCode("Normal (Toggle)")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle))
                | (Layout.Vertical().Gap(2)
                    | Text.InlineCode("Ghost (Toggle)")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Ghost());
    }
}

#pragma warning restore IVYHOOK001
