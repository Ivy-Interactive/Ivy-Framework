---
searchHints:
  - dropdown
  - autocomplete
  - search
  - async
  - picker
  - options
---

# AsyncSelectInput

<Ingress>
Create dropdown selectors that load options asynchronously from [APIs](../../03_Hooks/Core/11_UseService.md) or [databases](../../01_Onboarding/02_Concepts/01_Program.md), perfect for large datasets and on-demand loading.
</Ingress>

The `AsyncSelectInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides a select dropdown that loads options asynchronously. It's useful for scenarios where options need to be fetched from an [API](../../03_Hooks/Core/11_UseService.md) or when the list of options is large and should be loaded on-demand.

## Basic Usage

Here's a simple example of an `AsyncSelectInput` that fetches categories:

```csharp demo-below
public class AsyncSelectBasicDemo : ViewBase
{
    private static readonly string[] Categories = { "Electronics", "Clothing", "Books", "Home & Garden", "Sports" };

    public override object? Build()
    {
        var selectedCategory = UseState<string?>(default(string?));

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

        return selectedCategory.ToAsyncSelectInput(QueryCategories, LookupCategory, "Search categories...")
                .WithField()
                .Label("Select a category:")
                .Width(Size.Full());
    }
}
```

## Data Types

AsyncSelectInput supports various data types. Here's an example showing String, Integer, and Enum-based AsyncSelects:

```csharp demo-tabs
public class DataTypesDemo : ViewBase
{
    private static readonly Dictionary<string, string> CountryRegions = new()
    {
        { "Germany", "Europe" },
        { "France", "Europe" },
        { "Japan", "Asia" },
        { "China", "Asia" },
        { "USA", "North America" },
        { "Canada", "North America" },
        { "Australia", "Oceania" },
        { "Brazil", "South America" }
    };

    private enum ProgrammingLanguage
    {
        CSharp,
        Java,
        Python,
        JavaScript,
        Go,
        Rust,
        FSharp,
        Kotlin,
        Swift,
        TypeScript
    }

