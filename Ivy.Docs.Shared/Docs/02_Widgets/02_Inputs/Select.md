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
Create dropdown menus with single or multiple selection capabilities, option grouping, and custom rendering for user choices.
</Ingress>

The `SelectInput` widget provides a dropdown menu for selecting items from a predefined list of options. It supports single
and multiple selections, option grouping, and custom rendering of option items.

## Basic Usage

Here's a simple example of a `SelectInput` with a few options:

```csharp demo-below
public class SelectVariantDemo : ViewBase
{
    public override object? Build()
    {
        var langs = new string[]{"C#","Java","Go","JavaScript","F#","Kotlin","VB.NET","Rust"};
        
        var favLang = UseState("C#");
        return favLang.ToSelectInput(langs.ToOptions())
                         .Variant(SelectInputs.Select)
                         .WithField()
                         .Label("Select your favourite programming language")
                         .Width(Size.Full());
    }    
}
```

## Multiple Selection

Multiple selection is automatically enabled when you use a collection type (array, List, etc.) as your state. The framework automatically detects this and enables multi-select functionality.

`SelectInput` supports three variants: **Select** (dropdown), **List** (checkboxes), and **Toggle** (button toggles). Multi-select works with all variants and data types. Here's an example demonstrating different combinations:

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
        var stringOptions = new[]{"Option A", "Option B", "Option C", "Option D"}.ToOptions();
        var intOptions = new[]{1, 2, 3, 4, 5}.ToOptions();
        
        return Layout.Vertical()
            | Text.InlineCode("Select Variant (Enum)")
            | languagesSelect.ToSelectInput(languageOptions)
                .Variant(SelectInputs.Select)
                .Placeholder("Choose languages...")
            
            | Text.InlineCode("List Variant (String Array)")
            | stringArray.ToSelectInput(stringOptions)
                .Variant(SelectInputs.List)
            
            | Text.InlineCode("Toggle Variant (Integer Array)")
            | intArray.ToSelectInput(intOptions)
                .Variant(SelectInputs.Toggle);
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
        var showInfo = UseState(false);
        
        var categoryOptions = CategoryOptions.Keys.ToOptions();
        var skillOptions = CategoryOptions[selectedCategory.Value].ToOptions();
        
        return Layout.Vertical()
            | Layout.Grid().Columns(2)
                | selectedCategory.ToSelectInput(categoryOptions)
                    .Placeholder("Choose a category...")
                    .WithField()
                    .Label("Category:")
                
                | new SelectInput<string>(
                    value: selectedSkill.Value,
                    onChange: e =>
                    {
                        selectedSkill.Set(e.Value);
                        showInfo.Set(!string.IsNullOrEmpty(e.Value));
                    },
                    skillOptions)
                    .Placeholder("Select a skill...")
                    .WithField()
                    .Label("Skill:")
            
            | (showInfo.Value 
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
        var disabledSelect = UseState("");
        
        var options = new[]{"Option 1", "Option 2", "Option 3"}.ToOptions();
        
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
            
            | disabledSelect.ToSelectInput(options)
                .Placeholder("This is disabled...")
                .Disabled(true)
                .WithField()
                .Label("Disabled SelectInput:");
    }
}
```

<Callout Type="tip">
Use Select for single choice dropdowns, List for multiple selection with checkboxes, and Toggle for visual button-based selection. The List variant is particularly useful for forms where users need to select multiple options.
</Callout>

<WidgetDocs Type="Ivy.SelectInput" ExtensionTypes="Ivy.SelectInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/SelectInput.cs"/>

## Examples

<Details>
<Summary>
Ordering System
</Summary>
<Body>
A comprehensive example showing different SelectInput variants in a real-world scenario:

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
        
        var coffeeSizeMenu = coffeeSize.ToSelectInput(coffeeSizes.ToOptions())
                                       .Variant(SelectInputs.List);
        var availableCondiments = CoffeeAccompaniments[coffee.Value];
        
        var condimentMenu = selectedCondiments.ToSelectInput(availableCondiments.ToOptions())
            .Variant(SelectInputs.Toggle);
        
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
