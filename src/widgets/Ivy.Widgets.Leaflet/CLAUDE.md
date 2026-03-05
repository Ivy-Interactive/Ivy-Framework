# Ivy.Widgets.Leaflet - Ivy External Widget

## Overview

This project contains custom Ivy external widgets. External widgets allow you to create React components that integrate seamlessly with the Ivy framework.

## Architecture

- **C# Widget Class**: Defines props, events, and the widget's API surface
- **React Component**: The frontend implementation loaded at runtime
- **IIFE Bundle**: The React component is bundled as an IIFE and exposed on `window.Ivy_Widgets_Leaflet`

## Creating a New Widget

### 1. C# Widget Definition

Create a new `.cs` file with a record that inherits from `WidgetBase<T>`:

```csharp
using Ivy;
using Ivy.Core;
using Ivy.Core.ExternalWidgets;

namespace Ivy.Widgets.Leaflet;

[ExternalWidget("frontend/dist/Ivy_Widgets_Leaflet.js", ExportName = "MyWidget")]
public record MyWidget : WidgetBase<MyWidget>
{
    // Props - these are passed to the React component
    [Prop] public string? Title { get; set; }
    [Prop] public int Count { get; set; }
    [Prop] public bool Disabled { get; set; }

    // Events - these can be triggered from React
    [Event] public Func<Event<MyWidget>, ValueTask>? OnClick { get; set; }
    [Event] public Func<Event<MyWidget, string>, ValueTask>? OnChange { get; set; }
}

// Extension methods for fluent API
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
import { IvyEventHandler } from './types';
import { getWidth, getHeight } from './styles';

interface MyWidgetProps {
  // Required Ivy props (always include these)
  id: string;
  width?: string;
  height?: string;
  onIvyEvent: IvyEventHandler;

  // Your custom props (match the C# [Prop] attributes)
  title?: string;
  count?: number;
  disabled?: boolean;
}

export const MyWidget: React.FC<MyWidgetProps> = ({
  id,
  width,
  height,
  onIvyEvent,
  title,
  count,
  disabled,
}) => {
  // Call back to C# via the event handler
  const handleClick = () => {
    onIvyEvent('onClick', id, []);
  };

  const handleChange = (value: string) => {
    onIvyEvent('onChange', id, [value]);
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
  (window as unknown as Record<string, unknown>).Ivy_Widgets_Leaflet = {
    // Add all your widgets here
    Map,
    MyWidget,
  };
}

export { Map, MyWidget };
```

### 4. Create a Sample File

Create a `.samples/{WidgetName}.cs` file to demonstrate the widget:

```csharp
#:project ..\Ivy.Widgets.Leaflet.csproj

using Ivy;
using Ivy.Shared;
using Ivy.Widgets.Leaflet;

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

Run the sample with: `dotnet run MyWidget.cs`

## Key Concepts

### ExternalWidget Attribute

```csharp
[ExternalWidget("frontend/dist/Ivy_Widgets_Leaflet.js", ExportName = "MyWidget")]
```

- First parameter: Path to the bundled JS file (embedded resource)
- `ExportName`: The name of the React component in the bundle's exports

### Props vs Events

- `[Prop]`: Data passed from C# to React. Use C# property naming (PascalCase), they're automatically converted to camelCase in React.
- `[Event]`: Callbacks triggered from React to C#. Name them with `On` prefix (e.g., `OnClick`, `OnChange`).

### Event Handler Signature

In React, call `onIvyEvent(eventName, widgetId, args)`:
- `eventName`: The event name without 'On' prefix, in camelCase (e.g., 'onClick' for `OnClick`)
- `widgetId`: Always pass `id`
- `args`: Array of arguments to pass to the C# handler

### Size Props

Ivy passes size information via `width` and `height`. Use the helper functions:

```typescript
import { getWidth, getHeight } from './styles';

const style: React.CSSProperties = {
  ...getWidth(width),
  ...getHeight(height),
};
```

## Building

```bash
# Install dependencies
cd frontend && npm install

# Build the frontend (creates dist/Ivy_Widgets_Leaflet.js)
npm run build

# Build the C# project (embeds the frontend bundle)
cd .. && dotnet build
```

## Using the Widget in an Ivy App

```csharp
using Ivy.Widgets.Leaflet;

public class MyApp : AppBase
{
    protected override void Build()
    {
        new Map()
            .Label("Hello World")
            .OnClick(() => Console.WriteLine("Clicked!"));
    }
}
```
