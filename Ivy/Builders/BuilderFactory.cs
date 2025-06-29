namespace Ivy.Builders;

public interface IBuilderFactory<TModel>
{
}

public class BuilderFactory<TModel> : IBuilderFactory<TModel>
{
}

public static class BuilderFactoryExtensions
{
    public static IBuilder<TModel> Default<TModel>(this IBuilderFactory<TModel> factory)
    {
        return new DefaultBuilder<TModel>();
    }

    public static IBuilder<TModel> Text<TModel>(this IBuilderFactory<TModel> factory)
    {
        return new TextBuilder<TModel>();
    }

    public static IBuilder<TModel> Number<TModel>(this IBuilderFactory<TModel> factory)
    {
        return new NumberBuilder<TModel>();
    }

    public static IBuilder<TModel> Link<TModel>(this IBuilderFactory<TModel> factory)
    {
        return new LinkBuilder<TModel>();
    }

    public static IBuilder<TModel> CopyToClipboard<TModel>(this IBuilderFactory<TModel> factory)
    {
        return new CopyToClipboardBuilder<TModel>();
    }
}
