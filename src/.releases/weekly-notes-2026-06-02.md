# Ivy Framework Weekly Notes - Week of 2026-06-02

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## New Features

### Spacer Sample Application
We have added a new, comprehensive sample application for the `Spacer` primitive (`SpacerApp.cs`). 
This sample demonstrates:
- **Default Grow Behavior**: How the `Spacer` automatically distributes and expands to fill the remaining layout space in horizontal or vertical layouts.
- **Explicit Sizing**: Utilizing fixed `.Width(Size)` or `.Height(Size)` constraints on the `Spacer` to create precise gaps between components.
- **Real-world Application**: A practical navigation bar layout demonstrating the layout of brand logos, links, and user action buttons using spacers.

## Bug Fixes and Improvements

### Tabs Parent Padding Configuration
Added `.RemoveParentPadding()` to the `Layout.Tabs()` component variants. This enables sample layouts and complex tabs to easily expand and fill their parent's boundaries without extra margin/padding styling overhead.

### Markdown List Rendering Spacing
Improved the layout spacing of list items inside the markdown component. Single-line and inline list items are no longer wrapped in block-level element container structures by default, resulting in tighter, more consistent lists that match standard markdown styling.
