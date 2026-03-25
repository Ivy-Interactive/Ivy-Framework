---
searchHints:
  - dropdown
  - picker
  - options
  - choice
  - select
  - menu
---

# SelectInput

<Ingress>
Create dropdown [menus](../../01_Onboarding/02_Concepts/09_Navigation.md) with single or multiple selection capabilities, option grouping, and custom rendering for user choices.
</Ingress>

The `SelectInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides a dropdown menu for selecting items from a predefined list of options. It supports single
and multiple selections, option grouping, and custom rendering of option items.

## Basic Usage

Here's a simple example of a `SelectInput` with a few options. Use [Size](../../04_ApiReference/Ivy/Size.md) for `.Width(Size.Full())` to make the select fill available space:

```csharp demo-below
public class SelectVariantDemo : ViewBase
{
    public override object? Build()
    {
        var favLang = UseState("C#");
        return favLang.ToSelectInput(["C#", "Java", "Go", "JavaScript", "F#", "Kotlin", "VB.NET", "Rust"])
                         .Variant(SelectInputVariant.Select)
                         .WithField()
                         .Label("Select your favourite programming language")
                         .Width(Size.Full());
    }    
}
```

## Radio Buttons

The `Radio` variant renders traditional radio buttons for single-select scenarios. Radio buttons are ideal for small sets of mutually exclusive choices where all options should be visible:

```csharp demo-below
public class RadioVariantDemo : ViewBase
{
    public override object? Build()
    {
        var frequency = UseState("Daily");
        return frequency.ToSelectInput(["Immediately", "Daily", "Weekly", "Never"])
                         .Radio()
                         .WithField()
                         .Label("Notification frequency")
                         .Width(Size.Full());
    }
}
```

## Multiple Selection

Multiple selection is automatically enabled when you use a collection type (array, List, etc.) as your state. The framework automatically detects this and enables multi-select functionality.

`SelectInput` supports five variants: **Select** (dropdown), **List** (checkboxes), **Toggle** (button toggles), **Slider** (range slider for ordered options), and **Radio** (radio buttons for single-select). Multi-select works with all variants except Slider and Radio. Here's an example demonstrating different combinations:

```csharp demo-tabs
public class MultiSelectDemo : ViewBase
{
    private enum ProgrammingLanguages
    {
        CSharp,
        Java,
        Python,
        JavaScript,
        Go,
        Rust
    }
    
    public override object? Build()
    {
        var languagesSelect = UseState<ProgrammingLanguages[]>([]);
        var stringArray = UseState<string[]>([]);
        var intArray = UseState<int[]>([]);
        
        var languageOptions = typeof(ProgrammingLanguages).ToOptions();
        var stringOptions = new[] { "Option A", "Option B", "Option C", "Option D" };
        var intOptions = new[] { 1, 2, 3, 4, 5 }.ToOptions();
        
        return Layout.Vertical()
            | Text.Monospaced("Select Variant (Enum)")
            | languagesSelect.ToSelectInput(languageOptions)
                .Variant(SelectInputVariant.Select)
                .Placeholder("Choose languages...")
            
            | Text.Monospaced("List Variant (String Array)")
            | stringArray.ToSelectInput(stringOptions.ToOptions())
                .Variant(SelectInputVariant.List)
            
            | Text.Monospaced("Toggle Variant (Integer Array)")
            | intArray.ToSelectInput(intOptions)
                .Variant(SelectInputVariant.Toggle);
    }
}
```

## Event Handling

Handle change events and create dynamic option lists that respond to user selections:

```csharp demo-tabs
public class EventHandlingDemo : ViewBase
{
    private static readonly Dictionary<string, string[]> CategoryOptions = new()
    {
        ["Programming"] = new[]{"C#", "Java", "Python", "JavaScript"},
        ["Design"] = new[]{"Photoshop", "Figma", "Sketch"},
        ["Database"] = new[]{"SQL Server", "PostgreSQL", "MongoDB"}
    };
    
