using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets;

[App(icon: Icons.Table, path: ["Widgets"])]
public class DataTableApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("DataTable Widget Demo")
               | Text.H2("This widget connects to a gRPC server providing Apache Arrow tables stream service.")
               | new Separator()
               | Layout.Grid().Columns(2).Gap(16)
                   | new Card(
                       Layout.Vertical()
                       | Text.H3("Basic DataTable")
                       | Text.Block("Simple table with query input and refresh button")
                       | new DataTable()
                           .ServerUrl("http://localhost:50051")
                           .Query("SELECT * FROM users LIMIT 10")
                           .Title("Users Table")
                           .Description("Streaming user data from gRPC server")
                           .Limit(10)
                           .ShowQueryInput()
                           .ShowRefreshButton()
                           .ShowStatus()
                           .ResizableColumns()
                   )
                   | new Card(
                       Layout.Vertical()
                       | Text.H3("Read-Only DataTable")
                       | Text.Block("Table without query input for display only")
                       | new DataTable()
                           .ServerUrl("http://localhost:50051")
                           .Query("SELECT id, name, email, created_at FROM users ORDER BY created_at DESC LIMIT 20")
                           .Title("Recent Users")
                           .Description("Latest user registrations")
                           .Limit(20)
                           .ShowQueryInput(false)
                           .ShowRefreshButton()
                           .ShowStatus()
                   )
                   | new Card(
                       Layout.Vertical()
                       | Text.H3("Custom DataTable")
                       | Text.Block("Table with custom configuration")
                       | new DataTable()
                           .ServerUrl("http://localhost:50051")
                           .Query("SELECT product_id, name, price, category FROM products WHERE price > 100")
                           .Title("Premium Products")
                           .Description("Products with price over $100 - 10,000 examples")
                           .Limit(10000)
                           .Offset(0)
                           .ShowQueryInput()
                           .ShowRefreshButton()
                           .ShowStatus()
                   )
                   | new Card(
                       Layout.Vertical()
                       | Text.H3("Analytics DataTable")
                       | Text.Block("Table for displaying analytics data")
                       | new DataTable()
                           .ServerUrl("http://localhost:50051")
                           .Query("SELECT date, revenue, orders, avg_order_value FROM daily_analytics WHERE date >= '2024-01-01'")
                           .Title("Daily Analytics")
                           .Description("Revenue and order analytics by date")
                           .Limit(100)
                           .ShowQueryInput()
                           .ShowRefreshButton()
                           .ShowStatus()
                   )
               | new Separator()
               | Text.H2("Configuration Options")
               | new Details(new[]
               {
                   new Detail("ServerUrl", "The URL of the gRPC server providing Apache Arrow tables", false),
                   new Detail("Query", "The SQL query to execute on the server", false),
                   new Detail("Limit", "Maximum number of rows to fetch (default: 100)", false),
                   new Detail("Offset", "Number of rows to skip (default: 0)", false),
                   new Detail("ShowQueryInput", "Whether to show the query input field (default: true)", false),
                   new Detail("ShowRefreshButton", "Whether to show the refresh button (default: true)", false),
                   new Detail("ShowStatus", "Whether to show the status badge (default: true)", false),
                   new Detail("Title", "Optional title for the table", false),
                   new Detail("Description", "Optional description for the table", false)
               })
               | new Separator()
               | Text.H2("Events")
               | new Details(new[]
               {
                   new Detail("OnDataReceived", "Fired when new data is received from the stream", false),
                   new Detail("OnError", "Fired when an error occurs during streaming", false),
                   new Detail("OnStreamComplete", "Fired when the stream completes successfully", false)
               })
            ;
    }
}