---
searchHints:
  - useloading
  - loading
  - spinner
  - loader
  - progress
  - busy
  - modal
  - cancellation
  - async-work
---

# UseLoading

<Ingress>
Display a modal loading dialog while async work runs. Supports progress reporting, status messages, and cancellation.
</Ingress>

## Overview

The `UseLoading` [hook](../02_RulesOfHooks.md) provides a modal loading overlay for long-running async operations:

- **Modal Dialog** - Blocks interaction while work is in progress
- **Progress Tracking** - Report determinate (0–100%) or indeterminate progress
- **Status Messages** - Update the dialog message and status text during execution
- **Cancellation** - Optionally allow users to cancel the operation
- **Background Execution** - Work runs on a background thread, keeping the UI event queue free

<Callout type="info">
`UseLoading` follows the same pattern as [UseAlert](../../../01_Onboarding/02_Concepts/17_Alerts.md) — it returns a view that must be included in your layout, and a delegate to trigger the loading dialog.
</Callout>

## Basic Usage

`UseLoading` returns a tuple of `(loadingView, showLoading)`. Include `loadingView` in your layout and call `showLoading` to start async work:

```csharp demo-below
public class BasicLoadingExample : ViewBase
{
    public override object? Build()
    {
        var (loadingView, showLoading) = UseLoading();

        return new Fragment(
            loadingView,
            Layout.Vertical()
            | new Button("Start Work", () =>
            {
                showLoading(async ctx =>
                {
                    ctx.Message("Processing…");
                    for (var i = 0; i < 5; i++)
                    {
                        ctx.Progress(i * 100 / 5);
                        ctx.Status($"Step {i + 1} of 5");
                        await Task.Delay(1000);
                    }
                });
            })
        );
    }
}
```

## API Reference

### Return Value

| Value | Type | Description |
|-------|------|-------------|
| `loadingView` | `IView?` | The loading dialog view — must be included in your layout |
| `showLoading` | `ShowLoadingDelegate` | Delegate to start async work with the loading dialog |

### ShowLoadingDelegate

```csharp
public delegate void ShowLoadingDelegate(
    Func<ILoadingContext, Task> work,
    bool cancellable = false,
    LoadingOptions? options = null
);
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `work` | `Func<ILoadingContext, Task>` | — | Async function receiving a loading context |
| `cancellable` | `bool` | `false` | Whether the user can cancel the operation |
| `options` | `LoadingOptions?` | `null` | Optional initial configuration |

### ILoadingContext

The context object passed to your work function:

| Member | Type | Description |
|--------|------|-------------|
| `Message(string)` | `void` | Update the dialog header text |
| `Status(string?)` | `void` | Update the subtitle text below the progress bar |
| `Progress(int?)` | `void` | Set progress (0–100) or `null` for indeterminate |
| `CancellationToken` | `CancellationToken` | Token that is cancelled when the user dismisses the dialog |

### LoadingOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Message` | `string` | `"Loading..."` | Initial dialog header text |
| `Status` | `string?` | `null` | Initial subtitle text |
| `Progress` | `int?` | `null` | Initial progress value |
| `Indeterminate` | `bool` | `true` | Show indeterminate progress bar |
| `Cancellable` | `bool` | `false` | Show close button on the dialog |
| `CancellingDisplayDuration` | `TimeSpan` | `800ms` | How long the "Cancelling…" state is shown before the dialog closes |

## Cancellable Loading

Pass `cancellable: true` and observe `ctx.CancellationToken` to support user-initiated cancellation. When the user clicks the close button or the overlay, the token is cancelled and a brief "Cancelling…" message is displayed before the dialog closes.

```csharp demo-below
public class CancellableLoadingExample : ViewBase
{
    public override object? Build()
    {
        var (loadingView, showLoading) = UseLoading();

        return new Fragment(
            loadingView,
            Layout.Vertical()
            | new Button("Run Cancellable (~12s)", () =>
            {
                showLoading(async ctx =>
                {
                    ctx.Message("Working…");
                    ctx.Status("Close to cancel");
                    for (var i = 0; i < 12; i++)
                    {
                        ctx.CancellationToken.ThrowIfCancellationRequested();
                        ctx.Message($"Step {i + 1} / 12");
                        ctx.Progress(i * 100 / 12);
                        await Task.Delay(1000, ctx.CancellationToken);
                    }
                }, cancellable: true);
            })
        );
    }
}
```

## Custom Options

Use `LoadingOptions` to customize the initial state or the cancelling display duration:

