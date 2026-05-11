# Tabbed Container

A container component that organizes child components into multiple tabbed views, allowing users to switch between content sections. In Retool this is a preset version of Container preconfigured with multiple views and the Tabs component. In Ivy the equivalent is `TabsLayout`.

## Retool

```toolscript
// Tabbed Container is configured via the visual editor.
// It exposes the following at runtime:
tabbedContainer1.currentViewIndex   // index of the active tab
tabbedContainer1.currentViewKey     // key of the active tab
tabbedContainer1.setCurrentViewIndex(2) // switch to tab at index 2

// Views are defined statically with labels, icons, and tooltips:
// views: [{ key: "tab1", label: "Profile" }, { key: "tab2", label: "Settings" }]

// Event handler: onChange fires when the active tab changes.
```

## Ivy

```csharp
Layout.Tabs(
    new Tab("Profile", ProfileContent()),
    new Tab("Settings", SettingsContent()),
    new Tab("Preferences", PreferencesContent())
)

// Full constructor with events:
new TabsLayout(
    onSelect: (e) => Console.WriteLine($"Selected: {e.Value}"),
    onClose: (e) => Console.WriteLine($"Closed: {e.Value}"),
    onRefresh: (e) => Console.WriteLine($"Refreshed: {e.Value}"),
    onReorder: (indices) => Console.WriteLine("Reordered"),
    selectedIndex: 0,
    new Tab("Profile", ProfileContent()),
    new Tab("Settings", SettingsContent())
)
```

## Parameters

| Parameter | Retool | Ivy |
|---|---|---|
| Active tab index | `currentViewIndex` | `SelectedIndex` |
| Active tab key | `currentViewKey` | Not supported |
| Tab labels | `labels` | Tab constructor argument |
| Tab icons | `iconByIndex` | Tab icon support |
| Tab tooltips | `tooltipByIndex` | Not supported |
| Disabled tabs | `disabledByIndex` | Not supported |
| Hidden tabs | `hiddenByIndex` | Not supported |
| Views / Tabs | `views` (static list) | `Tab[]` params |
| On tab change | `onChange` event | `OnSelect` |
| On tab close | Not supported | `OnClose` |
| On tab refresh | Not supported | `OnRefresh` |
| On tab reorder | Not supported | `OnReorder` |
| Add tab button | Not supported | `OnAddButtonClick` / `AddButtonText` |
| Variant | Not supported | `Variant` (Tabs / Content) |
| View transition | `transition` | Not supported |
| Show border | `showBorder` | Not supported |
| Show header | `showHeader` | Not supported |
| Show footer | `showFooter` | Not supported |
| Header padding | `headerPadding` | Not supported |
| Footer padding | `footerPadding` | Not supported |
| Padding | `padding` | `Padding` |
| Margin | `margin` | Not supported |
| Loading indicator | `loading` / `hoistFetching` | Not supported |
| Visible | `isHiddenOnDesktop` / `isHiddenOnMobile` | `Visible` |
| Maintain space when hidden | `maintainSpaceWhenHidden` | Not supported |
| Full bleed | `enableFullBleed` | `RemoveParentPadding` |
| Disabled | `disabled` | Not supported |
| Custom style | `style` | Not supported |
| Height | Auto-sized | `Height` |
| Width | Auto-sized | `Width` |
| Scale | Not supported | `Scale` |
| Set active tab method | `setCurrentViewIndex()` | Set `SelectedIndex` |
