namespace Ivy.Builders;

public class LinkBuilder<TModel>(string? url = null) : IBuilder<TModel>
{
    public object? Build(object? value, TModel record)
    {
        if (value == null)
        {
            return null;
        }

        var actualUrl = url ?? value.ToString() ?? string.Empty;

        return new Button(url, variant: ButtonVariant.Link).Url(actualUrl);
    }
}