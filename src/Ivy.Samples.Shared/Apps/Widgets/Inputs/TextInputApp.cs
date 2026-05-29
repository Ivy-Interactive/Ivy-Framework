namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.TextCursorInput, group: ["Widgets", "Inputs"], searchHints: ["password", "textarea", "search", "email"])]
public class TextInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Text Input")
               | Layout.Tabs(
                   new Tab("Variants", new TextInputVariants()),
                   new Tab("Sizes", new TextInputSizes()),
                   new Tab("Affixes", new TextInputAffixes()),
                   new Tab("All Input Affixes", new InputAffixesGallery()),
                   new Tab("Data Binding", new TextInputDataBinding()),
                   new Tab("Length Constraints", new TextInputLengthConstraints()),
                   new Tab("Events", new TextInputEventsTab())
               ).Variant(TabsVariant.Content);
    }
}

public class TextInputVariants : ViewBase
{
    public override object Build()
    {
        var withoutValue = UseState((string?)null);
        var withValue = UseState("Hello");

        return Layout.Grid().Columns(5)
               | null!
               | Text.Monospaced("Empty")
               | Text.Monospaced("With Value")
               | Text.Monospaced("Disabled")
               | Text.Monospaced("Invalid")

               | Text.Monospaced("TextInputVariant.Text")
               | withoutValue.ToTextInput().Placeholder("Placeholder")
               | withValue.ToTextInput()
               | withValue.ToTextInput().Disabled()
               | withValue.ToTextInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

               | Text.Monospaced("TextInputVariant.Password")
               | withoutValue.ToPasswordInput().Placeholder("Placeholder").ShortcutKey("Ctrl+L")
               | withValue.ToPasswordInput()
               | withValue.ToPasswordInput().Disabled()
               | withValue.ToPasswordInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

               | Text.Monospaced("TextInputVariant.Textarea")
               | withoutValue.ToTextareaInput().Placeholder("Placeholder").ShortcutKey("Ctrl+T")
               | withValue.ToTextareaInput()
               | withValue.ToTextareaInput().Disabled()
               | withValue.ToTextareaInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

               | Text.Monospaced("TextInputVariant.Search")
               | withoutValue.ToSearchInput().Placeholder("Placeholder").ShortcutKey("Ctrl+K")
               | withValue.ToSearchInput()
               | withValue.ToSearchInput().Disabled()
               | withValue.ToSearchInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

               | Text.Monospaced("TextInputVariant.Email")
               | withoutValue.ToEmailInput().Placeholder("Placeholder").ShortcutKey("Ctrl+E")
               | withValue.ToEmailInput()
               | withValue.ToEmailInput().Disabled()
               | withValue.ToEmailInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

               | Text.Monospaced("TextInputVariant.Tel")
               | withoutValue.ToTelInput().Placeholder("Placeholder").ShortcutKey("Ctrl+J")
               | withValue.ToTelInput()
               | withValue.ToTelInput().Disabled()
               | withValue.ToTelInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

               | Text.Monospaced("TextInputVariant.Url")
               | withoutValue.ToUrlInput().Placeholder("Placeholder").ShortcutKey("Ctrl+U")
               | withValue.ToUrlInput()
               | withValue.ToUrlInput().Disabled()
               | withValue.ToUrlInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros");
    }
}

public class TextInputDataBinding : ViewBase
{
    public override object Build()
    {
        var stringState = UseState("");
        var nullStringState = UseState<string?>();

        return Layout.Grid().Columns(3)

               | Text.Monospaced("string")
               | (Layout.Vertical()
                  | stringState.ToTextInput()
                  | stringState.ToTextareaInput()
                  | stringState.ToPasswordInput()
                  | stringState.ToSearchInput()
               )
               | stringState

               | Text.Monospaced("string?")
               | (Layout.Vertical()
                  | nullStringState.ToTextInput()
                  | nullStringState.ToTextareaInput()
                  | nullStringState.ToPasswordInput()
                  | nullStringState.ToSearchInput()
               )
               | nullStringState;
    }
}

