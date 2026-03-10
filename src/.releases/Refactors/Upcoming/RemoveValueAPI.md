# Removal of `.Value()` API from Input Widgets

As part of Issue #2512, the fluent `.Value<T>()` extension method has been completely removed from all input widgets in the Ivy Framework (e.g., `TextInput`, `SelectInput`, `NumberInput`, etc.).

**Reasoning:**
The `.Value()` method was incorrectly being used to set initial values inline directly on the widget, which conflicts with Ivy's state-driven architecture.

**Migration:**
Initial values should now be strictly managed through the `IState<T>` via the `UseState(initialValue)` hook. They can then be bound to inputs using the respective `.To[InputType]()` methods.

Example (Before):

```csharp
var nameState = UseState("");
var input = new TextInput().Value("John Doe").OnChange(nameState.Set);
```

Example (After):

```csharp
var nameState = UseState("John Doe");
var input = nameState.ToTextInput();
```
