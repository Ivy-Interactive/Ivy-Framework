# Ivy Framework Basics & App Construction

## Ivy Framework Basics

- Ivy apps are .NET web apps started with `dotnet run -- --port <port>`
- The frontend client is in `src/frontend/` and built with `vp build` into `src/frontend/dist/`. These assets are embedded into the Ivy.dll via `<EmbeddedResource>`. If testing a commit that changed frontend `.ts` files, you MUST rebuild the frontend (`cd src/frontend && vp build`) before running the dotnet project, otherwise the old bundled JS is served.
- If `vp build` fails due to pre-existing TS errors in unrelated files, run vite directly: `npx vite build` — this skips tsc and still produces a working bundle. (Note: `node ./node_modules/vite/bin/vite.js` may fail with MODULE_NOT_FOUND if vite is hoisted; use `npx` instead)
- The server prints `Ivy is running on http://localhost:<port>` when ready
- Apps are decorated with `[App(icon: Icons.X, path: ["Apps"])]` and inherit `ViewBase`
- The `Build()` method returns the UI tree
- State is managed via `UseState<T>()` which returns reactive state objects
- Services are injected via `UseService<T>()`

## Ivy App Construction

- `ViewBase.Build()` is `public override`, NOT `protected override` — using `protected` causes CS0507
- Ivy apps MUST have a parameterless constructor — `AppDescriptor.CreateApp()` uses `Activator.CreateInstance` without DI
- Do NOT use primary constructor injection like `MyApp(IClientProvider client) : ViewBase` — causes `MissingMethodException` at runtime
- Instead, use `var client = UseService<IClientProvider>()` inside the `Build()` method

## Ivy Hooks Rules (Enforced by Ivy.Analyser)

- **IVYHOOK005**: ALL Ivy hooks (`UseState`, `UseEffect`, `UseService`, etc.) MUST be called at the very top of the `Build()` method, before any other statements or logic
- Hooks cannot be called conditionally or after value extraction — they must be the first lines in `Build()`
- Correct pattern:
  ```csharp
  public override object? Build() {
      // All hooks first
      var state1 = UseState<int>(() => 0);
      var state2 = UseState<string>(() => "");
      var service = UseService<IClientProvider>();

      // Then other logic
      var value = state1.Value * 2;
      return Layout.Vertical() | ...;
  }
  ```

## Standalone Test Project Setup

- The server class is `Server` (NOT `IvyServer`). Entry point: `var server = new Server(); await server.RunAsync();`
- Apps must be explicitly registered: `server.AddAppsFromAssembly(Assembly.GetExecutingAssembly());` — Ivy does NOT auto-discover apps
- Use ProjectReference to local Ivy: `<ProjectReference Include="D:\Repos\_Ivy\Ivy-Framework\src\Ivy\Ivy.csproj" />`
- The csproj needs `<Nullable>enable</Nullable>` to avoid CS8632 warnings
- `ButtonVariant` values: Primary, Destructive, Outline, Secondary, Success, Warning, Info, Ghost, Link (NO `Default`)
- `Card` has no `.Default()` method — use `new Card(content)` or `Layout.Vertical()` containers instead
- `Icons` enum: use PascalCase Lucide icon names. Not all names exist (e.g. `AlignCenter` doesn't exist, use `AlignCenterHorizontal`). Lucide renamed some icons: `AlertTriangle` → `TriangleAlert`
- `AppContext` is ambiguous with `System.AppContext` — always use `Ivy.AppContext` in standalone test projects (e.g. `UseService<Ivy.AppContext>()`)
- For `IDisposable` return in `UseEffect`, use `System.Reactive.Disposables.Disposable.Empty` (available via Ivy's dependency on System.Reactive)

## Frontend Build Issues

### Unknown Component Type Errors

When an Ivy app displays "Unknown component type: Ivy.WidgetName" instead of rendering a widget:

1. **Check frontend build status**: The widget may have TypeScript compilation errors preventing the frontend bundle from building
2. **Rebuild frontend**: Run `cd D:\Repos\_Ivy\Ivy-Framework\src\frontend && vp build` to see compilation errors
3. **Common causes**:
   - Unused variables in the TypeScript widget implementation
   - Type mismatches (e.g., XAxisProps[] not compatible with expected interface)
   - Missing imports or exports in the frontend code
   - Widget not registered in the frontend widget registry
4. **Verification**: A successful frontend build is required before testing any new or modified widgets
5. **Note**: The C# backend can compile successfully while the TypeScript frontend fails, causing this error

This is different from `[ExternalWidget]` "Unknown component type" errors which are Framework bundle serving issues.
