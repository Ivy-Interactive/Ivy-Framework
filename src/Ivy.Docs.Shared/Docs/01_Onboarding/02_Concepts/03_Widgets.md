---
searchHints:
  - components
  - ui
  - building-blocks
  - elements
  - widgets
  - primitives
---

# Widgets

<Ingress>
Discover the fundamental [building blocks](./02_Views.md) of Ivy [applications](./15_Apps.md) - Widgets provide declarative UI components inspired by React's component mode.
</Ingress>

Widgets are the fundamental building blocks of the Ivy framework. They represent the smallest unit of UI and are used to construct [Views](./02_Views.md).

## Basic usage

Ivy provides a comprehensive set of widgets organized into several [categories](../../02_Widgets/_Index.md):

The most frequently used widgets for building user interfaces:

```csharp demo-tabs
Layout.Vertical().Gap(2)
    | new Badge("Primary")
    | new Badge("New")
    | new Button("Primary Button")
    | new Progress(75)
    | new Card("Card Content")
```

### Widget Library

Ivy ships with a comprehensive set of strongly-typed widgets:

| Category | Examples |
|----------|----------|
| Common | [Button](../../02_Widgets/03_Common/01_Button.md), [Badge](../../02_Widgets/03_Common/02_Badge.md), [Tooltip](../../02_Widgets/03_Common/03_Tooltip.md), [Card](../../02_Widgets/03_Common/04_Card.md), [Details](../../02_Widgets/03_Common/05_Details.md), [Expandable](../../02_Widgets/03_Common/06_Expandable.md), [List](../../02_Widgets/03_Common/07_List.md), [Table](../../02_Widgets/03_Common/08_Table.md), [Pagination](../../02_Widgets/03_Common/09_Pagination.md), [Progress](../../02_Widgets/03_Common/10_Progress.md), [DropDownMenu](../../02_Widgets/03_Common/11_DropDownMenu.md), [Blades](../../02_Widgets/03_Common/12_Blades.md), [MetricView](../../02_Widgets/03_Common/13_MetricView.md), [Terminal](../../02_Widgets/03_Common/14_Terminal.md), [Dialog](../../02_Widgets/03_Common/15_Dialog.md), [Tree](../../02_Widgets/03_Common/16_Tree.md) |
| Inputs | [Field](../../02_Widgets/04_Inputs/01_Field.md), [TextInput](../../02_Widgets/04_Inputs/02_TextInput.md), [NumberInput](../../02_Widgets/04_Inputs/03_NumberInput.md), [BoolInput](../../02_Widgets/04_Inputs/04_BoolInput.md), [SelectInput](../../02_Widgets/04_Inputs/05_SelectInput.md), [AsyncSelectInput](../../02_Widgets/04_Inputs/06_AsyncSelectInput.md), [DateTimeInput](../../02_Widgets/04_Inputs/07_DateTimeInput.md), [DateRangeInput](../../02_Widgets/04_Inputs/08_DateRangeInput.md), [ColorInput](../../02_Widgets/04_Inputs/09_ColorInput.md), [FileInput](../../02_Widgets/04_Inputs/10_FileInput.md), [CodeInput](../../02_Widgets/04_Inputs/11_CodeInput.md), [AudioInput](../../02_Widgets/04_Inputs/12_AudioInput.md), [FeedbackInput](../../02_Widgets/04_Inputs/13_FeedbackInput.md), [ReadOnlyInput](../../02_Widgets/04_Inputs/14_ReadOnlyInput.md) |
| Primitives | [Text](../../02_Widgets/01_Primitives/01_TextBlock.md), [Icon](../../02_Widgets/01_Primitives/02_Icon.md), [Image](../../02_Widgets/01_Primitives/03_Image.md), [Box](../../02_Widgets/01_Primitives/04_Box.md), [Fragment](../../02_Widgets/01_Primitives/05_Fragment.md), [Spacer](../../02_Widgets/01_Primitives/06_Spacer.md), [Separator](../../02_Widgets/01_Primitives/07_Separator.md), [Avatar](../../02_Widgets/01_Primitives/08_Avatar.md), [Skeleton](../../02_Widgets/01_Primitives/09_Skeleton.md), [CodeBlock](../../02_Widgets/01_Primitives/10_CodeBlock.md), [Kbd](../../02_Widgets/01_Primitives/11_Kbd.md), [Callout](../../02_Widgets/01_Primitives/12_Callout.md), [Error](../../02_Widgets/01_Primitives/13_Error.md), [Markdown](../../02_Widgets/01_Primitives/14_Markdown.md), [Html](../../02_Widgets/01_Primitives/15_Html.md), [Xml](../../02_Widgets/01_Primitives/16_Xml.md), [Json](../../02_Widgets/01_Primitives/17_Json.md), [Svg](../../02_Widgets/01_Primitives/18_Svg.md), [Embed](../../02_Widgets/01_Primitives/19_Embed.md), [Iframe](../../02_Widgets/01_Primitives/20_Iframe.md), [AudioPlayer](../../02_Widgets/01_Primitives/21_AudioPlayer.md), [VideoPlayer](../../02_Widgets/01_Primitives/22_VideoPlayer.md), [Stepper](../../02_Widgets/01_Primitives/23_Stepper.md), [AutoScroll](../../02_Widgets/01_Primitives/26_AutoScroll.md) |
| Layouts | [GridLayout](../../02_Widgets/02_Layouts/03_GridLayout.md), [HeaderLayout](../../02_Widgets/02_Layouts/04_HeaderLayout.md), [FooterLayout](../../02_Widgets/02_Layouts/05_FooterLayout.md), [SidebarLayout](../../02_Widgets/02_Layouts/06_SidebarLayout.md), [ResizablePanelGroup](../../02_Widgets/02_Layouts/08_ResizablePanelGroup.md), [FloatingPanel](../../02_Widgets/02_Layouts/09_FloatingPanel.md) |
| Effects | [Animation](../../02_Widgets/05_Effects/Animation.md), [Confetti](../../02_Widgets/05_Effects/Confetti.md) |
| Charts | [LineChart](../../02_Widgets/06_Charts/01_LineChart.md), [BarChart](../../02_Widgets/06_Charts/02_BarChart.md), [AreaChart](../../02_Widgets/06_Charts/03_AreaChart.md), [PieChart](../../02_Widgets/06_Charts/04_PieChart.md) |
| Advanced | [DataTable](../../02_Widgets/07_Advanced/01_DataTable.md), [Sheet](../../02_Widgets/07_Advanced/02_Sheet.md), [Kanban](../../02_Widgets/07_Advanced/03_Kanban.md), [Chat](../../02_Widgets/07_Advanced/04_Chat.md), [ExternalWidgets](../../02_Widgets/07_Advanced/05_ExternalWidgets.md) |

