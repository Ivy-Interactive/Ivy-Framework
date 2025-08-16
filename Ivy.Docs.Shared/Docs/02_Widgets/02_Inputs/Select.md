# SelectInput

<Ingress>
Create dropdown menus with single or multiple selection capabilities, option grouping, and custom rendering for user choices.
</Ingress>

The `SelectInput` widget provides a dropdown menu for selecting items from a predefined list of options. It supports single
and multiple selections, option grouping, and custom rendering of option items.

## Basic Usage

Here's a simple example of a `SelectInput` with a few options:

```csharp demo-below
public class SelectColorDemo : ViewBase
{
    
    public override object? Build()
    {    
        var fruits = new string[]{"Apple","Guava","Banana","Watermelon"};
        var dishes = new string[]{"pie", "pickle", "shake", "juice"};
        var guess = this.UseState(fruits[0]);
        var fruitInput = guess.ToSelectInput(fruits.ToOptions())
                                   .Width(35);
        return Layout.Vertical() 
                | Text.Label("Your favourite fruit")
                | fruitInput
                | Text.Label($"{guess}  {dishes[Array.IndexOf(fruits,guess.Value)]} is delicious!");
    }
}    
```

## Variants

`SelectInput` supports several variants

### Select

Using this variant `SelectInputs.Select`, the options can be rendered as a list of items. The following shows an example usage
of this variant. This is the default variant. This should be used when only one item is expected
to be selected from the inputs.

```csharp demo-below 
public class SelectVariantDemo : ViewBase
{
    public override object? Build()
    {
        var langs = new string[]{"C#","Java","Go","JavaScript","F#","Kotlin","VB.NET","Rust"};
        
        var favLang = UseState("C#");
        return Layout.Vertical() 
                | Text.Label("Select your favourite progrmming language")
                | favLang.ToSelectInput(langs.ToOptions()).Variant(SelectInputs.Select);
    }    
}
```

### List

If it is required to render the select options as checkboxes, then this variant `SelectInputs.List` should be used.

```csharp demo-below 
// Option 1: Single Select (probably what you want)
public class SelectVariantDemoSingleSelect : ViewBase
{
    public override object? Build()
    {
        var options = new List<string>() { "Email", "Phone", "Both" };
        var selectedNotice = UseState("");
        return Layout.Vertical() 
                | Text.Label("How would you like to be notified?") 
                | selectedNotice.ToSelectInput(options.ToOptions())
                                .Variant(SelectInputs.List);                
    }
}
```

### Toggle

Sometimes it is visually useful to show different options in their own boxes so that those can be toggled.
The following example shows how to use it.

```csharp demo-below 
public class MealComboDemo : ViewBase
{
        private static readonly Dictionary<string, decimal> BurgerPrices = new()
        {
            ["Big Mac"] = 5.99m,
            ["McChicken"] = 3.49m,
            ["Quarter Pounder"] = 6.49m,
            ["Cheeseburger"] = 2.49m,
            [""]  = 0.0m
            
        };
        
        private static readonly Dictionary<string, decimal> FriesPrices = new()
        {
            ["Small Fries"] = 1.99m,
            ["Medium Fries"] = 2.49m,
            ["Large Fries"] = 2.99m,
            [""]  = 0.0m
        };
        
        private static readonly Dictionary<string, decimal> DrinkPrices = new()
        {
            ["Coke"] = 1.99m,
            ["Sprite"] = 1.99m,
            ["Orange Juice"] = 2.29m,
            ["Water"] = 0.00m, // Free!
            [""]  = 0.0m
        };
        
    public override object? Build()
    {
        var burger = UseState("Big Mac");
        var fries = UseState("Medium Fries");
        var drink = UseState("Water");
        
        return Layout.Vertical()
            | H3("Create your meal")
            | (Layout.Horizontal()
               | Text.Block("Choose Burger")
               | burger.ToSelectInput(new[]{"Big Mac", "McChicken", "Quarter Pounder", "Cheeseburger"}.ToOptions())
                  .Variant(SelectInputs.Toggle))
            | (Layout.Horizontal()
               | Text.Block("Choose Fries")    
               | fries.ToSelectInput(new[]{"Small Fries", "Medium Fries", "Large Fries"}.ToOptions())
                .Variant(SelectInputs.Toggle))
            | (Layout.Horizontal()
               | Text.Block("Choose Drink")
               | drink.ToSelectInput(new[]{"Coke", "Sprite", "Orange Juice", "Water"}.ToOptions())
                .Variant(SelectInputs.Toggle))
            | (Layout.Horizontal()
                | Text.Block("Bill")
                | new NumberInput<decimal>(BurgerPrices[burger.Value] + FriesPrices[fries.Value] +DrinkPrices[drink.Value], _ => { })
                           .FormatStyle(NumberFormatStyle.Currency)
                           .Currency("EUR")
                           .Disabled());
    }
}
```

## Multiple Selections

Multiple selection is implicit when the state to receive the result is a collection instead of a
single element. The following example shows this in action.

### Multiple selection in List variant

The following example shows how a `SelectInputs.List` allows multiple selection.

