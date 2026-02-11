---
searchHints:
  - external widget
  - custom widget
  - React component
  - Vite
  - IIFE
  - embedded resource
---

# External Widgets

<Ingress>
External widgets let you extend the Ivy Framework with custom React components built and bundled separately from the core framework. Use them for domain-specific UI (e.g. diagrams, charts, or rich editors) without coupling that code to the framework backend.
</Ingress>

The pattern has three parts: a **C# proxy** (a [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) record with `[ExternalWidget]`), a **React component** (your UI), and a **build pipeline** that compiles the frontend and embeds the assets into the assembly.

## Architecture Overview

1. **C# proxy** — A record inheriting from `WidgetBase<T>` with `[ExternalWidget]`, defining [props](../../01_Onboarding/02_Concepts/03_Widgets.md) and [events](../../01_Onboarding/02_Concepts/07_EventHandlers.md).
2. **React component** — The actual UI, built with standard React and tooling (e.g. Vite).
3. **Build pipeline** — MSBuild runs the frontend build and embeds the output (JS/CSS) as resources in the widget assembly.

The host app loads the script and CSS from embedded resources and renders your component, passing props and wiring events back to C#.

## Scaffolding with the CLI

You can generate a new external widget with the Ivy CLI so namespace, names, and build match the framework:

```terminal
ivy widget
Namespace: ExternalWidget
Widget: MyWidget
```

## C# Backend

Create a record that inherits from `WidgetBase<T>` and mark it with `[ExternalWidget]`. The attribute tells the framework where to find the bundled script and (optionally) CSS, and which export/global name to use.

```csharp
using Ivy.Core;
using Ivy.Core.ExternalWidgets;
using Ivy.Shared;

namespace MyProject.Widgets;

[ExternalWidget(
    "frontend/dist/ExternalWidget.js",
    StylePath = "frontend/dist/style.css",
    ExportName = "MyWidget",
    GlobalName = "MyProject_Widgets_MyWidget")]
public record MyWidget : WidgetBase<MyWidget>
{
    public MyWidget(string? label = null)
    {
        Label = label;
    }

    internal MyWidget() { }

    [Prop] public string? Label { get; set; }

    [Event] public Func<Event<MyWidget>, ValueTask>? OnClick { get; set; }
}
```

- **Script path** — Path to the JS file relative to the project (and to embedded resources). Often `frontend/dist/...`.
- **StylePath** — Optional path to a CSS file. If omitted, include styles in the JS bundle.
- **ExportName** — Name of the React component export the loader should use.
- **GlobalName** — Must match the Vite library `name` (and the global variable the IIFE assigns). Use the full namespace with dots replaced by underscores (e.g. `MyProject_Widgets_MyWidget`).

Use `[Prop]` for data and `[Event]` for callbacks. You can add extension methods for a fluent API (e.g. `.Label("...")`, `.HandleClick(...)`).

## Frontend (Vite Library)

The frontend should be a separate project (e.g. a `frontend/` folder) set up as a **library** build.

### Vite configuration

Build an IIFE so the host can load one script and get a global. The `name` in `build.lib` must match `GlobalName` in C#.

```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
  plugins: [
    react({
      jsxRuntime: 'classic', // Use global React; required for Ivy host
    }),
  ],
  build: {
    lib: {
      entry: resolve(__dirname, 'src/index.ts'),
      name: 'MyProject_Widgets_MyWidget',
      fileName: () => 'ExternalWidget.js',
      formats: ['iife'],
    },
    rollupOptions: {
      external: ['react', 'react-dom'],
      output: {
        globals: {
          react: 'React',
          'react-dom': 'ReactDOM',
        },
        extend: false,
      },
    },
  },
});
```

Using `fileName: () => 'ExternalWidget.js'` avoids Vite adding suffixes like `.iife.js`, so the path matches what you put in `[ExternalWidget]`.

### package.json

Keep React on the host: use `peerDependencies` (and `devDependencies` for build/IDE). Do not put `react`/`react-dom` in `dependencies`.

```json
{
  "peerDependencies": {
    "react": "^18.0.0 || ^19.0.0",
    "react-dom": "^18.0.0 || ^19.0.0"
  },
  "devDependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "@vitejs/plugin-react": "^4.3.0",
    "vite": "^5.0.0"
  }
}
```

### Entry point

Export your component and assign it to `window` under the same name as `build.lib.name` so the IIFE loader can find it.

```typescript
// src/index.ts
import './style.css';
import { MyWidget } from './MyWidget';

if (typeof window !== 'undefined') {
  (window as unknown as Record<string, unknown>).MyProject_Widgets_MyWidget = {
    MyWidget,
  };
}

export { MyWidget };
```

### React component

Ivy passes props (including `id`, `width`, `height`, `onIvyEvent`, `events`) and optional custom props (e.g. `label`). Use `onIvyEvent(eventName, widgetId, args)` to fire events back to C#.