### Common Widgets

The common widgets category offers you the opportunity to work with essential UI elements including [badges](../../02_Widgets/03_Common/02_Badge.md), [blades](../../02_Widgets/03_Common/12_Blades.md), [buttons](../../02_Widgets/03_Common/01_Button.md), [cards](../../02_Widgets/03_Common/04_Card.md), [details](../../02_Widgets/03_Common/05_Details.md) implementations, [dropdown menus](../../02_Widgets/03_Common/11_DropDownMenu.md), [expandable](../../02_Widgets/03_Common/06_Expandable.md) sections, [lists](../../02_Widgets/03_Common/07_List.md), [progress](../../02_Widgets/03_Common/10_Progress.md) bars, [tables](../../02_Widgets/03_Common/08_Table.md), and [tooltips](../../02_Widgets/03_Common/03_Tooltip.md). Each widget is designed with Ivy's signature approach to simplicity and functionality.

```mermaid
flowchart TB
    A[Common Widgets] --> B[Badges, Blades, Buttons, Cards]
    A --> C[Dropdown Menus, Expandables, Lists]
    A --> D[Progress Bars, Tables, Tooltips]
    A --> E[Details Implementations]
```

```csharp demo-tabs
public class CommonWidgetsDemo : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        return Layout.Grid().Columns(2).Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2)
                    | new Button("Click Me", onClick: _ => client.Toast("Hello!"))
                    | new Button("Destructive").Destructive()
                    | new Button("Secondary").Secondary()
            ).Title("Buttons").Description("Interactive button variants").Height(Size.Units(40))
            | new Card(
                Layout.Wrap().Gap(2)
                    | new Badge("Primary")
                    | new Badge("Success").Icon(Icons.Check)
                    | new Badge("Outline").Outline()
            ).Title("Badges").Description("Status and label badges").Height(Size.Units(40))
            | new Card(
                Layout.Vertical().Gap(2)
                    | new Progress(50).Goal("Task completion")
                    | new Progress(75).Color(Colors.Amber)
                    | new Progress(90)
            ).Title("Progress").Description("Task completion indicators").Height(Size.Units(50))
            | new Card(
                Layout.Vertical().Gap(2)
                    | new Card("Clickable Card").OnClick(_ => client.Toast("Clicked!"))
            ).Title("Cards").Description("Content containers").Height(Size.Units(50))
            | new Card(
                new[] {
                    new { Name = "Apple", Price = 1.20 },
                    new { Name = "Banana", Price = 0.80 },
                    new { Name = "Cherry", Price = 2.50 }
                }.ToTable()
            ).Title("Table").Description("Structured data display").Height(Size.Units(70))
            | new Card(
                Layout.Vertical().Gap(2)
                    | new Expandable("Click to expand", "Hidden content appears here")
                    | new Expandable("Another section", "More expandable content")
            ).Title("Expandable").Description("Collapsible sections").Height(Size.Units(70))
            | new Card(
                new List(new[]
                {
                    new ListItem("First item", icon: Icons.Circle),
                    new ListItem("Second item", icon: Icons.Circle),
                    new ListItem("Third item", icon: Icons.Circle)
                })
            ).Title("List").Description("Vertical item lists").Height(Size.Units(70))
            | new Card(
                new { Name = "John Doe", Email = "john@example.com", Role = "Admin" }
                    .ToDetails()
            ).Title("Details").Description("Label-value pairs").Height(Size.Units(70))
            | new Card(
                Layout.Vertical().AlignContent(Align.Center)
                    | new DropDownMenu(_ => { }, 
                        new Button("Menu"),
                        MenuItem.Default("Profile"),
                        MenuItem.Default("Settings"),
                        MenuItem.Separator(),
                        MenuItem.Default("Logout"))
            ).Title("DropDownMenu").Description("Action menus").Height(Size.Units(40))
            | new Card(
                Layout.Horizontal().AlignContent(Align.Center).Gap(2)
                    | new Button("Hover").Icon(Icons.Info).WithTooltip("This is a tooltip")
                    | new Button("Help").Icon(Icons.CircleQuestionMark).WithTooltip("Get help here")
            ).Title("Tooltip").Description("Contextual information").Height(Size.Units(40));
    }
}
```

