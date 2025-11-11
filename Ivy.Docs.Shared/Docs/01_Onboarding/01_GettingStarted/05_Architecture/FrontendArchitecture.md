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

## Technology Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| Build Tool | Vite | Development server and bundling |
| Framework | React 19 | UI framework |
| Language | TypeScript | Type safety |
| Styling | Tailwind CSS | Utility-first styling |
| UI Components | Radix UI | Accessible component primitives |
| State Management | React hooks | Local component state |
| Real-time Communication | SignalR | WebSocket communication |

## Frontend Communication Flow

The `useBackend` hook manages the WebSocket connection and state updates. It handles several message types:

- **Refresh**: Complete widget tree replacement
- **Update**: JSON patch updates to specific widgets
- **Toast**: Display notifications
- **Error**: Error handling with stack traces
- **SetJwt**: Authentication token management
- **SetTheme**: Theme switching

```123:127:frontend/src/hooks/use-backend.tsx
export function useBackend(appId: string, appArgs?: string) {
  const [widgetTree, setWidgetTree] = useState<Widget | null>(null);
  const [connectionState, setConnectionState] = useState<ConnectionState>("disconnected");
  // ... more code ...
```

### Message Handling

The hook processes different message types:

```162:174:frontend/src/hooks/use-backend.tsx
      case "refresh":
        setWidgetTree(message.widget);
        break;
      case "update":
        if (widgetTree) {
          const patched = applyPatch(widgetTree, message.patches, undefined, false, false).newDocument;
          setWidgetTree(patched);
        }
        break;
```

## Widget Rendering Pipeline

The widget rendering system maps C# widget definitions to React components through the `widgetMap.ts` registry.

The `renderWidgetTree` function handles:

- Component lookup from the widget map
- Props transformation and event binding
- Slot-based content distribution
- Lazy loading for chart components
- Fragment flattening for layout optimization

```29:97:frontend/src/widgets/WidgetRenderer.tsx
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
        ? slotContent.map((child) => renderWidgetTree(child, onEvent, depth + 1))
        : renderWidgetTree(slotContent, onEvent, depth + 1);
    });
  }

  // Handle children (non-slot content)
  if (widget.children) {
    props.children = Array.isArray(widget.children)
      ? widget.children.map((child) => renderWidgetTree(child, onEvent, depth + 1))
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

## Build Configuration

The frontend build process includes:

- TypeScript compilation
- Tailwind CSS processing
- Asset bundling and optimization
- Embedded resource generation for the C# assembly

```56:77:frontend/vite.config.ts
  build: {
    outDir: "dist",
    emptyOutDir: true,
    rollupOptions: {
      input: {
        main: path.resolve(__dirname, "index.html"),
      },
    },
    // Generate embedded resources for C# assembly
    rollupOptions: {
      output: {
        // Ensure consistent file names for embedded resources
        entryFileNames: "assets/[name].[hash].js",
        chunkFileNames: "assets/[name].[hash].js",
        assetFileNames: "assets/[name].[hash].[ext]",
      },
    },
  },
```

## Development Workflow

During development, Vite provides:

- Hot Module Replacement (HMR) for instant updates
- Fast refresh for React components
- TypeScript type checking
- Tailwind CSS JIT compilation

The development server runs independently and communicates with the backend via WebSocket, allowing for rapid iteration during development.

