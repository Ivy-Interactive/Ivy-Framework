---
searchHints:
  - frontend
  - react
  - typescript
  - vite
  - widget rendering
  - signalr
---

# Frontend Architecture

<Ingress>
The Ivy frontend is a single-page React application built with TypeScript and Vite. It uses a real-time communication model where the backend sends widget tree updates that are applied to the frontend state.
</Ingress>

For information about the backend C# framework that defines widgets and handles business logic, see [Backend Architecture](./02_BackendArchitecture.md). For details on how frontend and backend communicate via SignalR, see [Communication](./03_Communication.md).

## Technology Stack

The Ivy frontend is built using modern web technologies optimized for development speed and runtime performance:

| Component     | Technology   | Purpose                         |
| ------------- | ------------ | ------------------------------- |
| UI Framework  | React        | UI library                      |
| Language      | TypeScript   | Type safety                     |
| Build Tool    | Vite         | Build tool and dev server       |
| Styling       | Tailwind CSS | Utility-first styling           |
| UI Components | Radix UI     | Accessible component primitives |
| Communication | SignalR      | Real-time communication         |

The application uses Vite as the primary build tool, providing fast hot module replacement during development and optimized production builds. React 19 with concurrent features enables responsive UI updates, while TypeScript provides compile-time type safety for the entire codebase.

For a complete list of all dependencies and their versions, see the `package.json` file in the `frontend` directory.

## Build System and Development Environment

The build system uses Vite with custom plugins for seamless integration with the C# backend. The `injectMeta` plugin fetches HTML metadata from the backend server during development, enabling the frontend to inherit page titles and configuration from the server.

**Development Workflow:**

| Service             | Configuration                | Purpose                                             |
| ------------------- | ---------------------------- | --------------------------------------------------- |
| Frontend Dev Server | Port 5173 via `npm run dev`  | Development server with HMR                         |
| Backend Server      | Port 5010 via `dotnet watch` | C# backend with hot reload                          |
| Metadata Injection  | `injectMeta` plugin          | Synchronizes page metadata from backend to frontend |
| Hot Reload          | State preservation           | Preserves application state during code changes     |

```typescript
    plugins: [
      react(),
      injectMeta({
        host: process.env.IVY_HOST || "http://localhost:5010",
      }),
    ],
    server: {
      port: 5173,
      proxy: {
        "/ivy/messages": {
          target: process.env.IVY_HOST || "http://localhost:5010",
          ws: true,
        },
      },
    },
```

```json
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "lint": "eslint .",
    "preview": "vite preview",
    "format": "prettier --write \"src/**/*.{ts,tsx}\""
  },
```

## Real-Time Communication Architecture

The `useBackend` hook manages the SignalR connection lifecycle and processes real-time messages from the backend. The connection uses automatic reconnection and handles various message types for state synchronization.

**Message Processing:**

```mermaid
graph TD
    A[Backend Messages] --> B[Refresh Messages]
    A --> C[Update Messages]
    A --> D[Event Messages]
    A --> E[Authentication Messages]

    B --> B1[Complete Widget Trees]
    C --> C1[JSON Patches]
    D --> D1[User Interactions]
    E --> E1[JWT & Theme]
```

The hook applies JSON patches using `fast-json-patch` and `lodash.cloneDeep` to maintain immutable state updates while ensuring optimal performance.

```typescript
export function useBackend(appId: string, appArgs?: string) {
  const [widgetTree, setWidgetTree] = useState<Widget | null>(null);
  const [connectionState, setConnectionState] =
    useState<ConnectionState>("disconnected");
  const [hubConnection, setHubConnection] = useState<HubConnection | null>(
    null
  );
  const machineId = useMachineId();
  const { parentId } = useSearchParams();

  // ... connection setup and message handling ...
}
```

```typescript
const handleMessage = (message: BackendMessage) => {
  switch (message.type) {
    case "refresh":
      setWidgetTree(message.widget);
      break;
    case "update":
      if (widgetTree) {
        setWidgetTree(
          applyPatch(cloneDeep(widgetTree), message.patches).newDocument
        );
      }
      break;
    case "toast":
    case "error":
    case "setJwt":
    case "setTheme":
      // Handle respective message type
      break;
  }
};
```

## Widget Rendering System

The widget rendering system transforms C# widget definitions into React components through a centralized registry and rendering pipeline.

**Core Components:**