```typescript
// src/MyWidget.tsx
import React from 'react';

interface MyWidgetProps {
  id: string;
  width?: string;
  height?: string;
  onIvyEvent: (eventName: string, widgetId: string, args: unknown[]) => void;
  events?: string[];
  label?: string;
}

export const MyWidget: React.FC<MyWidgetProps> = ({
  id,
  onIvyEvent,
  events = [],
  label,
}) => {
  const handleClick = () => {
    if (events?.includes('OnClick')) {
      onIvyEvent('OnClick', id, []);
    }
  };

  return (
    <div className="p-4 border rounded-lg bg-[var(--background)] text-[var(--foreground)] border-[var(--border)]">
      <button
        onClick={handleClick}
        className="px-4 py-2 rounded bg-[var(--primary)] text-white hover:opacity-90"
      >
        {label ?? 'Click me'}
      </button>
    </div>
  );
};
```

Use Ivy theme variables (`--primary`, `--background`, `--foreground`, `--border`, etc.) so the widget matches the host app. Size props use Ivy’s format (e.g. `Full`, `Units:80`); you can parse them in the component or in a small helper.

## Project structure and build

### Standalone widget project

For a reusable widget (e.g. NuGet or shared repo), use a dedicated project and folder **outside** any host app directory so the host does not compile the widget’s sources.

Typical layout:

```text
MyWidget/
├── MyWidget.cs
├── MyWidget.csproj
└── frontend/
    ├── package.json
    ├── vite.config.ts
    ├── tsconfig.json
    └── src/
        ├── index.ts
        ├── MyWidget.tsx
        └── style.css
```

In the `.csproj`:

- Embed the built assets.
- Run the frontend build before the C# build.

```xml
<ItemGroup>
  <EmbeddedResource Include="frontend/dist/**/*" />
</ItemGroup>

<Target Name="BuildFrontend" BeforeTargets="Build" Condition="Exists('frontend/package.json')">
  <Exec Command="npm install" WorkingDirectory="frontend" />
  <Exec Command="npm run build" WorkingDirectory="frontend" />
</Target>

<ItemGroup>
  <ProjectReference Include="..\..\Ivy\Ivy.csproj" />
</ItemGroup>
```

Use **forward slashes** in paths (`frontend/dist/**/*`) for cross-platform builds.

### Integrated pattern (inside host app)

When the widget lives inside the host (e.g. `HostApp/Widgets/MyWidget/`), the host must **not** compile the widget’s C# or include its sources. Exclude the widget folder so only the widget project builds it:

```xml
<!-- HostApp.csproj -->
<PropertyGroup>
  <DefaultItemExcludes>$(DefaultItemExcludes);Widgets/MyWidget/**</DefaultItemExcludes>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="Widgets/MyWidget/MyWidget.csproj" />
</ItemGroup>
```

Otherwise you get duplicate type errors and conflicting resources.

### Multiple widgets in one bundle

You can ship several widgets from one frontend project. Use a single entry (e.g. `src/index.ts`) that exports and assigns all of them to one global object whose name matches the Vite `name` and C# `GlobalName`:

```typescript
// vite.config.ts — one name for the whole bundle
name: 'MyProject_Widgets',
fileName: () => 'ExternalWidgets.js',

// src/index.ts
(window as any).MyProject_Widgets = {
  MyWidget,
  AnotherWidget,
};
```

Then in C# use the same `GlobalName` for each widget and different `ExportName` values.

## Host requirements

External widgets that externalize React expect the host to provide React (and ReactDOM) on the global object.

The host’s entry point should set:

```typescript
(window as any).React = React;
(window as any).ReactDOM = ReactDOM; // or createRoot etc.
```

Ivy’s standard host (e.g. [Chrome](https://docs.ivy.app/onboarding/concepts/program.md)) does this. If you see “Global not found” or React-related errors, ensure the host exposes these globals before any external widget script runs.

## Troubleshooting

| Issue | What to check |
| ----- | ------------- |
| **Script resource not found** | Path in `[ExternalWidget]` must match the embedded path (project-relative, e.g. `frontend/dist/ExternalWidget.js`). Resource name is assembly name + path with `/` → `.`. After changing `fileName` in Vite, run `dotnet clean` then `dotnet build`. |
| **Global not found** | `GlobalName` in C# must equal Vite `build.lib.name`. In `src/index.ts`, assign the export to `window[GlobalName]`. Use `jsxRuntime: 'classic'` so the bundle uses the global React. |
| **Duplicate type / CS0579** | Widget project is under the host and the host is compiling its files. Exclude the widget directory in the host's `.csproj` with `DefaultItemExcludes` or `Compile Remove`. |
| **Invalid hook call / multiple React** | Widget must not bundle React when the host provides it. Keep `react` and `react-dom` in `external` and `globals` in Vite, and use `jsxRuntime: 'classic'`. |
| **Wrong filename (.iife.js)** | Set `fileName: () => 'ExternalWidget.js'` in `vite.config.ts` so the output name has no extra suffix. |

For more detail on resource naming, nesting pitfalls, migration from internal widgets, and self-contained builds (bundling React inside the widget), refer to your project’s external widget guide or the Ivy Framework repository.