### Input Widgets

We also provide our users with various input methods to capture user data. Users can work with simple input types such as [boolean inputs](../../02_Widgets/04_Inputs/04_BoolInput.md), [feedback](../../02_Widgets/04_Inputs/13_FeedbackInput.md) forms, [text inputs](../../02_Widgets/04_Inputs/02_TextInput.md), [number inputs](../../02_Widgets/04_Inputs/03_NumberInput.md), [date ranges](../../02_Widgets/04_Inputs/08_DateRangeInput.md), and [date-time pickers](../../02_Widgets/04_Inputs/07_DateTimeInput.md). Additionally, we offer specialized features including Ivy's [color](../../02_Widgets/04_Inputs/09_ColorInput.md) palette system and our implementation of [code](../../02_Widgets/04_Inputs/11_CodeInput.md) highlighting. We introduce our [file input](../../02_Widgets/04_Inputs/10_FileInput.md) implementations, [read-only](../../02_Widgets/04_Inputs/14_ReadOnlyInput.md) statements, and provide the ability to work with complex structures like [async select](../../02_Widgets/04_Inputs/06_AsyncSelectInput.md) operations in a simple, intuitive way.

```mermaid
graph BT
    A[Input Methods] --> B[Boolean, Feedback, Text, Number, Date, DateRange]
    A --> C[Color Palette, Code Highlighting, File Inputs, Read-Only]
    A --> D[Async Select, Complex Structures]
```

