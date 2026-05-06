# Signature

An input field to digitally capture a signature as an image. Users can draw their signature within the application, with optional clearing functionality and validation support.

## Retool

```toolscript
// Set focus on the signature component
signature.focus();

// Reset the signature to default value
signature.resetValue();

// Disable the signature input
signature.setDisabled(true);

// Validate the signature
signature.validate();
```

## Ivy

```csharp
// Not supported — Ivy does not have a Signature widget.
```

> **Note:** Ivy does not currently have a signature capture component.

## Parameters

| Parameter        | Documentation                                     | Ivy           |
|------------------|---------------------------------------------------|---------------|
| `value`          | Captured signature data (read-only)               | Not supported |
| `disabled`       | Whether interaction is disabled                   | Not supported |
| `required`       | Whether a value must be provided                  | Not supported |
| `showRedrawIcon` | Display "Clear signature" button                  | Not supported |
| `emptyMessage`   | Message displayed when no value is set            | Not supported |
| `labelPosition`  | Position of label (`top` or `left`)               | Not supported |
| `tooltipText`    | Hover tooltip text                                | Not supported |
| `margin`         | Margin spacing                                    | Not supported |
