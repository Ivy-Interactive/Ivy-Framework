# Migration Notes: Chrome to AppShell Renaming

The Ivy Framework has undergone a significant refactoring to rename "Chrome" to "AppShell" to avoid confusion with the Google Chrome browser.

## Breaking Changes

### Backend (C#)

- **Class Renames**:
    - `ChromeSettings` → `AppShellSettings`
    - `DefaultSidebarChrome` → `DefaultSidebarAppShell`
    - `StudioChrome` → `StudioAppShell`
- **Method Renames**:
    - `server.UseChrome()` → `server.UseAppShell()`
    - `server.UseDefaultAppChrome()` → `server.UseDefaultAppShell()` (if applicable)
- **Namespace Changes**:
    - `Ivy.Chrome` → `Ivy.AppShell`
- **Broadcast Signals**:
    - `BroadcastType.Chrome` → `BroadcastType.AppShell`

### Frontend (TypeScript/React)

- **Utility Functions**:
    - `getChromeParam()` → `getAppShellParam()`
- **Query Parameters**:
    - `chrome=true|false` → `appshell=true|false`
- **Component Changes**:
    - References to "Chrome" in props, state, and CSS classes have been updated to "AppShell".

### Documentation

- The documentation file `11_Chrome.md` has been renamed to `11_AppShell.md`.
- All internal links and references have been updated.

## Recommendations for Users

1.  **Update your `Program.cs`**:
    Replace `app.UseChrome()` with `app.UseAppShell()`.
2.  **Update Namespaces**:
    Change `using Ivy.Chrome;` to `using Ivy.AppShell;`.
3.  **Update Frontend Code**:
    If your frontend relies on the `chrome` query parameter or the `getChromeParam` utility, update them to use `appshell` and `getAppShellParam`.
4.  **Rebuild**:
    A full rebuild of both backend and frontend is recommended to ensure all changes are correctly picked up.