```csharp demo-tabs
public class InputWidgetsDemo : ViewBase
{
    private static readonly string[] Categories = { "Electronics", "Clothing", "Books", "Home & Garden", "Sports" };
    
    public override object? Build()
    {
        var textState = UseState("");
        var numberState = UseState(0);
        var boolState = UseState(false);
        var dateState = UseState(DateTime.Now);
        var dateRangeState = UseState<(DateOnly?, DateOnly?)>((null, null));
        var colorState = UseState("#00cc92");
        var codeState = UseState("var x = 10;");
        var fileState = UseState<FileUpload<byte[]>?>();
        var fileUpload = UseUpload(MemoryStreamUploadHandler.Create(fileState));
        var feedbackState = UseState(4);
        var selectState = UseState("");
        var asyncSelectState = UseState((string?)null);
        
        var selectedCategory = UseState<string?>(default(string?));
        var sliderState = UseState(0.0);
        var currencyState = UseState(0m);
        var percentState = UseState(0.0);
        var readOnlyState = UseState("Read-only value");

        QueryResult<Option<string>[]> QueryCategories(IViewContext context, string query)
        {
            return context.UseQuery<Option<string>[], (string, string)>(
                key: (nameof(QueryCategories), query),
                fetcher: ct => Task.FromResult(Categories
                    .Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(c => new Option<string>(c))
                    .ToArray()));
        }

        QueryResult<Option<string>?> LookupCategory(IViewContext context, string? category)
        {
            return context.UseQuery<Option<string>?, (string, string?)>(
                key: (nameof(LookupCategory), category),
                fetcher: ct => Task.FromResult(category != null ? new Option<string>(category) : null));
        }

        return Layout.Grid().Columns(2).Gap(4).Width(Size.Full())
            | new Card(
                Layout.Vertical().Gap(2)
                    | textState.ToTextInput().Placeholder("Enter text...")
                    | textState.ToTextInput().Variant(TextInputVariant.Password).Placeholder("Password")
                    | textState.ToTextInput().Variant(TextInputVariant.Email).Placeholder("Email")
                    | textState.ToTextInput().Variant(TextInputVariant.Search).Placeholder("Search...")
            ).Title("TextInput").Description("Text input variants").Height(Size.Units(80))
            | new Card(
                Layout.Vertical().Gap(2)
                    | sliderState.ToNumberInput().Min(0).Max(100).Variant(NumberInputVariant.Slider)
                    | numberState.ToNumberInput().Placeholder("Enter number")
                    | currencyState.ToNumberInput().FormatStyle(NumberFormatStyle.Currency).Currency("USD").Placeholder("$0.00")
                    | percentState.ToNumberInput().FormatStyle(NumberFormatStyle.Percent).Placeholder("0%")
            ).Title("NumberInput").Description("Number and slider").Height(Size.Units(80))
            | new Card(
                Layout.Vertical().Gap(2)
                    | boolState.ToBoolInput().WithField().Label("Accept terms and conditions")
                    | boolState.ToSwitchInput().WithField().Label("Enable notifications")
            ).Title("BoolInput").Description("Checkbox input").Height(Size.Units(65))
            | new Card(
                Layout.Vertical().Gap(2)
                    | fileState.ToFileInput(fileUpload).Placeholder("Upload file")
            ).Title("FileInput").Description("File upload").Height(Size.Units(65))
            | new Card(
                dateRangeState.ToDateRangeInput().Placeholder("Select date range")
            ).Title("DateRange").Description("Date range picker").Height(Size.Units(40))
            | new Card(
                dateState.ToDateTimeInput().Placeholder("Select date")
            ).Title("DateTimeInput").Description("Date and time picker").Height(Size.Units(40))
            | new Card(
                feedbackState.ToFeedbackInput().Stars()
            ).Title("Feedback").Description("Star rating").Height(Size.Units(40))
            | new Card(
                colorState.ToColorInput().Variant(ColorInputVariant.Picker)
            ).Title("Color").Description("Color picker").Height(Size.Units(40))
            | new Card(
                codeState.ToCodeInput().Language(Languages.Javascript).Height(Size.Units(15))
            ).Title("Code").Description("Code editor").Height(Size.Units(50))
            | new Card(
                selectState.ToSelectInput(new[] { "Option 1", "Option 2", "Option 3" }.ToOptions()).Placeholder("Select option")
            ).Title("Select").Description("Dropdown select").Height(Size.Units(50))
            | new Card(
                readOnlyState.ToReadOnlyInput()
            ).Title("ReadOnly").Description("Read-only display").Height(Size.Units(40))
            | new Card(
                selectedCategory.ToAsyncSelectInput(QueryCategories, LookupCategory, "Search categories...")
            ).Title("AsyncSelect").Description("Async dropdown").Height(Size.Units(40));
    }
}
```

