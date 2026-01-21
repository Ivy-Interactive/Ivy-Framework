---
searchHints:
  - hooks
  - useState
  - useEffect
  - useQuery
  - state-management
  - lifecycle
  - reactivity
  - functional-components
---

# Hooks

<Ingress>
Discover the powerful functions that let you "hook into" Ivy state and lifecycle features - Hooks enable you to use state and side effects in your [Views](../../01_Onboarding/02_Concepts/02_Views.md) without writing class components.
</Ingress>

## Basic usage

Ivy provides a comprehensive set of hooks organized into several categories. All hooks follow the naming convention of starting with `Use` followed by an uppercase letter, and they must be called at the top level of your view's `Build` method:

```csharp demo-tabs
public class HooksDemo : ViewBase
{
    public override object? Build()
    {
        var name = UseState("World");
        
        return Layout.Vertical()
            | name.ToTextInput().Placeholder("Enter your name")
            | Text.P($"Hello, {name.Value}!").Large();
    }
}
```

### Hook Library

Ivy ships with a comprehensive set of hooks organized by purpose:

| Category                     | Hooks                                                                                                                                     |
| ---------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| **Core**                     | [UseState](./Core/03_UseState.md), [UseEffect](./Core/04_UseEffect.md), [UseReducer](./Core/07_UseReducer.md)                            |
| **Performance**              | [UseMemo](./Core/05_UseMemo.md), [UseCallback](./Core/06_UseCallback.md)                                                                 |
| **References & Context**     | [UseRef](./Core/08_UseRef.md), [UseContext](./Core/12_UseContext.md), [UseArgs](./Core/13_UseArgs.md)                                     |
| **Data Fetching**            | [UseQuery](./Core/09_UseQuery.md), [UseMutation](./Core/14_UseMutation.md), [UseSignal](./Core/10_UseSignal.md)                           |
| **Services & Dependencies**  | [UseService](./Core/11_UseService.md), [UseRefreshToken](./Core/16_UseRefreshToken.md), [UseWebhook](./Core/19_UseWebhook.md)            |
| **UI & Interaction**         | [UseNavigation](./Core/23_UseNavigation.md), [UseAlert](./Core/24_UseAlert.md), [UseBlades](./Core/21_UseBlades.md), [UseTrigger](./Core/17_UseTrigger.md)  |
| **Forms**                    | [UseForm](./Core/22_UseForm.md)                                                                                                             |
| **Files**                    | [UseUpload](./Core/18_UseUpload.md), [UseDownload](./Core/15_UseDownload.md)                                                              |

## Core Hooks

Core hooks provide the fundamental building blocks for state management and side effects in your views.

```mermaid
flowchart TB
    A[Core Hooks] --> B[State Management]
    A --> C[Side Effects]
    A --> D[Complex State]
    
    B --> B1[UseState]
    C --> C1[UseEffect]
    D --> D1[UseReducer]
```

### UseState

Type-safe state management with `IState<T>`. Automatically triggers re-renders on state changes. Supports any type including primitives, objects, and collections. Supports lazy initialization with factory functions.

```csharp demo-tabs
public class StateDemo : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        var text = UseState("Hello");
        var items = UseState(() => new List<string> { "Item 1", "Item 2" });

        return Layout.Vertical(
            Text.P("State Management Demo").Large(),

            // Number state updates
            Layout.Horizontal(
                new Button($"Count: {count.Value}", _ => count.Set(count.Value + 1)),
                new Button("Reset", _ => count.Set(0))
            ),

            // String state updates
            Layout.Horizontal(
                text.ToTextInput("Enter text"),
                new Button("Clear", _ => text.Set("")),
                new Button("Uppercase", _ => text.Set(text.Value.ToUpper()))
            ),

            // Collection state updates
            Layout.Horizontal(
                new Button("Add Item", _ => {
                    var newItems = new List<string>(items.Value) { $"Item {items.Value.Count + 1}" };
                    items.Set(newItems);
                }),
                new Button("Clear", _ => items.Set(new List<string>()))
            ),

            Text.Literal($"Text: {text.Value}"),
            Text.Literal($"Items: {string.Join(", ", items.Value)}")
        );
    }
}
```