    public override object? Build()
    {
        var selectedCountry = UseState<string?>(default(string));
        var selectedYear = UseState<int?>(default(int));
        var selectedLanguage = UseState(ProgrammingLanguage.CSharp);

        QueryResult<Option<string>[]> QueryCountries(IViewContext context, string query)
        {
            return context.UseQuery<Option<string>[], (string, string)>(
                key: (nameof(QueryCountries), query),
                fetcher: ct => Task.FromResult(CountryRegions.Keys
                    .Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(c => new Option<string>(c, c, description: CountryRegions[c]))
                    .ToArray()));
        }

        QueryResult<Option<string>?> LookupCountry(IViewContext context, string? country)
        {
            return context.UseQuery<Option<string>?, (string, string?)>(
                key: (nameof(LookupCountry), country),
                fetcher: ct =>
                {
                    if (string.IsNullOrEmpty(country)) return Task.FromResult<Option<string>?>(null);
                    return Task.FromResult<Option<string>?>(new Option<string>(country, country, description: CountryRegions.GetValueOrDefault(country)));
                });
        }

        QueryResult<Option<int>[]> QueryYears(IViewContext context, string query)
        {
            return context.UseQuery<Option<int>[], (string, string)>(
                key: (nameof(QueryYears), query),
                fetcher: ct =>
                {
                    var currentYear = DateTime.Now.Year;
                    var years = Enumerable.Range(currentYear - 100, 101).ToArray();

                    if (string.IsNullOrEmpty(query))
                        return Task.FromResult(years.Take(20).Select(y => new Option<int>(y.ToString(), y)).ToArray());

                    if (int.TryParse(query, out var yearQuery))
                    {
                        return Task.FromResult(years
                            .Where(y => y >= yearQuery && y <= yearQuery + 10)
                            .Take(20)
                            .Select(y => new Option<int>(y.ToString(), y))
                            .ToArray());
                    }

                    return Task.FromResult(years
                        .Where(y => y.ToString().Contains(query))
                        .Take(20)
                        .Select(y => new Option<int>(y.ToString(), y))
                        .ToArray());
                });
        }

        QueryResult<Option<int>?> LookupYear(IViewContext context, int year)
        {
            return context.UseQuery<Option<int>?, (string, int)>(
                key: (nameof(LookupYear), year),
                fetcher: ct => Task.FromResult<Option<int>?>(new Option<int>(year.ToString(), year)));
        }

        QueryResult<Option<ProgrammingLanguage>[]> QueryLanguages(IViewContext context, string query)
        {
            return context.UseQuery<Option<ProgrammingLanguage>[], (string, string)>(
                key: (nameof(QueryLanguages), query),
                fetcher: ct =>
                {
                    var languages = new[]
                    {
                        ProgrammingLanguage.CSharp,
                        ProgrammingLanguage.Java,
                        ProgrammingLanguage.Python,
                        ProgrammingLanguage.JavaScript,
                        ProgrammingLanguage.Go,
                        ProgrammingLanguage.Rust,
                        ProgrammingLanguage.FSharp,
                        ProgrammingLanguage.Kotlin,
                        ProgrammingLanguage.Swift,
                        ProgrammingLanguage.TypeScript
                    };

                    if (string.IsNullOrEmpty(query))
                        return Task.FromResult(languages.Select(l => new Option<ProgrammingLanguage>(l.ToString(), l)).ToArray());

                    return Task.FromResult(languages
                        .Where(l => l.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                        .Select(l => new Option<ProgrammingLanguage>(l.ToString(), l))
                        .ToArray());
                });
        }

        QueryResult<Option<ProgrammingLanguage>?> LookupLanguage(IViewContext context, ProgrammingLanguage language)
        {
            return context.UseQuery<Option<ProgrammingLanguage>?, (string, ProgrammingLanguage)>(
                key: (nameof(LookupLanguage), language),
                fetcher: ct => Task.FromResult<Option<ProgrammingLanguage>?>(new Option<ProgrammingLanguage>(language.ToString(), language)));
        }

        return Layout.Vertical()
            | selectedCountry.ToAsyncSelectInput(QueryCountries, LookupCountry, placeholder: "Search countries...")
                .WithField()
                .Label("String-based AsyncSelect:")
                .Width(Size.Full())
            
            | selectedYear.ToAsyncSelectInput(QueryYears, LookupYear, placeholder: "Search years...")
                .WithField()
                .Label("Integer-based AsyncSelect:")
                .Width(Size.Full())
            
            | selectedLanguage.ToAsyncSelectInput(QueryLanguages, LookupLanguage, placeholder: "Search languages...")
                .WithField()
                .Label("Enum-based AsyncSelect:")
                .Width(Size.Full());
    }
}
```

## Advanced Patterns

### Custom Query Logic

Implement complex search logic with multiple criteria:

```csharp demo-tabs
public class AdvancedQueryDemo : ViewBase
{
    public record User
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = "";
        public string Email { get; init; } = "";
        public string Department { get; init; } = "";
        public bool IsActive { get; init; }
    }

    // Use consistent GUIDs for demo purposes
    private static readonly Guid JohnId = Guid.NewGuid();
    private static readonly Guid JaneId = Guid.NewGuid();
    private static readonly Guid BobId = Guid.NewGuid();
    private static readonly Guid AliceId = Guid.NewGuid();
    private static readonly Guid CharlieId = Guid.NewGuid();

    // Single source of user data
    private static readonly User[] Users = new[]
    {
        new User { Id = JohnId, Name = "John Doe", Email = "john@example.com", Department = "Engineering", IsActive = true },
        new User { Id = JaneId, Name = "Jane Smith", Email = "jane@example.com", Department = "Design", IsActive = true },
        new User { Id = BobId, Name = "Bob Johnson", Email = "bob@example.com", Department = "Marketing", IsActive = false },
        new User { Id = AliceId, Name = "Alice Brown", Email = "alice@example.com", Department = "Engineering", IsActive = true },
        new User { Id = CharlieId, Name = "Charlie Wilson", Email = "charlie@example.com", Department = "Sales", IsActive = true }
    };