### Primitives

Ivy also provides a special experience when working with [primitive](../../02_Widgets/01_Primitives/_Index.md) widgets. We make complex tasks simpler through our implementation of [boxes](../../02_Widgets/01_Primitives/04_Box.md), [callouts](../../02_Widgets/01_Primitives/12_Callout.md), [error](../../02_Widgets/01_Primitives/13_Error.md) displays, and [text blocks](../../02_Widgets/01_Primitives/01_TextBlock.md). You can easily add [avatars](../../02_Widgets/01_Primitives/08_Avatar.md), [icons](../../02_Widgets/01_Primitives/02_Icon.md), [images](../../02_Widgets/01_Primitives/03_Image.md), [spacers](../../02_Widgets/01_Primitives/06_Spacer.md), and [separators](../../02_Widgets/01_Primitives/07_Separator.md) to enhance your interfaces. We also provide our own implementations of [JSON](../../02_Widgets/01_Primitives/17_Json.md), [XML](../../02_Widgets/01_Primitives/16_Xml.md), [HTML](../../02_Widgets/01_Primitives/15_Html.md), and [code](../../02_Widgets/01_Primitives/10_CodeBlock.md) rendering capabilities.

```mermaid
flowchart LR
    A[Primitive Widgets] --> B[Boxes, Callouts, Errors, Text Blocks]
    A --> C[Avatars, Icons, Images, Spacers, Separators]
    A --> D[JSON, XML, HTML, Code Rendering]
```

