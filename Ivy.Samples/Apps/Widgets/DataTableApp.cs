using Ivy;

namespace Ivy.Samples.Apps.Widgets;

public class DataTableApp : SampleBase
{
    public override string Title => "DataTable Widget";
    public override string Description => "Demonstrates the DataTable widget with gRPC streaming support";

    protected override View Build()
    {
        return new StackLayout
        {
            new TextBlock("DataTable Widget Demo")
                .Variant(TextBlockVariant.H1)
                .Align(Align.Center),

            new TextBlock("This widget connects to a gRPC server providing Apache Arrow tables stream service.")
                .Variant(TextBlockVariant.Lead)
                .Align(Align.Center),

            new Separator(),

            new GridLayout(columns: 2, gap: 16)
            {
                // Basic DataTable with query input
                new Card
                {
                    new CardHeader
                    {
                        new CardTitle("Basic DataTable"),
                        new CardDescription("Simple table with query input and refresh button")
                    },
                    new CardContent
                    {
                        new DataTable()
                            .ServerUrl("http://localhost:50051")
                            .Query("SELECT * FROM users LIMIT 10")
                            .Title("Users Table")
                            .Description("Streaming user data from gRPC server")
                            .Limit(10)
                            .ShowQueryInput(true)
                            .ShowRefreshButton(true)
                            .ShowStatus(true)
                    }
                },

                // DataTable without query input (read-only)
                new Card
                {
                    new CardHeader
                    {
                        new CardTitle("Read-Only DataTable"),
                        new CardDescription("Table without query input for display only")
                    },
                    new CardContent
                    {
                        new DataTable()
                            .ServerUrl("http://localhost:50051")
                            .Query("SELECT id, name, email, created_at FROM users ORDER BY created_at DESC LIMIT 20")
                            .Title("Recent Users")
                            .Description("Latest user registrations")
                            .Limit(20)
                            .ShowQueryInput(false)
                            .ShowRefreshButton(true)
                            .ShowStatus(true)
                    }
                },

                // DataTable with custom styling
                new Card
                {
                    new CardHeader
                    {
                        new CardTitle("Custom DataTable"),
                        new CardDescription("Table with custom configuration")
                    },
                    new CardContent
                    {
                        new DataTable()
                            .ServerUrl("http://localhost:50051")
                            .Query("SELECT product_id, name, price, category FROM products WHERE price > 100")
                            .Title("Premium Products")
                            .Description("Products with price over $100")
                            .Limit(50)
                            .Offset(0)
                            .ShowQueryInput(true)
                            .ShowRefreshButton(true)
                            .ShowStatus(true)
                    }
                },

                // DataTable for analytics
                new Card
                {
                    new CardHeader
                    {
                        new CardTitle("Analytics DataTable"),
                        new CardDescription("Table for displaying analytics data")
                    },
                    new CardContent
                    {
                        new DataTable()
                            .ServerUrl("http://localhost:50051")
                            .Query("SELECT date, revenue, orders, avg_order_value FROM daily_analytics WHERE date >= '2024-01-01'")
                            .Title("Daily Analytics")
                            .Description("Revenue and order analytics by date")
                            .Limit(100)
                            .ShowQueryInput(true)
                            .ShowRefreshButton(true)
                            .ShowStatus(true)
                    }
                }
            },

            new Separator(),

            new TextBlock("Configuration Options")
                .Variant(TextBlockVariant.H2),

            new Details
            {
                new Detail("ServerUrl", "The URL of the gRPC server providing Apache Arrow tables"),
                new Detail("Query", "The SQL query to execute on the server"),
                new Detail("Limit", "Maximum number of rows to fetch (default: 100)"),
                new Detail("Offset", "Number of rows to skip (default: 0)"),
                new Detail("ShowQueryInput", "Whether to show the query input field (default: true)"),
                new Detail("ShowRefreshButton", "Whether to show the refresh button (default: true)"),
                new Detail("ShowStatus", "Whether to show the status badge (default: true)"),
                new Detail("Title", "Optional title for the table"),
                new Detail("Description", "Optional description for the table")
            },

            new Separator(),

            new TextBlock("Events")
                .Variant(TextBlockVariant.H2),

            new Details
            {
                new Detail("OnDataReceived", "Fired when new data is received from the stream"),
                new Detail("OnError", "Fired when an error occurs during streaming"),
                new Detail("OnStreamComplete", "Fired when the stream completes successfully")
            }
        };
    }
} 