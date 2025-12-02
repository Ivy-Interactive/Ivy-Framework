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
Create dropdown selectors that load options asynchronously from APIs or databases, perfect for large datasets and on-demand loading.
</Ingress>

The `AsyncSelectInput` widget provides a select dropdown that loads options asynchronously. It's useful for scenarios where options need to be fetched from an API or when the list of options is large and should be loaded on-demand.

## Basic Usage

Here's a simple example of an `AsyncSelectInput` that fetches categories:

```csharp demo-tabs
public class AsyncSelectBasicDemo : ViewBase
{
    private static readonly string[] Categories = { "Electronics", "Clothing", "Books", "Home & Garden", "Sports" };

    public override object? Build()
    {
        var selectedCategory = this.UseState<string?>(default(string?));

        Task<Option<string>[]> QueryCategories(string query)
        {
            return Task.FromResult(Categories
                .Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(c => new Option<string>(c))
                .ToArray());
        }

        Task<Option<string>?> LookupCategory(string? category)
        {
            return Task.FromResult(category != null ? new Option<string>(category) : null);
        }

        return Layout.Vertical()
            | selectedCategory.ToAsyncSelectInput(QueryCategories, LookupCategory, "Search categories...")
                .WithField()
                .Label("Select a category:")
            | Text.Small($"Selected: {selectedCategory.Value ?? "None"}");
    }
}
```

AsyncSelectInput supports various data types. Here are examples for different scenarios:

### String-based AsyncSelect

```csharp demo-tabs
public class StringAsyncSelectDemo : ViewBase
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

    public override object? Build()
    {
        var selectedCountry = this.UseState<string?>(default(string));

        Task<Option<string>[]> QueryCountries(string query)
        {
            return Task.FromResult(CountryRegions.Keys
                .Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(c => new Option<string>(c, c, description: CountryRegions[c]))
                .ToArray());
        }

        Task<Option<string>?> LookupCountry(string country)
        {
            if (string.IsNullOrEmpty(country)) return Task.FromResult<Option<string>?>(null);
            return Task.FromResult<Option<string>?>(new Option<string>(country, country, description: CountryRegions.GetValueOrDefault(country)));
        }

        return Layout.Vertical()
            | selectedCountry.ToAsyncSelectInput(QueryCountries, LookupCountry, placeholder: "Search countries...")
                .WithField()
                .Label("Select a country:")
            | Text.Small($"Selected: {selectedCountry.Value ?? "None"}");
    }
}
```

### Integer-based AsyncSelect

```csharp demo-tabs
public class IntegerAsyncSelectDemo : ViewBase
{
    public override object? Build()
    {
        var selectedYear = this.UseState<int?>(default(int));

        Task<Option<int>[]> QueryYears(string query)
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
        }

        Task<Option<int>?> LookupYear(int year)
        {
            return Task.FromResult<Option<int>?>(new Option<int>(year.ToString(), year));
        }

        return Layout.Vertical()
            | selectedYear.ToAsyncSelectInput(QueryYears, LookupYear, placeholder: "Search years...")
                .WithField()
                .Label("Select a year:")
            | Text.Small($"Selected: {selectedYear.Value?.ToString() ?? "None"}");
    }
}
```

### Enum-based AsyncSelect

```csharp demo-tabs
public class EnumAsyncSelectDemo : ViewBase
{
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
        var selectedLanguage = this.UseState(ProgrammingLanguage.CSharp);

        Task<Option<ProgrammingLanguage>[]> QueryLanguages(string query)
        {
            // Create a static array of languages to avoid runtime issues
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
        }

        Task<Option<ProgrammingLanguage>?> LookupLanguage(ProgrammingLanguage language)
        {
            return Task.FromResult<Option<ProgrammingLanguage>?>(new Option<ProgrammingLanguage>(language.ToString(), language));
        }

        return Layout.Vertical()
            | selectedLanguage.ToAsyncSelectInput(QueryLanguages, LookupLanguage, placeholder: "Search languages...")
                .WithField()
                .Label("Select a programming language:")
            | Text.Small($"Selected: {selectedLanguage.Value.ToString()}");
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
        var selectedUser = this.UseState<Guid>(default(Guid));
        var selectedUserInfo = this.UseState<string>("No user selected");

        // Update display when selection changes
        this.UseEffect(() =>
        {
            var user = Users.FirstOrDefault(u => u.Id == selectedUser.Value);
            selectedUserInfo.Set(user != null ? $"{user.Name} - {user.Email} ({user.Department})" : "No user selected");
        }, [selectedUser]);

        Task<Option<Guid>[]> QueryUsers(string query)
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
        }

        Task<Option<Guid>?> LookupUser(Guid id)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user != null 
                ? new Option<Guid>($"{user.Name} ({user.Department})", user.Id, description: user.Email) 
                : null);
        }

        var customAsyncSelect = new AsyncSelectInputView<Guid>(
            selectedUser.Value,
            e => selectedUser.Set(e.Value),
            QueryUsers,
            LookupUser,
            placeholder: "Search by name, email, or department..."
        );

        return Layout.Vertical()
            | customAsyncSelect
                .WithField()
                .Label("Search and select a user:")
            | Text.Small($"Selected: {selectedUserInfo.Value}");
    }
}
```

