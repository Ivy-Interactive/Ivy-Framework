---
searchHints:
  - iframe
  - external
  - integration
  - widget
  - embed
  - content
---

# Embed

<Ingress>
Embed external content from social media platforms, code repositories, and other web resources with automatic responsive containers and platform-specific optimizations.
</Ingress>

The `Embed` widget allows you to incorporate external content such as videos, social media posts, code repositories, and other web resources into your app. It automatically detects the content type and creates an appropriate responsive container.

## Basic Usage

Simply provide a URL to embed content from supported platforms:

```csharp demo-below
new Embed("https://www.youtube.com/watch?v=dQw4w9WgXcQ")
```

### Social Media Posts

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.H4("Twitter Tweet")
    | new Embed("https://publish.twitter.com/?url=https://twitter.com/_devJNS/status/1969643853691949555#")
    | Text.H4("Instagram Post")
    | new Embed("https://www.instagram.com/p/CSGnc0GlZ7R/?img_index=1")
    | Text.H4("LinkedIn Post")
    | new Embed("https://www.linkedin.com/posts/ivy-interactive_ai-dotnet-opensource-activity-7377309652004331520-YjqC")
    | Text.H4("Reddit Post")
    | new Embed("https://www.reddit.com/r/cats/comments/1nr7fbs/show_them/")
    | Text.H4("Pinterest Pin")
    | new Embed("https://pin.it/i/4yA1hkh77/")
    | Text.H4("Facebook post")
    | new Embed("https://www.facebook.com/share/p/1NRYEoLAnJ/")
    | Text.H4("TikTok Video")
    | new Embed("https://www.tiktok.com/@ivan.wllb/video/7550352363689741590")
```

### GitHub Content

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.H4("Repository")
    | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework")
    | Text.H4("Issue")
    | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework/issues/935")
    | Text.H4("Pull Request")
    | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework/pull/123")
    | Text.H4("Gist")
    | new Embed("https://gist.github.com/username/gistid")
    | Text.H4("GitHub Codespace")
    | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
```

<WidgetDocs Type="Ivy.Embed" ExtensionTypes="Ivy.EmbedExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Embed.cs"/>
