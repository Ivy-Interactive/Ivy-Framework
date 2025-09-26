# SelectInput

<Ingress>
Create dropdown menus with single or multiple selection capabilities, option grouping, and custom rendering for user choices.
</Ingress>

The `SelectInput` widget provides a dropdown menu for selecting items from a predefined list of options. It supports single
and multiple selections, option grouping, and custom rendering of option items.

## Basic Usage

Here's a simple example of a `SelectInput` with a few options:

```csharp demo-tabs
public class SelectVariantDemo : ViewBase
{
    public override object? Build()
    {
        var langs = new string[]{"C#","Java","Go","JavaScript","F#","Kotlin","VB.NET","Rust"};
        
        var favLang = UseState("C#");
        return Layout.Vertical() 
                | Text.Label("Select your favourite programming language")
                | favLang.ToSelectInput(langs.ToOptions()).Variant(SelectInputs.Select);
    }    
}
```

`SelectInput` supports three different variants for different use cases:

### Default Select

The default variant renders a traditional dropdown menu. Use this when only one item should be selected:

```csharp demo-tabs
public class SelectColorDemo : ViewBase
{
    public override object? Build()
    {    
        var fruits = new string[]{"Apple","Guava","Banana","Watermelon"};
        var dishes = new string[]{"pie", "pickle", "shake", "juice"};
        var guess = this.UseState(fruits[0]);
        var fruitInput = guess.ToSelectInput(fruits.ToOptions());
        return Layout.Vertical() 
                | Text.Label("Your favourite fruit")
                | fruitInput
                | Text.Label($"{guess}  {dishes[Array.IndexOf(fruits,guess.Value)]} is delicious!");
    }
}    
```

### List

The List variant renders options as checkboxes, perfect for multiple selection scenarios:

```csharp demo-tabs
public class ListVariantDemo : ViewBase
{
    public override object? Build()
    {
        var options = new List<string>() { "Email", "Phone", "SMS", "Push Notification" };
        var selectedNotice = UseState(new string[]{});
        return Layout.Vertical() 
                | Text.Label("How would you like to be notified?") 
                | selectedNotice.ToSelectInput(options.ToOptions())
                                .Variant(SelectInputs.List)
                | Text.Small($"Selected: {string.Join(", ", selectedNotice.Value)}");
    }
}
```

### Toggle

The Toggle variant displays options as toggleable buttons, great for visual selection interfaces:

```csharp demo-tabs
public class ToggleVariantDemo : ViewBase
{
    public override object? Build()
    {
        var mealOptions = new string[]{"Breakfast", "Lunch", "Dinner", "Snack"};
        var selectedMeals = UseState(new string[]{});
        
        return Layout.Vertical()
            | Text.Label("Select your meal preferences:")
            | selectedMeals.ToSelectInput(mealOptions.ToOptions())
                .Variant(SelectInputs.Toggle)
            | Text.Small($"You selected: {string.Join(", ", selectedMeals.Value)}");
    }
}
```

<Callout Type="tip">
The framework automatically detects when you use a collection type (array, List, etc.) as your state and enables multiple selection. No need to manually configure this!
</Callout>

## Multiple Selection

Multiple selection is automatically enabled when you use a collection type (array, List, etc.) as your state. The framework automatically detects this and enables multi-select functionality.

### Multi-Select with Different Variants

Here's a comprehensive example showing all three variants with multiple selection:

```csharp demo-tabs
public class MultiSelectVariantsDemo : ViewBase
{
    private enum ProgrammingLanguages
    {
        CSharp,
        Java,
        Python,
        JavaScript,
        Go,
        Rust,
        FSharp,
        Kotlin
    }
    
    public override object? Build()
    {
        var languagesSelect = UseState<ProgrammingLanguages[]>([]);
        var languagesList = UseState<ProgrammingLanguages[]>([]);
        var languagesToggle = UseState<ProgrammingLanguages[]>([]);
        var languageOptions = typeof(ProgrammingLanguages).ToOptions();
        
        return Layout.Vertical()
            | Text.H2("Multi-Select Variants")
            | Layout.Grid().Columns(3)
                | Text.InlineCode("Select Variant")
                | Text.InlineCode("List Variant")
                | Text.InlineCode("Toggle Variant")
                
                | languagesSelect.ToSelectInput(languageOptions)
                    .Variant(SelectInputs.Select)
                    .Placeholder("Choose languages...")
                | languagesList.ToSelectInput(languageOptions)
                    .Variant(SelectInputs.List)
                | languagesToggle.ToSelectInput(languageOptions)
                    .Variant(SelectInputs.Toggle)
                
                | Text.Small($"Selected: {string.Join(", ", languagesSelect.Value)}")
                | Text.Small($"Selected: {string.Join(", ", languagesList.Value)}")
                | Text.Small($"Selected: {string.Join(", ", languagesToggle.Value)}");
    }
}
```

### Multi-Select with Different Data Types

This example demonstrates multi-select with various data types:

