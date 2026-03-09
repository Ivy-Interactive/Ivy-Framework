---
searchHints:
  - html
  - pipeline
  - filter
  - meta
  - head
  - index
  - shell
---

# HtmlPipeline

<Ingress>
The HtmlPipeline processes the `index.html` shell before serving it to the browser, injecting meta tags, theme styles, manifest links, and other elements into the `<head>`.
</Ingress>

## Overview

When a browser requests your Ivy application, the server serves an `index.html` shell that bootstraps the frontend. The HtmlPipeline runs a series of **filters** over this HTML document before it reaches the client. Each filter can inspect and modify the parsed HTML structure using `XDocument`.

## Built-in Filters

| Filter | What it injects |
|---|---|
| **LicenseFilter** | `<meta name="ivy-license">` tag from `Ivy:License` configuration |
| **DevToolsFilter** | `<meta name="ivy-enable-dev-tools">` when `EnableDevTools` is true |
| **MetaDescriptionFilter** | `<meta name="description">` from `ServerArgs.MetaDescription` |
| **TitleFilter** | Updates the existing `<title>` element with `ServerArgs.MetaTitle` |
| **ThemeFilter** | Injects theme CSS `<style>` and `<meta name="ivy-theme">` from `IThemeService` |
| **ManifestFilter** | `<link rel="manifest" href="/manifest.json">` when `ManifestOptions` is registered |

## Creating a Custom Filter

Implement the `IHtmlFilter` interface. Filters receive a parsed `XDocument` and the `HtmlPipelineContext`, which provides access to `IServiceProvider` and `ServerArgs`.

```csharp
using System.Xml.Linq;
using Ivy.Core.Server.HtmlPipeline;

public class OpenGraphFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var head = document.Root?.Element("head");
        if (head == null) return;

        head.Add(new XElement("meta",
            new XAttribute("property", "og:title"),
            new XAttribute("content", "My App")));

        head.Add(new XElement("meta",
            new XAttribute("property", "og:description"),
            new XAttribute("content", "Built with Ivy")));
    }
}
```

## Registering Filters

### Append a single filter

Use `Server.UseHtmlFilter()` to add a filter after all built-in filters:

```csharp
var server = new Server();
server.UseHtmlFilter(new OpenGraphFilter());
await server.RunAsync();
```

### Full pipeline customization

Use `Server.UseHtmlPipeline()` to access the full pipeline. You can clear, reorder, or replace filters entirely:

```csharp
// Replace the entire pipeline with a single custom filter
server.UseHtmlPipeline(pipeline =>
{
    pipeline.Clear();
    pipeline.Use<OpenGraphFilter>();
});
```

```csharp
// Append additional filters via the pipeline configurator
server.UseHtmlPipeline(pipeline =>
{
    pipeline.Use<OpenGraphFilter>();
});
```

The pipeline configurator runs **after** all built-in and custom filters have been added, so `Clear()` removes everything, giving you full control.

## Inspecting the Pipeline

The `HtmlPipeline.Filters` property returns a read-only list of the currently registered filters:

```csharp
server.UseHtmlPipeline(pipeline =>
{
    foreach (var filter in pipeline.Filters)
    {
        Console.WriteLine(filter.GetType().Name);
    }
});
```
