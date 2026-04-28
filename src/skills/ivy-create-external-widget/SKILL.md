---
name: ivy-create-external-widget
description: >
  Create a new Ivy External Widget backed by React npm packages. Use when the user asks
  to create a widget, external widget, React widget, custom component, or wrap an npm
  package as an Ivy widget. Scaffolds a C# + React project, installs npm dependencies,
  implements the widget class and React component, and verifies the build.
allowed-tools: Bash(dotnet:*) Bash(npm:*) Bash(npx:*) Bash(ivy:*) Read Write Edit Glob Grep
effort: high
---

# ivy-create-external-widget

Create a new Ivy External Widget that wraps React npm packages with C# bindings.

## Reference Files

Before implementing, fetch the external widgets documentation using `ivy docs` with the query: "external widgets".

## Workflow

1. **Understand the widget purpose** -- Ask the user what widget they need if it is not already clear from context.

2. **Research npm packages** -- Search the web for the best React npm package(s) for the use case. Packages MUST be open-source with a copyleft-compatible license (MIT, Apache-2.0, BSD, ISC, MPL-2.0, etc.). Look for packages that are well-maintained, popular, and have good TypeScript support.

3. **Present options** -- Show the user 2-4 npm package options. Include the package name, description, license, and weekly downloads if available. Ask the user to pick one.

4. **Ask for a widget name** -- Suggest a name similar to the chosen React package but without the word "React" in it. Names must be in PascalCase. Ask the user to confirm.

5. **Determine the namespace** -- If the working directory already has a `.csproj` file, extract the namespace from it. Otherwise, suggest `Ivy.Widgets.<WidgetName>` as a default namespace and ask the user to confirm.

6. **Scaffold the widget project** -- Create the widget project structure with:
   - `[WidgetName].cs` -- the C# widget class
   - `frontend/` -- the React frontend project

   If the project already has a Widgets folder, move the new widget there and adjust the namespace accordingly.

7. **Install npm dependencies** -- Run in the `frontend/` directory:
   ```
   cd [WorkingDirectory]/frontend && npm install [NpmPackages]
   ```

8. **Implement the widget** -- Follow the Ivy External Widget Guide below to create:
   - The C# widget class (`[WidgetName].cs`) -- define props, events, and extension methods that map to the npm package's API
   - The React component (`frontend/src/[WidgetName].tsx`) -- wrap the npm package component with Ivy's event system, using the conditional event firing pattern
   - The export file (`frontend/src/index.ts`) -- export the new component
   - A sample app (`.samples/Apps/[WidgetName]App.cs`)
   - Update README.md to document the widget, its props, events, and which npm packages it uses

9. **Verify the build** -- Run `dotnet build` to make sure the project compiles. Fix any errors.

---

# Ivy External Widget Guide

## Architecture

- **C# Widget Class**: Defines props, events, and the widget's API surface
- **React Component**: The frontend implementation loaded at runtime
- **IIFE Bundle**: The React component is bundled as an IIFE and exposed on `window.{GlobalName}`

## C# Widget Definition

Create a `.cs` file with a record that inherits from `WidgetBase<T>`:

```csharp
using Ivy;
using Ivy.Core;
using Ivy.Core.ExternalWidgets;

namespace MyNamespace;

[ExternalWidget("frontend/dist/{GlobalName}.js", ExportName = "MyWidget")]
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

## React Component

Create a `.tsx` file in `frontend/src/`:

```tsx
import React from 'react';
import { IvyEventHandler } from './types';
import { getWidth, getHeight } from './styles';

interface MyWidgetProps {
  id: string;
  width?: string;
  height?: string;
  events?: string[];
  onIvyEvent: IvyEventHandler;
  title?: string;
  count?: number;
  disabled?: boolean;
}

export const MyWidget: React.FC<MyWidgetProps> = ({
  id,
  width = 'Full',
  height = 'Full',
  events = [],
  onIvyEvent,
  title,
  count,
  disabled,
}) => {
  const handleClick = () => {
    if (events.includes('OnClick')) {
      onIvyEvent('OnClick', id, []);
    }
  };

  const handleChange = (value: string) => {
    if (events.includes('OnChange')) {
      onIvyEvent('OnChange', id, [value]);
    }
  };

  const style: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div style={style}>
      <h3>{title}</h3>
      <p>Count: {count}</p>
      <button onClick={handleClick} disabled={disabled}>Click me</button>
      <input onChange={(e) => handleChange(e.target.value)} />
    </div>
  );
};
```

## Export the Component

Add the export to `frontend/src/index.ts`:

```typescript
import { MyWidget } from './MyWidget';

