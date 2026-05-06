# Alert

A message box to display important information with an optional title and clickable action.

## Retool

```toolscript
// Configure alert properties
alert1.title = "Deployment Notice";
alert1.description = "Your application has been deployed successfully.";
alert1.color = "success";
alert1.styleVariant = "solid";
alert1.icon = "/icon:bold/alert";
alert1.actionText = "View Details";

// Methods
alert1.setValue("New alert message");
alert1.setHidden(false);
alert1.clearValue();
alert1.resetValue();
alert1.scrollIntoView({ behavior: "smooth", block: "nearest" });
```

## Ivy

Ivy maps this to two concepts: the `Callout` widget for static inline alerts, and `UseAlert()`/`client.Toast()` for interactive and transient alerts.

### Callout (static banner)

```csharp
// Simple factory methods
Callout.Info("Your application has been deployed successfully.", "Deployment Notice");
Callout.Success("All tests passed.");
Callout.Warning("This action cannot be undone.");
Callout.Error("Deployment failed. Check the logs for details.");

// Full constructor with options
new Callout(
    description: "Your application has been deployed successfully.",
    title: "Deployment Notice",
    variant: CalloutVariant.Success,
    icon: Icons.Check
);
```

### Dialog Alert & Toast (interactive / transient)

```csharp
// Modal dialog alert with user confirmation
var (alertView, showAlert) = UseAlert();
showAlert("Are you sure you want to deploy?", result =>
{
    if (result == AlertResult.Yes)
        client.Toast("Deployed successfully!", "Deploy");
}, "Confirm Deploy", AlertButtonSet.YesNo);

// Toast notifications
client.Toast("Operation completed.", "Success");
```

## Parameters

| Parameter             | Retool Documentation                                                    | Ivy                                                                          |
|-----------------------|-------------------------------------------------------------------------|------------------------------------------------------------------------------|
| `title`               | Title text to display                                                   | `Callout.Title` / `title` constructor parameter                             |
| `description`         | Descriptive text displayed to user                                      | `Callout.Description` / `description` constructor parameter                  |
| `color`               | Specified color for the component                                       | `CalloutVariant` enum (`Info`, `Success`, `Warning`, `Error`)                |
| `styleVariant`        | Style variant (`solid` or `outline`)                                    | Not supported                                                                |
| `icon`                | Icon to display in the alert                                            | `Callout.Icon` / `Icons` enum                                               |
| `iconPosition`        | Icon position relative to text (`left`, `right`, `replace`)             | Not supported                                                                |
| `actionText`          | Text displayed on the action button                                     | Not supported (use nested `Button` in `Callout.Description`)                |
| `actionIcon`          | Icon displayed on the action button                                     | Not supported                                                                |
| `actionIconPosition`  | Position of action icon (`left` or `right`)                             | Not supported                                                                |
| `actionPosition`      | Position of action button (`top` or `bottom`)                           | Not supported                                                                |
| `actionTooltip`       | Tooltip shown when hovering over action button                          | Not supported                                                                |
| `backgroundColor`     | Background color of the alert                                           | Not supported (controlled by `Variant`)                                      |
| `borderColor`         | Border color of the alert                                               | Not supported (controlled by `Variant`)                                      |
| `hidden`              | Whether the component is hidden from view                               | `Callout.Visible`                                                            |
| `isHiddenOnMobile`    | Hide component in mobile layout                                         | Not supported                                                                |
| `isHiddenOnDesktop`   | Hide component in desktop layout                                        | Not supported                                                                |
| `maintainSpaceWhenHidden` | Reserve space when component is hidden                              | Not supported                                                                |
| `margin`              | Outside margin amount                                                   | Not supported (use layout widgets)                                           |
| `showInEditor`        | Keep component visible in editor when hidden                            | Not supported                                                                |
| `tooltip`             | Tooltip text shown on hover (supports Markdown)                         | Not supported (use `Tooltip` widget)                                         |
| `value`               | Current or default value                                                | Not supported                                                                |
| `id`                  | Unique identifier name for the component                                | Not applicable (C# variable reference)                                       |
| `closeIcon`           | Icon displayed when closed                                              | Not supported                                                                |
| `closeIconPosition`   | Close icon position relative to text                                    | Not supported                                                                |
| **Methods**           |                                                                         |                                                                              |
| `clearValue()`        | Clears the current values                                               | Not supported                                                                |
| `resetValue()`        | Resets value to default                                                  | Not supported                                                                |
| `scrollIntoView()`    | Scrolls component into visible area                                     | Not supported                                                                |
| `setHidden(hidden)`   | Toggles component visibility                                            | Set `Callout.Visible`                                                        |
| `setValue(value)`      | Sets current value programmatically                                     | Not supported                                                                |
| **Events**            |                                                                         |                                                                              |
| Close event           | Triggered when the component is closed                                  | Not supported (Callout is not closable; use `UseAlert` for dismissible alerts)|
