namespace Ivy.Samples.Shared.Apps.Tests;

using static InputAffixesGalleryHelpers;

[App(
    icon: Icons.TextCursorInput,
    group: ["Tests"],
    isVisible: true,
    searchHints: ["affix", "prefix", "suffix", "input", "bool", "color", "code", "feedback", "icon", "kbd", "shortcut", "density"])]
public class InputAffixesGalleryApp : SampleBase
{
    protected override object? BuildSample() =>
        Layout.Vertical()
               | Text.H1("Input Affixes Gallery")
               | Layout.Tabs(
                   new Tab("All inputs", new InputAffixesAllInputsView()),
                   new Tab("Text variants", new InputAffixesTextVariantsView()),
                   new Tab("Densities", new InputAffixesDensitiesView()),
                   new Tab("Number inputs", new InputAffixesNumberView()),
                   new Tab("Select inputs", new InputAffixesSelectView()),
                   new Tab("Bool inputs", new InputAffixesBoolView()),
                   new Tab("Color inputs", new InputAffixesColorView()),
                   new Tab("Feedback inputs", new InputAffixesFeedbackView()),
                   new Tab("Icon inputs", new InputAffixesIconView()),
                   new Tab("Code inputs", new InputAffixesCodeView())
               ).Variant(TabsVariant.Content);
}

public class InputAffixesAllInputsView : ViewBase
{
    public override object Build()
    {
        var textState = UseState("ivy.app");
        var passwordState = UseState("secret");
        var searchState = UseState("ivy.app");
        var emailState = UseState("user@ivy.app");
        var telState = UseState("5550100");
        var urlState = UseState("ivy.app");
        var textareaState = UseState("Notes");
        var numberState = UseState(42.5m);
        var currencyState = UseState(Currency.USD);
        var dateState = UseState(DateTime.Now);
        var rangeState = UseState((DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(7))));
        var boolState = UseState(true);
        var colorState = UseState("#6366f1");
        var feedbackState = UseState(3);
        var iconState = UseState(Icons.Heart);
        var codeState = UseState("console.log('ivy');");
        var shortcutTextState = UseState<string?>((string?)null);
        var shortcutPasswordState = UseState<string?>((string?)null);
        var shortcutSearchState = UseState<string?>((string?)null);
        var shortcutEmailState = UseState<string?>((string?)null);
        var shortcutTelState = UseState<string?>((string?)null);
        var shortcutUrlState = UseState<string?>((string?)null);
        var shortcutTextareaState = UseState<string?>((string?)null);
        var currencyOptions = typeof(Currency).ToOptions();

