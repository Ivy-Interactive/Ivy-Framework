
# Blades

<Ingress>
Create stacked navigation experiences where new views slide in from the right, managed through a blade controller for intuitive drill-down interfaces.
</Ingress>

`Blade`s provide a stacked navigation pattern where new views slide in from the right. Use the `UseBlades` extension to create a root blade and manage a stack of blades through `IBladeController`. Perfect for master-detail interfaces, wizards, and hierarchical navigation.

## Basic Usage

Create a blade container with a root view:

```csharp
this.UseBlades(() => new RootView("A"), "Blade 0");
```

```csharp demo-tabs 
public class BasicBladesDemo : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new RootBladeView(), "Products");
    }
}

public class RootBladeView : ViewBase
{
    public override object? Build()
    {
        var blades = this.UseContext<IBladeController>();
        var products = new[] { "iPhone", "MacBook", "iPad", "Apple Watch" };
        
        var items = products.Select(product => 
            new ListItem(product, onClick: _ => 
                blades.Push(this, new ProductDetailView(product), product))
        );
        
        return new List(items);
    }
}

public class ProductDetailView(string productName) : ViewBase
{
    public override object? Build()
    {
        return new Card($"Details for {productName}")
            | Text.Block($"This is the detail view for {productName}")
            | Text.Block("Price: $999")
            | Text.Block("In Stock: Yes");
    }
}
```

## Blade Controller

Access the blade controller to manage the blade stack:

```csharp
var blades = this.UseContext<IBladeController>();
```

### Pushing New Blades

Push a new blade onto the stack:

```csharp
action<Event<Button>> onClick = e =>
{
    var blades = this.UseContext<IBladeController>();
    blades.Push(this, new DetailView(e.Sender.Tag), "Details");
};
```

```csharp demo-tabs 
public class BladeNavigationDemo : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new NavigationRootView(), "Home");
    }
}

public class NavigationRootView : ViewBase
{
    public override object? Build()
    {
        var blades = this.UseContext<IBladeController>();
        var index = blades.GetIndex(this);
        
        return Layout.Vertical()
            | Text.Block($"This is blade level {index}")
            | new Button($"Push Blade {index + 1}", onClick: _ => 
                blades.Push(this, new NavigationRootView(), $"Level {index + 1}"))
            | (index > 0 ? new Button("Go Back", onClick: _ => blades.Pop()) : null);
    }
}
```

### Popping Blades

Remove blades from the stack:

```csharp
// Pop to the previous blade
blades.Pop();

// Pop to a specific index
blades.Pop(toIndex: 0);

// Pop with refresh (triggers re-render)
blades.Pop(refresh: true);
```

## Advanced Features

### Custom Blade Width

Set specific widths for blades to avoid layout jank:

```csharp
// Use specific units for consistent sizing
blades.Push(this, new DetailView(item), item.Name, width: Size.Units(100));

// Or use Size.Auto() with constraints
var width = Size.Auto().Min(Size.Units(80)).Max(Size.Units(120));
blades.Push(this, new DetailView(item), item.Name, width: width);
```

```csharp demo-tabs 
public class BladeSizingDemo : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new SizingRootView(), "Sizing Demo", Size.Units(60));
    }
}

public class SizingRootView : ViewBase
{
    public override object? Build()
    {
        var blades = this.UseContext<IBladeController>();
        
        return Layout.Vertical()
            | Text.Block("Click buttons to see different blade sizes:")
            | Layout.Horizontal().Gap(1)
                | new Button("Small Blade", onClick: _ =>
                    blades.Push(this, new SizeDemo("Small", Size.Units(40)), "Small", Size.Units(40)))
                | new Button("Large Blade", onClick: _ =>
                    blades.Push(this, new SizeDemo("Large", Size.Units(120)), "Large", Size.Units(120)));
    }
}

public class SizeDemo(string size, Size width) : ViewBase
{
    public override object? Build()
    {
        return new Card($"This is a {size.ToLower()} blade")
            | Text.Block($"Width: {width}")
            | Text.Block("Content scales to blade width");
    }
}
```

