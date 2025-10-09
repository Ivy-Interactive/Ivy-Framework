# Widgets

<Ingress>
Discover the fundamental building blocks of Ivy applications - Widgets provide declarative UI components inspired by React's component model.
</Ingress>

Widgets are the fundamental building blocks of the Ivy framework. They represent the smallest unit of UI and are used to construct Views. Inspired by React's component model, Widgets provide a declarative way to build user interfaces.

## Basic usage

Ivy provides a comprehensive set of widgets organized into several categories:

The most frequently used widgets for building user interfaces:

```csharp demo-below
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
| Common | [Button](../../02_Widgets/01_Common/Button.md), [Badge](../../02_Widgets/01_Common/Badge.md), [Progress](../../02_Widgets/01_Common/Progress.md), [Table](../../02_Widgets/01_Common/Table.md), [Card](../../02_Widgets/01_Common/Card.md), [Tooltip](../../02_Widgets/01_Common/Tooltip.md), [Expandable](../../02_Widgets/01_Common/Expandable.md), [Blades](../../02_Widgets/01_Common/Blades.md), [Details](../../02_Widgets/01_Common/Details.md), [DropDownMenu](../../02_Widgets/01_Common/DropDownMenu.md), [List](../../02_Widgets/01_Common/List.md)... |
| Inputs | [TextInput](../../02_Widgets/02_Inputs/Text.md), [NumberInput](../../02_Widgets/02_Inputs/Number.md), [BoolInput](../../02_Widgets/02_Inputs/Bool.md), [DateTimeInput](../../02_Widgets/02_Inputs/DateTime.md), [FileInput](../../02_Widgets/02_Inputs/File.md), [Feedback](../../02_Widgets/02_Inputs/Feedback.md), [DateRange](../../02_Widgets/02_Inputs/DateRange.md), [Color](../../02_Widgets/02_Inputs/Color.md), [Code](../../02_Widgets/02_Inputs/Code.md), [ReadOnly](../../02_Widgets/02_Inputs/ReadOnly.md), [AsyncSelect](../../02_Widgets/02_Inputs/AsyncSelect.md)... |
| Primitives | [Text](../../02_Widgets/03_Primitives/TextBlock.md), [Icon](../../02_Widgets/03_Primitives/Icon.md), [Image](../../02_Widgets/03_Primitives/Image.md), [Markdown](../../02_Widgets/03_Primitives/Markdown.md), [Json](../../02_Widgets/03_Primitives/Json.md), [Code](../../02_Widgets/03_Primitives/Code.md), [Avatar](../../02_Widgets/03_Primitives/Avatar.md), [Box](../../02_Widgets/03_Primitives/Box.md), [Callout](../../02_Widgets/03_Primitives/Callout.md), [Error](../../02_Widgets/03_Primitives/Error.md), [Spacer](../../02_Widgets/03_Primitives/Spacer.md), [Separator](../../02_Widgets/03_Primitives/Separator.md), [Xml](../../02_Widgets/03_Primitives/Xml.md), [Html](../../02_Widgets/03_Primitives/Html.md)... |
| Layouts | [GridLayout](../../02_Widgets/04_Layouts/GridLayout.md), [TabsLayout](../../02_Widgets/04_Layouts/TabsLayout.md), [SidebarLayout](../../02_Widgets/04_Layouts/SidebarLayout.md), [FloatingPanel](../../02_Widgets/04_Layouts/FloatingPanel.md), [ResizeablePanelGroup](../../02_Widgets/04_Layouts/ResizeablePanelGroup.md), [Header](../../02_Widgets/04_Layouts/HeaderLayout.md), [Footer](../../02_Widgets/04_Layouts/FooterLayout.md), [Wrap](../../02_Widgets/04_Layouts/WrapLayout.md)... |
| Effects | [Animation](../../02_Widgets/05_Effects/Animation.md), [Confetti](../../02_Widgets/05_Effects/Confetti.md)... |
| Charts | [LineChart](../../02_Widgets/06_Charts/LineChart.md), [BarChart](../../02_Widgets/06_Charts/BarChart.md), [PieChart](../../02_Widgets/06_Charts/PieChart.md), [AreaChart](../../02_Widgets/06_Charts/AreaChart.md)... |
| Advanced | [Sheet](../../02_Widgets/07_Advanced/Sheet.md), [Chat](../../02_Widgets/07_Advanced/Chat.md)... |

### Common Widgets

The common widgets category offers you the opportunity to work with essential UI elements including badges, blades, buttons, cards, details implementations, dropdown menus, expandable sections, lists, progress bars, tables, and tooltips. Each widget is designed with Ivy's signature approach to simplicity and functionality.

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
        var client = this.UseService<IClientProvider>();
        return Layout.Grid().Columns(4)
            | new Button("Click Me", onClick: _ => client.Toast("Hello!"))
            | new Button("Save").Icon(Icons.Save)
            | new Button("Destructive").Destructive()
            | new Button("Secondary").Secondary()
            | new Badge("Primary")
            | new Badge("Success").Icon(Icons.Check)
            | new Badge("Warning", variant: BadgeVariant.Warning)
            | new Badge("Outline").Outline()
            | new Progress(75)
            | new Progress(50).Goal("Task completion")
            | new Progress(90).ColorVariant(Progress.ColorVariants.EmeraldGradient)
            | new Progress(35).Goal("Upload...")
            | new Card("Card with content")
            | new Card("Clickable Card").HandleClick(_ => client.Toast("Clicked!"))
            | new Card("Card with Button", new Button("Action"))
            | new Card("Simple Card").Title("Title");
    }
}
```

