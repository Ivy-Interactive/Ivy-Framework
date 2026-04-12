using System.Xml.Linq;
using Ivy.Core.Server.HtmlPipeline;
using Ivy.Core.Server.HtmlPipeline.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Test;

public class BasePathFilterTests
{
    private static XDocument CreateHtmlDocument()
    {
        return XDocument.Parse("<html><head></head><body></body></html>");
    }

    private static HtmlPipelineContext CreateContext(string? basePath)
    {
        var args = new ServerArgs { BasePath = basePath };
        return new HtmlPipelineContext
        {
            ServerArgs = args,
            Services = new ServiceCollection().BuildServiceProvider()
        };
    }

    [Fact]
    public void Process_NoBasePath_InjectsBaseHrefSlash()
    {
        var doc = CreateHtmlDocument();
        var filter = new BasePathFilter();
        filter.Process(CreateContext(null), doc);

        var head = doc.Root!.Element("head")!;
        var baseEl = head.Element("base");
        Assert.NotNull(baseEl);
        Assert.Equal("/", baseEl.Attribute("href")!.Value);
    }

    [Fact]
    public void Process_WithBasePath_InjectsLeadingSlashInMetaTag()
    {
        var doc = CreateHtmlDocument();
        var filter = new BasePathFilter();
        filter.Process(CreateContext("/ivy"), doc);

        var head = doc.Root!.Element("head")!;
        var meta = head.Elements("meta").FirstOrDefault(m => m.Attribute("name")?.Value == "ivy-path-base");
        Assert.NotNull(meta);
        Assert.Equal("/ivy", meta.Attribute("content")!.Value);
    }

    [Fact]
    public void Process_WithBasePathNoLeadingSlash_StillGetsLeadingSlash()
    {
        var doc = CreateHtmlDocument();
        var filter = new BasePathFilter();
        filter.Process(CreateContext("ivy"), doc);

        var head = doc.Root!.Element("head")!;
        var meta = head.Elements("meta").FirstOrDefault(m => m.Attribute("name")?.Value == "ivy-path-base");
        Assert.NotNull(meta);
        Assert.Equal("/ivy", meta.Attribute("content")!.Value);
    }

    [Fact]
    public void Process_WithBasePathTrailingSlash_TrimsTrailingSlash()
    {
        var doc = CreateHtmlDocument();
        var filter = new BasePathFilter();
        filter.Process(CreateContext("/studio/"), doc);

        var head = doc.Root!.Element("head")!;
        var meta = head.Elements("meta").FirstOrDefault(m => m.Attribute("name")?.Value == "ivy-path-base");
        Assert.NotNull(meta);
        Assert.Equal("/studio", meta.Attribute("content")!.Value);
    }

    [Fact]
    public void Process_WithBasePath_BaseHrefHasTrailingSlash()
    {
        var doc = CreateHtmlDocument();
        var filter = new BasePathFilter();
        filter.Process(CreateContext("/ivy"), doc);

        var head = doc.Root!.Element("head")!;
        var baseEl = head.Element("base");
        Assert.NotNull(baseEl);
        Assert.Equal("/ivy/", baseEl.Attribute("href")!.Value);
    }
}
