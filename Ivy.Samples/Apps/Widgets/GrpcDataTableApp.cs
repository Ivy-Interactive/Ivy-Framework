using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets;

[App(icon: Icons.Database)]
public class GrpcDataTableApp : SampleBase
{
    public GrpcDataTableApp() : base(Align.TopLeft)
    {
    }

    protected override object? BuildSample()
    {
        return Layout.Vertical()
            .Gap(4)
            | Text.H1("gRPC DataTable Examples")
            | Text.H2("Simple gRPC DataTable with sorting and filtering")
            | new GrpcDataTable()
                .ServerUrl("http://localhost:50051")
                .SelectColumns("id", "name", "email", "created_at", "status")
                .SortBy("name", SortDirection.Asc)
                .SortBy("created_at", SortDirection.Desc)
                .Where("status", "equals", "active")
                .Limit(50)
                .Title("Active Users")
                .Description("Users with active status, sorted by name")
                .ShowStatus(true)
                .ShowRefreshButton(true)
                .ResizableColumns(true)
            | Text.H2("Complex gRPC DataTable with aggregations")
            | new GrpcDataTable()
                .ServerUrl("http://localhost:50051")
                .SelectColumns("country", "revenue", "users", "rating")
                .WhereAnd(
                    DataTableExtensions.Equals("status", "active"),
                    DataTableExtensions.GreaterThan("age", 18),
                    new Filter
                    {
                        Group = new FilterGroup
                        {
                            Op = LogicalOperator.Or,
                            Filters = new[]
                            {
                                DataTableExtensions.InSet("country", "US", "CA", "UK"),
                                DataTableExtensions.Equals("premium", true)
                            }
                        }
                    }
                )
                .Aggregate("revenue", "sum")
                .Aggregate("users", "count")
                .Aggregate("rating", "avg")
                .SortBy("revenue", SortDirection.Desc)
                .Limit(20)
                .Title("Revenue Analytics")
                .Description("Revenue analysis by country with complex filtering")
                .ShowStatus(true)
                .ShowRefreshButton(true)
            | Text.H2("Simple query with text search")
            | new GrpcDataTable()
                .ServerUrl("http://localhost:50051")
                .SelectColumns("id", "title", "content", "author")
                .Where("title", "contains", "important")
                .Where("author", "inSet", "admin", "moderator")
                .SortBy("created_at", SortDirection.Desc)
                .Limit(25)
                .Title("Important Posts")
                .Description("Posts with 'important' in title by admin/moderator")
                .ShowStatus(true)
                .ShowRefreshButton(true);
    }
}