```csharp demo-below
public class CustomLoadingOptionsExample : ViewBase
{
    public override object? Build()
    {
        var (loadingView, showLoading) = UseLoading();

        return new Fragment(
            loadingView,
            Layout.Vertical()
            | new Button("Run with 300ms cancel delay", () =>
            {
                showLoading(
                    async ctx =>
                    {
                        ctx.Message("Working…");
                        ctx.Status("Cancel to see a brief closing phase");
                        for (var i = 0; i < 30; i++)
                        {
                            ctx.CancellationToken.ThrowIfCancellationRequested();
                            ctx.Message($"Step {i + 1} / 30");
                            ctx.Progress(i * 100 / 30);
                            await Task.Delay(1000, ctx.CancellationToken);
                        }
                    },
                    cancellable: true,
                    options: new LoadingOptions
                    {
                        CancellingDisplayDuration = TimeSpan.FromMilliseconds(300)
                    }
                );
            })
        );
    }
}
```

## Non-Cancellable Loading

When `cancellable` is `false` (the default), the dialog has no close button and the overlay/Escape key do not dismiss it. The dialog only closes when the work completes:

```csharp demo-below
public class NonCancellableLoadingExample : ViewBase
{
    public override object? Build()
    {
        var (loadingView, showLoading) = UseLoading();

        return new Fragment(
            loadingView,
            Layout.Vertical()
            | new Button("Run (3s, no cancel)", () =>
            {
                showLoading(async ctx =>
                {
                    ctx.Message("Please wait");
                    ctx.Status("This cannot be cancelled");
                    ctx.Progress(null); // indeterminate
                    await Task.Delay(3000);
                });
            })
        );
    }
}
```

## Troubleshooting

Always include `loadingView` in your component's return value:

```csharp
// ❌ Error: loadingView not rendered — dialog will never appear
public override object? Build()
{
    var (loadingView, showLoading) = UseLoading();
    return new Button("Go", () => showLoading(async ctx =>
    {
        await Task.Delay(2000);
    }));
}

// ✅ Correct: include loadingView via Fragment
public override object? Build()
{
    var (loadingView, showLoading) = UseLoading();
    return new Fragment(
        loadingView,
        new Button("Go", () => showLoading(async ctx =>
        {
            await Task.Delay(2000);
        }))
    );
}
```

## Best Practices

- **Always include `loadingView`** in your layout — without it, the dialog will not appear
- **Use `cancellable: true`** for long-running operations so users are not stuck
- **Always observe `CancellationToken`** — pass it to `Task.Delay`, HTTP calls, and other async APIs, or check `ThrowIfCancellationRequested()` periodically
- **Update progress** with `ctx.Progress(0–100)` to give users a sense of how long the operation will take
- **Use `ctx.Message()` and `ctx.Status()`** to keep users informed about what is happening

## See Also

- [Alerts](../../../01_Onboarding/02_Concepts/17_Alerts.md) - Dialog alerts and toast notifications (similar pattern)
- [UseEffect](./04_UseEffect.md) - Side effects and lifecycle
- [State](./03_UseState.md) - Component state management
- [Rules of Hooks](../02_RulesOfHooks.md) - Understanding hook rules and best practices
- [Views](../../../01_Onboarding/02_Concepts/02_Views.md) - Understanding Ivy views and components

## Examples

<Details>
<Summary>
Multi-Step Data Import
</Summary>
<Body>

```csharp demo-tabs
public class DataImportExample : ViewBase
{
    public override object? Build()
    {
        var (loadingView, showLoading) = UseLoading();

        return new Fragment(
            loadingView,
            Layout.Vertical()
            | Text.H3("Data Import")
            | Text.P("Simulate a multi-step import process with progress reporting.")
            | new Button("Start Import", () =>
            {
                showLoading(async ctx =>
                {
                    var steps = new[] { "Validating data", "Processing records", "Updating indexes", "Finalizing" };
                    for (var i = 0; i < steps.Length; i++)
                    {
                        ctx.CancellationToken.ThrowIfCancellationRequested();
                        ctx.Message(steps[i]);
                        ctx.Status($"Step {i + 1} of {steps.Length}");
                        ctx.Progress(i * 100 / steps.Length);
                        await Task.Delay(2000, ctx.CancellationToken);
                    }
                    ctx.Progress(100);
                    ctx.Message("Complete!");
                    await Task.Delay(500);
                }, cancellable: true);
            })
        );
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Non-Cancellable Save Operation
</Summary>
<Body>

```csharp demo-tabs
public class SaveOperationExample : ViewBase
{
    public override object? Build()
    {
        var (loadingView, showLoading) = UseLoading();

        return new Fragment(
            loadingView,
            Layout.Vertical()
            | Text.H3("Save Changes")
            | Text.P("Non-cancellable save to prevent data corruption.")
            | new Button("Save", () =>
            {
                showLoading(async ctx =>
                {
                    ctx.Message("Saving changes…");
                    ctx.Progress(null);
                    await Task.Delay(3000);
                }, cancellable: false);
            }).Primary()
        );
    }
}
```

</Body>
</Details>