        return Layout.Vertical()
               | Callout.Info(
                   "Compare prefix, suffix, and both affixes across inputs with transparent affix chrome. Shortcut-key rows use empty nullable fields so the kbd hint is visible beside suffix affixes.")
               | Text.H2("Text inputs")
               | AffixHeaderRow()
               | AffixRow("Text", textState.ToTextInput().Prefix(Icons.Link), textState.ToTextInput().Suffix(Icons.Globe), textState.ToTextInput().Prefix(Icons.Link).Suffix(Icons.Globe))
               | AffixRow("Password", passwordState.ToPasswordInput().Prefix(Icons.Lock), passwordState.ToPasswordInput().Suffix(Icons.Key), passwordState.ToPasswordInput().Prefix(Icons.Lock).Suffix(Icons.Shield))
               | AffixRow("Search", searchState.ToSearchInput().Prefix(Icons.ListFilterPlus).Placeholder("Search..."), searchState.ToSearchInput().Suffix(Icons.Tag).Placeholder("Search..."), searchState.ToSearchInput().Prefix(Icons.Folder).Suffix(Icons.Globe).Placeholder("Search..."))
               | AffixRow("Email", emailState.ToEmailInput().Prefix(Icons.Mail), emailState.ToEmailInput().Suffix(Icons.AtSign), emailState.ToEmailInput().Prefix(Icons.Mail).Suffix(Icons.AtSign))
               | AffixRow("Tel", telState.ToTelInput().Prefix(Icons.Phone), telState.ToTelInput().Suffix(Icons.Hash), telState.ToTelInput().Prefix(Icons.Phone).Suffix(Icons.Hash))
               | AffixRow("Url", urlState.ToUrlInput().Prefix(Icons.Link), urlState.ToUrlInput().Suffix(Icons.ExternalLink), urlState.ToUrlInput().Prefix(Icons.Link).Suffix(Icons.ExternalLink))
               | AffixRow("Textarea", textareaState.ToTextareaInput().Prefix(Icons.FileText), textareaState.ToTextareaInput().Suffix(Icons.Type), textareaState.ToTextareaInput().Prefix(Icons.FileText).Suffix(Icons.Type))
               | Text.H2("Shortcut keys with affixes")
               | AffixHeaderRow()
               | AffixRow(
                   "Text",
                   shortcutTextState.ToTextInput().Nullable().Prefix(Icons.Link).ShortcutKey("Ctrl+/").Placeholder("Path"),
                   shortcutTextState.ToTextInput().Nullable().Suffix(Icons.Globe).ShortcutKey("Ctrl+/").Placeholder("Domain"),
                   shortcutTextState.ToTextInput().Nullable().Prefix(Icons.Link).Suffix(Icons.Globe).ShortcutKey("Ctrl+/").Placeholder("URL"))
               | AffixRow(
                   "Password",
                   shortcutPasswordState.ToPasswordInput().Nullable().Prefix(Icons.Lock).ShortcutKey("Ctrl+L").Placeholder("Password"),
                   shortcutPasswordState.ToPasswordInput().Nullable().Suffix(Icons.Key).ShortcutKey("Ctrl+L").Placeholder("Password"),
                   shortcutPasswordState.ToPasswordInput().Nullable().Prefix(Icons.Lock).Suffix(Icons.Shield).ShortcutKey("Ctrl+L").Placeholder("Password"))
               | AffixRow(
                   "Search",
                   shortcutSearchState.ToSearchInput().Nullable().Prefix(Icons.ListFilterPlus).ShortcutKey("Ctrl+K").Placeholder("Search..."),
                   shortcutSearchState.ToSearchInput().Nullable().Suffix(Icons.Tag).ShortcutKey("Ctrl+K").Placeholder("Search..."),
                   shortcutSearchState.ToSearchInput().Nullable().Prefix(Icons.Folder).Suffix(Icons.Globe).ShortcutKey("Ctrl+K").Placeholder("Search..."))
               | AffixRow(
                   "Email",
                   shortcutEmailState.ToEmailInput().Nullable().Prefix(Icons.Mail).ShortcutKey("Ctrl+E").Placeholder("Email"),
                   shortcutEmailState.ToEmailInput().Nullable().Suffix(Icons.AtSign).ShortcutKey("Ctrl+E").Placeholder("Email"),
                   shortcutEmailState.ToEmailInput().Nullable().Prefix(Icons.Mail).Suffix(Icons.AtSign).ShortcutKey("Ctrl+E").Placeholder("Email"))
               | AffixRow(
                   "Tel",
                   shortcutTelState.ToTelInput().Nullable().Prefix(Icons.Phone).ShortcutKey("Ctrl+J").Placeholder("Phone"),
                   shortcutTelState.ToTelInput().Nullable().Suffix(Icons.Hash).ShortcutKey("Ctrl+J").Placeholder("Extension"),
                   shortcutTelState.ToTelInput().Nullable().Prefix(Icons.Phone).Suffix(Icons.Hash).ShortcutKey("Ctrl+J").Placeholder("Phone"))
               | AffixRow(
                   "Url",
                   shortcutUrlState.ToUrlInput().Nullable().Prefix(Icons.Link).ShortcutKey("Ctrl+U").Placeholder("URL"),
                   shortcutUrlState.ToUrlInput().Nullable().Suffix(Icons.ExternalLink).ShortcutKey("Ctrl+U").Placeholder("URL"),
                   shortcutUrlState.ToUrlInput().Nullable().Prefix(Icons.Link).Suffix(Icons.ExternalLink).ShortcutKey("Ctrl+U").Placeholder("URL"))
               | AffixRow(
                   "Textarea",
                   shortcutTextareaState.ToTextareaInput().Nullable().Prefix(Icons.FileText).ShortcutKey("Ctrl+T").Placeholder("Notes"),
                   shortcutTextareaState.ToTextareaInput().Nullable().Suffix(Icons.Type).ShortcutKey("Ctrl+T").Placeholder("Notes"),
                   shortcutTextareaState.ToTextareaInput().Nullable().Prefix(Icons.FileText).Suffix(Icons.Type).ShortcutKey("Ctrl+T").Placeholder("Notes"))
               | Text.H2("Other inputs")
               | AffixHeaderRow()
               | AffixRow("Number", numberState.ToNumberInput().Prefix(Icons.DollarSign).Precision(1), numberState.ToNumberInput().Suffix(Icons.Percent).Precision(1), numberState.ToNumberInput().Prefix(Icons.DollarSign).Suffix(Icons.Coins).Precision(1))
               | AffixRow("Select", currencyState.ToSelectInput(currencyOptions).Prefix(Icons.DollarSign), currencyState.ToSelectInput(currencyOptions).Suffix(Icons.BadgeDollarSign), currencyState.ToSelectInput(currencyOptions).Prefix(Icons.DollarSign).Suffix(Icons.BadgeDollarSign))
               | AffixRow("DateTime", dateState.ToDateTimeInput().Prefix(Icons.Calendar), dateState.ToDateTimeInput().Suffix(Icons.Clock), dateState.ToDateTimeInput().Prefix(Icons.Calendar).Suffix(Icons.Clock))
               | AffixRow("Date range", rangeState.ToDateRangeInput().Prefix(Icons.CalendarRange), rangeState.ToDateRangeInput().Suffix(Icons.CalendarDays), rangeState.ToDateRangeInput().Prefix(Icons.CalendarRange).Suffix(Icons.CalendarDays))
               | AffixRow("Bool", boolState.ToBoolInput().Label("Enable").Prefix(Icons.Bell), boolState.ToBoolInput().Label("Enable").Suffix(Icons.BadgeQuestionMark), boolState.ToBoolInput().Label("Enable").Prefix(Icons.Bell).Suffix(Icons.BadgeQuestionMark))
               | AffixRow("Color", colorState.ToColorInput().Prefix(Icons.Palette), colorState.ToColorInput().Suffix(Icons.Pipette), colorState.ToColorInput().Prefix(Icons.Palette).Suffix(Icons.Pipette))
               | AffixRow("Feedback", feedbackState.ToFeedbackInput().Stars().Prefix(Icons.Star), feedbackState.ToFeedbackInput().Stars().Suffix(Icons.MessageSquare), feedbackState.ToFeedbackInput().Stars().Prefix(Icons.Star).Suffix(Icons.MessageSquare))
               | AffixRow("Icon", iconState.ToIconInput().Prefix(Icons.Search), iconState.ToIconInput().Suffix(Icons.Sparkles), iconState.ToIconInput().Prefix(Icons.Search).Suffix(Icons.Sparkles))
               | AffixRow(
                   "Code",
                   CodeAffixDemo(codeState).Prefix(Icons.Code),
                   CodeAffixDemo(codeState).Suffix(Icons.Braces),
                   CodeAffixDemo(codeState).Prefix(Icons.Code).Suffix(Icons.Braces));
    }

    private enum Currency
    {
        USD,
        EUR,
        GBP,
    }
}

