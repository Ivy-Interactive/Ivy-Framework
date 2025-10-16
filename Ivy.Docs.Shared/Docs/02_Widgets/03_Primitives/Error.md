---
searchHints:
  - exception
  - error
  - failure
  - crash
  - debugging
  - message
---

# Error

<Ingress>
Display error states consistently with standardized messaging, optional details, and recovery options for better user experience.
</Ingress>

The `Error` widget provides a standardized way to display error states in your app. It's designed to communicate that something went wrong and optionally provide details and recovery options.

## Basic Usage

The simplest way to create an Error widget is by passing content directly to the constructor:

```csharp demo-tabs
public class BasicErrorView : ViewBase
{
    public override object? Build()
    {
        return new Error("Connection Failed", "Unable to connect to the server");
    }
}
```

<Callout Type="tip">
Error widgets come with sensible defaults: no title, no message, and no stack trace. You can set these properties individually using the fluent extension methods.
</Callout>

### Title

Set a descriptive title for the error:

```csharp demo-tabs
public class ErrorTitleView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | new Error().Title("Authentication Failed")
            | new Error().Title("Network Error")
            | new Error().Title("Validation Error");
    }
}
```

### Message

Provide a user-friendly error message:

```csharp demo-tabs
public class ErrorMessageView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | new Error()
                .Title("Login Failed")
                .Message("Invalid username or password. Please try again.")
            | new Error()
                .Title("File Upload Error")
                .Message("The file size exceeds the maximum allowed limit of 10MB.")
            | new Error()
                .Title("Permission Denied")
                .Message("You don't have sufficient privileges to access this resource.");
    }
}
```

### Stack Trace

Include technical details for debugging (useful for developers):

```csharp demo-tabs
public class ErrorStackTraceView : ViewBase
{
    public override object? Build()
    {
        var stackTrace = @"at System.Net.Http.HttpClient.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
at MyApp.Services.ApiService.GetDataAsync(String endpoint) in /src/Services/ApiService.cs:line 45
at MyApp.Views.DataView.LoadDataAsync() in /src/Views/DataView.cs:line 23";

        return new Error()
            .Title("API Connection Error")
            .Message("Failed to connect to the external service")
            .StackTrace(stackTrace);
    }
}
```

## Alternative Error Display Methods

While the `Error` widget is excellent for detailed error information, Ivy Framework provides several other ways to display errors depending on your needs:

### Error Callouts

Use `Callout.Error` for prominent error messages that need attention:

```csharp demo-tabs
public class ErrorCalloutExamplesView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | Callout.Error("Failed to connect to the server. Please check your internet connection.")
            | Callout.Error("Invalid email format. Please enter a valid email address.", "Validation Error");
    }
}
```

### Error Toasts

Use `client.Toast` for non-intrusive error notifications:

```csharp demo-tabs
public class ClientErrorExamplesView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        return new Button("Show System Error", variant: ButtonVariant.Destructive)
            .HandleClick(_ => client.Error(new InvalidOperationException("System configuration validation failed")));
    }
}
```

### Text-Based Error Messages

Use `Text.Danger` for inline error text:

```csharp demo-tabs
public class TextErrorExamplesView : ViewBase
{
    public override object? Build()
    {
        return Text.Danger("Invalid email format");
    }
}
```

### Form Validation Errors

Display validation errors using the `Invalid` property on form inputs:

```csharp demo-tabs
public class FormValidationErrorExamplesView : ViewBase
{
    public override object? Build()
    {
        var email = UseState("");
        var password = UseState("");
        var age = UseState(0);
        
        var emailError = UseState<string?>();
        var passwordError = UseState<string?>();
        var ageError = UseState<string?>();
        
        void ValidateForm()
        {
            // Clear previous errors
            emailError.Set((string?)null);
            passwordError.Set((string?)null);
            ageError.Set((string?)null);
            
            var hasErrors = false;
            
            if (string.IsNullOrWhiteSpace(email.Value)) {
                emailError.Set("Email is required");
                hasErrors = true;
            } else if (!email.Value.Contains("@")) {
                emailError.Set("Email must be a valid email address");
                hasErrors = true;
            }
            
            if (string.IsNullOrWhiteSpace(password.Value)) {
                passwordError.Set("Password is required");
                hasErrors = true;
            } else if (password.Value.Length < 8) {
                passwordError.Set("Password must be at least 8 characters long");
                hasErrors = true;
            }
            
            if (age.Value < 18) {
                ageError.Set("You must be at least 18 years old");
                hasErrors = true;
            }
            
            if (!hasErrors) {
                // Form is valid, proceed with submission
                // This would typically call an API or perform an action
            }
        }
        
        return Layout.Vertical().Gap(4)
            | Text.H3("Form Validation Errors")
            | Layout.Vertical().Gap(3)
                | Text.Label("Email")
                | email.ToTextInput()
                    .Placeholder("Enter your email")
                    .Invalid(emailError.Value)
                | Text.Label("Password")
                | password.ToPasswordInput()
                    .Placeholder("Enter your password")
                    .Invalid(passwordError.Value)
                | Text.Label("Age")
                | age.ToNumberInput()
                    .Placeholder("Enter your age")
                    .Invalid(ageError.Value)
            | new Button("Validate Form", variant: ButtonVariant.Primary)
                .HandleClick(ValidateForm)
            | (emailError.Value != null || passwordError.Value != null || ageError.Value != null
                ? Callout.Error("Please fix the validation errors above", "Form Validation Failed")
                : null);
    }
}
```