if (typeof window !== 'undefined') {
  (window as Record<string, unknown>).{GlobalName} = {
    MyWidget,
  };
}

export { MyWidget };
```

## Sample App

The `.samples/` folder contains a runnable Ivy project. Create or update the app in `.samples/Apps/[WidgetName]App.cs`:

```csharp
using Ivy;
using [Namespace];

namespace [Namespace].Samples.Apps;

[App]
public class MyWidgetApp : ViewBase
{
    public override object Build() =>
        new MyWidget();
}
```

## Key Concepts

### ExternalWidget Attribute

```csharp
[ExternalWidget("frontend/dist/{GlobalName}.js", ExportName = "MyWidget")]
```

- First parameter: Path to the bundled JS file (embedded resource)
- `ExportName`: The name of the React component in the bundle's exports

### Props vs Events

- `[Prop]`: Data passed from C# to React. PascalCase in C#, auto-converted to camelCase in React.
- `[Event]`: Callbacks from React to C#. Name with `On` prefix (e.g., `OnClick`, `OnChange`).

### Event Handler Signature

In React, call `onIvyEvent(eventName, widgetId, args)`:
- `eventName`: PascalCase with 'On' prefix (e.g., 'OnClick', 'OnChange')
- `widgetId`: Always pass `id`
- `args`: Array of arguments to pass to the C# handler

### Conditional Event Firing

Always check the `events` array before firing:

```tsx
const handleClick = () => {
  if (events.includes('OnClick')) {
    onIvyEvent('OnClick', id, []);
  }
};
```

### Event Types

- `Event<TWidget>` -- event with no value. Properties: `.Id` (the widget ID)
- `Event<TWidget, TValue>` -- event with a value. Properties: `.Id` (the widget ID), `.Value` (the value of type `TValue` passed from React)

Use `Event<TWidget>` for simple notifications (e.g., button clicks) and `Event<TWidget, TValue>` when React needs to pass data back to C# (e.g., input changes, selections).

### Key Namespaces

- `Ivy` -- `WidgetBase<T>`, `ViewBase`, `Layout`, `Text`, `Icons`, `Size`, `Colors`, `ChromeSettings`
- `Ivy.Core` -- `PropAttribute`, `EventAttribute`, `Event<TWidget>`, `Event<TWidget, TValue>`
- `Ivy.Core.ExternalWidgets` -- `ExternalWidgetAttribute`

### Size Props

Use helper functions for sizing:

```typescript
import { getWidth, getHeight } from './styles';

const style: React.CSSProperties = {
  ...getWidth(width),
  ...getHeight(height),
};
```

## Smooth Input Handling Pattern

For input widgets, use local state with server sync to prevent laggy typing:

```tsx
const [localValue, setLocalValue] = useState(value || '');
const [isFocused, setIsFocused] = useState(false);
const localValueRef = useRef(localValue);

useEffect(() => {
  if (!isFocused && value !== localValueRef.current) {
    queueMicrotask(() => setLocalValue(value || ''));
  }
}, [value, isFocused]);

useEffect(() => {
  localValueRef.current = localValue;
}, [localValue]);

const handleChange = useCallback((newValue: string) => {
  setLocalValue(newValue);
  if (events.includes('OnChange') && onIvyEvent) {
    onIvyEvent('OnChange', id, [newValue]);
  }
}, [events, onIvyEvent, id]);
```

## Important Guidelines

- Never use `<summary>` XML documentation blocks
- Update README.md when making changes
- Create app files for each widget in `.samples/Apps/[WidgetName]App.cs`
- Use react-icons/lucide for icons
- Prefer shadcn building blocks when UI components are needed
- Default to full width and height (`Width = Size.Full()`, `Height = Size.Full()`)
- Always include a default constructor (use `internal`)
- Default prop values must be specified both in C# and React