public class InputAffixesTextVariantsView : ViewBase
{
    public override object Build()
    {
        var textState = UseState("ivy.app");
        var passwordState = UseState("secret");
        var searchState = UseState("ivy.app");
        var emailState = UseState("user@ivy.app");
        var telState = UseState("5550100");
        var urlState = UseState("ivy.app");
        var textareaState = UseState("Notes");

        return Layout.Vertical()
               | Callout.Info(
                   "Compare trailing controls (password eye, search clear, nullable clear) with suffix affixes. Eye and clear buttons should sit in the same column between field and suffix across variants.")
               | AffixHeaderRow()
               | AffixRow(
                   "Text",
                   textState.ToTextInput().Nullable().Prefix(Icons.Link),
                   textState.ToTextInput().Nullable().Suffix(Icons.Globe),
                   textState.ToTextInput().Nullable().Prefix(Icons.Link).Suffix(Icons.Globe))
               | AffixRow(
                   "Password",
                   passwordState.ToPasswordInput().Prefix(Icons.Lock),
                   passwordState.ToPasswordInput().Suffix(Icons.Key),
                   passwordState.ToPasswordInput().Prefix(Icons.Lock).Suffix(Icons.Shield))
               | AffixRow(
                   "Search",
                   searchState.ToSearchInput().Prefix(Icons.ListFilterPlus).Placeholder("Search..."),
                   searchState.ToSearchInput().Suffix(Icons.Tag).Placeholder("Search..."),
                   searchState.ToSearchInput().Prefix(Icons.Folder).Suffix(Icons.Globe).Placeholder("Search..."))
               | AffixRow(
                   "Email",
                   emailState.ToEmailInput().Prefix(Icons.Mail),
                   emailState.ToEmailInput().Suffix(Icons.AtSign),
                   emailState.ToEmailInput().Prefix(Icons.Mail).Suffix(Icons.AtSign))
               | AffixRow(
                   "Tel",
                   telState.ToTelInput().Prefix(Icons.Phone),
                   telState.ToTelInput().Suffix(Icons.Hash),
                   telState.ToTelInput().Prefix(Icons.Phone).Suffix(Icons.Hash))
               | AffixRow(
                   "Url",
                   urlState.ToUrlInput().Prefix(Icons.Link),
                   urlState.ToUrlInput().Suffix(Icons.ExternalLink),
                   urlState.ToUrlInput().Prefix(Icons.Link).Suffix(Icons.ExternalLink))
               | AffixRow(
                   "Textarea",
                   textareaState.ToTextareaInput().Nullable().Prefix(Icons.FileText),
                   textareaState.ToTextareaInput().Nullable().Suffix(Icons.Type),
                   textareaState.ToTextareaInput().Nullable().Prefix(Icons.FileText).Suffix(Icons.Type));
    }
}