public class TextInputEventsTab : ViewBase
{
    public override object Build()
    {
        var onChangedState = UseState("");
        var onChangeLabel = UseState("");
        UseEffect(() => { onChangeLabel.Set(string.IsNullOrEmpty(onChangedState.Value) ? "" : "Changed"); }, onChangedState);
        var onBlurState = UseState("");
        var onBlurLabel = UseState("");
        var onFocusState = UseState("");
        var onFocusLabel = UseState("");
        var searchQuery = UseState("");
        var searchResult = UseState("");
        var tag = UseState("");
        var tags = UseState<List<string>>(new List<string>());
        var password = UseState("");
        var loginResult = UseState("");

        return Layout.Vertical()
               | Text.H3("OnChange")
               | Layout.Horizontal(
                   onChangedState.ToTextInput(),
                   onChangeLabel
                )
               | (Layout.Vertical()
                   | new Card(
                       Layout.Vertical().Gap(2)
                           | Text.P("The blur event fires when the text input loses focus.").Small()
                           | onBlurState.ToTextInput().OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
                           | (onBlurLabel.Value != ""
                               ? Callout.Success(onBlurLabel.Value)
                               : Callout.Info("Interact then click away to see blur events"))
                   ).Title("OnBlur Handler")
                   | new Card(
                       Layout.Vertical().Gap(2)
                           | Text.P("The focus event fires when you click on or tab into the text input.").Small()
                           | onFocusState.ToTextInput().OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
                           | (onFocusLabel.Value != ""
                               ? Callout.Success(onFocusLabel.Value)
                               : Callout.Info("Click or tab into the input to see focus events"))
                   ).Title("OnFocus Handler")
               )
               | Text.H3("OnSubmit (press Enter)")
               | Text.P("Search example (type and press Enter):")
               | Layout.Horizontal(
                   searchQuery.ToSearchInput()
                       .Placeholder("Search...")
                       .OnSubmit(() => searchResult.Set($"Searched for: {searchQuery.Value}")),
                   searchResult
               )
               | Text.P("Quick-add tags (type and press Enter to add):")
               | Layout.Horizontal(
                   tag.ToTextInput()
                       .Placeholder("Add a tag...")
                       .OnSubmit(() =>
                       {
                           if (!string.IsNullOrWhiteSpace(tag.Value))
                           {
                               tags.Set(new List<string>(tags.Value) { tag.Value });
                               tag.Set("");
                           }
                       }),
                   Layout.Horizontal().Gap(2) | tags.Value.Select(t => new Badge(t))
               )
               | Text.P("Password submit (type and press Enter to login):")
               | Layout.Horizontal(
                   password.ToPasswordInput()
                       .Placeholder("Enter password...")
                       .ShortcutKey("Ctrl+Enter")
                       .OnSubmit(() => loginResult.Set(
                           string.IsNullOrWhiteSpace(password.Value)
                               ? "Password cannot be empty"
                               : "Login submitted")),
                   loginResult
               );
    }
}

public class TextInputLengthConstraints : ViewBase
{
    public override object Build()
    {
        var minLengthState = UseState("");
        var maxLengthState = UseState("");
        var bothLengthState = UseState("");

        return Layout.Grid().Columns(3)
               | Text.Monospaced("MinLength(3)")
               | Text.Monospaced("MaxLength(10)")
               | Text.Monospaced("MinLength(5) + MaxLength(10)")
               | minLengthState.ToTextInput().Placeholder("At least 3 characters").MinLength(3)
               | maxLengthState.ToTextInput().Placeholder("Up to 10 characters").MaxLength(10)
               | bothLengthState.ToTextInput().Placeholder("Between 5 and 10 characters").MinLength(5).MaxLength(10);
    }
}

public class TextInputSizes : ViewBase
{
    public override object Build()
    {
        var textState = UseState("Hello");
        var passwordState = UseState("Hello");
        var textareaState = UseState("Hello");
        var searchState = UseState("Hello");

        return Layout.Grid().Columns(4)
               | Text.Monospaced("Description")
               | Text.Monospaced("Small")
               | Text.Monospaced("Medium")
               | Text.Monospaced("Large")

               | Text.Monospaced("TextInputVariant.Text")
               | textState.ToTextInput().Small()
               | textState.ToTextInput()
               | textState.ToTextInput().Large()

               | Text.Monospaced("TextInputVariant.Password")
               | passwordState.ToPasswordInput().Small()
               | passwordState.ToPasswordInput()
               | passwordState.ToPasswordInput().Large()

               | Text.Monospaced("TextInputVariant.Textarea")
               | textareaState.ToTextareaInput().Small()
               | textareaState.ToTextareaInput()
               | textareaState.ToTextareaInput().Large()

               | Text.Monospaced("TextInputVariant.Search")
               | searchState.ToSearchInput().Small()
               | searchState.ToSearchInput()
               | searchState.ToSearchInput().Large();
    }
}

