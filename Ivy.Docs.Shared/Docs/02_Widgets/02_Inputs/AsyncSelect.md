# AsyncSelectInput

<Ingress>
Create dropdown selectors that load options asynchronously from APIs or databases, perfect for large datasets and on-demand loading.
</Ingress>

The `AsyncSelectInput` widget provides a select dropdown that loads options asynchronously. It's useful for scenarios where options need to be fetched from an API or when the list of options is large and should be loaded on-demand.

## Basic Usage

Here's a simple example of an `AsyncSelectInput` that fetches categories from a database:

```csharp
var guidState = this.UseState(default(Guid?));

async Task<Option<Guid?>[]> QueryCategories(string query)
{
    await using var db = factory.CreateDbContext();
    return (await db.Categories
            .Where(e => e.Name.Contains(query))
            .Select(e => new { e.Id, e.Name })
            .Take(50)
            .ToArrayAsync())
        .Select(e => new Option<Guid?>(e.Name, e.Id))
        .ToArray();
}

async Task<Option<Guid?>?> LookupCategory(Guid? id)
{
    if (id == null) return null;
    await using var db = factory.CreateDbContext();
    var category = await db.Categories.FindAsync(id);
    if(category == null) return null;
    return new Option<Guid?>(category.Name, category.Id);
}

return guidState.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder:"Select Category");
```

### Event Handling

The `AsyncSelectInput` can handle selection events using the `OnChange` parameter:

```csharp
var state = this.UseState(default(Guid?));
var input = state.ToAsyncSelectInput(QueryCategories, LookupCategory);
input.OnChange = e => Console.WriteLine($"Selected: {e.Value}");
```

### Styling

The `AsyncSelectInput` can be customized with various styling options, such as setting a placeholder or disabling the input:

```csharp
var input = state.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder:"Select Category", disabled: true);
```

<WidgetDocs Type="Ivy.AsyncSelectInput" ExtensionTypes="Ivy.AsyncSelectInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/AsyncSelectInput.cs"/>

## Examples

### Advanced Usage

Here's an example of using `AsyncSelectInput` in a form:

```csharp
var product = this.UseState(() => new ProductCreateRequest());
return product
    .ToForm()
    .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder: "Select Category"))
    .ToDialog(isOpen, title: "Create Product", submitTitle: "Create");
``` 