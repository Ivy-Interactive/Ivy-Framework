using System.Reflection;

namespace Ivy.Docs.Helpers;

public class EnumValuesView(Type type) : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        if (!type.GetTypeInfo().IsEnum)
        {
            return Text.Danger("EnumDisplayView: Type is not an enum.");
        }

        var enumValues = Enum.GetValues(type).Cast<Enum>().Select(e => Enum.GetName(type, e)).ToList();

        void CopyToClipboard(Event<ListItem> @event)
        {
            var value = type.Name + "." + @event.Sender.Title;
            client.CopyToClipboard(value);
            client.Toast($"{value} copied to clipboard.");
        }
        return Layout.Vertical().Gap(2)
               | Text.H1(type.Name)
               | new List(enumValues.Select(e => new ListItem(e, onClick: CopyToClipboard)))
            ;
    }
}