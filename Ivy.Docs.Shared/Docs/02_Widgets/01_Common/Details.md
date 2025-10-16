---
searchHints:
  - properties
  - fields
  - data
  - information
  - key-value
  - details
---

# Details

<Ingress>
Display structured label-value pairs from models with automatic formatting using the ToDetails() extension method.
</Ingress>

`Detail` widgets display label and value pairs. They are usually generated from a model using `ToDetails()`.

## Basic Usage

The simplest way to create details is by calling `ToDetails()` on any object:

```csharp demo-below
new { Name = "John Doe", Email = "john@example.com", Age = 30 }
    .ToDetails()
```

### Automatic Field Removal

Remove empty or null fields using the `RemoveEmpty()` method. This removes fields that are:

- `null` values
- Empty or whitespace strings
- `false` boolean values

Use this when you want to hide fields that don't have meaningful values, keeping your details clean and focused:

```csharp demo-tabs
new { FirstName = "John", LastName = "Doe", Age = 30, MiddleName = "" }
    .ToDetails()
    .RemoveEmpty()
```

### Custom Field Removal

Selectively remove specific fields using the `Remove()` method. This is useful when you want to hide sensitive information like IDs or internal fields from the user interface:

```csharp demo-tabs
new { Id = 123, Name = "John Doe", Email = "john@example.com" }
    .ToDetails()
    .Remove(x => x.Id)
```

### Multi-Line Fields

Mark specific fields as multi-line for better text display. This is perfect for long descriptions, notes, or any text content that would benefit from wrapping across multiple lines:

```csharp demo-tabs
new { Name = "Widget", Description = "Long description text" }
    .ToDetails()
    .MultiLine(x => x.Description)
```

## Custom Builders

Override the default rendering for specific fields using custom builders. This allows you to customize how individual fields are displayed and add interactive functionality.

### Copy to Clipboard

Make values copyable to clipboard. This is especially useful for IDs, email addresses, or any text that users might want to copy for use elsewhere:

```csharp demo-tabs
new { Id = "ABC-123", Name = "John Doe" }
    .ToDetails()
    .Builder(x => x.Id, b => b.CopyToClipboard())
```

### Links

Convert values to clickable links. Automatically transform URLs, email addresses, or any text into clickable links that users can interact with:

```csharp demo-tabs
new { Name = "John Doe", Website = "https://example.com" }
    .ToDetails()
    .Builder(x => x.Website, b => b.Link())
```

### Text Formatting

Control how text values are displayed. Use this when you want to ensure consistent text formatting or apply specific styling to text fields:

```csharp demo-tabs
new { Name = "John Doe", Description = "Long description" }
    .ToDetails()
    .Builder(x => x.Description, b => b.Text())
```

## Nested Objects

Details automatically handle nested objects by converting them to their own detail views. This creates a hierarchical display that's perfect for complex data structures with parent-child relationships:

```csharp demo-tabs
new { 
    Name = "John", 
    Address = new { Street = "123 Main St", City = "Anytown" }.ToDetails() 
}.ToDetails()
```

## Working with State

Details work seamlessly with reactive state. When the underlying data changes, the details automatically update to reflect the new values, making it perfect for dynamic, interactive interfaces:

```csharp demo-tabs
UseState(() => new { Name = "John Doe", Age = 30 })
    .ToDetails()
```

<WidgetDocs Type="Ivy.Details" ExtensionTypes="Ivy.Views.Builders.DetailsBuilderExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Views/Builders/DetailsBuilder.cs"/>
