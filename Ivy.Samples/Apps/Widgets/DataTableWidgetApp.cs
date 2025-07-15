using Ivy.DataTables;
using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets;

[App(icon: Icons.DatabaseZap)]
public class DataTableWidgetApp : SampleBase
{
    protected override object? BuildSample()
    {
        IQueryable<User> users = SampleData.GetUsers(100).AsQueryable();
        return users.ToDataTable()
            .Header(e => e.Name, "First Name")
            .Header(e => e.Email, "Email Address")
            .Header(e => e.Age, "Age")
            .Width(Size.Full())
            .Height(Size.Fraction(0.8f));
    }
} 