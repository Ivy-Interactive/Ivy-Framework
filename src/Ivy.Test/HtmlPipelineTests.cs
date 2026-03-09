using System.Xml.Linq;
using Ivy.Core.Server.HtmlPipeline;
using Ivy.Core.Server.HtmlPipeline.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Test;

public class HtmlPipelineTests
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

    private static HtmlPipelineContext CreateContext(ServerArgs? args = null, Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        configureServices?.Invoke(services);
        var sp = services.BuildServiceProvider();
        return new HtmlPipelineContext
        {
            Services = sp,
            ServerArgs = args ?? new ServerArgs()
        };
    }

    [Fact]
    public void EmptyPipeline_PreservesHtmlStructure()
    {
        var pipeline = new HtmlPipeline();
        var context = CreateContext();

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("<!doctype html>", result);
        Assert.Contains("<html", result);
        Assert.Contains("<head>", result);
        Assert.Contains("<title>Ivy</title>", result);
        Assert.Contains("<div id=\"root\"></div>", result);
        Assert.Contains("</body>", result);
    }

    [Fact]
    public void TitleFilter_ReplacesExistingTitle()
    {
        var pipeline = new HtmlPipeline().Use<TitleFilter>();
        var context = CreateContext(new ServerArgs { MetaTitle = "My App" });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("<title>My App</title>", result);
        Assert.DoesNotContain("<title>Ivy</title>", result);
    }

    [Fact]
    public void TitleFilter_NoOp_WhenMetaTitleIsNull()
    {
        var pipeline = new HtmlPipeline().Use<TitleFilter>();
        var context = CreateContext();

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("<title>Ivy</title>", result);
    }

    [Fact]
    public void MetaDescriptionFilter_AddsMetaTag()
    {
        var pipeline = new HtmlPipeline().Use<MetaDescriptionFilter>();
        var context = CreateContext(new ServerArgs { MetaDescription = "A cool app" });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("<meta name=\"description\" content=\"A cool app\" />", result);
    }

    [Fact]
    public void DevToolsFilter_AddsMetaTag_WhenEnabled()
    {
        var pipeline = new HtmlPipeline().Use<DevToolsFilter>();
        var context = CreateContext(new ServerArgs { EnableDevTools = true });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("<meta name=\"ivy-enable-dev-tools\" content=\"true\" />", result);
    }

    [Fact]
    public void DevToolsFilter_NoOp_WhenDisabled()
    {
        var pipeline = new HtmlPipeline().Use<DevToolsFilter>();
        var context = CreateContext(new ServerArgs { EnableDevTools = false });

        var result = pipeline.Process(context, SampleHtml);

        Assert.DoesNotContain("ivy-enable-dev-tools", result);
    }

    [Fact]
    public void LicenseFilter_AddsLicenseMeta_WhenConfigured()
    {
        var pipeline = new HtmlPipeline().Use<LicenseFilter>();
        var context = CreateContext(configureServices: services =>
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Ivy:License"] = "test-license-key"
                })
                .Build();
            services.AddSingleton<IConfiguration>(config);
        });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("<meta name=\"ivy-license\" content=\"test-license-key\" />", result);
    }

    [Fact]
    public void Clear_RemovesAllFilters()
    {
        var pipeline = new HtmlPipeline()
            .Use<TitleFilter>()
            .Use<MetaDescriptionFilter>();

        Assert.Equal(2, pipeline.Filters.Count);

        pipeline.Clear();

        Assert.Empty(pipeline.Filters);

        var context = CreateContext(new ServerArgs { MetaTitle = "Changed", MetaDescription = "Desc" });
        var result = pipeline.Process(context, SampleHtml);

        // No filters ran, so title should be unchanged
        Assert.Contains("<title>Ivy</title>", result);
        Assert.DoesNotContain("name=\"description\"", result);
    }

    [Fact]
    public void UseHtmlPipeline_CanReplaceEntirePipeline()
    {
        var server = new Server(new ServerArgs());
        server.UseHtmlPipeline(p =>
        {
            p.Clear();
            p.Use<MetaDescriptionFilter>();
        });

        var configurator = server.GetPipelineConfigurator();
        Assert.NotNull(configurator);

        // Simulate what UseFrontend does
        var pipeline = new HtmlPipeline()
            .Use<LicenseFilter>()
            .Use<DevToolsFilter>()
            .Use<TitleFilter>();

        configurator!(pipeline);

        // After configurator runs, only MetaDescriptionFilter should remain
        Assert.Single(pipeline.Filters);
        Assert.IsType<MetaDescriptionFilter>(pipeline.Filters[0]);
    }

    [Fact]
    public void ManifestFilter_AddsLink_WhenManifestRegistered()
    {
        var pipeline = new HtmlPipeline().Use<ManifestFilter>();
        var context = CreateContext(configureServices: services =>
        {
            services.AddSingleton(new Ivy.Core.Server.ManifestOptions());
        });

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("<link rel=\"manifest\" href=\"/manifest.json\" />", result);
    }

    [Fact]
    public void ManifestFilter_NoOp_WhenNoManifest()
    {
        var pipeline = new HtmlPipeline().Use<ManifestFilter>();
        var context = CreateContext();

        var result = pipeline.Process(context, SampleHtml);

        Assert.DoesNotContain("manifest", result);
    }

    [Fact]
    public void BareAttributes_AreNormalized()
    {
        var htmlWithCrossorigin = """
            <!doctype html>
            <html lang="en">
              <head>
                <script type="module" crossorigin src="/assets/main.js"></script>
                <title>Test</title>
              </head>
              <body>
                <div id="root"></div>
              </body>
            </html>
            """;

        var pipeline = new HtmlPipeline();
        var context = CreateContext();

        // Should not throw (crossorigin without value is invalid XML)
        var result = pipeline.Process(context, htmlWithCrossorigin);

        Assert.Contains("<script", result);
        Assert.Contains("</script>", result);
    }

    [Fact]
    public void NonSelfClosedLinkTags_ParseSuccessfully()
    {
        var html = """
            <!doctype html>
            <html lang="en">
              <head>
                <link rel="stylesheet" href="test.css">
                <title>Test</title>
              </head>
              <body>
                <div id="root"></div>
              </body>
            </html>
            """;

        var pipeline = new HtmlPipeline();
        var context = CreateContext();

        var result = pipeline.Process(context, html);

        Assert.Contains("link", result);
        Assert.Contains("test.css", result);
    }

    [Fact]
    public void MixedSelfClosedAndNonSelfClosed_VoidElements_Survive()
    {
        var html = """
            <!doctype html>
            <html lang="en">
              <head>
                <meta charset="UTF-8" />
                <link rel="stylesheet" href="x">
                <title>Test</title>
              </head>
              <body>
                <div id="root"></div>
              </body>
            </html>
            """;

        var pipeline = new HtmlPipeline();
        var context = CreateContext();

        var result = pipeline.Process(context, html);

        Assert.Contains("meta", result);
        Assert.Contains("link", result);
        Assert.Contains("charset", result);
        Assert.Contains("href=\"x\"", result);
    }

    [Fact]
    public void DistIndexHtml_WithModulepreloadLinks_ParsesSuccessfully()
    {
        var html = """
            <!doctype html>
            <html lang="en">
              <head>
                <script type="module" crossorigin src="/assets/main-abc123.js"></script>
                <link rel="modulepreload" crossorigin href="/assets/vendor-react.CRhFMK9I.js">
                <link rel="modulepreload" crossorigin href="/assets/vendor-signalr.D2xF4k9J.js">
                <link rel="stylesheet" crossorigin href="/assets/main-xyz789.css">
                <meta charset="UTF-8" />
                <title>Ivy</title>
              </head>
              <body>
                <div id="root"></div>
              </body>
            </html>
            """;

        var pipeline = new HtmlPipeline();
        var context = CreateContext();

        var result = pipeline.Process(context, html);

        Assert.Contains("vendor-react", result);
        Assert.Contains("vendor-signalr", result);
        Assert.Contains("main-xyz789.css", result);
    }

    private class CustomTestFilter : IHtmlFilter
    {
        public void Process(HtmlPipelineContext context, XDocument document)
        {
            var head = document.Root?.Element("head");
            head?.Add(new XElement("meta",
                new XAttribute("name", "custom"),
                new XAttribute("content", "test")));
        }
    }

    [Fact]
    public void CustomFilter_CanBeAdded()
    {
        var pipeline = new HtmlPipeline().Use(new CustomTestFilter());
        var context = CreateContext();

        var result = pipeline.Process(context, SampleHtml);

        Assert.Contains("<meta name=\"custom\" content=\"test\" />", result);
    }
}