```csharp demo-tabs
public class PrimitiveWidgetsDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(2).Gap(4).Width(Size.Full())
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.H3("Heading 3")
                    | Text.P("Paragraph text")
                    | Text.Label("Label text")
                    | Text.P("Large text").Large()
                    | Text.Lead("Lead text")
            ).Title("Text").Description("Text variants").Height(Size.Units(75))
            | new Card(
                Layout.Vertical().AlignContent(Align.Center)
                    | new Image("https://api.images.cat/150/150")
            ).Title("Image").Description("Image display").Height(Size.Units(75))
            | new Card(
                Layout.Horizontal().Gap(4)
                    | new Icon(Icons.Heart, Colors.Red)
                    | new Icon(Icons.Star, Colors.Yellow)
                    | new Icon(Icons.Check, Colors.Green)
                    | new Icon(Icons.Settings, Colors.Blue)
                    | new Icon(Icons.Bell, Colors.Orange)
                    | new Icon(Icons.Mail, Colors.Purple)
                    | new Icon(Icons.User, Colors.Cyan)
            ).Title("Icon").Description("Vector icons").Height(Size.Units(40))
            | new Card(
                Layout.Horizontal().Gap(2)
                    | new Avatar("John Doe")
                    | new Avatar("JD", "https://api.images.cat/150/150?1")
                    | new Avatar("AB")
                    | new Avatar("Mary Smith")
                    | new Avatar("TC", "https://api.images.cat/150/150?2")
                    | new Avatar("XY")
            ).Title("Avatar").Description("User avatars").Height(Size.Units(40))
            | new Card(
                Layout.Vertical().Gap(2)
                    | Callout.Info("Info message")
                    | Callout.Warning("Warning message")
            ).Title("Callout").Description("Alert messages").Height(Size.Units(70))
            | new Card(
                Layout.Vertical().Gap(2)
                    | new Box("Solid border").BorderStyle(BorderStyle.Solid)
                    | new Box("Full radius").BorderRadius(BorderRadius.Full)
            ).Title("Box").Description("Content container").Height(Size.Units(70))
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.P("Content above")
                    | new Separator()
                    | Text.P("Content below")
            ).Title("Separator").Description("Visual divider").Height(Size.Units(50))
            | new Card(
                Layout.Vertical()
                    | Text.P("Top content")
                    | new Spacer().Height(Size.Units(4))
                    | Text.P("Bottom content")
            ).Title("Spacer").Description("Empty space").Height(Size.Units(50))
            | new Card(
                Text.Code("var x = 10;\nconsole.log(x);", Languages.Javascript)
            ).Title("Code").Description("Syntax highlighting").Height(Size.Units(60))
            | new Card(
                new Markdown("**Bold** and *italic* text\n\n- Item 1\n- Item 2")
            ).Title("Markdown").Description("Markdown rendering").Height(Size.Units(60))
            | new Card(
                Text.Json("{ \"name\": \"value\", \"count\": 42 }")
            ).Title("Json").Description("JSON display").Height(Size.Units(50))
            | new Card(
                Text.Xml("<root><item>Value</item></root>")
            ).Title("Xml").Description("XML display").Height(Size.Units(50))
            | new Card(
                Text.Html("<div><strong>Bold</strong> text and <em>italic</em> text</div>")
            ).Title("Html").Description("HTML rendering").Height(Size.Units(40))
            | new Card(
                new Error("An error occurred")
            ).Title("Error").Description("Error display").Height(Size.Units(40));
    }
}
```

### Layouts

Ivy makes working with layouts not just easier, but satisfying. We provide a much more intuitive way to work with layouts and their elements, allowing you to create complex arrangements with minimal effort.

```mermaid
graph LR
    A[Layout Widgets] --> B[Basic Layouts]
    A --> C[Panel Layouts]
    A --> D[Section Layouts]

    B --> B1[Grid]
    B --> B2[Horizontal]
    B --> B3[Vertical]
    B --> B4[Wrap]

    C --> C1[Floating Panel]
    C --> C2[Resizable Panel Group]
    C --> C3[Sidebar]
    C --> C4[Tabs]

    D --> D1[Header]
    D --> D2[Footer]
```