```mermaid
graph LR
    A[WidgetNode Interface] --> B[renderWidgetTree]
    B --> C[widgetMap Registry]
    C --> B
    B --> D[Slot System]
    D --> B
```

**Rendering Process:**

1. Backend sends widget definitions as `WidgetNode` structures
2. `renderWidgetTree` looks up components in `widgetMap`
3. Props are mapped and children are processed recursively
4. Slot widgets enable named content placement
5. Lazy components use `React.Suspense` with custom loading states

```typescript
export function renderWidgetTree(
  widget: Widget | null,
  onEvent: (event: WidgetEvent) => void,
  depth: number = 0
): React.ReactNode {
  if (!widget) {
    return null;
  }

  // Handle fragments - flatten them for layout optimization
  if (widget.type === "fragment") {
    const fragment = widget as FragmentWidget;
    return (
      <>
        {fragment.children?.map((child, index) =>
          renderWidgetTree(child, onEvent, depth + 1)
        )}
      </>
    );
  }

  // Look up the React component for this widget type
  const Component = widgetMap[widget.type];
  if (!Component) {
    console.warn(`Unknown widget type: ${widget.type}`);
    return null;
  }

  // Transform widget props to React component props
  const props: any = {
    ...widget.props,
    key: widget.id,
  };

  // Handle event binding - convert onClick, onChange, etc. to event handlers
  if (widget.props?.onClick) {
    props.onClick = () => {
      onEvent({
        widgetId: widget.id,
        eventType: "click",
        data: {},
      });
    };
  }

  if (widget.props?.onChange) {
    props.onChange = (value: any) => {
      onEvent({
        widgetId: widget.id,
        eventType: "change",
        data: { value },
      });
    };
  }

  // Handle slot-based content distribution
  if (widget.slots) {
    Object.keys(widget.slots).forEach((slotName) => {
      const slotContent = widget.slots![slotName];
      props[slotName] = Array.isArray(slotContent)
        ? slotContent.map((child) =>
            renderWidgetTree(child, onEvent, depth + 1)
          )
        : renderWidgetTree(slotContent, onEvent, depth + 1);
    });
  }

  // Handle children (non-slot content)
  if (widget.children) {
    props.children = Array.isArray(widget.children)
      ? widget.children.map((child) =>
          renderWidgetTree(child, onEvent, depth + 1)
        )
      : renderWidgetTree(widget.children, onEvent, depth + 1);
  }

  // Lazy load chart components
  if (widget.type.startsWith("chart")) {
    return (
      <Suspense fallback={<LoadingScreen />}>
        <Component {...props} />
      </Suspense>
    );
  }

  return <Component {...props} />;
}
```

```typescript
export interface WidgetNode {
  id: string;
  type: string;
  props?: Record<string, any>;
  children?: WidgetNode | WidgetNode[];
  slots?: Record<string, WidgetNode | WidgetNode[]>;
}
```

## Theming and Styling System

The theming system uses CSS custom properties with comprehensive light and dark mode support, built on the **Ivy Design System** tokens. The system provides a complete design token set covering colors, typography, spacing, and animations.

**CSS Custom Properties Structure:** Theme variables are defined in two CSS scopes: `:root` contains light theme variables, while `.dark` contains dark theme variants. When the `dark` class is applied to the document element, dark theme variables take precedence. The backend `IThemeService.GenerateThemeCss()` method generates a `<style>` block with both `:root` and `.dark` selectors containing all theme variables.

**ThemeProvider Implementation:** The `ThemeProvider` component manages theme state and persistence. It stores the current theme mode (`light`, `dark`, or `system`) in localStorage using the key `ivy-ui-theme`. When the theme changes, it manipulates the `documentElement.classList` to add or remove the `dark` class, which triggers CSS cascade to use dark theme variables. For `system` mode, it uses `window.matchMedia('(prefers-color-scheme: dark)')` to detect OS preference.

**Theme Update Flow:** When the backend calls `IClientProvider.ApplyTheme(css)`, it sends a `setTheme` message via SignalR. The frontend receives this message in the `useBackend` hook's message handler and injects the generated CSS into the document. The `useThemeWithMonitoring()` hook uses a `MutationObserver` to watch for class changes on `documentElement`, automatically detecting theme switches and updating component state.

**Styling Stack:**

- CSS custom properties for all design tokens
- Automatic theme detection via `MutationObserver`
- Semantic color palette with light/dark variants
- Typography scales with Geist font family
- Tailwind CSS integration for utility-first styling

