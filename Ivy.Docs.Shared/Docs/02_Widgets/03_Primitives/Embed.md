# Embed

The `Embed` widget allows you to incorporate external content such as videos, maps, or other web resources into your app. It creates a responsive container for embedded content.

## Supported Platforms

The Embed widget supports a wide range of social media and content platforms:

- **YouTube** - Videos and playlists
- **Twitter/X** - Tweets and posts
- **Facebook** - Posts and videos
- **Instagram** - Posts and stories
- **TikTok** - Videos
- **LinkedIn** - Posts and articles
- **Pinterest** - Pins and boards
- **GitHub** - Repositories, issues, pull requests, gists, and files (using emgithub.com)
- **Reddit** - Posts and comments

```csharp demo-tabs
public class ResourcesView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.H3("YouTube Video")
            | new Embed("https://www.youtube.com/watch?v=dQw4w9WgXcQ")
            | Text.H3("GitHub Repo")
            | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework")
            | Text.H3("GitHub File")
            | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/README.md")
            | Text.H3("Reddit Post")
            | new Embed("https://www.reddit.com/r/cats/comments/1nr7fbs/show_them/")
            | Text.H3("Twitter Tweet")
            | new Embed("https://twitter.com/username/status/1234567890")
            | Text.H3("Instagram")
            | new Embed("https://www.instagram.com/p/DPBZl8zDGiA/")
            | Text.H3("LinkedIn")
            | new Embed("https://www.linkedin.com/posts/ivy-interactive_ai-dotnet-opensource-activity-7377309652004331520-YjqC?utm_source=share&utm_medium=member_desktop&rcm=ACoAAFd2BW0BpNAXf_IY-1TO5Br9SnDmmmYsuXk")
            | Text.H3("Pinterest")
            | new Embed("https://pin.it/i/3jntlkhhj/")
            | Text.H3("TikTok")
            | new Embed("https://www.tiktok.com/@ivan.wllb/video/7550352363689741590?q=programming&t=1758927009747");
    }
}
```

<WidgetDocs Type="Ivy.Embed" ExtensionTypes="Ivy.EmbedExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Embed.cs"/>
