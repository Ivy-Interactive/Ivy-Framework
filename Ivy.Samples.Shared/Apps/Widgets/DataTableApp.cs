using Ivy.DataTables;
using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets;

public record Foo(string Name, string LastName, int Age);

[App(icon: Icons.DatabaseZap)]
public class DataTableApp : SampleBase
{
    protected override object? BuildSample()
    {
        IQueryable<User> users = SampleData.GetUsers(100).AsQueryable();
        return users.ToDataTable()
            .Header(e => e.Name, "First Name");
    }
}