See [UseState](./Core/03_UseState.md) for detailed documentation.

### UseEffect

Runs effects on mount, state changes, or every render. Supports cleanup functions for resource management. Async effect support with `Task.Delay` and dependency tracking for optimal performance.

```csharp demo-tabs
public class EffectDemo : ViewBase
{
    public override object? Build()
    {
        var message = UseState("Initialized");
        var trigger = UseState(0);

        // Effect runs when trigger changes
        UseEffect(async () =>
        {
            message.Set("Loading...");
            await Task.Delay(2000); // Simulate API call
            message.Set("Data loaded!");
        }, trigger);

        return Layout.Vertical()
            | new Button("Reload Data", _ => trigger.Set(trigger.Value + 1))
            | Text.P(message.Value).Large();
    }
}
```

See [UseEffect](./Core/04_UseEffect.md) for detailed documentation.

### UseReducer

Centralized state update logic in a single reducer function. Predictable state transitions with type-safe action dispatching. Ideal for complex state machines and applications with many state transitions.

```csharp demo-tabs
public class BasicReducerDemo : ViewBase
{
    // Reducer function
    private int CounterReducer(int state, string action) => action switch
    {
        "increment" => state + 1,
        "decrement" => state - 1,
        "reset" => 0,
        _ => state
    };

    public override object? Build()
    {
        var (count, dispatch) = this.UseReducer(CounterReducer, 0);

        return Layout.Vertical(
            Text.P($"Count: {count}").Large(),
            Layout.Horizontal(
                new Button("-", _ => dispatch("decrement")),
                new Button("Reset", _ => dispatch("reset")),
                new Button("+", _ => dispatch("increment"))
            )
        );
    }
}
```

See [UseReducer](./Core/07_UseReducer.md) for detailed documentation.

## Performance Hooks

Optimize rendering performance with memoization hooks:

```mermaid
flowchart TB
    A[Performance Hooks] --> B[Memoization]
    
    B --> B1[UseMemo]
    B --> B2[UseCallback]
```

### UseMemo

Recomputes only when dependencies change, reducing unnecessary calculations and improving render performance. Type-safe dependency tracking ensures optimal memoization behavior.

```csharp demo-tabs
public class MemoDemo : ViewBase
{
    public override object? Build()
    {
        var number = UseState(0);
        var renderCount = UseRef(0);
        
        // Increment render counter on every build (UseRef doesn't trigger re-renders)
        renderCount.Value++;
        
        // Memoized expensive calculation - only recomputes when number changes
        var squared = UseMemo(() => 
        {
            // Simulate expensive computation
            var result = number.Value * number.Value;
            return result;
        }, number.Value);
        
        return Layout.Vertical()
            | Text.P("Move the slider - the square only recomputes when the number changes")
            | number.ToNumberInput()
                .Min(0)
                .Max(20)
                .Variant(NumberInputs.Slider)
                .WithField()
                .Label($"Number: {number.Value}")
            | Text.P($"{number.Value}² = {squared}")
            | Text.P($"Component rendered {renderCount.Value} times");
    }
}
```

See [UseMemo](./Core/05_UseMemo.md) for detailed documentation.

### UseCallback

Stable function references prevent unnecessary child component re-renders. Dependency-based memoization optimizes component composition and ensures callbacks have stable references when dependencies haven't changed.

```csharp demo-tabs
public class CallbackDemo : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        var multiplier = UseState(2);
        
        // Memoized callback - only recreates when count changes
        var handleIncrement = UseMemo(() => (Action)(() => 
        {
            count.Set(count.Value + 1);
        }), count);
        
        // Stable callback with no dependencies - never changes
        var handleReset = UseMemo(() => (Action)(() => 
        {
            count.Set(0);
        }));
        
        return Layout.Vertical()
            | Text.P($"Count: {count.Value} × {multiplier.Value} = {count.Value * multiplier.Value}")
            | multiplier.ToNumberInput()
                .Min(1)
                .Max(10)
                .Variant(NumberInputs.Slider)
                .WithField()
                .Label("Multiplier")
            | (Layout.Horizontal()
                | new Button("Increment", _ => handleIncrement())
                | new Button("Reset", _ => handleReset()));
    }
}
```