**Available Theme Colors:**

| Category    | Variables                                                                                                    |
| ----------- | ------------------------------------------------------------------------------------------------------------ |
| Main        | `--primary`, `--primary-foreground`, `--secondary`, `--secondary-foreground`, `--background`, `--foreground` |
| Semantic    | `--destructive`, `--success`, `--warning`, `--info` (with `-foreground` variants)                            |
| UI Elements | `--border`, `--input`, `--ring`, `--muted`, `--accent`, `--card`, `--popover` (with `-foreground` variants)  |

**Font System:** The application uses Geist and Geist Mono fonts with `font-display: swap` for optimal loading performance. Font files are served locally with multiple weights (400, 500, 600, 700).

**Component Integration:** Radix UI components receive theme-aware styling through CSS custom properties, ensuring consistent appearance across light and dark modes.

```css
@font-face {
  font-family: "Geist";
  src: url("/fonts/Geist-Regular.woff2") format("woff2");
  font-weight: 400;
  font-style: normal;
  font-display: swap;
}
```

- **Typography**: Geist and Geist Mono fonts (weights 400, 500, 600, 700) with `font-display: swap`. shadcn/ui typography utilities (`typography-h1` through `typography-h4`, `typography-p`, `typography-lead`, etc.)
- **Colors**: Semantic tokens mapped to Tailwind classes (`bg-primary`, `text-muted-foreground`). 16 chromatic colors for charts/visualization (red, orange, amber, yellow, lime, green, emerald, teal, cyan, sky, blue, indigo, violet, purple, fuchsia, pink, rose)
- **Animations**: Accordion, sheet/dialog slides, toast, alert, and overlay animations integrated with Radix UI `data-state` attributes
- **Components**: Radix UI primitives styled via CSS custom properties. Tailwind utility classes with custom theme extension. Native scrollbars, autofill, and form elements themed

**Component Theme Consumption:** React components access theme variables through CSS custom properties. Tailwind's theme configuration extends to read these variables, allowing utility classes like `bg-primary` to resolve to `var(--primary)`. The `useThemeWithMonitoring()` hook extracts computed CSS variable values from the DOM using `getComputedStyle()`, enabling components to reactively respond to theme changes.

## Development Tools and Hot Reload

The development environment provides comprehensive hot reload capabilities for both frontend and backend changes. The system maintains application state during code updates and provides detailed debugging information.

**Development Features:**

```mermaid
graph LR
    A[Frontend Changes] --> B[Vite HMR]
    C[Backend Changes] --> D[SignalR Hot Reload]
    B --> E[State Preservation]
    D --> E
    E --> F[XML Debugging]
    F --> G[Error Overlay]
```

**Development Commands:**

| Command          | Purpose                            |
| ---------------- | ---------------------------------- |
| `npm run dev`    | Start development server with HMR  |
| `npm run build`  | Production build with optimization |
| `npm run lint`   | ESLint code analysis               |
| `npm run format` | Prettier code formatting           |

**Backend Integration:** The development server connects to the backend via environment variable `IVY_HOST` (defaults to `http://localhost:5010`). The `injectMeta` plugin synchronizes metadata between frontend and backend during development.

```typescript
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
import { injectMeta } from "./vite-plugin-inject-meta";

export default defineConfig({
  plugins: [
    react(),
    injectMeta({
      host: process.env.IVY_HOST || "http://localhost:5010",
    }),
  ],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    port: 5173,
    proxy: {
      "/ivy/messages": {
        target: process.env.IVY_HOST || "http://localhost:5010",
        ws: true,
      },
    },
  },
  build: {
    outDir: "dist",
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: "assets/[name].[hash].js",
        chunkFileNames: "assets/[name].[hash].js",
        assetFileNames: "assets/[name].[hash].[ext]",
      },
    },
  },
});
```

```typescript
connection.on("refresh", (message: RefreshMessage) => {
  handleMessage({ type: "refresh", widget: message.widget });
});

connection.on("update", (message: UpdateMessage) => {
  handleMessage({ type: "update", patches: message.patches });
});
```

```typescript
    connection.start()
      .then(() => {
        setConnectionState("connected");
      })
      .catch((error) => {
        console.error("Connection error:", error);
        setConnectionState("error");
      });

    connection.onclose(() => {
      setConnectionState("disconnected");
    });

    setHubConnection(connection);

    return () => {
      connection.stop();
    };
  }, [appId, appArgs, machineId, parentId]);
```