public class TextInputAffixes : ViewBase
{
    public override object Build()
    {
        var textState = UseState("example");
        var nullableState = UseState<string?>((string?)null);

        return Layout.Grid().Columns(4)
               | null!
               | Text.Monospaced("Prefix only")
               | Text.Monospaced("Suffix only")
               | Text.Monospaced("Both")

               | Text.Monospaced("Text prefix/suffix")
               | textState.ToTextInput().Prefix("https://")
               | textState.ToTextInput().Suffix(".com")
               | textState.ToTextInput().Prefix("https://").Suffix(".com")

               | Text.Monospaced("Icon prefix/suffix")
               | textState.ToTextInput().Prefix(Icons.Mail)
               | textState.ToTextInput().Suffix(Icons.Mail)
               | textState.ToTextInput().Prefix(Icons.Mail).Suffix(Icons.Mail)

               | Text.Monospaced("Button prefix/suffix")
               | textState.ToTextInput().Prefix(new Button("Copy", () => { }, icon: Icons.Copy).Ghost().Small())
               | textState.ToTextInput().Suffix(new Button("Clear", () => { textState.Value = ""; }).Ghost().Small())
               | textState.ToTextInput().Prefix(new Button("Copy", () => { }, icon: Icons.Copy).Ghost().Small()).Suffix(new Button("Send").Ghost().Small())

               | Text.Monospaced("Badge prefix/suffix")
               | textState.ToTextInput().Prefix(new Badge("NEW", BadgeVariant.Success))
               | textState.ToTextInput().Suffix(new Badge($"{textState.Value.Length} chars", BadgeVariant.Secondary))
               | textState.ToTextInput().Prefix(new Badge("v2", BadgeVariant.Info)).Suffix(new Badge("OK", BadgeVariant.Success))

               | Text.Monospaced("Nullable with prefix/suffix")
               | nullableState.ToTextInput().Prefix("$").Placeholder("Amount")
               | nullableState.ToTextInput().Suffix("%").Placeholder("Percentage")
               | nullableState.ToTextInput().Prefix("https://").Suffix(".com").Placeholder("domain");
    }
}

public class InputAffixesGallery : ViewBase
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
                   "Compare prefix, suffix, and both affixes across inputs with transparent affix chrome. Each row is its own four-column grid so columns stay aligned. Shortcut-key rows use empty nullable fields so the kbd hint is visible; with a suffix affix it sits between the field and the suffix.")
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
               | AffixRow("Bool", boolState.ToBoolInput().Label("Enable").Prefix(Icons.Bell), boolState.ToBoolInput().Label("Enable").Suffix(Icons.Info), boolState.ToBoolInput().Label("Enable").Prefix(Icons.Bell).Suffix(Icons.Info))
               | AffixRow("Color", colorState.ToColorInput().Prefix(Icons.Palette), colorState.ToColorInput().Suffix(Icons.Pipette), colorState.ToColorInput().Prefix(Icons.Palette).Suffix(Icons.Pipette))
               | AffixRow("Feedback", feedbackState.ToFeedbackInput().Stars().Prefix(Icons.Star), feedbackState.ToFeedbackInput().Stars().Suffix(Icons.MessageSquare), feedbackState.ToFeedbackInput().Stars().Prefix(Icons.Star).Suffix(Icons.MessageSquare))
               | AffixRow("Icon", iconState.ToIconInput().Prefix(Icons.Search), iconState.ToIconInput().Suffix(Icons.Sparkles), iconState.ToIconInput().Prefix(Icons.Search).Suffix(Icons.Sparkles))
               | AffixRow("Code", codeState.ToCodeInput().Prefix(Icons.Code), codeState.ToCodeInput().Suffix(Icons.Braces), codeState.ToCodeInput().Prefix(Icons.Code).Suffix(Icons.Braces));
    }

    private enum Currency
    {
        USD,
        EUR,
        GBP,
    }

    private static GridView AffixHeaderRow() =>
        Layout.Grid().Columns(4)
               | null!
               | Text.Monospaced("Prefix only")
               | Text.Monospaced("Suffix only")
               | Text.Monospaced("Both");

    private static GridView AffixRow(string label, object prefixOnly, object suffixOnly, object both) =>
        Layout.Grid().Columns(4)
               | Text.Monospaced(label)
               | prefixOnly
               | suffixOnly
               | both;
}