See [UseCallback](./Core/06_UseCallback.md) for detailed documentation.

## References & Context

```mermaid
flowchart TB
    A[References & Context] --> B[References]
    A --> C[Context]
    A --> D[Navigation]
    
    B --> B1[UseRef]
    C --> C1[UseContext]
    D --> D1[UseArgs]
```

### UseRef

Store mutable values that persist across re-renders without triggering updates. Perfect for storing component instance values, accessing previous values, and imperative API access.

```csharp demo-tabs
public class RefDemo : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        var previousCount = UseRef(() => (int?)null);
        
        // Store previous value before it changes
        var previous = previousCount.Value;
        var delta = previous.HasValue ? count.Value - previous.Value : 0;
        
        // Update ref for next render (doesn't trigger re-render)
        previousCount.Value = count.Value;
        
        return Layout.Vertical()
            | Text.P($"Current: {count.Value}")
            | Text.P($"Previous: {previous?.ToString() ?? "None"}")
            | Text.P($"Change: {delta:+0;-0;+0}")
            | (Layout.Horizontal()
                | new Button("+1", _ => count.Set(count.Value + 1))
                | new Button("Reset", _ => {
                    count.Set(0);
                    previousCount.Value = null;
                })
                | new Button("-1", _ => count.Set(count.Value -1)));
    }
}
```

See [UseRef](./Core/08_UseRef.md) for detailed documentation.

### UseContext

Share values across component trees without prop drilling. Type-safe context access follows a provider/consumer pattern where parent components create context and child components consume it.

```csharp demo-below
public record AppSettings(string Theme, int FontSize);

public class ContextProvider : ViewBase
{
    public override object? Build()
    {
        // Create context for child components - shared without prop drilling
        CreateContext(() => new AppSettings("dark", 14));
        
        return Layout.Vertical()
            | Text.P("Parent Component").Bold()
            | Text.P("Settings configured in context")
            | new Separator()
            | Text.P("Child Component").Bold()
            | new ChildView();
    }
}

public class ChildView : ViewBase
{
    public override object? Build()
    {
        // Access context from parent - no props needed!
        var settings = UseContext<AppSettings>();
        return Layout.Vertical()
            | Text.P($"Theme: {settings.Theme}")
            | Text.P($"Font Size: {settings.FontSize}px");
    }
}
```

See [UseContext](./Core/12_UseContext.md) for detailed documentation.

### UseArgs

Access view arguments passed during navigation with type-safe argument reading. Arguments are automatically serialized and deserialized as JSON. Supports tuple arguments and optional argument handling.

```csharp demo-tabs
public record ArgsDemoArgs(string Message, int Count);

public class ArgsDemo : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<ArgsDemoArgs>();
        
        return Layout.Vertical()
            | Text.P($"Message: {args?.Message ?? "No message"}")
            | Text.P($"Count: {args?.Count ?? 0}");
    }
}
```

See [UseArgs](./Core/13_UseArgs.md) for detailed documentation.

## Data Fetching

```mermaid
flowchart TB
    A[Data Fetching] --> B[Fetching]
    A --> C[Cache Control]
    A --> D[Communication]
    
    B --> B1[UseQuery]
    C --> C1[UseMutation]
    D --> D1[UseSignal]
```

### UseQuery

Automatic caching and revalidation with loading and error states. Background data synchronization keeps your data fresh. Supports optimistic updates and follows an SWR-inspired API pattern.

```csharp demo-tabs
public class QueryDemo : ViewBase
{
    public override object? Build()
    {
        var query = UseQuery(
            key: "user-data",
            fetcher: async ct => {
                await Task.Delay(1000, ct);
                return new { Name = "Alice", Email = "alice@example.com" };
            }
        );
        
        if (query.Loading) return Text.P("Loading...");
        if (query.Error != null) return Text.P($"Error: {query.Error.Message}");
        
        return Layout.Vertical()
            | Text.P($"Name: {query.Value?.Name}")
            | Text.P($"Email: {query.Value?.Email}")
            | new Button("Refetch", _ => query.Mutator.Revalidate());
    }
}
```

