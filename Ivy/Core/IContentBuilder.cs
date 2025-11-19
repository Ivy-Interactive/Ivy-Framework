namespace Ivy.Core;

public interface IContentBuilder
{
    public bool CanHandle(object? content);

    public object? Format(object? content);
}