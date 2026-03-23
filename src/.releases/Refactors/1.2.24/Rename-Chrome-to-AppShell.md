# Rename Chrome to AppShell

## Summary

The "Chrome" concept has been renamed to "AppShell" to avoid confusion with the Google Chrome browser and to better describe the feature as an application shell that provides navigation and shared UI.

## What Changed

### Backend (C#)
- Namespace `Ivy.Chrome` renamed to `Ivy.AppShell`.
- Class `ChromeSettings` renamed to `AppShellSettings`.
- Class `DefaultSidebarChrome` renamed to `DefaultSidebarAppShell`.
- Extension method `server.UseChrome()` renamed to `server.UseAppShell()`.
- SignalR `BroadcastType.Chrome` renamed to `BroadcastType.AppShell`.
- Project `StudioChrome` renamed to `StudioAppShell`.

### Frontend (TypeScript)
- Query parameter `?chrome=true|false` renamed to `?appshell=true|false`.
- Utility function `getChromeParam()` renamed to `getAppShellParam()`.

## Before

```csharp
using Ivy.Chrome;

server.UseChrome(new ChromeSettings()
    .DefaultApp<MyHomeApp>()
);
```

```typescript
const isChrome = getChromeParam();
```

## After

```csharp
using Ivy.AppShell;

server.UseAppShell(new AppShellSettings()
    .DefaultApp<MyHomeApp>()
);
```

```typescript
const isAppShell = getAppShellParam();
```

## Migration Path

1. **Update Namespaces**: Replace `using Ivy.Chrome;` with `using Ivy.AppShell;`.
2. **Update Setup**: Replace `server.UseChrome(...)` with `server.UseAppShell(...)` and `ChromeSettings` with `AppShellSettings`.
3. **Update Frontend**: Update any frontend logic relying on `chrome` query parameter to use `appshell`.
4. **Update App IDs**: While `AppIds.AppShell` is now the constant name, it still resolves to `"$chrome"` for backward compatibility of stored IDs. Use the constant `AppIds.AppShell`.

## Verification

Run a full build of both backend and frontend:

```bash
dotnet build
pnpm run build
```
