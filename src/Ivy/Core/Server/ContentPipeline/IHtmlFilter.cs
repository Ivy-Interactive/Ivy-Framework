namespace Ivy.Core.Server.ContentPipeline;

public interface IHtmlFilter
{
    string Process(HtmlPipelineContext context, string html);
}

public class HtmlPipelineContext
{
    public required IServiceProvider Services { get; init; }
    public required ServerArgs ServerArgs { get; init; }
}