See [UseQuery](./Core/09_UseQuery.md) for detailed documentation.

### UseMutation

Control query caches from any component with `Revalidate()` to refresh data and `Invalidate()` to clear cache. Supports optimistic updates and automatic query invalidation with type-safe mutations.

```csharp demo-tabs
public class MutationDemo : ViewBase
{
    public override object? Build()
    {
        // Control a query cache
        var mutator = UseMutation("user-data");
        
        return Layout.Vertical().Gap(2)
            | new Button("Refresh", _ => mutator.Revalidate())
            | new Button("Clear Cache", _ => mutator.Invalidate());
    }
}
```

See [UseMutation](./Core/14_UseMutation.md) for detailed documentation.

### UseSignal

Cross-component communication with event-like behavior. Type-safe signal emission and subscription management enable one-to-many and request-response patterns across your application.

```csharp demo-tabs
public class CounterSignal : AbstractSignal<int, string> { }

public class SignalExample : ViewBase
{
    public override object? Build()
    {
        var signal = CreateSignal<CounterSignal, int, string>();
        var output = UseState("");

        async ValueTask OnClick(Event<Button> _)
        {
            var results = await signal.Send(1);
            output.Set(string.Join(", ", results));
        }

        return Layout.Vertical(
            new Button("Send Signal", OnClick),
            new ChildReceiver(),
            output.Value
        );
    }
}

public class ChildReceiver : ViewBase
{
    public override object? Build()
    {
        var signal = UseSignal<CounterSignal, int, string>();
        var counter = UseState(0);

        UseEffect(() => signal.Receive(input =>
        {
            counter.Set(counter.Value + input);
            return $"Child received: {input}, total: {counter.Value}";
        }));

        return new Card($"Counter: {counter.Value}");
    }
}
```

See [UseSignal](./Core/10_UseSignal.md) for detailed documentation.

## Services & Dependencies

```mermaid
flowchart TB
    A[Services & Dependencies] --> B[Services]
    A --> C[Triggers]
    A --> D[Webhooks]
    
    B --> B1[UseService]
    C --> C1[UseRefreshToken]
    D --> D1[UseWebhook]
```

### UseService

Access any registered service from the dependency injection container with type-safe service resolution. Services have scoped lifetime within the component tree and integrate seamlessly with your DI container.

```csharp demo-tabs
public class ServiceDemo : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var count = UseState(0);
        
        return Layout.Vertical()
            | Text.P($"Count: {count.Value}")
            | new Button("Increment", _ => count.Set(count.Value + 1))
            | new Button("Show Toast", _ => client.Toast($"Count is {count.Value}"));
    }
}
```

See [UseService](./Core/11_UseService.md) for detailed documentation.

### UseRefreshToken

Manually trigger UI updates and effect executions. The refresh token changes on each refresh, triggering dependent effects to run again. Perfect for refresh buttons, manual data reloading, and triggering reactive updates on demand.

```csharp demo-tabs
public class RefreshTokenDemo : ViewBase
{
    public override object? Build()
    {
        var refreshToken = UseRefreshToken();
        var timestamp = UseState(DateTime.Now);
        
        UseEffect(() => {
            timestamp.Set(DateTime.Now);
        }, refreshToken);
        
        return Layout.Vertical()
            | Text.P($"Last refreshed: {timestamp.Value:HH:mm:ss}")
            | new Button("Refresh", _ => refreshToken.Refresh());
    }
}
```

See [UseRefreshToken](./Core/16_UseRefreshToken.md) for detailed documentation.

### UseWebhook

Create HTTP endpoints that external systems can call. The webhook handler receives HTTP requests and can update component state, making it ideal for integrating with third-party services, payment processors, and webhook providers.

```csharp demo-tabs
public class WebhookDemo : ViewBase
{
    public override object? Build()
    {
        var counter = UseState(0);
        var webhook = UseWebhook(_ => {
            counter.Set(counter.Value + 1);
        });
        
        return Layout.Vertical()
            | Text.P($"Webhook called {counter.Value} times")
            | Text.Code(webhook.GetUri().ToString());
    }
}
```

