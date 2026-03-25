using System.ComponentModel;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.LassoSelect, group: ["Widgets", "Inputs"], searchHints: ["dropdown", "picker", "options", "choice", "select", "menu"])]
public class SelectInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
            | Text.H1("Select Input")
            | Layout.Tabs(
            new Tab("Basic", new SelectInputBasicExample()),
            new Tab("Events", new SelectInputEventsExample()),
            new Tab("Sizes", new SelectInputSizesExample()),
            new Tab("Variants", new SelectInputVariantsExample()),
            new Tab("Radio", new SelectInputRadioExample()),
            new Tab("Slider", new SelectInputSliderExample()),
            new Tab("Disabled Options", new SelectInputDisabledOptionsExample()),
            new Tab("Tooltips", new SelectInputTooltipsExample()),
            new Tab("Nullable & Edge Cases", new SelectInputAdvancedExample()),
            new Tab("Advanced Props", new SelectInputAdvancedPropsExample()),
            new Tab("Ghost", new SelectInputGhostExample()),
            new Tab("Descriptions", new SelectInputDescriptionsExample())
        ).Variant(TabsVariant.Content)
        ;
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

public class SelectInputEventsExample : ViewBase
{
    public override object? Build()
    {
        var onBlurState = UseState("Allowed");
        var onBlurLabel = UseState("");
        var onFocusState = UseState("Allowed");
        var onFocusLabel = UseState("");

        return Layout.Vertical()
            | Text.H3("Events")
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.P("The blur event fires when the input loses focus.").Small()
                    | (onBlurLabel.Value != ""
                        ? Callout.Success(onBlurLabel.Value)
                        : Callout.Info("Interact then click away to see blur events"))
                    | onBlurState.ToSelectInput(["Refused", "Allowed", "Ignored"]).OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
            ).Title("OnBlur Handler")
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.P("The focus event fires when you click on or tab into the input. Feedback is shown above the control so the open menu does not cover it.").Small()
                    | (onFocusLabel.Value != ""
                        ? Callout.Success(onFocusLabel.Value)
                        : Callout.Info("Click or tab into the input to see focus events"))
                    | onFocusState.ToSelectInput(["Refused", "Allowed", "Ignored"]).OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
            ).Title("OnFocus Handler");
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
            | Text.Monospaced("Description")
            | Text.Monospaced("Small")
            | Text.Monospaced("Medium")
            | Text.Monospaced("Large")

            | Text.Monospaced("SelectInputVariant")
            | colorState.ToSelectInput(colorOptions).Small()
            | colorState.ToSelectInput(colorOptions)
            | colorState.ToSelectInput(colorOptions).Large()

            | Text.Monospaced("SelectInputVariant.List")
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Small()
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
            | colorStateSelectList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Large()

            | Text.Monospaced("SelectInputVariant.Select")
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select).Small()
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select)
            | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select).Large()

            | Text.Monospaced("SelectInputVariant.List (multi)")
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Small()
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
            | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Large()

            | Text.Monospaced("SelectInputVariant.Toggle")
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Small()
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle)
            | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Large()

            | Text.Monospaced("SelectInputVariant.Radio")
            | colorState.ToSelectInput(colorOptions).Radio().Small()
            | colorState.ToSelectInput(colorOptions).Radio()
            | colorState.ToSelectInput(colorOptions).Radio().Large();

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

        var colorStateSelect = UseState<Colors[]>([]);
        var colorStateList = UseState<Colors[]>([]);
        var colorStateToggle = UseState<Colors[]>([]);

        var colorOptions = typeof(Colors).ToOptions();

        return Layout.Vertical()
            | Text.H3("Variants")
            | Text.P("Different visual states: default, disabled, invalid, with placeholder, nullable.")
            | Layout.Vertical().Gap(6)
                | (Layout.Horizontal().Gap(6)
                    | Text.Monospaced("SelectInputVariant.Select")
                    | colorState.ToSelectInput(colorOptions)
                    | colorState.ToSelectInput(colorOptions).Disabled()
                    | colorState.ToSelectInput(colorOptions).Invalid("Invalid")
                    | colorState.ToSelectInput(colorOptions).Placeholder("Select a color")
                    | nullableColorArrayState.ToSelectInput(colorOptions)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.Monospaced("SelectInputVariant.List")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Disabled()
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Invalid("Invalid")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Placeholder("Select colors")
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.Monospaced("SelectInputVariant.Toggle")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle)
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Disabled()
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Invalid("Invalid")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Placeholder("Select a color")
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle)
                    | nullableColorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.Monospaced("SelectInputVariant.Radio")
                    | colorState.ToSelectInput(colorOptions).Radio()
                    | colorState.ToSelectInput(colorOptions).Radio().Disabled()
                    | colorState.ToSelectInput(colorOptions).Radio().Invalid("Invalid")
                    | colorState.ToSelectInput(colorOptions).Radio().Placeholder("Select a color")
                    | nullableColorArrayState.ToSelectInput(colorOptions).Radio().Nullable()
                    | nullableColorArrayState.ToSelectInput(colorOptions).Radio().Invalid("Invalid"))
                | (Layout.Horizontal().Gap(6)
                    | Text.Monospaced("Toggle with Icons")
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle)
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle).Disabled()
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle).Invalid("Invalid")
                    | iconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle).Placeholder("Select a color")
                    | nullableIconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle)
                    | nullableIconsState.ToSelectInput(IconOptions).Variant(SelectInputVariant.Toggle).Invalid("Invalid"))
            | Text.H3("Multi-Select Variants")
            | CreateMultiSelectVariants(colorStateSelect, colorStateList, colorStateToggle);
    }

    private object CreateMultiSelectVariants(IState<Colors[]> colorStateSelect, IState<Colors[]> colorStateList, IState<Colors[]> colorStateToggle)
    {
        var colorOptions = typeof(Colors).ToOptions();

        return Layout.Vertical().Gap(6)
            | (Layout.Horizontal().Gap(6)
                | Text.Monospaced("SelectInputVariant.Select")
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select)
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select).Disabled()
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select).Invalid("Invalid")
                | colorStateSelect.ToSelectInput(colorOptions).Variant(SelectInputVariant.Select).Placeholder("Select colors")
                | Text.Monospaced($"[{string.Join(", ", colorStateSelect.Value)}]"))
            | (Layout.Horizontal().Gap(6)
                | Text.Monospaced("SelectInputVariant.List")
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Disabled()
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Invalid("Invalid")
                | colorStateList.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Placeholder("Select colors")
                | Text.Monospaced($"[{string.Join(", ", colorStateList.Value)}]"))
            | (Layout.Horizontal().Gap(6)
                | Text.Monospaced("SelectInputVariant.Toggle")
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle)
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Disabled()
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Invalid("Invalid")
                | colorStateToggle.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Placeholder("Select colors")
                | Text.Monospaced($"[{string.Join(", ", colorStateToggle.Value)}]"));
    }
}

