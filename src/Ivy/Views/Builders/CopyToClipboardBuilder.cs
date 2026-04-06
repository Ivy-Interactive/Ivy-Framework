// ReSharper disable once CheckNamespace
namespace Ivy;

public class CopyToClipboardBuilder<TModel> : IBuilder<TModel>
{
    public object? Build(object? value, TModel record)
    {
        if (value == null)
        {
            return null;
        }

        return new FuncView((context) =>
        {
            var client = context.UseService<IClientProvider>();
            return Layout.Horizontal().Gap(1).AlignContent(Align.Center)
                   | value
                   | Icons.ClipboardCopy.ToButton(@event =>
                   {
                       client.CopyToClipboard(value.ToString()!);
                       client.Toast($"Copied '{value}' to clipboard", "Value Copied");
                   }).Variant(ButtonVariant.Ghost);
        });
    }
}