```csharp demo-tabs
public class LayoutWidgetsDemo : ViewBase
{
    public override object? Build()
    {
        var showPanel = UseState(false);
        var singleColumnExamples = Layout.Vertical().Gap(4).Width(Size.Full())
            | new Card(
                Layout.Grid().Columns(2).Width(Size.Full()).Gap(2)
                    | new Box("1").Width(Size.Full())
                    | new Box("2").Width(Size.Full())
                    | new Box("3").Width(Size.Full())
                    | new Box("4").Width(Size.Full())
            ).Title("GridLayout").Description("2D grid arrangement").Height(Size.Units(50))
            | new Card(
                new HeaderLayout(
                    header: new Card("Fixed Header Area").Title("Header"),
                    content: Layout.Vertical().Gap(2)
                        | Text.P("More scrollable content")
                )
            ).Title("Header").Description("Fixed header").Height(Size.Units(60))
            | new Card(
                new FooterLayout(
                    footer: Layout.Horizontal().Gap(2)
                        | new Button("Cancel").Secondary()
                        | new Button("Save"),
                    content: Layout.Vertical().Gap(2)
                        | Text.P("Footer stays at bottom")
                )
            ).Title("Footer").Description("Fixed footer").Height(Size.Units(60));
        
        var twoColumnExamples = Layout.Grid().Columns(2).Gap(4).Width(Size.Full())
            | new Card(
                Layout.Horizontal().Gap(2)
                    | new Box("Item 1").Width(Size.Fraction(1/3f))
                    | new Box("Item 2").Width(Size.Fraction(1/3f))
                    | new Box("Item 3").Width(Size.Fraction(1/3f))
            ).Title("Horizontal").Description("Horizontal flow").Height(Size.Units(50))
            | new Card(
                Layout.Vertical().Gap(2)
                    | new Box("Item 1").Width(Size.Full())
                    | new Box("Item 2").Width(Size.Full())
            ).Title("Vertical").Description("Vertical stack").Height(Size.Units(50))
            | new Card(
                Layout.Wrap().Gap(2)
                    | new Badge("Item 1")
                    | new Badge("Item 2")
                    | new Badge("Item 3")
                    | new Badge("Item 4")
                    | new Badge("Item 5")
                    | new Badge("Item 6")
                    | new Badge("Item 7")
            ).Title("Wrap").Description("Auto-wrapping layout").Height(Size.Units(50))
            | new Card(
                Layout.Tabs(
                    new Tab("Tab 1", new Badge("Content 1")),
                    new Tab("Tab 2", new Badge("Content 2")),
                    new Tab("Tab 3", new Badge("Content 3"))
                )
            ).Title("TabsLayout").Description("Tabbed interface").Height(Size.Units(50))
            | new Card(
                Layout.Vertical().Gap(4)
                | new Card(
                    Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                        | new Button("Show Panel", onClick: _ => showPanel.Set(true))
                        | new Button("Hide Panel", onClick: _ => showPanel.Set(false))
                ).Width(Size.Full())
                | (showPanel.Value ? new FloatingPanel(
                    new Button("Floating Action")
                        .Icon(Icons.Plus)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                ) : null)
            ).Title("FloatingPanel").Description("Fixed position overlay").Height(Size.Units(60))
            | new Card(
                new ResizablePanelGroup(
                    new ResizablePanel(Size.Fraction(0.4f),
                        new Card("Left")),
                    new ResizablePanel(Size.Fraction(0.6f),
                        new Card("Right"))
                )
            ).Title("ResizablePanelGroup").Description("Resizable panels").Height(Size.Units(60));
        
        return Layout.Vertical().Gap(4)
            | twoColumnExamples
            | singleColumnExamples;
    }
}
```

### Charts

Additionally, Ivy has its own implementation of charts, which makes data visualization much simpler to work with.

```mermaid
flowchart TB
    A[Chart Widgets] --> B[Area Chart]
    A --> C[Bar Chart]
    A --> D[Line Chart]
    A --> E[Pie Chart]
```

```csharp demo-tabs
public class ChartWidgetsDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 100 },
            new { Month = "Feb", Desktop = 305, Mobile = 200 },
            new { Month = "Mar", Desktop = 237, Mobile = 300 },
            new { Month = "Apr", Desktop = 186, Mobile = 100 }
        };
        
        return Layout.Grid().Columns(2).Gap(4).Width(Size.Full())
            | new Card(
                data.ToLineChart()
                    .Dimension("Month", e => e.Month)
                    .Measure("Desktop", e => e.Sum(f => f.Desktop))
                    .Measure("Mobile", e => e.Sum(f => f.Mobile))
            ).Title("LineChart").Description("Trend visualization").Height(Size.Units(100))
            | new Card(
                data.ToBarChart()
                    .Dimension("Month", e => e.Month)
                    .Measure("Desktop", e => e.Sum(f => f.Desktop))
            ).Title("BarChart").Description("Bar comparison").Height(Size.Units(100))
            | new Card(
                data.ToAreaChart()
                    .Dimension("Month", e => e.Month)
                    .Measure("Desktop", e => e.Sum(f => f.Desktop))
            ).Title("AreaChart").Description("Area under curve").Height(Size.Units(120))
            | new Card(
                data.ToPieChart(
                    e => e.Month,
                    e => e.Sum(f => f.Desktop))
            ).Title("PieChart").Description("Part-to-whole").Height(Size.Units(120));
    }
}
```