### Error Handling and Loading States

Handle errors gracefully and show loading states:

```csharp demo-tabs
public class ErrorHandlingDemo : ViewBase
{
    public override object? Build()
    {
        var selectedItem = this.UseState<string?>(default(string));
        var errorMessage = this.UseState<string?>(default(string));

        Task<Option<string>[]> QueryWithErrors(string query)
        {
            try
            {
                // Simulate random errors
                if (Random.Shared.Next(10) == 0)
                    throw new Exception("Simulated network error");
                
                var items = new[] { "Apple", "Banana", "Cherry", "Date", "Elderberry" };
                return Task.FromResult(items
                    .Where(item => item.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(item => new Option<string>(item))
                    .ToArray());
            }
            catch (Exception ex)
            {
                errorMessage.Set(ex.Message);
                return Task.FromResult(Array.Empty<Option<string>>());
            }
        }

        Task<Option<string>?> LookupWithErrors(string item)
        {
            try
            {
                errorMessage.Set(default(string)); // Clear previous errors
                return Task.FromResult<Option<string>?>(new Option<string>(item));
            }
            catch (Exception ex)
            {
                errorMessage.Set($"Lookup failed: {ex.Message}");
                return Task.FromResult<Option<string>?>(null);
            }
        }

        return Layout.Vertical()
            | selectedItem.ToAsyncSelectInput(QueryWithErrors, LookupWithErrors, placeholder: "Search items...")
                .WithField()
                .Label("AsyncSelect with error handling:")
            | (errorMessage.Value != null ? Text.Block(errorMessage.Value) : null)
            | Text.Small($"Selected: {selectedItem.Value ?? "None"}");
    }
}
```

### Styling and States

Customize the `AsyncSelectInput` with various styling options:

```csharp demo-tabs
public class StylingDemo : ViewBase
{
    public override object? Build()
    {
        var normalSelect = this.UseState<string?>(default(string));
        var invalidSelect = this.UseState<string?>(default(string));
        var disabledSelect = this.UseState<string?>(default(string));

        Task<Option<string>[]> QueryOptions(string query)
        {
            var options = new[] { "Option 1", "Option 2", "Option 3" };
            return Task.FromResult(options
                .Where(opt => opt.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(opt => new Option<string>(opt))
                .ToArray());
        }

        Task<Option<string>?> LookupOption(string option)
        {
            return Task.FromResult<Option<string>?>(new Option<string>(option));
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
AsyncSelectInput automatically handles loading states and provides a smooth user experience. The query function is called as the user types, and the lookup function is called when displaying the selected value.
</Callout>

<WidgetDocs Type="Ivy.AsyncSelectInput" ExtensionTypes="Ivy.AsyncSelectInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/AsyncSelectInput.cs"/>

## Examples

<Details>
<Summary>
Real-world Search Scenario
</Summary>
<Body>
A comprehensive example showing AsyncSelectInput for user search functionality:

```csharp demo-tabs
public class UserSearchDemo : ViewBase
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
        new User { Id = BobId, Name = "Bob Johnson", Email = "bob@example.com", Department = "Marketing", IsActive = true },
        new User { Id = AliceId, Name = "Alice Brown", Email = "alice@example.com", Department = "Engineering", IsActive = true },
        new User { Id = CharlieId, Name = "Charlie Wilson", Email = "charlie@example.com", Department = "Sales", IsActive = true }
    };

    public override object? Build()
    {
        var selectedUser = this.UseState<Guid>(default(Guid));
        var selectedUserDetails = this.UseState<string>("No user selected");

        // Update display when selection changes
        this.UseEffect(() =>
        {
            var user = Users.FirstOrDefault(u => u.Id == selectedUser.Value);
            selectedUserDetails.Set(user != null ? $"{user.Name} - {user.Email} - {user.Department}" : "No user selected");
        }, [selectedUser]);

        Task<Option<Guid>[]> QueryUsers(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Task.FromResult(Users.Take(5)
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
        }

        Task<Option<Guid>?> LookupUser(Guid id)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user != null 
                ? new Option<Guid>($"{user.Name} ({user.Department})", user.Id, description: user.Email) 
                : null);
        }

        var customAsyncSelect = new AsyncSelectInputView<Guid>(
            selectedUser.Value,
            e => selectedUser.Set(e.Value),
            QueryUsers,
            LookupUser,
            placeholder: "Search users by name, email, or department..."
        );

        return Layout.Vertical()
            | customAsyncSelect
            | Text.Small($"Selected: {selectedUserDetails.Value}");
    }
}
```

</Body>
</Details>

<Callout Type="tip">
Use AsyncSelectInput for foreign key relationships, large datasets, and when you need to provide search functionality. It's perfect for scenarios where the full list of options would be too large to load upfront.
</Callout>