### Input Widgets

We also provide our users with various input methods to capture user data. Users can work with simple input types such as boolean inputs, feedback forms, text inputs, number inputs, date ranges, and date-time pickers. Additionally, we offer specialized features including Ivy's color palette system and our implementation of code highlighting. We introduce our file input implementations, read-only statements, and provide the ability to work with complex structures like async select operations in a simple, intuitive way.

```mermaid
graph BT
    A[Input Methods] --> B[Boolean, Feedback, Text, Number, Date, DateRange]
    A --> C[Color Palette, Code Highlighting, File Inputs, Read-Only]
    A --> D[Async Select, Complex Structures]
```

```csharp demo-tabs
public class InputWidgetsDemo : ViewBase
{
    public override object? Build()
    {
        var textState = UseState("");
        var numberState = UseState(0);
        var boolState = UseState(false);
        var dateState = UseState(DateTime.Now);
        
        return Layout.Grid().Columns(4)
            | new TextInput(textState).Placeholder("Enter text...")
            | new TextInput(textState).Variant(TextInputs.Password).Placeholder("Password")
            | new TextInput(textState).Variant(TextInputs.Search).Placeholder("Search...")
            | new TextInput(textState).Variant(TextInputs.Email).Placeholder("Email")
            | new NumberInput<int>(numberState).Placeholder("Number")
            | new NumberInput<double>(numberState).Min(0).Max(100).Variant(NumberInputs.Slider)
            | new BoolInput(boolState).Label("Accept terms")
            | new DateTimeInput<DateTime>(dateState)
            | new TextInput(textState).Variant(TextInputs.Textarea).Placeholder("Description...")
            | new TextInput(textState).Variant(TextInputs.Tel).Placeholder("+1-123-4567")
            | new TextInput(textState).Variant(TextInputs.Url).Placeholder("https://")
            | new ReadOnlyInput<string>("Read-only value");
    }
}
```

### Primitives

Ivy also provides a special experience when working with primitive widgets. We make complex tasks simpler through our implementation of boxes, callouts, error displays, and text blocks. You can easily add avatars, icons, images, spacers, and separators to enhance your interfaces. We also provide our own implementations of JSON, XML, HTML, and code rendering capabilities.

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
        return Layout.Grid().Columns(4)
            | Text.H1("Heading 1")
            | Text.H2("Heading 2")
            | Text.H3("Heading 3")
            | Text.P("Paragraph text")
            | new Icon(Icons.Heart)
            | new Icon(Icons.Star, Colors.Yellow)
            | new Icon(Icons.Check, Colors.Green)
            | new Icon(Icons.CircleAlert, Colors.Red)
            | new Avatar("John Doe")
            | new Avatar("JD", "https://via.placeholder.com/150")
            | new Avatar("User")
            | new Avatar("AB")
            | Callout.Info("Info message")
            | Callout.Warning("Warning")
            | Callout.Success("Success")
            | Callout.Error("Error");
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
    A --> E[Special Layouts]
    
    B --> B1[Grid]
    B --> B2[Horizontal]
    B --> B3[Vertical]
    
    C --> C1[Floating Panel]
    C --> C2[Resizeable Panel Group]
    C --> C3[Sidebar]
    C --> C4[Tabs]
    
    D --> D1[Header]
    D --> D2[Footer]
    
    E --> E1[Wrap]
```

```csharp demo-tabs
public class LayoutWidgetsDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.H4("Grid Layout")
            | Layout.Grid().Columns(4)
                | new Card("1")
                | new Card("2")
                | new Card("3")
                | new Card("4")
            | Text.H4("Horizontal Layout")
            | Layout.Horizontal()
                | new Badge("Item 1")
                | new Badge("Item 2")
                | new Badge("Item 3")
                | new Badge("Item 4")
            | Text.H4("Wrap Layout")
            | Layout.Wrap()
                | new Button("Button 1")
                | new Button("Button 2")
                | new Button("Button 3")
                | new Button("Button 4")
                | new Button("Button 5");
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
        
        return Layout.Grid().Columns(2)
            | data.ToLineChart()
                .Dimension("Month", e => e.Month)
                .Measure("Desktop", e => e.Sum(f => f.Desktop))
                .Measure("Mobile", e => e.Sum(f => f.Mobile))
            | data.ToBarChart()
                .Dimension("Month", e => e.Month)
                .Measure("Desktop", e => e.Sum(f => f.Desktop))
            | data.ToAreaChart()
                .Dimension("Month", e => e.Month)
                .Measure("Desktop", e => e.Sum(f => f.Desktop))
            | data.ToPieChart(
                e => e.Month,
                e => e.Sum(f => f.Desktop));
    }
}
```

### Effects

Ivy provides a rich collection of built-in effects and animations to enhance your user interfaces. Working with effects in Ivy is incredibly simple and intuitive. For detailed information about specific effects, refer to the animation and confetti documentation pages.

```csharp demo-tabs ivy-bg
public class EffectWidgetsDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(4)
            | new Button("Click for Confetti!").WithConfetti(AnimationTrigger.Click)
            | new Card("Hover me").WithConfetti(AnimationTrigger.Hover)
            | new Badge("Celebrate!").WithConfetti(AnimationTrigger.Click)
            | Text.H3("Auto Confetti").WithConfetti(AnimationTrigger.Auto);
    }
}
```

### Advanced

In the Advanced section, we introduce our specialized implementations for working with sheets and chat functionality. These advanced widgets provide sophisticated features for complex user interface requirements.
