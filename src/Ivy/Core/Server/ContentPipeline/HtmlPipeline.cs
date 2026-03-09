namespace Ivy.Core.Server.ContentPipeline;

public class HtmlPipeline
{
    private readonly List<IHtmlFilter> _filters = new();

    public HtmlPipeline Use(IHtmlFilter filter)
    {
        _filters.Add(filter);
        return this;
    }

    public HtmlPipeline Use<T>() where T : IHtmlFilter, new() => Use(new T());

    public string Process(HtmlPipelineContext context, string html)
    {
        foreach (var filter in _filters)
            html = filter.Process(context, html);
        return html;
    }
}
