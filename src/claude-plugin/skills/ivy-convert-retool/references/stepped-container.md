# Stepped Container

A container that groups components into a series of sequential steps. It is a preset version of Retool's Container component, preconfigured with multiple views and a Steps navigation component. Users can navigate between steps to complete multi-step workflows like wizards and forms.

## Retool

```toolscript
// Stepped Container is a preconfigured Container + Steps combo.
// The Steps component controls which Container view is visible.

// Navigate programmatically:
steppedContainer1.showNextView()
steppedContainer1.showPreviousView()
steppedContainer1.setCurrentViewIndex(2)
steppedContainer1.setCurrentView("step3")

// Read current state:
steppedContainer1.currentViewIndex  // 0
steppedContainer1.currentViewKey    // "step1"

// Steps sub-component properties:
steps1.selectedIndex       // current step index
steps1.indicateCompletedSteps  // show checkmarks on completed steps
steps1.showStepNumbers     // display numeric indicators
steps1.orientation         // "horizontal" or "vertical"
steps1.navigateContainer   // link steps to a container
steps1.targetContainerId   // linked container id
```

## Ivy

```csharp
public class SteppedContainerDemo : ViewBase
{
    StepperItem[] GetItems(int selectedIndex) =>
    [
        new("1", selectedIndex > 0 ? Icons.Check : null, "Company", "Setup company"),
        new("2", selectedIndex > 1 ? Icons.Check : null, "Raise", "Raise capital"),
        new("3", null, "Founders", "Add founders"),
    ];

    public override object? Build()
    {
        var selectedIndex = UseState(0);

        var items = GetItems(selectedIndex.Value);

        return Layout.Vertical()
            | new Stepper(OnSelect, selectedIndex.Value, items)
            | (Layout.Horizontal().Gap(0)
                | new Button("Previous").Link().OnClick(() =>
                {
                    selectedIndex.Set(Math.Clamp(selectedIndex.Value - 1, 0, items.Length - 1));
                })
                | new Button("Next").Link().OnClick(() =>
                {
                    selectedIndex.Set(Math.Clamp(selectedIndex.Value + 1, 0, items.Length - 1));
                })
            );

        ValueTask OnSelect(Event<Stepper, int> e)
        {
            selectedIndex.Set(e.Value);
            return ValueTask.CompletedTask;
        }
    }
}
```

## Parameters

| Parameter                | Retool Documentation                                                                 | Ivy                                                             |
|--------------------------|--------------------------------------------------------------------------------------|-----------------------------------------------------------------|
| Current step index       | `currentViewIndex` / `steps1.selectedIndex` — read/set the active step               | `selectedIndex` constructor param (zero-based `int?`)           |
| Step items/views         | `views` array on Container with `label`, `key`, `icon`, `tooltip` per view           | `StepperItem[]` with `symbol`, `icon`, `label`, `description`   |
| Navigate next            | `showNextView()` method                                                              | Manual via `selectedIndex.Set(index + 1)` with buttons          |
| Navigate previous        | `showPreviousView()` method                                                          | Manual via `selectedIndex.Set(index - 1)` with buttons          |
| Set view by key          | `setCurrentView(key)` method                                                         | Not supported (index-based only)                                |
| Set view by index        | `setCurrentViewIndex(index)` method                                                  | `selectedIndex.Set(index)`                                      |
| Completed step indicator | `steps1.indicateCompletedSteps` — show checkmarks on past steps                      | Manual via conditional `Icons.Check` in `StepperItem` icon      |
| Show step numbers        | `steps1.showStepNumbers` — display numeric indicators                                | `symbol` field on `StepperItem` (e.g. `"1"`, `"2"`)            |
| Step orientation         | `steps1.orientation` — `"horizontal"` or `"vertical"`                                | Not supported (horizontal only)                                 |
| Forward selection        | Steps allow clicking any step by default                                             | `AllowSelectForward()` setter (disabled by default)             |
| Selection event          | Steps `Change` event handler                                                         | `OnSelect` / `HandleSelect` — `Event<Stepper, int>`            |
| Linked container         | `steps1.navigateContainer` / `steps1.targetContainerId` — link Steps to a Container  | Not supported (stepper is standalone, no linked container)       |
| View transition          | `transition` — `none`, `fade`, or `slide` between views                              | Not supported                                                   |
| Header / Footer          | `showHeader`, `showFooter`, `headerPadding`, `footerPadding`                         | Not supported (compose with layouts)                            |
| Loading state            | `loading` / `hoistFetching` — show loading indicator                                 | Not supported                                                   |
| Disabled                 | `disabled` — disable interaction on the entire container                              | Not supported (pass `null` for `onSelect` to disable selection) |
| Visibility               | `isHiddenOnDesktop` / `isHiddenOnMobile` / `hidden`                                  | `Visible` property                                              |
| Dimensions               | Container CSS sizing / margin / padding                                              | `Height`, `Width`, `Scale` properties                           |
| Tooltip                  | `tooltipText` on Steps and Container                                                 | Not supported                                                   |
| Horizontal alignment     | `steps1.horizontalAlign` — `left`, `center`, `right`                                 | Not supported                                                   |
| Show border              | `showBorder` on Container                                                            | Not supported (use Box or layout styling)                       |
| Style overrides          | `style` object on Container and Steps                                                | Not supported                                                   |
