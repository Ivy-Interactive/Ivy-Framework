namespace Ivy.Builders;

public class LinkBuilder<TModel> : IBuilder<TModel>
{
    public object? Build(object? value, TModel record)
    {
        if (value == null)
        {
            return null;
        }

        var url = value.ToString() ?? string.Empty;

        return new Button(url, variant: ButtonVariant.Link).Url(url);
    }
}