```csharp demo-below 
public class SelectMultiVariantDemo : ViewBase
{
    public override object? Build()
    {
        var langs = new string[]{"Swedish","English","German","French"};
        
        var langSpeaks = UseState(new string[]{"English"});
        return Layout.Vertical() 
                | Text.Label("Languages you can speak")
                | langSpeaks.ToSelectInput(langs.ToOptions())
                            .Variant(SelectInputs.List);                             
    }    
}
```

### Multiple selection in Toggle variant

The following example shows how a `SelectInputs.Toggle` allows multiple selection.

```csharp demo-below 
public class SelectMultiVariantDemo : ViewBase
{
    public override object? Build()
    {
        var seats = new string[]{"1A","1B","1C","1D"};
        
        var seatsBooked = UseState(new string[]{"1C"});
        return Layout.Vertical() 
                | Text.Label("Seats you want")
                | seatsBooked.ToSelectInput(seats.ToOptions())
                            .Variant(SelectInputs.Toggle);                             
    }    
}
```

## Event Handling

Handle change events using the `onChange` parameter:

```csharp demo-below 
public class SelectVariantDemoSingleSelect : ViewBase
{
    public override object? Build()
    {
        var options = new List<string>() { "Email", "Phone", "Both" };
        var selectedNotice = UseState("");
        var showEmail = UseState(false);
        var showPhone = UseState(false);
        
        return Layout.Vertical() 
                | Text.Label("How would you like to be notified?") 
                | new SelectInput<string>(
                            value: selectedNotice.Value, 
                            onChange: e =>
                                    {
                                        selectedNotice.Set(e.Value);
                                        switch(e.Value)
                                        {
                                            case "Email":
                                                 showEmail.Set(true);
                                                 showPhone.Set(false); 
                                                break;
                                            case "Phone":
                                                 showEmail.Set(false);
                                                 showPhone.Set(true);
                                                break;
                                            case "Both":
                                                showEmail.Set(true);
                                                showPhone.Set(true);
                                                break;
                                        }
                                    }, 
                                    options.ToOptions()) 
                | new TextInput()
                        .Placeholder("email")
                        .Variant(TextInputs.Email)
                        .Disabled(!showEmail.Value) 
                | new TextInput()
                        .Placeholder("phone")
                        .Variant(TextInputs.Tel)
                        .Disabled(!showPhone.Value);
    }
}
```

## Styling

Customize the `SelectInput` with various styling options:

### Invalid

A `SelectInput` can be rendered as invalid using the `Invalid` function.

### Disabled

A `SelectInput` can be rendered as disabled using the `Disabled` function.

The following code shows how to use these functions and how it will render

```csharp demo-below 

public class SimpleSelectDemo : ViewBase
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

<WidgetDocs Type="Ivy.SelectInput" ExtensionTypes="Ivy.SelectInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/SelectInput.cs"/>

## Examples

### Coffee Condiments Selection

The following demo shows how different condiment options for different coffees can be shown possibly
in a software designed for a coffee shop.  

```csharp demo-tabs 
public class CoffeeShopDemo: ViewBase
{
    // Coffee varieties and their typical accompaniments
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
        },
        ["Americano"] = new List<string> 
        { 
            "Cream", "Milk", "Sugar", "Sweetener", "Lemon slice", 
            "Honey", "Cinnamon" 
        },
        ["Macchiato"] = new List<string> 
        { 
            "Caramel drizzle", "Extra foam", "Vanilla syrup", "Cinnamon", 
            "Whipped cream", "Chocolate shavings" 
        },
        ["Turkish"] = new List<string> 
        { 
            "Turkish delight", "Lokum", "Water", "Brown sugar", 
            "Cardamom", "Rose water", "Pistachio" 
        },
        ["Irish"] = new List<string> 
        { 
            "Whipped cream", "Brown sugar", "Irish whiskey", "Cinnamon", 
            "Nutmeg", "Vanilla extract" 
        }
    };
    string[] coffeeSizes = new string[]{"Short", "Tall", "Grande", "Venti", "Trenta"};
    
    public override object? Build()
    {
        var coffee = UseState("Cappuccino");
        var coffeeSize = UseState("Tall");
        var selectedCondiments = UseState(new string[]{});
        var previousCoffee = UseState("Cappuccino");
        var coffeeSelected = coffeeSize + " " + coffee.Value;
        var coffeeInput = coffee.ToSelectInput(CoffeeAccompaniments.Keys.ToOptions());
        
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
        
        if (selectedCondiments.Value.Length > 0)
        {
            if(selectedCondiments.Value.Length == 1)
            {
                coffeeSelected += " with " + selectedCondiments.Value[0];
            }
            else
            {                  
                 coffeeSelected += " with " + selectedCondiments.Value
                                                 .Take(selectedCondiments.Value.Length - 1)
                                                 .Aggregate((a,b) =>  a + ", " + b)
                                                 + " and " + selectedCondiments.Value[selectedCondiments.Value.Length - 1];
            }
        }
        
        return Layout.Vertical()
                | Text.Block("Coffee ")
                | coffeeInput
                | Text.Block("Size ")
                | coffeeSizeMenu
                | Text.Block("Condiments ")
                | condimentMenu
                | Text.Small("You are ordering...")
                | new Icon(Icons.Coffee) 
                | Text.Small(coffeeSelected);
    }
}
```
