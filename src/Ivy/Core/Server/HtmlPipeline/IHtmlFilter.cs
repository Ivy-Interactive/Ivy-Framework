using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline;

public interface IHtmlFilter
{
    void Process(HtmlPipelineContext context, XDocument document);
}

public class HtmlPipelineContext
{
    public required IServiceProvider Services { get; init; }
    public required ServerArgs ServerArgs { get; init; }
}
