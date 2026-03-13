# Ivy.Widgets.Xterm - Ivy External Widget

## Overview

This project contains custom Ivy external widgets. External widgets allow you to create React components that integrate seamlessly with the Ivy framework.

## Architecture

- **C# Widget Class**: Defines props, events, and the widget's API surface
- **React Component**: The frontend implementation loaded at runtime
- **IIFE Bundle**: The React component is bundled as an IIFE and exposed on `window.Ivy_Widgets_Xterm`

## Creating a New Widget

### 1. C# Widget Definition

Create a new `.cs` file with a record that inherits from `WidgetBase<T>`:

```csharp
using Ivy;
using Ivy.Core;

namespace Ivy.Widgets.Xterm;

[ExternalWidget("frontend/dist/Ivy_Widgets_Xterm.js", ExportName = "MyWidget")]
public record MyWidget : WidgetBase<MyWidget>
{
    internal MyWidget() 
    { 
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public string? Title { get; set; }
    [Prop] public int Count { get; set; }
    [Prop] public bool Disabled { get; set; }

    [Event] public Func<Event<MyWidget>, ValueTask>? OnClick { get; set; }
    [Event] public Func<Event<MyWidget, string>, ValueTask>? OnChange { get; set; }
}

public static class MyWidgetExtensions
{
    public static MyWidget Title(this MyWidget widget, string title) =>
        widget with { Title = title };

    public static MyWidget Count(this MyWidget widget, int count) =>
        widget with { Count = count };

    public static MyWidget HandleClick(this MyWidget widget, Action handler) =>
        widget with { OnClick = _ => { handler(); return ValueTask.CompletedTask; } };

    public static MyWidget HandleChange(this MyWidget widget, Action<string> handler) =>
        widget with { OnChange = e => { handler(e.Value); return ValueTask.CompletedTask; } };
}
```

### 2. React Component

Create a corresponding `.tsx` file in `frontend/src/`:

```tsx
import React from 'react';
import { EventHandler } from './types';
import { getWidth, getHeight } from './styles';

interface MyWidgetProps {
  id: string;
  width?: string;
  height?: string;
  eventHandler: EventHandler;
  title?: string;
  count?: number;
  disabled?: boolean;
}

export const MyWidget: React.FC<MyWidgetProps> = ({
  id,
  width = 'Full',
  height = 'Full',
  eventHandler,
  title,
  count,
  disabled,
}) => {
  const handleClick = () => {
    eventHandler('onClick', id, []);
  };

  const handleChange = (value: string) => {
    eventHandler('onChange', id, [value]);
  };

  const style: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div style={style}>
      <h3>{title}</h3>
      <p>Count: {count}</p>
      <button onClick={handleClick} disabled={disabled}>
        Click me
      </button>
      <input onChange={(e) => handleChange(e.target.value)} />
    </div>
  );
};
```

### 3. Export the Component

Add the export to `frontend/src/index.ts`:

```typescript
import { MyWidget } from './MyWidget';

if (typeof window !== 'undefined') {
  (window as Record<string, unknown>).Ivy_Widgets_Xterm = {
    Terminal,
    MyWidget,
  };
}

export { Terminal, MyWidget };
```

### 4. Create a Sample Project

The `.samples/` folder contains a full Ivy project that demonstrates the widget. Update `.samples/Program.cs` to add a view for the new widget:

```csharp
using Ivy;
using Ivy.Widgets.Xterm;

var server = new Server();
server.AddApp<MyWidgetView>();
await server.RunAsync();

[App]
class MyWidgetView : ViewBase
{
    public override object Build() =>
        new MyWidget();
}
```

The `.samples/Program.csproj` references the widget project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Ivy.Widgets.Xterm.csproj" />
  </ItemGroup>
</Project>
```

Run the sample with: `cd .samples && dotnet run`

### 5. Update README.md

Document the new widget in README.md including:
- Widget name and purpose
- Basic usage example
- Available props and events
- External React libraries used (e.g., leaflet, chart.js, etc.)

## Key Concepts

### ExternalWidget Attribute

```csharp
[ExternalWidget("frontend/dist/Ivy_Widgets_Xterm.js", ExportName = "MyWidget")]
```

- First parameter: Path to the bundled JS file (embedded resource)
- `ExportName`: The name of the React component in the bundle's exports

### Props vs Events

- `[Prop]`: Data passed from C# to React. Use C# property naming (PascalCase), they're automatically converted to camelCase in React.
- `[Event]`: Callbacks triggered from React to C#. Name them with `On` prefix (e.g., `OnClick`, `OnChange`).

### Event Handler Signature

In React, call `eventHandler(eventName, widgetId, args)`:
- `eventName`: The event name in PascalCase with 'On' prefix (e.g., 'OnClick', 'OnChange')
- `widgetId`: Always pass `id`
- `args`: Array of arguments to pass to the C# handler

### Conditional Event Firing

Ivy passes an `events` array prop containing the names of registered event handlers. **Always check if an event is registered before firing it** to avoid unnecessary backend calls:

```tsx
interface MyWidgetProps {
  id: string;
  events?: string[];
  eventHandler: EventHandler;
  // ... other props
}

export const MyWidget: React.FC<MyWidgetProps> = ({
  id,
  events = [],
  eventHandler,
}) => {
  const handleClick = () => {
    // Only fire if the event handler is registered in C#
    if (events.includes('OnClick')) {
      eventHandler('OnClick', id, []);
    }
  };

  const handleChange = (value: string) => {
    if (events.includes('OnChange')) {
      eventHandler('OnChange', id, [value]);
    }
  };

  return (
    <button onClick={handleClick}>Click me</button>
  );
};
```

This pattern:
- Reduces unnecessary network traffic to the backend
- Prevents errors when events aren't handled
- Matches how the main Ivy frontend widgets work

### Size Props

Ivy passes size information via `width` and `height`. Use the helper functions:

```typescript
import { getWidth, getHeight } from './styles';

