namespace Ivy.Views.Builders;

public class ImageBuilder<TModel> : IBuilder<TModel>
{
    private Size? _width;
    private Size? _height;
    private string? _alt;
    private ImageFit _objectFit = ImageFit.Contain;

    public ImageBuilder<TModel> Width(Size width)
    {
        _width = width;
        return this;
    }

    public ImageBuilder<TModel> Height(Size height)
    {
        _height = height;
        return this;
    }

    public ImageBuilder<TModel> Alt(string alt)
    {
        _alt = alt;
        return this;
    }

    public ImageBuilder<TModel> ObjectFit(ImageFit fit)
    {
        _objectFit = fit;
        return this;
    }

    public object? Build(object? value, TModel record)
    {
        if (value == null)
        {
            return null;
        }

        var src = value.ToString();
        if (string.IsNullOrWhiteSpace(src))
        {
            return null;
        }

        var image = new Image(src)
            .Width(_width ?? Size.Units(8))
            .Height(_height ?? Size.Units(8));

        if (_alt != null)
        {
            image = image with { Alt = _alt };
        }

        image = image with { ObjectFit = _objectFit };

        return image;
    }
}
