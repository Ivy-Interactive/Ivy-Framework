# Json

The `Json` widget displays JSON data in a formatted, syntax-highlighted view. It's useful for debugging, data visualization, and displaying API responses.

## Basic Usage

The simplest way to display JSON data is by passing a serialized string directly to the Json widget.

```csharp demo-tabs 
public class BasicJsonExample : ViewBase
{
    public override object? Build()
    {
        var simpleData = new
        {
            name = "John Doe",
            age = 30,
            isActive = true,
            tags = new[] { "developer", "designer", "architect" }
        };
        
        return Layout.Vertical().Gap(4)
            | new Json(System.Text.Json.JsonSerializer.Serialize(simpleData));
    }
}
```

## API Response Viewer

This example demonstrates how to create an interactive JSON viewer that fetches data from an API and displays it with real-time updates.

```csharp demo-tabs 
public class ApiResponseViewer : ViewBase
{
    public override object? Build()
    {
        var apiResponse = this.UseState<string>("");
        var isLoading = this.UseState(false);
        
        async Task FetchRandomData()
        {
            try
            {
                isLoading.Value = true;
                
                // Generate sample data - in a real app, this would be from an API
                await Task.Delay(500); // Simulate network delay
                
                var sampleData = new
                {
                    id = Guid.NewGuid().ToString(),
                    timestamp = DateTime.Now.ToString("o"),
                    user = new
                    {
                        id = 1234,
                        name = "Jane Doe",
                        email = "jane.doe@example.com",
                        roles = new[] { "admin", "user" }
                    },
                    stats = new
                    {
                        visits = Random.Shared.Next(100, 1000),
                        actions = Random.Shared.Next(10, 100),
                        conversion = Math.Round(Random.Shared.NextDouble(), 2)
                    },
                    settings = new Dictionary<string, object>
                    {
                        ["darkMode"] = Random.Shared.Next(2) == 0,
                        ["notifications"] = true,
                        ["displayDensity"] = "compact"
                    }
                };
                
                apiResponse.Value = System.Text.Json.JsonSerializer.Serialize(sampleData);
            }
            finally
            {
                isLoading.Value = false;
            }
        }
        
        // Fetch data initially
        UseEffect(() => {
            _ = FetchRandomData();
        }, []);
        
        return Layout.Vertical().Gap(4)
            | new Button("Fetch New Data", onClick: _ => Task.Run(FetchRandomData)).Disabled(isLoading.Value)
            | (isLoading.Value
                ? "Loading..."
                : apiResponse.Value != ""
                    ? new Json(apiResponse.Value)
                    : Text.Muted("No data available"));
    }
}
```

## Complex Data Structure

Demonstrates nested objects, arrays, and complex data types with automatic formatting.

```csharp demo-tabs 
public class ComplexJsonExample : ViewBase
{
    public override object? Build()
    {
        var complexData = new
        {
            company = new
            {
                name = "TechCorp Inc.",
                founded = 2010,
                headquarters = new
                {
                    city = "San Francisco",
                    country = "USA",
                    coordinates = new
                    {
                        latitude = 37.7749,
                        longitude = -122.4194
                    }
                }
            },
            employees = new[]
            {
                new
                {
                    id = 1,
                    name = "Alice Johnson",
                    position = "CEO",
                    department = "Executive",
                    skills = new[] { "leadership", "strategy", "management" },
                    contact = new
                    {
                        email = "alice@techcorp.com",
                        phone = "+1-555-0101"
                    }
                },
                new
                {
                    id = 2,
                    name = "Bob Smith",
                    position = "Lead Developer",
                    department = "Engineering",
                    skills = new[] { "C#", "JavaScript", "React", "Azure" },
                    contact = new
                    {
                        email = "bob@techcorp.com",
                        phone = "+1-555-0102"
                    }
                }
            },
            projects = new[]
            {
                new
                {
                    id = "proj-001",
                    name = "E-commerce Platform",
                    status = "In Progress",
                    progress = 75.5,
                    technologies = new[] { "C#", "React", "SQL Server", "Azure" },
                    team = new[] { 1, 2 }
                }
            }
        };
        
        var jsonString = System.Text.Json.JsonSerializer.Serialize(complexData, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        return Layout.Vertical().Gap(4)
            | Text.Muted("Showing nested objects, arrays, and various data types")
            | new Json(jsonString);
    }
}
```

## Error Handling and Validation

It shows error handling patterns and how to integrate the Json widget with other input components for a complete user experience.

```csharp demo-tabs 
public class JsonValidationExample : ViewBase
{
    public override object? Build()
    {
        var jsonInput = this.UseState<string>("");
        var parsedData = this.UseState<string>("");
        var error = this.UseState<string>("");
        
        void ParseJson()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonInput.Value))
                {
                    error.Value = "Please enter some JSON data";
                    parsedData.Value = "";
                    return;
                }
                
                var data = System.Text.Json.JsonSerializer.Deserialize<object>(jsonInput.Value);
                parsedData.Value = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                error.Value = "";
            }
            catch (System.Text.Json.JsonException ex)
            {
                error.Value = $"Invalid JSON: {ex.Message}";
                parsedData.Value = "";
            }
        }
        
        return Layout.Vertical().Gap(4)
            | Text.Muted("Enter JSON data to validate and display")
            | new TextInput(jsonInput, placeholder: "{ \"name\": \"example\", \"value\": 42 }", variant: TextInputs.Textarea)
            | new Button("Parse JSON", onClick: _ => ParseJson())
            | (error.Value != ""
                ? Text.Muted(error.Value)
                : parsedData.Value != ""
                    ? new Json(parsedData.Value)
                    : Text.Muted("Enter valid JSON above to see the result"));
    }
}
```

<WidgetDocs Type="Ivy.Json" ExtensionTypes="Ivy.JsonExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Json.cs"/>
