---
searchHints:
  - communication
  - signalr
  - websocket
  - protocol
  - frontend-backend
  - real-time
---

# Frontend-Backend Communication Architecture

<Ingress>
The communication between frontend and backend uses SignalR with a custom protocol for widget updates and event handling. This enables real-time, bidirectional communication between the React frontend and C# backend.
</Ingress>

## SignalR Connection Flow

The `useBackend` hook establishes the WebSocket connection with query parameters for app identification:

```
/messages?appId=${appId}&appArgs=${appArgs}&machineId=${machineId}&parentId=${parentId}
```

### Connection Parameters

- **appId**: The identifier of the application to load
- **appArgs**: Optional arguments passed to the application
- **machineId**: Unique identifier for the client machine
- **parentId**: Optional parent application identifier for nested apps

```295:338:frontend/src/hooks/use-backend.tsx
  useEffect(() => {
    if (!appId) return;

    const connection = new HubConnectionBuilder()
      .withUrl(`/messages?appId=${appId}&appArgs=${encodeURIComponent(appArgs || "")}&machineId=${machineId}&parentId=${parentId || ""}`)
      .withAutomaticReconnect()
      .build();

    connection.on("refresh", (message: RefreshMessage) => {
      handleMessage({ type: "refresh", widget: message.widget });
    });

    connection.on("update", (message: UpdateMessage) => {
      handleMessage({ type: "update", patches: message.patches });
    });

    connection.on("toast", (message: ToastMessage) => {
      handleMessage({ type: "toast", toast: message.toast });
    });

    connection.on("error", (message: ErrorMessage) => {
      handleMessage({ type: "error", error: message.error });
    });

    connection.on("setJwt", (message: SetJwtMessage) => {
      handleMessage({ type: "setJwt", jwt: message.jwt });
    });

    connection.on("setTheme", (message: SetThemeMessage) => {
      handleMessage({ type: "setTheme", theme: message.theme });
    });

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

## Message Types

### Backend to Frontend Messages

#### Refresh Message
Complete widget tree replacement. Used for initial load and full updates.

```typescript
interface RefreshMessage {
  type: "refresh";
  widget: Widget;
}
```

#### Update Message
Incremental updates using JSON Patch. More efficient than full refreshes.

```typescript
interface UpdateMessage {
  type: "update";
  patches: Operation[];
}
```

The frontend uses `fast-json-patch` to apply patches:

```166:174:frontend/src/hooks/use-backend.tsx
      case "update":
        if (widgetTree) {
          const patched = applyPatch(widgetTree, message.patches, undefined, false, false).newDocument;
          setWidgetTree(patched);
        }
        break;
```

#### Toast Message
Display notifications to the user.

```typescript
interface ToastMessage {
  type: "toast";
  toast: {
    title?: string;
    description?: string;
    variant?: "default" | "destructive" | "success";
  };
}
```

#### Error Message
Error reporting with stack traces for debugging.

```typescript
interface ErrorMessage {
  type: "error";
  error: {
    message: string;
    stackTrace?: string;
  };
}
```

#### SetJwt Message
Authentication token management.

```typescript
interface SetJwtMessage {
  type: "setJwt";
  jwt: string | null;
}
```

#### SetTheme Message
Theme switching (light/dark mode).

```typescript
interface SetThemeMessage {
  type: "setTheme";
  theme: "light" | "dark" | "system";
}
```

### Frontend to Backend Messages

#### Event Message
User interaction events sent to the backend.

```typescript
interface WidgetEvent {
  widgetId: string;
  eventType: "click" | "change" | "submit" | "focus" | "blur" | string;
  data: Record<string, any>;
}
```

Events are sent via the SignalR connection:

```typescript
hubConnection.invoke("HandleEvent", event);
```

## Widget Serialization

Widgets are serialized from C# to JSON using `System.Text.Json`:

1. **Widget Tree Construction**: Views build widget trees using C# objects
2. **JSON Serialization**: Widget trees are serialized to JSON
3. **Network Transfer**: JSON is sent over WebSocket
4. **Frontend Deserialization**: JSON is parsed into TypeScript objects
5. **React Rendering**: Widgets are rendered as React components

### Widget Type Mapping

The frontend maps C# widget types to React components:

| C# Widget Type | React Component |
|----------------|----------------|
| `Ivy.TextInput` | `TextInput` |
| `Ivy.MarkdownRenderer` | `MarkdownRenderer` |
| `Ivy.SidebarChrome` | `SidebarChrome` |
| `Ivy.Button` | `Button` |
| `Ivy.Card` | `Card` |

The mapping is defined in `widgetMap.ts` and used by `renderWidgetTree()`:

```30:32:frontend/src/widgets/WidgetRenderer.tsx
  // Look up the React component for this widget type
  const Component = widgetMap[widget.type];
  if (!Component) {
```

## State Synchronization

### State Update Flow

1. **User Interaction**: User clicks a button or changes input
2. **Event Sent**: Frontend sends event message to backend
3. **Handler Execution**: Backend executes C# event handler
4. **State Change**: Handler updates state (e.g., `UseState`)
5. **Re-render**: View's `Build()` method is called again
6. **Diff Calculation**: Backend calculates differences in widget tree
7. **Update Sent**: Backend sends update message (patch or refresh)
8. **UI Update**: Frontend applies update and re-renders affected components

### Incremental Updates

The backend uses JSON Patch for efficient incremental updates:

- Only changed widgets are included in patches
- Patches are applied using `fast-json-patch` on the frontend
- Reduces network traffic and improves performance
- Maintains React component state where possible

## Connection Management

### Reconnection Handling

SignalR automatically handles reconnection:

- **Automatic Reconnect**: Built-in reconnection logic
- **Connection State**: Frontend tracks connection state
- **State Recovery**: Widget tree is refreshed on reconnect
- **Error Handling**: Errors are displayed to the user

### Connection Lifecycle

1. **Initial Connection**: Frontend connects when component mounts
2. **App Load**: Backend loads application and sends initial widget tree
3. **Active State**: Bidirectional communication for events and updates
4. **Reconnection**: Automatic reconnection on network issues
5. **Cleanup**: Connection closed when component unmounts

## Performance Optimizations

### Widget Tree Optimization

- **Fragment Flattening**: Fragments are flattened during rendering
- **Lazy Loading**: Chart components are lazy-loaded
- **Memoization**: React components use memoization where appropriate
- **Selective Updates**: Only changed widgets trigger re-renders

### Network Optimization

- **JSON Patch**: Incremental updates instead of full refreshes
- **Compression**: SignalR supports message compression
- **Batching**: Multiple updates can be batched together
- **Caching**: Static assets are cached with appropriate headers