public class InputAffixesNumberView : ViewBase
{
    public override object Build()
    {
        var numberState = UseState(42.5m);
        var nullableState = UseState<decimal?>(() => null);
        var invalidState = UseState(-5m);

        return Layout.Vertical()
               | Callout.Info(
                   "Number input affixes: icon size matches trailing invalid/clear controls; symmetric padding around prefix/suffix icons.")
               | Text.H2("Affix layouts")
               | AffixHeaderRow()
               | AffixRow(
                   "Number",
                   numberState.ToNumberInput().Precision(1).Prefix(Icons.DollarSign),
                   numberState.ToNumberInput().Precision(1).Suffix(Icons.Percent),
                   numberState.ToNumberInput().Precision(1).Prefix(Icons.DollarSign).Suffix(Icons.Coins))
               | AffixRow(
                   "Nullable",
                   nullableState.ToNumberInput().Nullable().Precision(1).Prefix(Icons.DollarSign),
                   nullableState.ToNumberInput().Nullable().Precision(1).Suffix(Icons.Percent),
                   nullableState.ToNumberInput().Nullable().Precision(1).Prefix(Icons.DollarSign).Suffix(Icons.Coins))
               | AffixRow(
                   "Invalid",
                   invalidState.ToNumberInput().Precision(0).Prefix(Icons.DollarSign).Invalid("Must be positive"),
                   invalidState.ToNumberInput().Precision(0).Suffix(Icons.Percent).Invalid("Must be positive"),
                   invalidState
                     .ToNumberInput()
                     .Precision(0)
                     .Prefix(Icons.DollarSign)
                     .Suffix(Icons.Coins)
                     .Invalid("Must be positive"))
               | AffixRow(
                   "Nullable + invalid",
                   nullableState.ToNumberInput().Nullable().Precision(1).Prefix(Icons.DollarSign).Invalid("Required"),
                   nullableState.ToNumberInput().Nullable().Precision(1).Suffix(Icons.Percent).Invalid("Required"),
                   nullableState
                     .ToNumberInput()
                     .Nullable()
                     .Precision(1)
                     .Prefix(Icons.DollarSign)
                     .Suffix(Icons.Coins)
                     .Invalid("Required"))
               | Text.H2("Densities")
               | Callout.Info("Both prefix and suffix with nullable clear at Small, Medium, and Large density.")
               | DensityHeaderRow()
               | NumberDensityRow(
                   "Number",
                   numberState.ToNumberInput().Nullable().Precision(1).Prefix(Icons.DollarSign).Suffix(Icons.Percent))
               | NumberDensityRow(
                   "Both + invalid",
                   invalidState
                     .ToNumberInput()
                     .Nullable()
                     .Precision(0)
                     .Prefix(Icons.DollarSign)
                     .Suffix(Icons.Coins)
                     .Invalid("Must be positive"));
    }
}

public class InputAffixesSelectView : ViewBase
{
    public override object Build()
    {
        var currencyState = UseState(Currency.USD);
        var nullableState = UseState<Currency?>(() => null);
        var currencyOptions = typeof(Currency).ToOptions();

        return Layout.Vertical()
               | Callout.Info(
                   "Select affixes: clear and invalid sit beside the suffix icon (not inside the trigger); icon glyphs match trailing control size.")
               | Text.H2("Affix layouts")
               | AffixHeaderRow()
               | AffixRow(
                   "Select",
                   currencyState.ToSelectInput(currencyOptions).Prefix(Icons.DollarSign),
                   currencyState.ToSelectInput(currencyOptions).Suffix(Icons.BadgeDollarSign),
                   currencyState.ToSelectInput(currencyOptions).Prefix(Icons.DollarSign).Suffix(Icons.BadgeDollarSign))
               | AffixRow(
                   "Nullable",
                   nullableState.ToSelectInput(currencyOptions).Nullable().Prefix(Icons.DollarSign),
                   nullableState.ToSelectInput(currencyOptions).Nullable().Suffix(Icons.BadgeDollarSign),
                   nullableState
                     .ToSelectInput(currencyOptions)
                     .Nullable()
                     .Prefix(Icons.DollarSign)
                     .Suffix(Icons.BadgeDollarSign))
               | AffixRow(
                   "Invalid",
                   currencyState.ToSelectInput(currencyOptions).Prefix(Icons.DollarSign).Invalid("Required"),
                   currencyState.ToSelectInput(currencyOptions).Suffix(Icons.BadgeDollarSign).Invalid("Required"),
                   currencyState
                     .ToSelectInput(currencyOptions)
                     .Prefix(Icons.DollarSign)
                     .Suffix(Icons.BadgeDollarSign)
                     .Invalid("Required"))
               | AffixRow(
                   "Nullable + invalid",
                   nullableState.ToSelectInput(currencyOptions).Nullable().Prefix(Icons.DollarSign).Invalid("Required"),
                   nullableState.ToSelectInput(currencyOptions).Nullable().Suffix(Icons.BadgeDollarSign).Invalid("Required"),
                   nullableState
                     .ToSelectInput(currencyOptions)
                     .Nullable()
                     .Prefix(Icons.DollarSign)
                     .Suffix(Icons.BadgeDollarSign)
                     .Invalid("Required"))
               | Text.H2("Densities")
               | Callout.Info("Both prefix and suffix with nullable clear at Small, Medium, and Large density.")
               | DensityHeaderRow()
               | SelectDensityRow(
                   "Select",
                   currencyState.ToSelectInput(currencyOptions).Nullable().Prefix(Icons.DollarSign).Suffix(Icons.BadgeDollarSign))
               | SelectDensityRow(
                   "Both + invalid",
                   currencyState
                     .ToSelectInput(currencyOptions)
                     .Nullable()
                     .Prefix(Icons.DollarSign)
                     .Suffix(Icons.BadgeDollarSign)
                     .Invalid("Required"));
    }