    public override object? Build()
    {
        var selectedUser = UseState<Guid>(default(Guid));
        var selectedUserInfo = UseState<string>("No user selected");

        // Update display when selection changes
        UseEffect(() =>
        {
            var user = Users.FirstOrDefault(u => u.Id == selectedUser.Value);
            selectedUserInfo.Set(user != null ? $"{user.Name} - {user.Email} ({user.Department})" : "No user selected");
        }, [selectedUser]);

        QueryResult<Option<Guid>[]> QueryUsers(IViewContext context, string query)
        {
            return context.UseQuery<Option<Guid>[], (string, string)>(
                key: (nameof(QueryUsers), query),
                fetcher: ct =>
                {
                    if (string.IsNullOrEmpty(query))
                        return Task.FromResult(Users.Where(u => u.IsActive).Take(5)
                            .Select(u => new Option<Guid>($"{u.Name} ({u.Department})", u.Id, description: u.Email))
                            .ToArray());

                    var queryLower = query.ToLowerInvariant();
                    return Task.FromResult(Users
                        .Where(u => u.IsActive &&
                                   (u.Name.ToLowerInvariant().Contains(queryLower) ||
                                    u.Email.ToLowerInvariant().Contains(queryLower) ||
                                    u.Department.ToLowerInvariant().Contains(queryLower)))
                        .Take(10)
                        .Select(u => new Option<Guid>($"{u.Name} ({u.Department})", u.Id, description: u.Email))
                        .ToArray());
                });
        }

        QueryResult<Option<Guid>?> LookupUser(IViewContext context, Guid id)
        {
            return context.UseQuery<Option<Guid>?, (string, Guid)>(
                key: (nameof(LookupUser), id),
                fetcher: ct =>
                {
                    var user = Users.FirstOrDefault(u => u.Id == id);
                    return Task.FromResult(user != null
                        ? new Option<Guid>($"{user.Name} ({user.Department})", user.Id, description: user.Email)
                        : null);
                });
        }

        var customAsyncSelect = new AsyncSelectInputView<Guid>(
            selectedUser.Value,
            e => { selectedUser.Set(e.Value); return ValueTask.CompletedTask; },
            QueryUsers,
            LookupUser,
            placeholder: "Search by name, email, or department..."
        );

        return customAsyncSelect
                .WithField()
                .Label("Search and select a user:")
                .Width(Size.Full());
    }
}
```

<Callout Type="tip">
Use AsyncSelectInput for foreign key relationships, large datasets, and when you need to provide search functionality. It's perfect for scenarios where the full list of options would be too large to load upfront.
</Callout>

### Styling and States

Customize the `AsyncSelectInput` with various styling options:

```csharp demo-tabs
public class StylingDemo : ViewBase
{
    public override object? Build()
    {
        var normalSelect = UseState<string?>(default(string));
        var invalidSelect = UseState<string?>(default(string));
        var disabledSelect = UseState<string?>(default(string));

        QueryResult<Option<string>[]> QueryOptions(IViewContext context, string query)
        {
            var options = new[] { "Option 1", "Option 2", "Option 3" };
            return context.UseQuery<Option<string>[], (string, string)>(
                key: (nameof(QueryOptions), query),
                fetcher: ct => Task.FromResult(options
                    .Where(opt => opt.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(opt => new Option<string>(opt))
                    .ToArray()));
        }

        QueryResult<Option<string>?> LookupOption(IViewContext context, string? option)
        {
            return context.UseQuery<Option<string>?, (string, string?)>(
                key: (nameof(LookupOption), option),
                fetcher: ct => Task.FromResult<Option<string>?>(option != null ? new Option<string>(option) : null));
        }

        return Layout.Vertical()
            | normalSelect.ToAsyncSelectInput(QueryOptions, LookupOption, placeholder: "Choose an option...")
                .WithField()
                .Label("Normal AsyncSelectInput:")
            
            | invalidSelect.ToAsyncSelectInput(QueryOptions, LookupOption, placeholder: "This has an error...")
                .Invalid("This field is required")
                .WithField()
                .Label("Invalid AsyncSelectInput:")
            
            | disabledSelect.ToAsyncSelectInput(QueryOptions, LookupOption, placeholder: "This is disabled...")
                .Disabled(true)
                .WithField()
                .Label("Disabled AsyncSelectInput:");
    }
}
```

<Callout Type="tip">
AsyncSelectInput automatically handles loading states and provides a smooth user [experience](../../01_Onboarding/02_Concepts/02_Views.md). The query function is called as the user types, and the lookup function is called when displaying the selected value.
</Callout>

<WidgetDocs Type="Ivy.AsyncSelectInput" ExtensionTypes="Ivy.AsyncSelectInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/AsyncSelectInput.cs"/>
