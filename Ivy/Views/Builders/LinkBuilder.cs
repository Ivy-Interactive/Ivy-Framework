using Ivy;

namespace Ivy.Views.Builders;

public class LinkBuilder<TModel>(string? url = null, string? label = null) : IBuilder<TModel>
{
    public object? Build(object? value, TModel record)
    {
        if (value == null)
        {
            return null;
        }

        var actualUrl = url ?? value.ToString() ?? string.Empty;

        // Validate URL to prevent open redirect vulnerabilities
        var validatedUrl = Utils.ValidateLinkUrl(actualUrl);
        if (validatedUrl == null)
        {
            // Invalid URL, return button with disabled state or empty URL
            return new Button(label ?? actualUrl, variant: ButtonVariant.Inline).Disabled(true);
        }

        return new Button(label ?? validatedUrl, variant: ButtonVariant.Inline).Url(validatedUrl);
    }
}