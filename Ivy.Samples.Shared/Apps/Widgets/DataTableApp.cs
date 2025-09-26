using Ivy.Samples.Shared.Apps;
using Ivy.Shared;
using Ivy.Views.DataTables;

namespace Ivy.Samples.Apps.Widgets;

public record Foo(string Name, string LastName, int Age);

[App(icon: Icons.DatabaseZap)]
public class DataTableApp : SampleBase
{
    protected override object? BuildSample()
    {
        IQueryable<User> users = SampleData.GetUsers(1000).AsQueryable();
        return users.ToDataTable()
            .Header(e => e.Name, "First Name");
    }
}