public class SelectInputSliderExample : ViewBase
{
    private enum Priority { Low, Medium, High, Critical }

    public override object? Build()
    {
        var sizeState = UseState("M");
        var priorityState = UseState(Priority.Medium);
        var sizeOptions = new[] { "XS", "S", "M", "L", "XL", "XXL" }.ToOptions();

        return Layout.Vertical()
            | Text.H3("Slider Variant")
            | Text.P("The Slider variant is ideal for selecting from an ordered list of discrete options.")
            | Layout.Vertical().Gap(6)
                | sizeState.ToSelectInput(sizeOptions).Slider()
                    .WithField().Label("T-Shirt Size")
                | priorityState.ToSelectInput().Slider()
                    .WithField().Label("Priority (Enum)")
                | sizeState.ToSelectInput(sizeOptions).Slider().Disabled()
                    .WithField().Label("Disabled Slider")
            | Text.H3("Densities")
            | Layout.Grid().Columns(3).Gap(6)
                | sizeState.ToSelectInput(sizeOptions).Slider().Small()
                    .WithField().Label("Small")
                | sizeState.ToSelectInput(sizeOptions).Slider().Medium()
                    .WithField().Label("Medium")
                | sizeState.ToSelectInput(sizeOptions).Slider().Large()
                    .WithField().Label("Large");
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
                    | Text.Monospaced("Select Variant")
                    | fruitState.ToSelectInput(fruitOptions)
                        .Placeholder("Select a fruit..."))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("List Variant")
                    | colorState.ToSelectInput(colorOptions)
                        .Variant(SelectInputVariant.List))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Toggle Variant")
                    | colorState.ToSelectInput(colorOptions)
                        .Variant(SelectInputVariant.Toggle));
    }
}