    private enum Currency
    {
        USD,
        EUR,
        GBP,
    }
}

public class InputAffixesBoolView : ViewBase
{
    public override object Build()
    {
        var boolState = UseState(true);
        var nullableState = UseState<bool?>(() => null);

        return Layout.Vertical()
               | Callout.Info(
                   "Bool affixes: prefix/suffix icon scale matches number/select; invalid sits in the suffix cluster beside the suffix glyph (not on the checkbox). Use this tab to verify gaps, density, and invalid+suffix layout.")
               | Text.H2("Affix layouts")
               | AffixHeaderRow()
               | AffixRow(
                   "Checkbox",
                   boolState.ToBoolInput().Label("Enable").Prefix(Icons.Bell),
                   boolState.ToBoolInput().Label("Enable").Suffix(Icons.BadgeQuestionMark),
                   boolState.ToBoolInput().Label("Enable").Prefix(Icons.Bell).Suffix(Icons.BadgeQuestionMark))
               | AffixRow(
                   "Switch",
                   boolState.ToSwitchInput().Label("Enable").Prefix(Icons.Bell),
                   boolState.ToSwitchInput().Label("Enable").Suffix(Icons.BadgeQuestionMark),
                   boolState.ToSwitchInput().Label("Enable").Prefix(Icons.Bell).Suffix(Icons.BadgeQuestionMark))
               | AffixRow(
                   "Toggle",
                   boolState.ToToggleInput(Icons.Star).Label("Enable").Prefix(Icons.Bell),
                   boolState.ToToggleInput(Icons.Star).Label("Enable").Suffix(Icons.BadgeQuestionMark),
                   boolState
                     .ToToggleInput(Icons.Star)
                     .Label("Enable")
                     .Prefix(Icons.Bell)
                     .Suffix(Icons.BadgeQuestionMark))
               | AffixRow(
                   "Nullable",
                   nullableState.ToBoolInput().Label("Enable").Nullable().Prefix(Icons.Bell),
                   nullableState.ToBoolInput().Label("Enable").Nullable().Suffix(Icons.BadgeQuestionMark),
                   nullableState
                     .ToBoolInput()
                     .Label("Enable")
                     .Nullable()
                     .Prefix(Icons.Bell)
                     .Suffix(Icons.BadgeQuestionMark))
               | AffixRow(
                   "Invalid",
                   boolState.ToBoolInput().Label("Enable").Prefix(Icons.Bell).Invalid("Must be enabled"),
                   boolState.ToBoolInput().Label("Enable").Suffix(Icons.BadgeQuestionMark).Invalid("Must be enabled"),
                   boolState
                     .ToBoolInput()
                     .Label("Enable")
                     .Prefix(Icons.Bell)
                     .Suffix(Icons.BadgeQuestionMark)
                     .Invalid("Must be enabled"))
               | AffixRow(
                   "Nullable + invalid",
                   nullableState.ToBoolInput().Label("Enable").Nullable().Prefix(Icons.Bell).Invalid("Required"),
                   nullableState.ToBoolInput().Label("Enable").Nullable().Suffix(Icons.BadgeQuestionMark).Invalid("Required"),
                   nullableState
                     .ToBoolInput()
                     .Label("Enable")
                     .Nullable()
                     .Prefix(Icons.Bell)
                     .Suffix(Icons.BadgeQuestionMark)
                     .Invalid("Required"))
               | Text.H2("Densities")
               | Callout.Info("Both prefix and suffix at Small, Medium, and Large — compare row height and icon gaps to the Number/Select tabs.")
               | DensityHeaderRow()
               | BoolDensityRow(
                   "Checkbox",
                   boolState.ToBoolInput().Label("Enable").Prefix(Icons.Bell).Suffix(Icons.BadgeQuestionMark))
               | BoolDensityRow(
                   "Both + invalid",
                   boolState
                     .ToBoolInput()
                     .Label("Enable")
                     .Prefix(Icons.Bell)
                     .Suffix(Icons.BadgeQuestionMark)
                     .Invalid("Must be enabled"));
    }
}

