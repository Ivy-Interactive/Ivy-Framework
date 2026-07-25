# Ivy Framework Weekly Notes - Week of 2026-07-23

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release introduces a collapsible 54px icon rail for `SidebarLayout`, real-time client synchronization for external widgets registered by plugins, `OnLinkClick` support for `Svg` primitives, and several plugin lifecycle and configuration enhancements.

## New Features & Improvements

### UI & Layouts
- **Collapsible Sidebar Icon Rail**: Main application sidebars in `SidebarLayout` can now collapse into a sleek 54px icon rail instead of hiding off-screen.
  - Menu items render icon-only with tooltips (with fallback initial labels for icon-less items).
  - New optional slots `SidebarHeaderCollapsed` and `SidebarFooterCollapsed` allow custom rail header/footer views.
  - Sidebar toggle button moves to the top right of the sidebar (centered top row in rail mode).
- **SVG Link Click Event**: Added `OnLinkClick` event to the `Svg` primitive widget for capturing internal link navigation (such as `plan://` protocol links in DAG charts).

### Plugins & Server
- **Runtime External Widget Sync**: When plugins register external React/npm widgets at runtime, registry updates are now pushed immediately over SignalR to connected clients without requiring a page refresh.
- **Plugin Metadata for Unloaded Plugins**: `PluginCandidate` now carries the plugin's `Title` and `Icon` so the UI displays human-readable names and icons for unloaded or failed plugins.
- **Plugin Configuration Views**: `PluginConfigurationView` now automatically reconfigures plugins on save and exposes an `ExtraActions` slot for custom action buttons (e.g. Uninstall).
- **Deferred Loading Startup Fix**: Queued `UseEndpoints` registrations during deferred startup to prevent lifecycle race conditions before ASP.NET application setup completes.

### Security
- **Security Dependency Audit**: Updated `System.Security.Cryptography.Xml` to version `10.0.10`.
