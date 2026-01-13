# Ivy.Widgets.Leaflet

Custom Ivy external widgets.

## Getting Started

1. Install frontend dependencies:
   ```bash
   cd frontend
   npm install
   ```

2. Build the frontend:
   ```bash
   npm run build
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

## Creating New Widgets

1. Create a new C# record inheriting from `WidgetBase<T>` with the `[ExternalWidget]` attribute
2. Create a corresponding React component in `frontend/src/`
3. Export the component from `frontend/src/index.ts`

## Widget Structure

- **C# Widget**: Define props with `[Prop]` and events with `[Event]` attributes
- **React Component**: Receives props and can call back to C# via the event handler

## Example Usage

```csharp
// In your Ivy app
new Map("Hello World")
```