public class InputAffixesColorView : ViewBase
{
    public override object Build()
    {
        var colorState = UseState("#6366f1");
        var nullableState = UseState<string?>(() => null);

        ColorInputBase TextAffix(ColorInputBase input) => input.Variant(ColorInputVariant.Text);
        ColorInputBase PickerAffix(ColorInputBase input) => input;

        return Layout.Vertical()
               | Callout.Info(
                   "Color affixes match text/bool: same affix shell, field padding, and invalid/clear in the suffix cluster when suffix is present.")
               | Text.H2("Affix layouts")
               | AffixHeaderRow()
               | AffixRow(
                   "Text",
                   TextAffix(colorState.ToColorInput()).Prefix(Icons.Palette),
                   TextAffix(colorState.ToColorInput()).Suffix(Icons.Pipette),
                   TextAffix(colorState.ToColorInput()).Prefix(Icons.Palette).Suffix(Icons.Pipette))
               | AffixRow(
                   "Text + picker",
                   PickerAffix(colorState.ToColorInput()).Prefix(Icons.Palette),
                   PickerAffix(colorState.ToColorInput()).Suffix(Icons.Pipette),
                   PickerAffix(colorState.ToColorInput()).Prefix(Icons.Palette).Suffix(Icons.Pipette))
               | AffixRow(
                   "Nullable",
                   TextAffix(nullableState.ToColorInput()).Nullable().Prefix(Icons.Palette),
                   TextAffix(nullableState.ToColorInput()).Nullable().Suffix(Icons.Pipette),
                   TextAffix(nullableState.ToColorInput())
                     .Nullable()
                     .Prefix(Icons.Palette)
                     .Suffix(Icons.Pipette))
               | AffixRow(
                   "Invalid",
                   TextAffix(colorState.ToColorInput()).Prefix(Icons.Palette).Invalid("Invalid color"),
                   TextAffix(colorState.ToColorInput()).Suffix(Icons.Pipette).Invalid("Invalid color"),
                   TextAffix(colorState.ToColorInput())
                     .Prefix(Icons.Palette)
                     .Suffix(Icons.Pipette)
                     .Invalid("Invalid color"))
               | AffixRow(
                   "Nullable + invalid",
                   TextAffix(nullableState.ToColorInput()).Nullable().Prefix(Icons.Palette).Invalid("Required"),
                   TextAffix(nullableState.ToColorInput()).Nullable().Suffix(Icons.Pipette).Invalid("Required"),
                   TextAffix(nullableState.ToColorInput())
                     .Nullable()
                     .Prefix(Icons.Palette)
                     .Suffix(Icons.Pipette)
                     .Invalid("Required"))
               | Text.H2("Densities")
               | Callout.Info("Text + picker with both affixes at Small, Medium, and Large.")
               | DensityHeaderRow()
               | ColorDensityRow(
                   "Both",
                   PickerAffix(colorState.ToColorInput()).Prefix(Icons.Palette).Suffix(Icons.Pipette))
               | ColorDensityRow(
                   "Both + invalid",
                   PickerAffix(colorState.ToColorInput())
                     .Prefix(Icons.Palette)
                     .Suffix(Icons.Pipette)
                     .Invalid("Invalid color"));
    }
}

public class InputAffixesFeedbackView : ViewBase
{
    public override object Build()
    {
        var starsState = UseState(3);
        var emojisState = UseState(3);
        var thumbsState = UseState(true);
        var nullableThumbsState = UseState<bool?>(() => null);

        return Layout.Vertical()
               | Callout.Info(
                   "Feedback affixes match bool/color: shrink-to-fit shell, text field padding, invalid in suffix cluster when suffix is present.")
               | Text.H2("Affix layouts")
               | AffixHeaderRow()
               | AffixRow(
                   "Stars",
                   starsState.ToFeedbackInput().Stars().Prefix(Icons.Star),
                   starsState.ToFeedbackInput().Stars().Suffix(Icons.MessageSquare),
                   starsState.ToFeedbackInput().Stars().Prefix(Icons.Star).Suffix(Icons.MessageSquare))
               | AffixRow(
                   "Emojis",
                   emojisState.ToFeedbackInput().Emojis().Prefix(Icons.Smile),
                   emojisState.ToFeedbackInput().Emojis().Suffix(Icons.MessageSquare),
                   emojisState.ToFeedbackInput().Emojis().Prefix(Icons.Smile).Suffix(Icons.MessageSquare))
               | AffixRow(
                   "Thumbs",
                   thumbsState.ToFeedbackInput().Thumbs().Prefix(Icons.ThumbsUp),
                   thumbsState.ToFeedbackInput().Thumbs().Suffix(Icons.MessageSquare),
                   thumbsState.ToFeedbackInput().Thumbs().Prefix(Icons.ThumbsUp).Suffix(Icons.MessageSquare))
               | AffixRow(
                   "Invalid",
                   starsState.ToFeedbackInput().Stars().Prefix(Icons.Star).Invalid("Required"),
                   starsState.ToFeedbackInput().Stars().Suffix(Icons.MessageSquare).Invalid("Required"),
                   starsState
                     .ToFeedbackInput()
                     .Stars()
                     .Prefix(Icons.Star)
                     .Suffix(Icons.MessageSquare)
                     .Invalid("Required"))
               | AffixRow(
                   "Nullable thumbs",
                   nullableThumbsState.ToFeedbackInput().Thumbs().Nullable().Prefix(Icons.ThumbsUp),
                   nullableThumbsState.ToFeedbackInput().Thumbs().Nullable().Suffix(Icons.MessageSquare),
                   nullableThumbsState
                     .ToFeedbackInput()
                     .Thumbs()
                     .Nullable()
                     .Prefix(Icons.ThumbsUp)
                     .Suffix(Icons.MessageSquare))
               | Text.H2("Densities")
               | Callout.Info("Stars with both affixes at Small, Medium, and Large.")
               | DensityHeaderRow()
               | FeedbackDensityRow(
                   "Both",
                   starsState.ToFeedbackInput().Stars().Prefix(Icons.Star).Suffix(Icons.MessageSquare))
               | FeedbackDensityRow(
                   "Both + invalid",
                   starsState
                     .ToFeedbackInput()
                     .Stars()
                     .Prefix(Icons.Star)
                     .Suffix(Icons.MessageSquare)
                     .Invalid("Required"));
    }
}

