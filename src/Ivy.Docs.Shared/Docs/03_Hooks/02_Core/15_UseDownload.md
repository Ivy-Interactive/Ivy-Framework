---
searchHints:
  - download
  - usedownload
  - file-download
  - blob
  - file-export
  - download-link
---

# UseDownload

<Ingress>
The `UseDownload` [hook](../02_RulesOfHooks.md) enables file downloads in your [application](../../../01_Onboarding/02_Concepts/10_Apps.md), generating download links for files created on-demand.
</Ingress>

## Overview

The `UseDownload` [hook](../02_RulesOfHooks.md) provides file download functionality:

- **On-Demand Generation** - Generate files dynamically when needed
- **Async Support** - Support for asynchronous file generation
- **MIME Types** - Specify content types for proper file handling
- **Automatic Cleanup** - Downloads are automatically cleaned up when components unmount

## Basic Usage

```csharp demo-below
public class DownloadBasicDemo : ViewBase
{
    public override object? Build()
    {
        var content = UseState("Hello, World!");
        
        var downloadUrl = UseDownload(
            factory: () => System.Text.Encoding.UTF8.GetBytes(content.Value),
            mimeType: "text/plain",
            fileName: "hello.txt"
        );

        return Layout.Vertical()
            | content.ToTextInput("Content")
            | (downloadUrl.Value != null
                ? new Button("Download File").Url(downloadUrl.Value)
                : Text.Block("Preparing download..."));
    }
}
```

## Common Patterns

### CSV Export

```csharp demo-below
public class DownloadCsvExportDemo : ViewBase
{
    public override object? Build()
    {
        var name = UseState("John Doe");
        var email = UseState("john@example.com");
        var age = UseState(30);

        var downloadUrl = UseDownload(
            factory: () =>
            {
                var csv = $"Name,Email,Age\n{name.Value},{email.Value},{age.Value}";
                return System.Text.Encoding.UTF8.GetBytes(csv);
            },
            mimeType: "text/csv",
            fileName: $"export-{DateTime.Now:yyyy-MM-dd}.csv"
        );

        return Layout.Vertical()
            | name.ToTextInput("Name")
            | email.ToTextInput("Email")
            | age.ToNumberInput("Age")
            | (downloadUrl.Value != null
                ? new Button("Export to CSV").Url(downloadUrl.Value).Variant(ButtonVariant.Primary)
                : Text.Block("Preparing export..."));
    }
}
```

### Multiple Format Export

```csharp demo-below
public class DownloadMultiFormatDemo : ViewBase
{
    public override object? Build()
    {
        var id = UseState(1);
        var name = UseState("Sample Item");

        var csvUrl = UseDownload(
            factory: () =>
            {
                var csv = $"Id,Name\n{id.Value},{name.Value}";
                return System.Text.Encoding.UTF8.GetBytes(csv);
            },
            mimeType: "text/csv",
            fileName: "export.csv"
        );

        var jsonUrl = UseDownload(
            factory: () => System.Text.Encoding.UTF8.GetBytes(
                $"{{\"id\":{id.Value},\"name\":\"{name.Value}\"}}"),
            mimeType: "application/json",
            fileName: "export.json"
        );

        return Layout.Vertical()
            | id.ToNumberInput("ID")
            | name.ToTextInput("Name")
            | (Layout.Horizontal()
                | (csvUrl.Value != null ? new Button("Download CSV").Url(csvUrl.Value) : null)
                | (jsonUrl.Value != null ? new Button("Download JSON").Url(jsonUrl.Value) : null));
    }
}
```

## See Also

- [State](./03_UseState.md) - Component state management
- [Effects](./04_UseEffect.md) - Side effects and lifecycle
- [Clients](../../../01_Onboarding/02_Concepts/19_Clients.md) - Client-side interactions including `DownloadFile()`
- [Rules of Hooks](../02_RulesOfHooks.md) - Understanding hook rules and best practices