    public override object? Build()
    {
        var selectedCategory = UseState("Programming");
        var selectedSkill = UseState("");
        
        var categoryOptions = CategoryOptions.Keys.ToOptions();
        var skillOptions = CategoryOptions[selectedCategory.Value].ToOptions();
        
        UseEffect(() => {
            selectedSkill.Set("");
        }, selectedCategory);
        
        return Layout.Vertical()
            | Layout.Grid().Columns(2)
                | selectedCategory.ToSelectInput(categoryOptions)
                    .Placeholder("Choose a category...")
                    .WithField()
                    .Label("Category:")
                
                | selectedSkill.ToSelectInput(skillOptions)
                    .Placeholder("Select a skill...")
                    .WithField()
                    .Label("Skill:")
            
            | (!string.IsNullOrEmpty(selectedSkill.Value) 
                ? Text.Block($"Selected: {selectedCategory.Value} → {selectedSkill.Value}") 
                : null);
    }
}
```

## Styling and States

Customize the `SelectInput` with various styling options:

```csharp demo-tabs
public class SelectStylingDemo : ViewBase
{
    public override object? Build()
    {
        var normalSelect = UseState("");
        var invalidSelect = UseState("");
        var ghostSelect = UseState("");
        var loadingSelect = UseState("");
        var disabledSelect = UseState("");

        var options = new[] { "Option 1", "Option 2", "Option 3" };

        var isLoading = UseState(true);

        return Layout.Vertical()
            | normalSelect.ToSelectInput(options)
                .Placeholder("Choose an option...")
                .WithField()
                .Label("Normal SelectInput:")

            | invalidSelect.ToSelectInput(options)
                .Placeholder("This has an error...")
                .Invalid("This field is required")
                .WithField()
                .Label("Invalid SelectInput:")

            | ghostSelect.ToSelectInput(options)
                .Placeholder("This is ghost...")
                .Ghost()
                .WithField()
                .Label("Ghost SelectInput:")

            | loadingSelect.ToSelectInput(options)
                .Placeholder("This is loading...")
                .Loading(isLoading.Value)
                .WithField()
                .Label("Loading SelectInput")

            | disabledSelect.ToSelectInput(options)
                .Placeholder("This is disabled...")
                .Disabled(true)
                .WithField()
                .Label("Disabled SelectInput:");
    }
}
```

## Slider Variant

The **Slider** variant is ideal for selecting from an ordered list of discrete options, such as T-shirt sizes, quality levels, or priority levels. It renders a range slider that snaps to each option.

```csharp demo-below
public class SliderVariantDemo : ViewBase
{
    private enum Priority { Low, Medium, High, Critical }

    public override object? Build()
    {
        var size = UseState("M");
        var priority = UseState(Priority.Medium);

        return Layout.Vertical()
            | size.ToSelectInput(new[] { "XS", "S", "M", "L", "XL", "XXL" }.ToOptions())
                .Slider()
                .WithField()
                .Label("T-Shirt Size")
                .Width(Size.Full())
            | priority.ToSelectInput()
                .Slider()
                .WithField()
                .Label("Priority")
                .Width(Size.Full());
    }
}
```

<Callout Type="info">
The Slider variant only supports single-select. If used with a collection state type, it will fall back to single-select behavior with a console warning.
</Callout>

## Advanced Features

SelectInput supports search, selection limits, and loading state. These work across all variants (Select, List, Toggle).

### Search Support

Enable search with `.Searchable(true)`, set the matching mode with `.SearchMode()`, and customize the empty state with `.EmptyMessage()`:

```csharp demo-below
public class SelectSearchDemo : ViewBase
{
    public override object? Build()
    {
        var selected = UseState("");
        var options = new[] { "C#", "Java", "Python", "JavaScript", "Go", "Rust", "F#", "Kotlin", "TypeScript" };
        return selected.ToSelectInput(options.ToOptions())
            .Searchable(true)
            .SearchMode(SearchMode.Fuzzy)
            .EmptyMessage("No items found")
            .Placeholder("Search languages...")
            .WithField()
            .Label("Language")
            .Width(Size.Full());;
    }
}
```

`SearchMode` can be `SearchMode.Fuzzy`, `SearchMode.CaseInsensitive`, or `SearchMode.CaseSensitive`.

### Selection Limits

For multi-select variants, use `.MinSelections()` and `.MaxSelections()` to enforce how many options can be selected:

```csharp demo-below
public class SelectionLimitsDemo : ViewBase
{
    public override object? Build()
    {
        var colors = UseState<string[]>([]);
        var options = new[] { "Red", "Green", "Blue", "Yellow", "Purple" }.ToOptions();
        return colors.ToSelectInput(options)
            .Variant(SelectInputVariant.Toggle)
            .MinSelections(1)
            .MaxSelections(3)
            .Placeholder("Pick 1 to 3 colors")
            .WithField()
            .Label("Colors")
            .Width(Size.Full());;
    }
}
```

### Option Tooltips

Add hover tooltips to individual options using the `tooltip` parameter on `Option<T>`. Tooltips provide contextual help for technical terms, abbreviations, or disabled options:

```csharp demo-tabs
public class OptionTooltipsDemo : ViewBase
{
    public override object? Build()
    {
        var strategy = UseState("lru");
        var strategies = UseState<string[]>([]);

        var options = new IAnyOption[]
        {
            new Option<string>("LRU", "lru", tooltip: "Least Recently Used — evicts the oldest accessed entry first"),
            new Option<string>("LFU", "lfu", tooltip: "Least Frequently Used — evicts the least accessed entry first"),
            new Option<string>("FIFO", "fifo", tooltip: "First In, First Out — evicts entries in insertion order"),
        };

        return Layout.Vertical()
            | Text.Monospaced("Select Variant")
            | strategy.ToSelectInput(options)
                .Placeholder("Select a cache strategy...")

            | Text.Monospaced("Toggle Variant")
            | strategies.ToSelectInput(options)
                .Variant(SelectInputVariant.Toggle)

            | Text.Monospaced("List Variant")
            | strategies.ToSelectInput(options)
                .Variant(SelectInputVariant.List);
    }
}
```

### Disabled Options

Individual options can be disabled using the fluent `.Disabled()` method on `Option<T>`. Disabled options appear greyed out and cannot be selected, but remain visible in the list:

```csharp demo-tabs
public class DisabledOptionsDemo : ViewBase
{
    public override object? Build()
    {
        var fruit = UseState("apple");
        var colors = UseState<string[]>([]);

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
        };