public class InputAffixesIconView : ViewBase
{
    public override object Build()
    {
        var iconState = UseState(Icons.Heart);
        var nullableState = UseState<Icons?>(() => null);

        return Layout.Vertical()
               | Callout.Info(
                   "Icon affixes match feedback: same affix shell and content padding; prefix icon aligns with other shrink-to-fit inputs. Compare prefix column to Feedback tab.")
               | Text.H2("Affix layouts")
               | AffixHeaderRow()
               | AffixRow(
                   "Icon",
                   iconState.ToIconInput().Prefix(Icons.Search),
                   iconState.ToIconInput().Suffix(Icons.Sparkles),
                   iconState.ToIconInput().Prefix(Icons.Search).Suffix(Icons.Sparkles))
               | AffixRow(
                   "Nullable",
                   nullableState.ToIconInput().Nullable().Prefix(Icons.Search),
                   nullableState.ToIconInput().Nullable().Suffix(Icons.Sparkles),
                   nullableState
                     .ToIconInput()
                     .Nullable()
                     .Prefix(Icons.Search)
                     .Suffix(Icons.Sparkles))
               | AffixRow(
                   "Invalid",
                   iconState.ToIconInput().Prefix(Icons.Search).Invalid("Required"),
                   iconState.ToIconInput().Suffix(Icons.Sparkles).Invalid("Required"),
                   iconState
                     .ToIconInput()
                     .Prefix(Icons.Search)
                     .Suffix(Icons.Sparkles)
                     .Invalid("Required"))
               | AffixRow(
                   "Nullable + invalid",
                   nullableState.ToIconInput().Nullable().Prefix(Icons.Search).Invalid("Required"),
                   nullableState.ToIconInput().Nullable().Suffix(Icons.Sparkles).Invalid("Required"),
                   nullableState
                     .ToIconInput()
                     .Nullable()
                     .Prefix(Icons.Search)
                     .Suffix(Icons.Sparkles)
                     .Invalid("Required"))
               | Text.H2("Densities")
               | Callout.Info("Both affixes at Small, Medium, and Large — compare prefix alignment to Feedback row above.")
               | DensityHeaderRow()
               | IconDensityRow(
                   "Both",
                   iconState.ToIconInput().Prefix(Icons.Search).Suffix(Icons.Sparkles))
               | IconDensityRow(
                   "Both + invalid",
                   iconState
                     .ToIconInput()
                     .Prefix(Icons.Search)
                     .Suffix(Icons.Sparkles)
                     .Invalid("Required"));
    }
}

public class InputAffixesCodeView : ViewBase
{
    public override object Build()
    {
        var codeState = UseState("console.log('ivy');");
        var nullableState = UseState<string?>(() => null);
        var invalidState = UseState("console.log('ivy');");

        return Layout.Vertical()
               | Callout.Info(
                   "Code affixes: prefix/suffix icons share one row with the copy control; copy sits in a trailing affix column when no suffix slot. Compare prefix alignment and right inset to Icon/Feedback tabs.")
               | Text.H2("Affix layouts")
               | AffixHeaderRow()
               | AffixRow(
                   "Code",
                   CodeAffixDemo(codeState).Prefix(Icons.Code),
                   CodeAffixDemo(codeState).Suffix(Icons.Braces),
                   CodeAffixDemo(codeState).Prefix(Icons.Code).Suffix(Icons.Braces))
               | AffixRow(
                   "Nullable",
                   CodeAffixDemo(nullableState).Nullable().Prefix(Icons.Code),
                   CodeAffixDemo(nullableState).Nullable().Suffix(Icons.Braces),
                   CodeAffixDemo(nullableState)
                     .Nullable()
                     .Prefix(Icons.Code)
                     .Suffix(Icons.Braces))
               | AffixRow(
                   "Invalid",
                   CodeAffixDemo(invalidState).Prefix(Icons.Code).Invalid("Invalid snippet"),
                   CodeAffixDemo(invalidState).Suffix(Icons.Braces).Invalid("Invalid snippet"),
                   CodeAffixDemo(invalidState)
                     .Prefix(Icons.Code)
                     .Suffix(Icons.Braces)
                     .Invalid("Invalid snippet"))
               | AffixRow(
                   "Nullable + invalid",
                   CodeAffixDemo(nullableState).Nullable().Prefix(Icons.Code).Invalid("Required"),
                   CodeAffixDemo(nullableState).Nullable().Suffix(Icons.Braces).Invalid("Required"),
                   CodeAffixDemo(nullableState)
                     .Nullable()
                     .Prefix(Icons.Code)
                     .Suffix(Icons.Braces)
                     .Invalid("Required"))
               | Text.H2("Densities")
               | Callout.Info("Both affixes at Small, Medium, and Large — compare affix row height, icon gaps, and copy placement.")
               | DensityHeaderRow()
               | CodeDensityRow(
                   "Both",
                   CodeAffixDemo(codeState).Prefix(Icons.Code).Suffix(Icons.Braces))
               | CodeDensityRow(
                   "Both + invalid",
                   CodeAffixDemo(invalidState)
                     .Prefix(Icons.Code)
                     .Suffix(Icons.Braces)
                     .Invalid("Invalid snippet"));
    }
}

