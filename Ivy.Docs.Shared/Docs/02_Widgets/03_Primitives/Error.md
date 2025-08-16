# Error

<Ingress>
Display error states consistently with standardized messaging, optional details, and recovery options for better user experience.
</Ingress>

The `Error` widget provides a standardized way to display error states in your app. It's designed to communicate that something went wrong and optionally provide details and recovery options.

```csharp demo-tabs 
public class DataLoadingView : ViewBase
{
    public override object? Build()
    {
        var isLoading = UseState(false);
        var hasError = UseState(false);
        var data = UseState<List<string>?>();
        
        async Task LoadData()
        {
            try {
                isLoading.Set(true);
                hasError.Set(false);
                
                // Simulate API call with random chance of failure
                await Task.Delay(1000);
                if (Random.Shared.Next(2) == 0)
                    throw new Exception("API connection timeout");
                    
                data.Set(new List<string> { "Item 1", "Item 2", "Item 3" });
            }
            catch {
                hasError.Set(true);
            }
            finally {
                isLoading.Set(false);
            }
        }
        
        // Initial load
        UseEffect(() => {
            _ = LoadData();
        }, []);
        
        return Layout.Vertical()
            | (isLoading.Value 
                ? "Loading..." 
                : hasError.Value 
                    ? new Error()
                        .Title("Failed to load data")
                        .Message("There was a problem connecting to the server")
                    : data.Value != null 
                        ? Layout.Vertical() | Text.H3("Data Items") | string.Join(", ", data.Value)
                        : null);
    }
}
```

<WidgetDocs Type="Ivy.Error" ExtensionTypes="Ivy.ErrorExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Error.cs"/>
