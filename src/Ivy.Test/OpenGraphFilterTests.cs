using Ivy.Core.Server.HtmlPipeline;
using Ivy.Core.Server.HtmlPipeline.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Test;

public class OpenGraphFilterTests
{
    private const string SampleHtml = """
        <!doctype html>
        <html lang="en">
          <head>
            <meta charset="UTF-8" />
            <title>Ivy</title>
          </head>
          <body>
            <div id="root"></div>
          </body>
        </html>
        """;

    private static HtmlPipelineContext CreateContext(ServerArgs? args = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        var sp = services.BuildServiceProvider();
        return new HtmlPipelineContext
        {
            Services = sp,
            ServerArgs = args ?? new ServerArgs()
        };
    }

    [Fact]
    public void Should_AddOgTitle_FromMetaTitle()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext(new ServerArgs { Metadata = new ServerMetadata { Title = "Test App" } });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("property=\"og:title\" content=\"Test App\"", result);
    }

    [Fact]
    public void Should_FallbackTitle_ToAssemblyName_WhenMetaTitleIsNull()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext();

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("property=\"og:title\"", result);
    }

    [Fact]
    public void Should_AddOgDescription_WhenMetaDescriptionIsSet()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext(new ServerArgs
        {
            Metadata = new ServerMetadata { Title = "App", Description = "A cool app" }
        });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("property=\"og:description\" content=\"A cool app\"", result);
    }

    [Fact]
    public void Should_NotAddOgDescription_WhenMetaDescriptionIsNull()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext(new ServerArgs { Metadata = new ServerMetadata { Title = "App" } });

        var result = pipeline.Process(context, SampleHtml);

        Assert.DoesNotContain("og:description", result);
    }

    [Fact]
    public void Should_AutoDeriveOgImage_FromGitHubUrl()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext(new ServerArgs
        {
            Metadata = new ServerMetadata
            {
                Title = "My App",
                GitHubUrl = "https://github.com/owner/repo"
            }
        });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("property=\"og:image\" content=\"https://banner.ivy.app/ogImage?text=My+App&amp;repo=owner/repo\"", result);
    }

    [Fact]
    public void Should_UseExplicitOgImage_WhenSet()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext(new ServerArgs
        {
            Metadata = new ServerMetadata
            {
                Title = "App",
                OgImage = "https://example.com/img.png",
                GitHubUrl = "https://github.com/owner/repo"
            }
        });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("property=\"og:image\" content=\"https://example.com/img.png\"", result);
    }

    [Fact]
    public void Should_NotAddOgImage_WhenNoGitHubUrlAndNoExplicit()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext(new ServerArgs { Metadata = new ServerMetadata { Title = "App" } });

        var result = pipeline.Process(context, SampleHtml);

        Assert.DoesNotContain("og:image", result);
    }

    [Fact]
    public void Should_UseExplicitSiteName_WhenSet()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext(new ServerArgs
        {
            Metadata = new ServerMetadata { Title = "App", OgSiteName = "Custom Site" }
        });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("property=\"og:site_name\" content=\"Custom Site\"", result);
    }

    [Fact]
    public void Should_AddTwitterCard_Tags()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext(new ServerArgs
        {
            Metadata = new ServerMetadata
            {
                Title = "App",
                GitHubUrl = "https://github.com/owner/repo"
            }
        });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("name=\"twitter:card\" content=\"summary_large_image\"", result);
        Assert.Contains("name=\"twitter:title\" content=\"App\"", result);
        Assert.Contains("name=\"twitter:image\"", result);
    }

    [Fact]
    public void Should_AddOgType_DefaultsToWebsite()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext(new ServerArgs { Metadata = new ServerMetadata { Title = "App" } });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("property=\"og:type\" content=\"website\"", result);
    }

    [Fact]
    public void Should_AddOgImageDimensions_WhenImagePresent()
    {
        var pipeline = new HtmlPipeline().Use<OpenGraphFilter>();
        var context = CreateContext(new ServerArgs
        {
            Metadata = new ServerMetadata
            {
                Title = "App",
                GitHubUrl = "https://github.com/owner/repo"
            }
        });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("property=\"og:image:width\" content=\"1200\"", result);
        Assert.Contains("property=\"og:image:height\" content=\"630\"", result);
    }
}