See [UseWebhook](./Core/19_UseWebhook.md) for detailed documentation.

## UI & Interaction

```mermaid
flowchart TB
    A[UI & Interaction] --> B[Navigation]
    A --> C[Dialogs]
    A --> D[Layouts]
    A --> E[Forms]
    
    B --> B1[UseNavigation]
    C --> C1[UseAlert]
    C --> C2[UseTrigger]
    D --> D1[UseBlades]
    E --> E1[UseForm]
```

### UseNavigation

Programmatic navigation between apps using type-safe navigation with app classes or URI-based navigation for dynamic scenarios. Supports navigation arguments and external URL navigation.

```csharp demo-tabs
public class NavigationDemo : ViewBase
{
    public override object? Build()
    {
        var navigation = UseNavigation();
        return new Button("Open External URL", _ => navigation.Navigate("https://docs.ivy.app"));
    }
}
```

See [UseNavigation](./Core/23_UseNavigation.md) for detailed documentation.

### UseAlert

Display modal alert dialogs for confirmations and user feedback. Supports alert dialogs, confirmation dialogs, and input prompts with async dialog handling for user interactions.

```csharp demo-tabs
public class AlertDemo : ViewBase
{
    public override object? Build()
    {
        var (alertView, showAlert) = UseAlert();
        
        return Layout.Vertical().Gap(2)
            | new Button("Show Alert", _ => 
                showAlert("Are you sure you want to continue?", result => {
                    Console.WriteLine($"User selected: {result}");
                }, "Alert Title"))
            | alertView;
    }
}
```

See [UseAlert](./Core/24_UseAlert.md) for detailed documentation.

### UseBlades

Create side panel interfaces with blade navigation. Manage blade stacks with push and pop operations. Context-aware blades share state through the blade service context.

```csharp demo-tabs
public class BladeNavigationDemo : ViewBase
{
    public override object? Build()
    {
        return UseBlades(() => new NavigationRootView(), "Home");
    }
}

public class NavigationRootView : ViewBase
{
    public override object? Build()
    {
        var blades = UseContext<IBladeService>();
        var index = blades.GetIndex(this);

        return Layout.Horizontal().Height(Size.Units(50))
        | (Layout.Vertical()
            | Text.Block($"This is blade level {index}")
            | new Button($"Push Blade {index + 1}", onClick: _ =>
                blades.Push(this, new NavigationRootView(), $"Level {index + 1}"))
            | new Button($"Push Wide Blade", onClick: _ =>
                blades.Push(this, new NavigationRootView(), $"Wide Level {index + 1}", width: Size.Units(100)))
            | (index > 0 ? new Button("Go Back", onClick: _ => blades.Pop()) : null));
    }
}
```

See [UseBlades](./Core/21_UseBlades.md) for detailed documentation.

### UseTrigger

Conditionally render components based on trigger state. Perfect for modals, dialogs, and other conditional UI elements. The hook manages visibility state internally and provides a callback to show/hide components programmatically.

```csharp demo-tabs
public class SimpleTriggerExample : ViewBase
{
    public override object? Build()
    {
        var (triggerView, showTrigger) = UseTrigger((IState<bool> isOpen) =>
            isOpen.Value ? new ModalDialog(isOpen) : null);

        return Layout.Vertical()
            | new Button("Show Modal", onClick: _ => showTrigger())
            | triggerView;
    }
}

public class ModalDialog(IState<bool> isOpen) : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Block("This is a modal dialog")
            | new Button("Close", onClick: _ => isOpen.Set(false));
    }
}
```

See [UseTrigger](./Core/17_UseTrigger.md) for detailed documentation.

### Forms

```mermaid
graph LR
    A[UseForm] --> B[Builder]
    A --> C[Validation]
    A --> D[Submit]
```

### UseForm

Comprehensive form state management with built-in validation. Type-safe form builders enable custom layouts and support loading and error states during form submission.