### Blade Headers with BladeHelper

Use `BladeHelper.WithHeader()` to add toolbar content to blades:

```csharp
return BladeHelper.WithHeader(
    Layout.Horizontal(
        searchString.ToSearchInput().Placeholder("Search..."),
        new Button(icon: Icons.Plus, onClick: onCreate, variant: ButtonVariant.Outline)
    ).Gap(1),
    new List(items)
);
```

```csharp demo-tabs 
public class BladeHeaderDemo : ViewBase
{
    public override object? Build()
    {
        return Context.UseBlades(() => new SearchableListView(), "Search Products");
    }
}

public class SearchableListView : ViewBase
{
    public override object? Build()
    {
        var blades = this.UseContext<IBladeController>();
        var searchTerm = this.UseState("");
        var products = new[] { "iPhone 15", "MacBook Pro", "iPad Air", "Apple Watch", "AirPods Pro" };
        
        var filteredProducts = products
            .Where(p => p.Contains(searchTerm.Value, StringComparison.OrdinalIgnoreCase))
            .ToArray();
            
        var items = filteredProducts.Select(product => 
            new ListItem(product, onClick: _ => 
                blades.Push(this, new ProductDetailView(product), product))
        );
        
        return BladeHelper.WithHeader(
            Layout.Horizontal(
                searchTerm.ToTextInput().Placeholder("Search products..."),
                new Button(icon: Icons.Filter, variant: ButtonVariant.Outline)
            ).Gap(1),
            filteredProducts.Any() 
                ? new List(items) 
                : Text.Block("No products found")
        );
    }
}
```

### Refresh Tokens and State Management

Coordinate blade updates using refresh tokens:

```csharp
var refreshToken = this.UseRefreshToken();

this.UseEffect(() =>
{
    if (refreshToken.ReturnValue is Guid itemId)
    {
        blades.Pop(this, true); // Refresh current blade
        blades.Push(this, new DetailView(itemId));
    }
}, [refreshToken]);
```

## Common Patterns

### Master-Detail Interface

```csharp
public class ListBlade : ViewBase
{
    public override object? Build()
    {
        var blades = this.UseContext<IBladeController>();
        var items = this.UseState(SampleData.GetItems());
        
        var onItemClick = new Action<Event<ListItem>>(e =>
        {
            var item = (Item)e.Sender.Tag!;
            blades.Push(this, new DetailBlade(item), item.Name);
        });
        
        return new List(items.Value.Select(item => 
            new ListItem(item.Name, onClick: onItemClick, tag: item)));
    }
}
```

### Progressive Disclosure

```csharp
// Navigate deeper into a hierarchy
blades.Push(this, new CategoryView(categoryId), category.Name);
blades.Push(this, new ProductView(productId), product.Name);
blades.Push(this, new VariantView(variantId), variant.Name);
```

### Modal Replacement

```csharp
// Replace modal dialogs with blade-based workflows
var createButton = Icons.Plus.ToButton(_ =>
{
    blades.Pop(this); // Ensure clean state
}).ToTrigger((isOpen) => new CreateDialog(isOpen, refreshToken));
```

## Size Constraints

Blades have sensible defaults but can be customized:

```csharp
// Default width with minimum constraint
Size.Auto().Min(Size.Units(80))

// Fixed width for consistent layout
Size.Units(100)

// Responsive width with constraints
Size.Auto().Min(Size.Units(60)).Max(Size.Units(150))

// Use in UseBlades initialization
this.UseBlades(() => new RootView(), "Root", Size.Units(75));
```

## Error Handling

Blades handle errors gracefully and provide context for debugging:

```csharp
public class BladeWithError : ViewBase
{
    public override object? Build()
    {
        throw new InvalidOperationException("This blade has an error");
    }
}

// Error blades show error details while maintaining navigation context
blades.Push(this, new BladeWithError(), "Error Blade");
```

<WidgetDocs Type="Ivy.Blade" ExtensionTypes="Ivy.Views.Blades.UseBladesExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Views/Blades/UseBlades.cs"/>
