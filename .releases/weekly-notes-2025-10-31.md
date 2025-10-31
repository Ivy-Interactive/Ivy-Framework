# Ivy Framework Weekly Notes - Week of 2025-10-31

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Overview

This week's main features are the Kanban widget, AI-powered DataTable filtering, and URL-state preservation.

## Bug Fixes

- **Checkbox/Radio Alignment**: Fixed vertical centering of labels
- **Chart Layout**: Improved flexbox with proper height handling (200px minimum)
- **DataTable Type Metadata**: Fixed Icon columns rendering as "?" by preserving backend metadata
- **File Input Clear**: Fixed clear button for single/multi-file uploads
- **Search Shortcut**: Fixed sidebar Ctrl+K by merging focus and input refs
- **DataTable Config**: Renamed `config` to `configuration`, added `ShowColumnTypeIcons`
- **Scroll Areas**: Removed delay before hiding scrollbars (now instant)
- **Card Spacing**: Reduced gap between title and content
- **Progress Hover**: Goal badge responds to its own hover
- **Package Versions**: Locked npm dependencies to prevent auto-upgrades

## Improvements

### Enhanced AI-Powered DataTable Filtering

Intelligent query interpretation now handles type mismatches (e.g., "[Activity] above 45" → [Age] > 45), conceptual queries (e.g., "drinking age" → [Age] >= 18), superlatives, and partial field matches. This can also be used to transform human language into a valid filter expression.

### DataTable UI & Performance

- **Batch Loading**: Automatically loads more rows to fill tall containers
- **Filter UI**: Inline expansion with smooth animations, fixed-width container prevents layout shifts
- **Keyboard Shortcuts**: Only visible when input is empty

### Text Styling

Added `.Bold()`, `.Italic()`, and `.Muted()` modifiers to TextBlock that can be chained:

```csharp
Text.P("Bold italic").Bold().Italic()
```

### Visual Improvements

- Sidebar accent color updated for better contrast
- Blade buttons use consistent styling

## New Features

### Kanban Widget

Visualize and manage data in a drag-and-drop board interface with automatic grouping:

```csharp
var tasks = new[] {
    new Task { Id = "1", Title = "Design Homepage", Status = "Todo", ... },
    new Task { Id = "2", Title = "Code Review", Status = "In Progress", ... },
    new Task { Id = "3", Title = "Deploy", Status = "Done", ... }
};

return tasks.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    titleSelector: e => e.Title,
    descriptionSelector: e => e.Description)
    .HandleMove(moveData => { /* Update task status */ })
    .HandleAdd(columnKey => { /* Add new task */ })
    .HandleDelete(cardId => { /* Remove task */ })
    .HandleClick(cardId => { /* Show details */ });
```

### URL State Preservation

Full browser history integration with path-based URLs (`/my-app`), forward/back navigation, tab state preservation, and `chrome=false` parameter for chromeless mode.

### DataTable Enhancements

- **Search**: Enable with `config.ShowSearch = true`, toggle with Ctrl/Cmd+F
- **Collapsible Groups**: Click group headers to collapse/expand when `ShowGroups` is enabled
- **Row Hover**: `config.EnableRowHover = true` for theme-aware hover effects
- **Cell Click Events**: Handle single-click (`OnCellClick`) and double-click (`OnCellActivated`) with access to row/column/value data
- **Vertical Borders**: `config.ShowVerticalBorders = false` to hide column borders

## CLI & Tooling

### Database Generator

- **Prompt Preservation**: Schema prompts saved to README
- **Auto-Fix Retry**: Up to 3 attempts with automatic error-based fixes
- **File Permissions**: Control read/write access with `--write-allow`, `--write-disallow`, `--read-allow`, `--read-disallow` flags
- **Clean Runs**: Generator directory auto-cleared before each run