```csharp demo-tabs
public class MultiSelectDataTypesDemo : ViewBase
{
    public override object? Build()
    {
        var stringArray = UseState<string[]>([]);
        var intArray = UseState<int[]>([]);
        var guidArray = UseState<Guid[]>([]);
        
        var stringOptions = new[]{"Option A", "Option B", "Option C", "Option D"}.ToOptions();
        var intOptions = new[]{1, 2, 3, 4, 5}.ToOptions();
        var guidOptions = new[]{Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()}.ToOptions();
        
        return Layout.Vertical()
            | Layout.Grid().Columns(3)
                | Text.InlineCode("String Array")
                | Text.InlineCode("Integer Array")
                | Text.InlineCode("Guid Array")
                
                | stringArray.ToSelectInput(stringOptions)
                    .Variant(SelectInputs.List)
                    .Placeholder("Select strings...")
                | intArray.ToSelectInput(intOptions)
                    .Variant(SelectInputs.List)
                    .Placeholder("Select numbers...")
                | guidArray.ToSelectInput(guidOptions)
                    .Variant(SelectInputs.List)
                    .Placeholder("Select GUIDs...")
                
                | Text.Small($"Count: {stringArray.Value.Length}")
                | Text.Small($"Count: {intArray.Value.Length}")
                | Text.Small($"Count: {guidArray.Value.Length}");
    }
}
```

## Event Handling

Handle change events using the `onChange` parameter for custom logic:

```csharp demo-tabs
public class SelectEventHandlingDemo : ViewBase
{
    public override object? Build()
    {
        var selectedCountry = UseState("");
        var showEuropeInfo = UseState(false);
        var showAsiaInfo = UseState(false);
        var showAmericaInfo = UseState(false);
        
        var countries = new[]{"Germany", "France", "Japan", "China", "USA", "Canada"}.ToOptions();
        
        return Layout.Vertical() 
                | Text.Label("Select a country:") 
                | new SelectInput<string>(
                    value: selectedCountry.Value, 
                    onChange: e =>
                    {
                        selectedCountry.Set(e.Value);
                        showEuropeInfo.Set(e.Value is "Germany" or "France");
                        showAsiaInfo.Set(e.Value is "Japan" or "China");
                        showAmericaInfo.Set(e.Value is "USA" or "Canada");
                    }, 
                    countries)
                | Layout.Horizontal()
                    | (showEuropeInfo.Value ? Text.Block("🇪🇺 European Union member") : null)
                    | (showAsiaInfo.Value ? Text.Block("🌏 Asian country") : null)
                    | (showAmericaInfo.Value ? Text.Block("🦅 American country") : null);
    }
}
```

### Dynamic Options Based on Selection

This example shows how to dynamically change available options based on user selection:

```csharp demo-tabs
public class DynamicOptionsDemo : ViewBase
{
    private static readonly Dictionary<string, string[]> CategoryOptions = new()
    {
        ["Programming"] = new[]{"C#", "Java", "Python", "JavaScript", "Go", "Rust"},
        ["Design"] = new[]{"Photoshop", "Illustrator", "Figma", "Sketch", "InDesign"},
        ["Database"] = new[]{"SQL Server", "PostgreSQL", "MySQL", "MongoDB", "Redis"},
        ["Cloud"] = new[]{"AWS", "Azure", "GCP", "DigitalOcean", "Heroku"}
    };
    
    public override object? Build()
    {
        var selectedCategory = UseState("Programming");
        var selectedSkills = UseState<string[]>([]);
        
        var categoryOptions = CategoryOptions.Keys.ToOptions();
        var skillOptions = CategoryOptions[selectedCategory.Value].ToOptions();
        
        return Layout.Vertical()
            | Layout.Grid().Columns(2)
                | Text.Label("Category:")
                | selectedCategory.ToSelectInput(categoryOptions)
                    .Placeholder("Choose a category...")
                
                | Text.Label("Skills:")
                | selectedSkills.ToSelectInput(skillOptions)
                    .Variant(SelectInputs.List)
                    .Placeholder("Select your skills...")
            
            | Text.P("Selected Skills:")
            | Text.Block(string.Join(", ", selectedSkills.Value));
    }
}
```

## Styling and States

Customize the `SelectInput` with various styling options:

### Invalid State

Display validation errors using the `Invalid` function:

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
            | Text.Label("Normal SelectInput:")
            | normalSelect.ToSelectInput(options)
                .Placeholder("Choose an option...")
            
            | Text.Label("Invalid SelectInput:")
            | invalidSelect.ToSelectInput(options)
                .Placeholder("This has an error...")
                .Invalid("This field is required")
            
            | Text.Label("Disabled SelectInput:")
            | disabledSelect.ToSelectInput(options)
                .Placeholder("This is disabled...")
                .Disabled(true);
    }
}
```

### Nullable Support

Handle nullable types with automatic null handling:

```csharp demo-tabs
public class NullableSelectDemo : ViewBase
{
    public override object? Build()
    {
        var nullableString = UseState<string?>(() => null);
        var nullableArray = UseState<string[]?>(() => null);
        
        var options = new[]{"Red", "Green", "Blue"}.ToOptions();
        
        return Layout.Vertical()
            | Text.Label("Nullable Single Select:")
            | nullableString.ToSelectInput(options)
                .Placeholder("Choose a color (optional)")
            
            | Text.Label("Nullable Multi-Select:")
            | nullableArray.ToSelectInput(options)
                .Variant(SelectInputs.List)
                .Placeholder("Choose colors (optional)")
            
            | Text.Small($"Single: {nullableString.Value ?? "None"}")
            | Text.Small($"Multiple: {nullableArray.Value?.Length ?? 0} selected");
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
                    | Text.Label("Coffee Type:")
                    | coffee.ToSelectInput(CoffeeAccompaniments.Keys.ToOptions())
                    
                    | Text.Label("Size:")
                    | coffeeSizeMenu
                    
                    | Text.Label("Condiments:")
                    | condimentMenu
                    
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
