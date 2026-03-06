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
            new Tab("Nullable & Edge Cases", new SelectInputAdvancedExample()),
            new Tab("Advanced Props", new SelectInputAdvancedPropsExample())
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
                    .Variant(SelectInputVariants.List)
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

            | Text.InlineCode("SelectInputVariants")
            | colorState.ToSelectInput(colorOptions).Small()
            | colorState.ToSelectInput(colorOptions)
            | colorState.ToSelectInput(colorOptions).Large()

            | Text.InlineCode("SelectInputVariants.List")
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Small()
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputVariants.List)
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Large()

            | Text.InlineCode("SelectInputVariants.Select")
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariants.Select).Small()
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariants.Select)
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariants.Select).Large()

            | Text.InlineCode("SelectInputVariants.List (multi)")
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Small()
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariants.List)
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Large()

            | Text.InlineCode("SelectInputVariants.Toggle")
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle).Small()
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle)
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle).Large();

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
                    | Text.InlineCode("SelectInputVariants.Select")
                    | colorState.ToSelectInput(colorOptions)
                    | colorState.ToSelectInput(colorOptions).Disabled()
                    | colorState.ToSelectInput(colorOptions).Invalid("Invalid")
                    | colorState.ToSelectInput(colorOptions).Placeholder("Select a color")
                    | nullableColorArrayState.ToSelectInput(colorOptions)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("SelectInputVariants.List")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.List)
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Disabled()
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Invalid("Invalid")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Placeholder("Select colors")
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.List)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("SelectInputVariants.Toggle")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle)
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle).Disabled()
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle).Invalid("Invalid")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle).Placeholder("Select a color")
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.InlineCode("Toggle with Icons")
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariants.Toggle)
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariants.Toggle).Disabled()
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariants.Toggle).Invalid("Invalid")
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariants.Toggle).Placeholder("Select a color")
                    | nullableIconsState.ToSelectInput(IconOptions).Variant(SelectInputVariants.Toggle)
                    | nullableIconsState.ToSelectInput(IconOptions).Variant(SelectInputVariants.Toggle).Invalid("Invalid"))
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
                | Text.InlineCode("SelectInputVariants.Select")
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariants.Select)
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariants.Select).Disabled()
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariants.Select).Invalid("Invalid")
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariants.Select).Placeholder("Select colors")
                | Text.InlineCode($"[{string.Join(", ", colorStateSelect.Value)}]"))
            | (Layout.Horizontal().Gap(6)
                | Text.InlineCode("SelectInputVariants.List")
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariants.List)
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Disabled()
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Invalid("Invalid")
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Placeholder("Select colors")
                | Text.InlineCode($"[{string.Join(", ", colorStateList.Value)}]"))
            | (Layout.Horizontal().Gap(6)
                | Text.InlineCode("SelectInputVariants.Toggle")
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle)
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle).Disabled()
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle).Invalid("Invalid")
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle).Placeholder("Select colors")
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
            | nullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariants.List).Nullable()
            | nullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle).Nullable()

            | Text.InlineCode("Non-Nullable")
            | nonNullableColorState.ToSelectInput(colorOptions)
            | nonNullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariants.List)
            | nonNullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariants.Toggle);

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
            | singleSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputVariants.List)
            | singleSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputVariants.Toggle)

            | Text.InlineCode("Multi Select")
            | multiSelectState.ToSelectInput(namingConventionOptions)
            | multiSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputVariants.List)
            | multiSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputVariants.Toggle);

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

        var options = typeof(Frameworks).ToOptions();

        var isLoading = UseState(false);
        var isSearchable = UseState(true);

        return Layout.Vertical()
            | Text.H3("Advanced properties")
            | (Layout.Horizontal()
                | isLoading.ToSwitchInput().Label("Loading State")
                | isSearchable.ToSwitchInput().Label("Searchable"))
            | Layout.Grid().Columns(2)
                | (Layout.Vertical()
                    | Text.H4("Select (Single)")
                    | (Layout.Horizontal()
                        | fwSingle.ToSelectInput(options).Variant(SelectInputVariants.Select)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).EmptyMessage("No frameworks found").SearchMode(SearchMode.Fuzzy).Width(Size.Grow())
                        | fwNullableSingle.ToSelectInput(options).Variant(SelectInputVariants.Select)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).EmptyMessage("No frameworks found").SearchMode(SearchMode.Fuzzy).Width(Size.Grow()).Nullable(true)))
                | (Layout.Vertical()
                    | Text.H4("Select (Multi, Min=1, Max=3)")
                    | (Layout.Horizontal()
                        | fwMultiSelect.ToSelectInput(options).Variant(SelectInputVariants.Select)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("No frameworks found").Width(Size.Grow())
                        | fwNullableMultiSelect.ToSelectInput(options).Variant(SelectInputVariants.Select)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("No frameworks found").Width(Size.Grow()).Nullable(true)))
                | (Layout.Vertical()
                    | Text.H4("List (Multi, Min=1, Max=3)")
                    | (Layout.Horizontal()
                        | fwMultiList.ToSelectInput(options).Variant(SelectInputVariants.List)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("No frameworks found").Width(Size.Grow())
                        | fwNullableMultiList.ToSelectInput(options).Variant(SelectInputVariants.List)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("No frameworks found").Width(Size.Grow()).Nullable(true)))
                | (Layout.Vertical()
                    | Text.H4("Toggle (Multi, Min=1, Max=3)")
                    | (Layout.Horizontal()
                        | fwMultiToggle.ToSelectInput(options).Variant(SelectInputVariants.Toggle)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("Nothing here").Width(Size.Grow())
                        | fwNullableMultiToggle.ToSelectInput(options).Variant(SelectInputVariants.Toggle)
                            .Searchable(isSearchable.Value).Loading(isLoading.Value).MinSelections(1).MaxSelections(3).EmptyMessage("Nothing here").Width(Size.Grow()).Nullable(true)));
    }
}

#pragma warning restore IVYHOOK001