const style: React.CSSProperties = {
  ...getWidth(width),
  ...getHeight(height),
};
```

## Creating Input Widgets

Input widgets follow a specific inheritance pattern that enables state binding. Here's the structure:

### 1. Interface Hierarchy

```csharp
public interface IAnyBoolInput : IAnyInput
{
    bool? Value { get; }
}
```

The interface extends `IAnyInput` and defines the value type for reading.

### 2. Abstract Base Class

```csharp
public abstract record BoolInputBase : WidgetBase<BoolInputBase>, IAnyBoolInput
{
    internal BoolInputBase()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public string? Label { get; set; }
    [Prop] public bool Disabled { get; set; }

    // Common props shared by all bool inputs
}
```

The base class implements the interface and defines common props that all variants share.

### 3. Generic Typed Class

```csharp
public record BoolInput<TBool> : BoolInputBase, IInput<TBool>
{
    internal BoolInput() { }

    [Prop] public TBool? Value { get; set; }
    [Event] public Func<Event<BoolInput<TBool>, TBool>, ValueTask>? OnChange { get; set; }

    bool? IAnyBoolInput.Value => Value is bool b ? b : null;
}
```

The generic class adds the typed `Value` and `OnChange` event. It implements `IInput<TBool>` for state binding.

### 5. State Extension Method

```csharp
public static class BoolInputExtensions
{
    public static BoolInput<TValue> ToBoolInput<TValue>(this IAnyState<TValue> state)
        where TValue : struct
    {
        var input = new BoolInput<TValue> { Value = state.Value };
        input.OnChange = e =>
        {
            state.Set(e.Value);
            return ValueTask.CompletedTask;
        };
        return input;
    }
}
```

The extension method creates an input from any `IAnyState<T>`, automatically binding the state's value and wiring up the `OnChange` event to update the state.

## Smooth Input Handling Pattern

When building input widgets, Ivy sends `OnChange` events to the backend which modifies state and resyncs with the frontend. To prevent laggy typing and cursor jumping, use the **local state with server sync** pattern:

### The Pattern

```tsx
import React, { useState, useEffect, useRef, useCallback } from 'react';

export const MyInput: React.FC<MyInputProps> = ({
  id,
  value = '',
  events = [],
  eventHandler,
}) => {
  // 1. Local state for immediate UI updates
  const [localValue, setLocalValue] = useState(value || '');
  const [isFocused, setIsFocused] = useState(false);
  const localValueRef = useRef(localValue);

  // 2. Sync server value to local only when NOT focused
  useEffect(() => {
    if (!isFocused && value !== localValueRef.current) {
      queueMicrotask(() => setLocalValue(value || ''));
    }
  }, [value, isFocused]);

  // 3. Keep ref in sync with state
  useEffect(() => {
    localValueRef.current = localValue;
  }, [localValue]);

  // 4. Update local immediately AND notify server
  const handleChange = useCallback((newValue: string) => {
    setLocalValue(newValue);
    if (events.includes('OnChange') && eventHandler) {
      eventHandler('OnChange', id, [newValue]);
    }
  }, [events, eventHandler, id]);

  const handleFocus = useCallback(() => {
    setIsFocused(true);
    if (events.includes('OnFocus') && eventHandler) {
      eventHandler('OnFocus', id, []);
    }
  }, [events, eventHandler, id]);

  const handleBlur = useCallback(() => {
    setIsFocused(false);
    if (events.includes('OnBlur') && eventHandler) {
      eventHandler('OnBlur', id, []);
    }
  }, [events, eventHandler, id]);

  // 5. Use localValue for display
  return (
    <input
      value={localValue}
      onChange={(e) => handleChange(e.target.value)}
      onFocus={handleFocus}
      onBlur={handleBlur}
    />
  );
};
```

### Key Concepts

1. **`localValue` state**: Separate from the server `value` prop, used for immediate UI updates
2. **`isFocused` state**: Tracks whether user is actively editing
3. **`localValueRef`**: Ref to compare against server value without triggering re-renders
4. **Server sync only when unfocused**: Prevents server response from overwriting user input mid-typing
5. **`queueMicrotask`**: Batches the state update for performance

### For Rich Text Editors (Tiptap, etc.)

When the editor manages its own internal state, sync `localValue` to the editor:

```tsx
// In onUpdate callback, update localValue
onUpdate: ({ editor }) => {
  const html = editor.getHTML();
  setLocalValue(html);
  if (events.includes('OnChange') && eventHandler) {
    eventHandler('OnChange', id, [html]);
  }
},

// Sync localValue changes to editor (from server sync)
useEffect(() => {
  if (editor && localValue !== editor.getHTML()) {
    editor.commands.setContent(localValue);
  }
}, [editor, localValue]);
```

## Important Guidelines

- **Never use `<summary>` XML documentation blocks** - keep code clean without XML docs
- **Update README.md** when making changes to document how to use widgets and list external React libraries used
- **Create sample files** for each widget in `.samples/{WidgetName}.cs`
- **Use react-icons/lucide** for icons in React components
- **Prefer shadcn building blocks** when UI components are needed
- **Default to full width and height** - most widgets should use `Width = Size.Full()` and `Height = Size.Full()` as defaults (set in internal constructor)
- **Always include a default constructor** - use `internal` if other public constructors exist
- **Default prop values must be specified both in C# and React** as default values are not serialized.