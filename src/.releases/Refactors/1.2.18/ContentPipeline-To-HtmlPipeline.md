# ContentPipeline Renamed to HtmlPipeline with XDocument-Based Filters - v1.2.18

## Summary

The `ContentPipeline` directory and namespace have been renamed to `HtmlPipeline` to match the existing class name. Filters now operate on `XDocument` instead of raw strings, and a new `Server.UseHtmlPipeline()` API allows full pipeline customization.

## What Changed

### 1. Namespace Rename

**Before:**
```csharp
using Ivy.Core.Server.ContentPipeline;
using Ivy.Core.Server.ContentPipeline.Filters;
```

**After:**
```csharp
using Ivy.Core.Server.HtmlPipeline;
using Ivy.Core.Server.HtmlPipeline.Filters;
```

### 2. Filter Interface Change

The `IHtmlFilter.Process` method now takes an `XDocument` instead of a raw HTML string.

**Before:**
```csharp
public class MyFilter : IHtmlFilter
{
    public string Process(HtmlPipelineContext context, string html)
    {
        html = html.Replace("</head>", "  <meta name=\"custom\" content=\"value\" />\n</head>");
        return html;
    }
}
```

**After:**
```csharp
using System.Xml.Linq;

public class MyFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var head = document.Root?.Element("head");
        head?.Add(new XElement("meta",
            new XAttribute("name", "custom"),
            new XAttribute("content", "value")));
    }
}
```

### 3. New Pipeline Customization API

```csharp
// Replace the entire pipeline
server.UseHtmlPipeline(p =>
{
    p.Clear();
    p.Use<MyCustomFilter>();
});

// Append to the default pipeline
server.UseHtmlPipeline(p => p.Use<ExtraFilter>());
```

## How to Find Affected Code

```regex
Ivy\.Core\.Server\.ContentPipeline
```

```regex
IHtmlFilter
```

```regex
string Process\(HtmlPipelineContext
```

## How to Refactor

1. Update `using` statements from `Ivy.Core.Server.ContentPipeline` to `Ivy.Core.Server.HtmlPipeline`.
2. Change custom `IHtmlFilter` implementations:
   - Change return type from `string` to `void`.
   - Change second parameter from `string html` to `XDocument document`.
   - Replace string manipulation with `XElement` operations on the document.
3. Add `using System.Xml.Linq;` to filter files.

## Verification

After refactoring, run:

```bash
dotnet build
```
