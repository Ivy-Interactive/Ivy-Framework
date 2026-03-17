# Input Constructors Internal

## API Changes
The public constructors for all `Input` widget classes (e.g., `TextInput`, `SelectInput`, `NumberInput`, `BoolInput`, `ColorInput`, `DateTimeInput`, `DateRangeInput`, `FileInput`, `FeedbackInput`, `IconInput`) have been made `internal`.

## How to Reproduce
Directly instantiating an input widget will now result in a compilation error:
```csharp
var input = new TextInput(state);
// Error CS0122: 'TextInput.TextInput(IAnyState, string?, bool)' is inaccessible due to its protection level
```

## How to Fix
You must use the `To[Type]Input()` extension methods to create input widgets from a state, value, or primitive type.

**Instead of:**
```csharp
var input = new TextInput(state);
var select = new SelectInput<string>(state, options);
var number = new NumberInput();
```

**Use:**
```csharp
var input = state.ToTextInput();
var select = state.ToSelectInput(options);
// For parameterless/stateless components, create a primitive or a state first:
var numberState = UseState(0);
var number = numberState.ToNumberInput();
```

## Verification
Ensure that all instances of `new [Type]Input(...)` are replaced with `.To[Type]Input(...)` calls and that the project compiles successfully.
