namespace Ivy.Builders;

public class LinkBuilder<TModel>(string? url = null, string? label = null) : IBuilder<TModel>
{
    public object? Build(object? value, TModel record)
    {
        if (value == null)
        {
            return null;
        }

        var actualUrl = url ?? value.ToString() ?? string.Empty;

        return new Button(label ?? actualUrl, variant: ButtonVariant.Inline).Url(actualUrl);
    }
}