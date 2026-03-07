using Ivy.Core.Server;
using Ivy.Core.Server.ContentPipeline;
using Ivy.Core.Server.ContentPipeline.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Tests.ContentPipeline;

public class HtmlPipelineTests
{
    private const string SampleHtml = "<html><head><title>Ivy</title></head><body></body></html>";

    private static HtmlPipelineContext CreateContext(Action<IServiceCollection>? configureServices = null, ServerArgs? args = null)
    {
        var services = new ServiceCollection();
        configureServices?.Invoke(services);
        var provider = services.BuildServiceProvider();
        return new HtmlPipelineContext
        {
            Services = provider,
            ServerArgs = args ?? new ServerArgs()
        };
    }

    [Fact]
    public void Pipeline_RunsFiltersInOrder()
    {
        var order = new List<int>();
        var pipeline = new HtmlPipeline()
            .Use(new TrackingFilter(order, 1, "<!-- first -->"))
            .Use(new TrackingFilter(order, 2, "<!-- second -->"));

        var context = CreateContext(s => s.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build()));
        var result = pipeline.Process(context, SampleHtml);

        Assert.Equal([1, 2], order);
        Assert.Contains("<!-- first -->", result);
        Assert.Contains("<!-- second -->", result);
    }

    [Fact]
    public void LicenseFilter_InjectsLicenseMetaTag()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Ivy:License"] = "test-license-key" })
            .Build();

        var context = CreateContext(s => s.AddSingleton<IConfiguration>(config));
        var filter = new LicenseFilter();
        var result = filter.Process(context, SampleHtml);

        Assert.Contains("<meta name=\"ivy-license\" content=\"test-license-key\" />", result);
    }

    [Fact]
    public void LicenseFilter_SkipsWhenNoLicense()
    {
        var config = new ConfigurationBuilder().Build();
        var context = CreateContext(s => s.AddSingleton<IConfiguration>(config));
        var filter = new LicenseFilter();
        var result = filter.Process(context, SampleHtml);

        Assert.DoesNotContain("ivy-license", result);
    }

    [Fact]
    public void DevToolsFilter_InjectsWhenEnabled()
    {
        var args = new ServerArgs { EnableDevTools = true };
        var context = CreateContext(args: args);
        var filter = new DevToolsFilter();
        var result = filter.Process(context, SampleHtml);

        Assert.Contains("<meta name=\"ivy-enable-dev-tools\" content=\"true\" />", result);
    }

    [Fact]
    public void DevToolsFilter_SkipsWhenDisabled()
    {
        var context = CreateContext();
        var filter = new DevToolsFilter();
        var result = filter.Process(context, SampleHtml);

        Assert.DoesNotContain("ivy-enable-dev-tools", result);
    }

    [Fact]
    public void MetaDescriptionFilter_InjectsDescription()
    {
        var args = new ServerArgs { MetaDescription = "My app description" };
        var context = CreateContext(args: args);
        var filter = new MetaDescriptionFilter();
        var result = filter.Process(context, SampleHtml);

        Assert.Contains("<meta name=\"description\" content=\"My app description\" />", result);
    }

    [Fact]
    public void TitleFilter_ReplacesTitle()
    {
        var args = new ServerArgs { MetaTitle = "My Custom Title" };
        var context = CreateContext(args: args);
        var filter = new TitleFilter();
        var result = filter.Process(context, SampleHtml);

        Assert.Contains("<title>My Custom Title</title>", result);
        Assert.DoesNotContain("<title>Ivy</title>", result);
    }

    [Fact]
    public void ThemeFilter_SkipsWhenNoThemeService()
    {
        var context = CreateContext();
        var filter = new ThemeFilter();
        var result = filter.Process(context, SampleHtml);

        Assert.Equal(SampleHtml, result);
    }

    [Fact]
    public void ManifestFilter_InjectsLinkWhenRegistered()
    {
        var context = CreateContext(s => s.AddSingleton(new ManifestOptions()));
        var filter = new ManifestFilter();
        var result = filter.Process(context, SampleHtml);

        Assert.Contains("<link rel=\"manifest\" href=\"/manifest.json\" />", result);
    }

    [Fact]
    public void ManifestFilter_SkipsWhenNotRegistered()
    {
        var context = CreateContext();
        var filter = new ManifestFilter();
        var result = filter.Process(context, SampleHtml);

        Assert.DoesNotContain("manifest", result);
    }

    private class TrackingFilter(List<int> order, int id, string tag) : IHtmlFilter
    {
        public string Process(HtmlPipelineContext context, string html)
        {
            order.Add(id);
            return html.Replace("</head>", $"  {tag}\n</head>");
        }
    }
}
