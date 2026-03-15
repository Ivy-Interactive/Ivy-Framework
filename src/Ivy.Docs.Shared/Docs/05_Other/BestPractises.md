# Best Practises

## Use UseQuery for Data Fetching

Prefer `UseQuery` over manual `UseEffect` + `UseState` combinations for data fetching. UseQuery provides built-in loading, error, and caching (SWR) support.

```csharp
// Bad - manual state management
var data = UseState(ImmutableArray.Create<Item>());
var error = UseState<string?>();
var loaded = UseState(false);

UseEffect(async () =>
{
    try
    {
        var response = await client.GetItemsAsync();
        data.Set(response.ToImmutableArray());
    }
    catch (Exception ex)
    {
        error.Set(ex.Message);
    }
    finally
    {
        loaded.Set(true);
    }
}, EffectTrigger.OnMount());

// Good - UseQuery handles loading, error, and caching
var query = UseQuery(
    key: "items",
    fetcher: async ct =>
    {
        var response = await client.GetItemsAsync();
        return response.ToImmutableArray();
    });

if (query.Loading) return Text.Muted("Loading...");
if (query.Error is { } error) return Callout.Error(error.Message);
```

For conditional fetching, pass `null` as the key to disable the query:

```csharp
var search = UseQuery(
    key: searchTerm is { } term ? $"search:{term}" : null,
    fetcher: async ct => await client.SearchAsync(searchTerm!, ct));
```

## Don't Use Padding and Gap

Layouts use a default gap of 4. Avoid adding `.Padding()` and `.Gap()` unless absolutely necessary.

```csharp
// Bad
Layout.Vertical().Padding(6).Gap(6)
    | Text.H2("Title")
    | Text.Muted("Subtitle");

// Good
Layout.Vertical()
    | Text.H2("Title")
    | Text.Muted("Subtitle");
```

## Use ToTable Instead of TableView

Prefer `.ToTable()` over manually constructing `TableView` with `TableColumn` definitions. `ToTable()` auto-scaffolds columns from the model's properties with smart defaults (link builders for URL fields, right-alignment for numbers, etc.).

```csharp
// Bad - verbose manual column definitions
new TableView<RepoRow>(
    query.Value,
    new TableColumn<RepoRow, string>(r => r.FullName, "Repository"),
    new TableColumn<RepoRow, string>(r => r.Language, "Language"),
    new TableColumn<RepoRow, int>(r => r.Stars, "Stars"),
    new TableColumn<RepoRow, int>(r => r.Forks, "Forks"));

// Good - auto-scaffolded with header overrides where needed
query.Value.ToTable()
    .Header(r => r.FullName, "Repository");
```