public class InputAffixesDensitiesView : ViewBase
{
    public override object Build()
    {
        var textState = UseState("ivy.app");
        var passwordState = UseState("secret");
        var searchState = UseState("ivy.app");
        var emailState = UseState("user@ivy.app");
        var telState = UseState("5550100");
        var urlState = UseState("ivy.app");
        var textareaState = UseState("Notes");

        return Layout.Vertical()
               | Callout.Info("Both prefix and suffix affixes with trailing controls at Small, Medium, and Large density.")
               | DensityHeaderRow()
               | DensityRow("Text", textState.ToTextInput().Nullable().Prefix(Icons.Link).Suffix(Icons.Globe))
               | DensityRow("Password", passwordState.ToPasswordInput().Prefix(Icons.Lock).Suffix(Icons.Key))
               | DensityRow("Search", searchState.ToSearchInput().Prefix(Icons.ListFilterPlus).Suffix(Icons.Tag).Placeholder("Search..."))
               | DensityRow("Email", emailState.ToEmailInput().Prefix(Icons.Mail).Suffix(Icons.AtSign))
               | DensityRow("Tel", telState.ToTelInput().Prefix(Icons.Phone).Suffix(Icons.Hash))
               | DensityRow("Url", urlState.ToUrlInput().Prefix(Icons.Link).Suffix(Icons.ExternalLink))
               | DensityRow("Textarea", textareaState.ToTextareaInput().Nullable().Prefix(Icons.FileText).Suffix(Icons.Type));
    }
}

static class InputAffixesGalleryHelpers
{
    internal static CodeInputBase CodeAffixDemo(IAnyState state) =>
        state.ToCodeInput().Language(Languages.Javascript).Height(Size.Units(24));

    internal static GridView AffixHeaderRow() =>
        Layout.Grid().Columns(4)
               | null!
               | Text.Monospaced("Prefix only")
               | Text.Monospaced("Suffix only")
               | Text.Monospaced("Both");

    internal static GridView AffixRow(string label, object prefixOnly, object suffixOnly, object both) =>
        Layout.Grid().Columns(4)
               | Text.Monospaced(label)
               | prefixOnly
               | suffixOnly
               | both;

    internal static GridView DensityHeaderRow() =>
        Layout.Grid().Columns(4)
               | null!
               | Text.Monospaced("Small")
               | Text.Monospaced("Medium")
               | Text.Monospaced("Large");

    internal static GridView DensityRow(string label, TextInputBase input) =>
        Layout.Grid().Columns(4)
               | Text.Monospaced(label)
               | input.Small()
               | input
               | input.Large();

    internal static GridView NumberDensityRow(string label, NumberInputBase input) =>
        Layout.Grid().Columns(4)
               | Text.Monospaced(label)
               | input.Small()
               | input
               | input.Large();

    internal static GridView SelectDensityRow(string label, SelectInputBase input) =>
        Layout.Grid().Columns(4)
               | Text.Monospaced(label)
               | input.Small()
               | input
               | input.Large();

    internal static GridView BoolDensityRow(string label, BoolInputBase input) =>
        Layout.Grid().Columns(4)
               | Text.Monospaced(label)
               | input.Small()
               | input
               | input.Large();

    internal static GridView ColorDensityRow(string label, ColorInputBase input) =>
        Layout.Grid().Columns(4)
               | Text.Monospaced(label)
               | input.Small()
               | input
               | input.Large();

    internal static GridView FeedbackDensityRow(string label, FeedbackInputBase input) =>
        Layout.Grid().Columns(4)
               | Text.Monospaced(label)
               | input.Small()
               | input
               | input.Large();

    internal static GridView IconDensityRow(string label, IconInputBase input) =>
        Layout.Grid().Columns(4)
               | Text.Monospaced(label)
               | input.Small()
               | input
               | input.Large();

    internal static GridView CodeDensityRow(string label, CodeInputBase input) =>
        Layout.Grid().Columns(4)
               | Text.Monospaced(label)
               | input.Small()
               | input
               | input.Large();
}