```csharp demo-tabs
public record UserModel(string Name, string Email, int Age);

public class FormDemo : ViewBase
{
    public override object? Build()
    {
        var user = UseState(() => new UserModel("", "", 25));
        var (onSubmit, formView, validationView, loading) = UseForm(() => user.ToForm()
            .Required(m => m.Name, m => m.Email));
        
        async ValueTask HandleSubmit()
        {
            if (await onSubmit())
            {
                Console.WriteLine($"Submitted: {user.Value.Name}, {user.Value.Email}, {user.Value.Age}");
            }
        }
        return Layout.Vertical().Gap(4)
            | formView
            | validationView
            | new Button("Submit", _ => HandleSubmit())
                .Primary()
                .Disabled(loading)
                .Loading(loading);
    }
}
```

See [UseForm](./Core/22_UseForm.md) for detailed documentation.

### Files

```mermaid
flowchart TB
    A[Files] --> B[File Handling]
    
    B --> B1[UseUpload]
    B --> B2[UseDownload]
```

### UseUpload

File selection and upload with progress tracking. Supports multiple files and provides type-safe upload handlers for processing file streams. Automatically updates state as files are uploaded.

```csharp demo-tabs
public class UploadDemo : ViewBase
{
    public override object? Build()
    {
        var fileState = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(MemoryStreamUploadHandler.Create(fileState));
        
        return Layout.Vertical()
            | fileState.ToFileInput(upload).Placeholder("Choose file...")
            | (fileState.Value != null ? Text.P($"File: {fileState.Value.FileName}") : null);
    }
}
```

See [UseUpload](./Core/18_UseUpload.md) for detailed documentation.

### UseDownload

Generate file downloads on-demand with custom file names and MIME types. Support for various content types with browser download integration. Files are generated dynamically when the download link is accessed.

```csharp demo-tabs
public class DownloadDemo : ViewBase
{
    public override object? Build()
    {
        var content = UseState("Hello, World!");
        var downloadUrl = UseDownload(
            factory: () => System.Text.Encoding.UTF8.GetBytes(content.Value),
            mimeType: "text/plain",
            fileName: "hello.txt"
        );
        
        return Layout.Vertical()
            | (downloadUrl.Value != null 
                ? new Button("Download File").Url(downloadUrl.Value) 
                : Text.P("Preparing download..."));
    }
}
```

See [UseDownload](./Core/15_UseDownload.md) for detailed documentation.

## Best Practices

1. **Call Hooks at Top Level** - Always call hooks at the top level of your `Build` method, never inside loops, conditions, or nested functions. See [Rules of Hooks](./02_RulesOfHooks.md) for details.

2. **Use Appropriate Hooks** - Choose the right hook for your use case:
   - `UseState` for simple local state
   - `UseReducer` for complex state logic
   - `UseQuery` for server data fetching
   - `UseMemo`/`UseCallback` for performance optimization

3. **Handle Loading States** - Always handle loading and error states when using async hooks like `UseQuery` and `UseMutation`.

4. **Clean Up Effects** - Return cleanup functions from `UseEffect` to prevent memory leaks and cancel ongoing operations.

5. **Custom Hooks** - Extract reusable hook logic into custom hooks following the `UseX` naming convention.

## Creating Custom Hooks

You can build your own hooks to reuse stateful logic between components. A custom hook is a function whose name starts with "Use" and that may call other hooks:

```csharp
public static IState<string> UseLocalStorage(string key, string defaultValue)
{
    var state = UseState(defaultValue);
    
    UseEffect(() => {
        var stored = localStorage.GetItem(key);
        if (stored != null) state.Set(stored);
    }, EffectTrigger.OnMount());
    
    UseEffect(() => {
        localStorage.SetItem(key, state.Value);
    }, state);
    
    return state;
}
```

## See Also

- [Rules of Hooks](./02_RulesOfHooks.md) - Essential rules for using hooks correctly
- [Views](../../01_Onboarding/02_Concepts/02_Views.md) - Understanding Ivy Views
- [Forms](../../01_Onboarding/02_Concepts/04_Forms.md) - Working with forms in Ivy
- [UseService](./Core/11_UseService.md) - Dependency injection and services