public class SelectInputTooltipsExample : ViewBase
{
    public override object? Build()
    {
        var cacheStrategy = UseState("lru");
        var cacheStrategyMulti = UseState<string[]>([]);

        var cacheOptions = new IAnyOption[]
        {
            new Option<string>("LRU", "lru", tooltip: "Least Recently Used — evicts the oldest accessed entry first"),
            new Option<string>("LFU", "lfu", tooltip: "Least Frequently Used — evicts the least accessed entry first"),
            new Option<string>("FIFO", "fifo", tooltip: "First In, First Out — evicts entries in insertion order"),
            new Option<string>("Write-Through", "write-through", tooltip: "Writes to cache and backing store simultaneously"),
            new Option<string>("Write-Back", "write-back", tooltip: "Writes to cache first, syncs to backing store later").Disabled(),
        };

        return Layout.Vertical()
            | Text.H3("Option Tooltips")
            | Text.P("Hover over options to see contextual help tooltips. Tooltips work across all variants.")
            | Layout.Grid().Columns(3).Gap(6)
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Select Variant")
                    | cacheStrategy.ToSelectInput(cacheOptions)
                        .Placeholder("Select a cache strategy..."))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("List Variant")
                    | cacheStrategyMulti.ToSelectInput(cacheOptions)
                        .Variant(SelectInputVariant.List))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Toggle Variant")
                    | cacheStrategyMulti.ToSelectInput(cacheOptions)
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

    private object CreateNullableTestSection(IState<Colors?> nullableColorState, IState<Colors> nonNullableColorState)
    {
        var colorOptions = typeof(Colors).ToOptions();

        var nullableGrid = Layout.Grid().Columns(4)
            | Text.Monospaced("Type")
            | Text.Monospaced("Select")
            | Text.Monospaced("List")
            | Text.Monospaced("Toggle")

            | Text.Monospaced("Nullable")
            | nullableColorState.ToSelectInput(colorOptions).Nullable()
            | nullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Nullable()
            | nullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Nullable()

            | Text.Monospaced("Non-Nullable")
            | nonNullableColorState.ToSelectInput(colorOptions)
            | nonNullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List)
            | nonNullableColorState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle);

        return Layout.Vertical()
            | Text.H3("Nullable vs Non-Nullable")
            | nullableGrid;
    }

    private object CreateLabelValueEdgeCasesSection(IState<DatabaseNamingConvention> singleSelectState, IState<DatabaseNamingConvention[]> multiSelectState)
    {
        var namingConventionOptions = typeof(DatabaseNamingConvention).ToOptions();

        var edgeCasesGrid = Layout.Grid().Columns(4)
            | Text.Monospaced("Type")
            | Text.Monospaced("Select")
            | Text.Monospaced("List")
            | Text.Monospaced("Toggle")

            | Text.Monospaced("Single Select")
            | singleSelectState.ToSelectInput(namingConventionOptions)
            | singleSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputVariant.List)
            | singleSelectState.ToSelectInput(namingConventionOptions).Variant(SelectInputVariant.Toggle)

            | Text.Monospaced("Multi Select")
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
        var nullableColorState = UseState((Colors?)null);
        var nonNullableColorState = UseState(Colors.Red);

        var singleSelectState = UseState(DatabaseNamingConvention.PascalCase);
        var multiSelectState = UseState<DatabaseNamingConvention[]>([DatabaseNamingConvention.PascalCase, DatabaseNamingConvention.SnakeCase]);

        var namingConventionOptions = typeof(DatabaseNamingConvention).ToOptions();

        return Layout.Vertical()
            | CreateNullableTestSection(nullableColorState, nonNullableColorState)
            | CreateLabelValueEdgeCasesSection(singleSelectState, multiSelectState);
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
                    | Text.Monospaced("Normal")
                    | colorState.ToSelectInput(colorOptions))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Ghost")
                    | colorState.ToSelectInput(colorOptions).Ghost())
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Normal (List)")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Ghost (List)")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.List).Ghost())
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Normal (Toggle)")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Ghost (Toggle)")
                    | colorArrayState.ToSelectInput(colorOptions).Variant(SelectInputVariant.Toggle).Ghost());
    }
}