        return Layout.Vertical()
            | Text.Monospaced("Select Variant")
            | fruit.ToSelectInput(fruitOptions)
                .Placeholder("Select a fruit...")

            | Text.Monospaced("Toggle Variant")
            | colors.ToSelectInput(colorOptions)
                .Variant(SelectInputVariant.Toggle)

            | Text.Monospaced("List Variant")
            | colors.ToSelectInput(colorOptions)
                .Variant(SelectInputVariant.List);
    }
}
```

<Callout Type="tip">
Use Select for single choice dropdowns, List for multiple selection with checkboxes, Toggle for visual button-based selection, and Radio for single-select radio buttons in forms. The Radio variant is particularly useful for settings and configuration UIs where all options should be visible.
</Callout>

<WidgetDocs Type="Ivy.SelectInput" ExtensionTypes="Ivy.SelectInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/SelectInput.cs"/>

## Examples

<Details>
<Summary>
Ordering System
</Summary>
<Body>
A comprehensive example showing different SelectInput [variants](../../01_Onboarding/02_Concepts/17_Theming.md) in a real-world scenario:

```csharp demo-tabs
public class CoffeeShopDemo: ViewBase
{
    private static readonly Dictionary<string, List<string>> CoffeeAccompaniments = new()
    {
        ["Cappuccino"] = new List<string> 
        { 
            "Cinnamon powder", "Cocoa powder", "Sugar cubes", "Biscotti", 
            "Cantuccini", "Amaretti", "Whipped cream" 
        },
        ["Espresso"] = new List<string> 
        { 
            "Lemon peel", "Sugar cubes", "Water", "Chocolate square", 
            "Praline", "Biscotti" 
        },
        ["Latte"] = new List<string> 
        { 
            "Vanilla syrup", "Caramel syrup", "Hazelnut syrup", "Cocoa powder", 
            "Cinnamon", "Croissant", "Muffin", "Steamed milk art" 
        },
        ["Mocha"] = new List<string> 
        { 
            "Whipped cream", "Chocolate shavings", "Cocoa powder", "Marshmallows", 
            "Cinnamon stick", "Caramel drizzle", "Vanilla syrup" 
        }
    };
    
    string[] coffeeSizes = new string[]{"Short", "Tall", "Grande", "Venti"};
    
    public override object? Build()
    {
        var coffee = UseState("Cappuccino");
        var coffeeSize = UseState("Tall");
        var selectedCondiments = UseState(new string[]{});
        var previousCoffee = UseState("Cappuccino");
        
        if (previousCoffee.Value != coffee.Value)
        {
            selectedCondiments.Set(new string[]{});
            previousCoffee.Set(coffee.Value);
        }
        
        var coffeeSizeMenu = coffeeSize.ToSelectInput(coffeeSizes)
                                       .Variant(SelectInputVariant.List);
        var availableCondiments = CoffeeAccompaniments[coffee.Value];
        
        var condimentMenu = selectedCondiments.ToSelectInput(availableCondiments.ToOptions())
            .Variant(SelectInputVariant.Toggle);
        
        var orderSummary = BuildOrderSummary(coffee.Value, coffeeSize.Value, selectedCondiments.Value);
        
        return Layout.Vertical()
                | Layout.Grid().Columns(2)
                    | coffee.ToSelectInput(CoffeeAccompaniments.Keys.ToOptions())
                            .WithField()
                            .Label("Coffee Type:")
                    
                    | coffeeSizeMenu
                        .WithField()
                        .Label("Size:")
                    
                    | condimentMenu
                        .WithField()
                        .Label("Condiments:")
                    
                | new Icon(Icons.Coffee) 
                | Text.Block(orderSummary);
    }
    
    private string BuildOrderSummary(string coffee, string size, string[] condiments)
    {
        var summary = $"{size} {coffee}";
        
        if (condiments.Length > 0)
        {
            if(condiments.Length == 1)
            {
                summary += $" with {condiments[0]}";
            }
            else
            {                  
                 summary += " with " + condiments
                                                 .Take(condiments.Length - 1)
                                                 .Aggregate((a,b) =>  a + ", " + b)
                                                 + " and " + condiments[condiments.Length - 1];
            }
        }
        
        return summary;
    }
}
```

</Body>
</Details>