### Effects

Ivy provides a rich collection of built-in effects and animations to enhance your user interfaces. Working with effects in Ivy is incredibly simple and intuitive. For detailed information about specific effects, refer to the [animation](../../03_Hooks/02_Core/04_UseEffect.md) and [confetti](../../03_Hooks/02_Core/04_UseEffect.md) documentation pages.

```csharp demo-tabs ivy-bg
public class EffectWidgetsDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(2).Gap(4).Width(Size.Full())
            | new Card(
                Layout.Horizontal().AlignContent(Align.Center).Gap(2)
                    | new Button("Click Confetti").WithConfetti(AnimationTrigger.Click)
                    | new Button("Hover Confetti").WithConfetti(AnimationTrigger.Hover)
            ).Title("Confetti").Description("Celebration effects").Height(Size.Units(40))
            | new Card(
                Layout.Horizontal().Gap(4)
                    | Icons.Heart.ToIcon().Color(Colors.Red).WithAnimation(AnimationType.Pulse).Trigger(AnimationTrigger.Click)
                    | Icons.Bell.ToIcon().Color(Colors.Orange).WithAnimation(AnimationType.Shake).Trigger(AnimationTrigger.Click)
                    | Icons.Star.ToIcon().Color(Colors.Yellow).WithAnimation(AnimationType.Bounce).Trigger(AnimationTrigger.Click)
                    | Icons.LoaderCircle.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Rotate).Trigger(AnimationTrigger.Click)
                    | Icons.Zap.ToIcon().Color(Colors.Purple).WithAnimation(AnimationType.Pulse).Trigger(AnimationTrigger.Click)
                    | Icons.TrendingUp.ToIcon().Color(Colors.Green).WithAnimation(AnimationType.Bounce).Trigger(AnimationTrigger.Click)
                    | Icons.Sparkles.ToIcon().Color(Colors.Pink).WithAnimation(AnimationType.Rotate).Trigger(AnimationTrigger.Click)
                    | Icons.CircleAlert.ToIcon().Color(Colors.Destructive).WithAnimation(AnimationType.Shake).Trigger(AnimationTrigger.Click)
            ).Title("Animation").Description("Click icons to animate").Height(Size.Units(40));
    }
}
```

### Advanced

In the Advanced section, we introduce our specialized implementations for working with [sheets](../../02_Widgets/07_Advanced/02_Sheet.md) and [chat functionality](../../02_Widgets/07_Advanced/04_Chat.md). These advanced widgets provide sophisticated features for complex user interface requirements.

```csharp demo-tabs
public class AdvancedWidgetsDemo : ViewBase
{
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "Hello! I'm a demo chat bot.")
        ));
        
        void OnSend(Event<Chat, string> @event)
        {
            var currentMessages = messages.Value;
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.User, @event.Value)));
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.Assistant, $"You said: {@event.Value}")));
        }
        
        return Layout.Vertical().Gap(4).Width(Size.Full())
            | new Card(
                new Button("Open Sheet").WithSheet(
                    () => Layout.Vertical()
                        | Text.H3("Sheet Content")
                        | Text.P("This is content inside a sheet")
                        | new Button("Close"),
                    title: "Demo Sheet",
                    description: "Sheet demonstration"
                )
            ).Title("Sheet").Description("Side panel overlay").Height(Size.Units(40))
            | new Card(
                new Chat(messages.Value.ToArray(), OnSend)
                    .Height(Size.Units(30))
            ).Title("Chat").Description("Conversation interface").Height(Size.Units(70));
    }
}
```
