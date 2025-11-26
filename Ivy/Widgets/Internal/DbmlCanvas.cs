using Ivy.Core;

namespace Ivy.Widgets.Internal;

public record DbmlCanvas : WidgetBase<DbmlCanvas>
{
    public DbmlCanvas(string? dbml)
    {
        Dbml = dbml;
    }

    [Prop] public string? Dbml { get; set; }
}