### Exception Handling

Use the Error widget to display error states:

```csharp demo-tabs
public class ExceptionHandlingView : ViewBase
{
    public override object? Build()
    {
        var showError = UseState(false);
        var showDetails = UseState(false);
        
        void SimulateError()
        {
            showError.Set(true);
        }
        
        return Layout.Vertical().Gap(4)
            | new Button("Simulate Error", variant: ButtonVariant.Destructive).HandleClick(SimulateError)
            | (showError.Value 
                ? Layout.Vertical().Gap(4)
                    | new Error()
                        .Title("Simulated Error")
                        .Message("This is a simulated error for demonstration purposes")
                        .StackTrace(showDetails.Value ? "at MyApp.Views.ErrorView.SimulateError() in /src/Views/ErrorView.cs:line 15" : null)
                    | new Button(showDetails.Value ? "Hide Details" : "Show Details")
                        .Variant(ButtonVariant.Outline)
                        .HandleClick(() => showDetails.Set(!showDetails.Value))
                : Text.Muted("Click the button above to simulate an error"));
    }
}
```

### Effect Error Handling

Demonstrate how effects can handle error states:

```csharp demo-tabs
public class EffectErrorView : ViewBase
{
    public override object? Build()
    {
        var showError = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | new Button("Show Error", variant: ButtonVariant.Primary)
                .HandleClick(_ => showError.Set(true))
            | (showError.Value 
                ? new Error()
                    .Title("Effect Failed")
                    .Message("The effect encountered an error during execution")
                : Text.Muted("Click button to show error"));
    }
}
```

<WidgetDocs Type="Ivy.Error" ExtensionTypes="Ivy.ErrorExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Error.cs"/>

## Examples

<Details>
<Summary>
Data Loading with Error Handling
</Summary>
<Body>
Handle errors gracefully in data loading scenarios:

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
            isLoading.Set(true);
            hasError.Set(false);
            
            // Simulate API call with random chance of failure
            await Task.Delay(1000);
            if (Random.Shared.Next(2) == 0)
            {
                hasError.Set(true);
            }
            else
            {
                data.Set(new List<string> { "Item 1", "Item 2", "Item 3" });
            }
            
            isLoading.Set(false);
        }
        
        // Initial load
        UseEffect(() => {
            _ = LoadData();
        }, []);
        
        return Layout.Vertical().Gap(4)
            | Layout.Horizontal().Gap(2)
                | new Button("Reload Data", variant: ButtonVariant.Primary).HandleClick(async _ => await LoadData())
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

</Body>
</Details>

<Details>
<Summary>
Error with Recovery Actions
</Summary>
<Body>
Combine error display with actionable recovery options:

```csharp demo-tabs
public class ErrorRecoveryExamplesView : ViewBase
{
    public override object? Build()
    {
        var errorState = UseState<Exception?>();
        var recoveryStep = UseState(0);
        
        void SimulateRecoverableError()
        {
            errorState.Set(new InvalidOperationException("File system access denied. Insufficient permissions."));
            recoveryStep.Set(1);
        }
        
        void TryRecovery()
        {
            if (recoveryStep.Value < 3)
            {
                recoveryStep.Set(recoveryStep.Value + 1);
            }
            else
            {
                errorState.Set((Exception?)null);
                recoveryStep.Set(0);
            }
        }
        
        void SkipRecovery()
        {
            errorState.Set((Exception?)null);
            recoveryStep.Set(0);
        }
        
        return Layout.Vertical().Gap(4)
            | (Layout.Horizontal().Gap(2)
                | new Button("Simulate Error", variant: ButtonVariant.Destructive)
                    .HandleClick(SimulateRecoverableError)
                | new Button("Try Recovery", variant: ButtonVariant.Primary)
                    .HandleClick(TryRecovery)
                    .Disabled(errorState.Value == null)
                | new Button("Skip Recovery", variant: ButtonVariant.Outline)
                    .HandleClick(SkipRecovery)
                    .Disabled(errorState.Value == null))
            | (errorState.Value != null 
                ? Layout.Vertical().Gap(3)
                    | new Error()
                        .Title("Recoverable Error")
                        .Message($"Step {recoveryStep.Value}/3: {errorState.Value.Message}")
                    | Callout.Info($"Recovery attempt {recoveryStep.Value} of 3. Click 'Try Recovery' to proceed.")
                    | (recoveryStep.Value >= 3 
                        ? Text.Success("Recovery completed successfully!")
                        : Text.Muted("Continue with recovery steps..."))
                : Text.Muted("Click 'Simulate Error' to start the recovery workflow"));
    }
}
```

</Body>
</Details>