public class SelectInputDescriptionsExample : ViewBase
{
    public override object? Build()
    {
        var genreToggle = UseState("Comedy");
        var genreRadio = UseState("Comedy");
        var genreCheckbox = UseState<string[]>([]);

        var genreOptions = new IAnyOption[]
        {
            new Option<string>("Comedy", "Comedy") { Description = "Laugh out loud." },
            new Option<string>("Drama", "Drama") { Description = "Get the popcorn." },
            new Option<string>("Documentary", "Documentary") { Description = "Never stop learning." },
            new Option<string>("Action", "Action") { Description = "Edge of your seat thrills." },
        };

        return Layout.Vertical()
            | Text.H3("Option Descriptions")
            | Text.P("Options can include descriptions that appear as caption text below the label in Toggle and Radio/Checkbox (List) variants.")
            | Layout.Vertical().Gap(6)
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Toggle Variant")
                    | genreToggle.ToSelectInput(genreOptions)
                        .Variant(SelectInputVariant.Toggle)
                        .WithField()
                        .Label("Favorite genre"))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Radio Variant (List, Single-Select)")
                    | genreRadio.ToSelectInput(genreOptions)
                        .Variant(SelectInputVariant.List)
                        .WithField()
                        .Label("Favorite genre"))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Checkbox Variant (List, Multi-Select)")
                    | genreCheckbox.ToSelectInput(genreOptions)
                        .Variant(SelectInputVariant.List)
                        .WithField()
                        .Label("Favorite genres"));
    }
}

public class SelectInputRadioExample : ViewBase
{
    private enum Genre { Comedy, Drama, Documentary, Horror, SciFi }

    public override object? Build()
    {
        var genre = UseState(Genre.Comedy);
        var nullableGenre = UseState((Genre?)null);
        var notificationFrequency = UseState("Daily");
        var genreOptions = typeof(Genre).ToOptions();
        var frequencyOptions = new[] { "Immediately", "Daily", "Weekly", "Never" }.ToOptions();

        return Layout.Vertical()
            | Text.H3("Radio Variant")
            | Text.P("Radio buttons for single-select scenarios. A familiar form element for choosing one option from a small set of mutually exclusive choices.")
            | Layout.Grid().Columns(3).Gap(6)
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Basic")
                    | genre.ToSelectInput(genreOptions).Radio()
                        .WithField().Label("Favorite genre"))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Nullable")
                    | nullableGenre.ToSelectInput(genreOptions).Radio().Nullable()
                        .WithField().Label("Favorite genre (optional)"))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Disabled")
                    | genre.ToSelectInput(genreOptions).Radio().Disabled()
                        .WithField().Label("Favorite genre"))
            | Layout.Grid().Columns(3).Gap(6)
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Invalid")
                    | genre.ToSelectInput(genreOptions).Radio().Invalid("Please select a different genre")
                        .WithField().Label("Favorite genre"))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("Ghost")
                    | genre.ToSelectInput(genreOptions).Radio().Ghost()
                        .WithField().Label("Favorite genre"))
                | (Layout.Vertical().Gap(2)
                    | Text.Monospaced("String options")
                    | notificationFrequency.ToSelectInput(frequencyOptions).Radio()
                        .WithField().Label("Notification frequency"));
    }
}
