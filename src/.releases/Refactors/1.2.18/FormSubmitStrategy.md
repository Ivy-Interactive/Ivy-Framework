# Form Submission Strategy Breaking Changes - v1.2.18

## Summary

In previous versions, a `Form` would implicitly render a "Save" button and only validate and commit state changes when that button was clicked. With version 1.2.18, the framework introduces a `FormSubmitStrategy` enum to explicitly control when a form validates and submits changes to its bounded state. The default is still `OnSubmit`, but if you rely on immediate state updates, this logic has been formalized. Additionally, overriding the submit button and its click handler has changed due to the `EventHandler` wrapper Refactoring.

## What Changed

### Form Submit Strategy

You can now use the `.SubmitStrategy()` fluent method on a `FormBuilder` to dictate how state is updated:

- `FormSubmitStrategy.OnSubmit`: (Default) Renders the submit button. State is validated and updated only when the user clicks the button.
- `FormSubmitStrategy.OnBlur`: The submit button is hidden. State is validated and updated whenever the user tabs away or clicks out of a field.
- `FormSubmitStrategy.OnChange`: The submit button is hidden. State is validated and updated immediately upon every keystroke or selection change.

### The Submit Button and `OnSubmit` Callback

Previously, you might have used `.HandleSubmit()` on the button builder. Now, there is a dedicated `.OnSubmit()` callback directly on the `FormBuilder` to handle async operations, and the click handler for a custom button builder uses `.OnClick()` due to the `EventHandler` update.

#### Before

```csharp
model.ToForm()
    .SubmitBuilder(isLoading => new Button("Apply")
        .Loading(isLoading)
        .HandleClick(_ => { /* custom logic */ return default; }))
```

#### After

```csharp
model.ToForm()
    .SubmitBuilder(isLoading => new Button("Apply").Loading(isLoading))
    .OnSubmit(async (m) => {
        // Runs after successful validation, before state is committed
        await myService.SaveAsync(m);
    })
```

## How to Refactor

1. If you need a form to update state immediately without a "Save" button, chain `.SubmitStrategy(FormSubmitStrategy.OnChange)` or `.SubmitStrategy(FormSubmitStrategy.OnBlur)`.
2. Find uses of `.HandleSubmit()` on the `SubmitBuilder` button and move the logic to the `FormBuilder`'s `.OnSubmit()` method. Change the button's action handler to `.OnClick()` if you still need it for standard buttons, but for Forms, `.OnSubmit(Func<TModel, Task>)` is the correct hook for validation-aware submissions.

## Verification

After applying these changes, run:

```bash
dotnet build
```
