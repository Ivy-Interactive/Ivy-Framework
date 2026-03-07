using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using AiChatMessage = Microsoft.Extensions.AI.ChatMessage;
using AiChatRole = Microsoft.Extensions.AI.ChatRole;

namespace Ivy.Agent.EfQuery;

public class EfQueryAgent<TContext>(IChatClient chatClient, IDbContextFactory<TContext> contextFactory)
    where TContext : DbContext
{
    private const int MaxSqlIterations = 3;
    private const int MaxXamlIterations = 3;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IChatClient _chatClient = chatClient
        .AsBuilder().UseFunctionInvocation().Build();

    public async Task<EfQueryResult> QueryAsync(string query, CancellationToken ct = default)
    {
        long totalInputTokens = 0;
        long totalOutputTokens = 0;
        int totalIterations = 0;

        // Step A: Collect Schema
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var schema = SchemaCollector.CollectSchema(context);

        // Step B: Planning Agent
        var (plan, planUsage) = await RunPlanningAgentAsync(schema, query, ct);
        TrackUsage(planUsage, ref totalInputTokens, ref totalOutputTokens);
        totalIterations++;

        // Step C: SQL Agent
        var (sql, sqlData, sqlUsage, sqlIterations) = await RunSqlAgentAsync(schema, plan, context, ct);
        TrackUsage(sqlUsage, ref totalInputTokens, ref totalOutputTokens);
        totalIterations += sqlIterations;

        // Step D: XAML Agent
        var (xaml, xamlUsage, xamlIterations) = await RunXamlAgentAsync(plan, sql, sqlData, ct);
        TrackUsage(xamlUsage, ref totalInputTokens, ref totalOutputTokens);
        totalIterations += xamlIterations;

        return new EfQueryResult
        {
            Xaml = xaml,
            Sql = sql,
            Plan = plan,
            InputTokens = totalInputTokens,
            OutputTokens = totalOutputTokens,
            Iterations = totalIterations
        };
    }

    private async Task<(string Plan, UsageDetails? Usage)> RunPlanningAgentAsync(
        string schema, string query, CancellationToken ct)
    {
        var widgetList = GetWidgetList();

        var systemPrompt = $"""
            You are a database query planner. Given a schema and user question, output a plan describing what SQL query to write and what XAML visualization to use. Be concise.

            Available XAML widgets for visualization:
            {widgetList}

            Database schema:
            {schema}
            """;

        var messages = new List<AiChatMessage>
        {
            new(AiChatRole.System, systemPrompt),
            new(AiChatRole.User, query)
        };

        var response = await _chatClient.GetResponseAsync(messages, cancellationToken: ct);
        return (response.Text ?? "", response.Usage);
    }

    private async Task<(string Sql, string DataPreview, UsageDetails? Usage, int Iterations)> RunSqlAgentAsync(
        string schema, string plan, DbContext context, CancellationToken ct)
    {
        string finalSql = "";
        string finalDataPreview = "";
        UsageDetails? totalUsage = null;

        var finished = false;

        var chatOptions = new ChatOptions
        {
            Tools =
            [
                AIFunctionFactory.Create(
                    async (string sql) =>
                    {
                        var result = await SqlExecutor.ExecuteAsync(context, sql, ct);
                        if (result.Error != null)
                            return $"ERROR: {result.Error}";

                        finalSql = sql;
                        var preview = FormatDataPreview(result);
                        finalDataPreview = preview;
                        return preview;
                    },
                    name: "execute_sql",
                    description: "Execute a read-only SQL SELECT query and return results. Use this to test your query."
                ),
                AIFunctionFactory.Create(
                    (string sql) =>
                    {
                        finalSql = sql;
                        finished = true;
                        return "SQL finalized.";
                    },
                    name: "finish_sql",
                    description: "Call this when you are satisfied with the SQL query results. Pass the final SQL."
                )
            ]
        };

        var messages = new List<AiChatMessage>
        {
            new(AiChatRole.System, $"""
                You are a SQL expert. Write a read-only SQL SELECT query based on the plan.
                Only SELECT statements are allowed. No INSERT/UPDATE/DELETE/DROP/ALTER/CREATE.
                Limit results to 1000 rows.

                Database schema:
                {schema}

                Plan:
                {plan}

                Use the execute_sql tool to test your query. When satisfied with results, call finish_sql with the final SQL.
                """),
            new(AiChatRole.User, "Write and execute the SQL query described in the plan.")
        };

        int iterations = 0;
        while (!finished && iterations < MaxSqlIterations)
        {
            iterations++;
            var response = await _chatClient.GetResponseAsync(messages, chatOptions, ct);
            totalUsage = MergeUsage(totalUsage, response.Usage);

            if (finished) break;

            messages.AddRange(response.Messages);
            messages.Add(new AiChatMessage(AiChatRole.User,
                "Please call finish_sql when you are satisfied with the query, or try a different query."));
        }

        return (finalSql, finalDataPreview, totalUsage, iterations);
    }

    private async Task<(string Xaml, UsageDetails? Usage, int Iterations)> RunXamlAgentAsync(
        string plan, string sql, string dataPreview, CancellationToken ct)
    {
        var widgetDocs = GetWidgetDocumentation();
        string finalXaml = "";
        var finished = false;

        var chatOptions = new ChatOptions
        {
            Tools =
            [
                AIFunctionFactory.Create(
                    (string xaml) =>
                    {
                        try
                        {
                            var builder = new XamlBuilder();
                            builder.Build(xaml);
                            return "VALID: XAML parsed successfully.";
                        }
                        catch (InvalidOperationException ex)
                        {
                            return $"INVALID: {ex.Message}";
                        }
                        catch (Exception ex)
                        {
                            return $"INVALID: {ex.Message}";
                        }
                    },
                    name: "validate_xaml",
                    description: "Validate Ivy XAML markup. Returns success or error message."
                ),
                AIFunctionFactory.Create(
                    (string xaml) =>
                    {
                        finalXaml = xaml;
                        finished = true;
                        return "XAML finalized.";
                    },
                    name: "finish_xaml",
                    description: "Call this with the final validated XAML."
                )
            ]
        };

        var messages = new List<AiChatMessage>
        {
            new(AiChatRole.System, $"""
                You are a XAML UI builder for the Ivy framework. Generate XAML to display query results.

                Available widgets and their properties:
                {widgetDocs}

                Important XAML rules:
                - Root element should be a layout widget (e.g., StackLayout)
                - Use proper XML syntax with self-closing tags where appropriate
                - Widget properties are set as XML attributes
                - For chart data, use a child element with JSON CDATA: <Data><![CDATA[...json array...]]></Data>
                - Only use widgets and properties listed above

                Plan: {plan}
                SQL: {sql}
                Query Results:
                {dataPreview}

                Generate XAML to visualize these results. Use validate_xaml to test, then call finish_xaml with the final XAML.
                """),
            new(AiChatRole.User, "Generate the XAML visualization for the query results.")
        };

        int iterations = 0;
        while (!finished && iterations < MaxXamlIterations)
        {
            iterations++;
            var response = await _chatClient.GetResponseAsync(messages, chatOptions, ct);

            if (finished) break;

            messages.AddRange(response.Messages);
            messages.Add(new AiChatMessage(AiChatRole.User,
                "Please fix the XAML and try again, or call finish_xaml with valid XAML."));
        }

        UsageDetails? totalUsage = null;
        // We track usage in the loop but simplify here
        return (finalXaml, totalUsage, iterations);
    }

    private static string FormatDataPreview(SqlExecutionResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Columns: {string.Join(", ", result.Columns)}");
        sb.AppendLine($"Row count: {(result.TotalRowCount == -1 ? "1000+ (truncated)" : result.TotalRowCount.ToString())}");

        var previewRows = result.Rows.Take(20).ToList();
        sb.AppendLine($"Preview ({previewRows.Count} rows):");
        sb.AppendLine(JsonSerializer.Serialize(previewRows, JsonOptions));

        return sb.ToString();
    }

    private static string GetWidgetList()
    {
        return """
            Layout: StackLayout
            Data Display: Table, Detail, Details, Json, Xml
            Charts: AreaChart, BarChart, LineChart, PieChart
            Text/Content: TextBlock, Markdown, CodeBlock
            Feedback: Callout
            Containers: Card, Expandable
            Other: Separator, Spacer
            """;
    }

    private static string GetWidgetDocumentation()
    {
        var sb = new StringBuilder();
        var widgetTypes = new[]
        {
            "StackLayout", "Table", "Detail", "Details", "Json", "Xml",
            "AreaChart", "BarChart", "LineChart", "PieChart",
            "TextBlock", "Markdown", "CodeBlock",
            "Callout", "Card", "Expandable",
            "Separator", "Spacer"
        };

        var assembly = typeof(Ivy.Core.AbstractWidget).Assembly;

        foreach (var widgetName in widgetTypes)
        {
            var type = assembly.GetTypes()
                .FirstOrDefault(t => string.Equals(t.Name, widgetName, StringComparison.OrdinalIgnoreCase)
                                     && !t.IsAbstract && !t.IsNested);

            if (type == null) continue;

            sb.AppendLine($"### {type.Name}");

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<Ivy.PropAttribute>() != null)
                .ToList();

            if (props.Count > 0)
            {
                foreach (var prop in props)
                {
                    var propType = prop.PropertyType;
                    var typeName = GetFriendlyTypeName(propType);
                    sb.AppendLine($"  - {prop.Name}: {typeName}");
                }
            }
            else
            {
                sb.AppendLine("  (no configurable properties, uses children)");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(float)) return "float";
        if (type == typeof(double)) return "double";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(object)) return "object";

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
            return $"{GetFriendlyTypeName(underlying)}?";

        if (type.IsArray)
            return $"{GetFriendlyTypeName(type.GetElementType()!)}[]";

        if (type.IsEnum)
            return $"{type.Name} ({string.Join("|", Enum.GetNames(type))})";

        return type.Name;
    }

    private static void TrackUsage(UsageDetails? usage, ref long inputTokens, ref long outputTokens)
    {
        if (usage == null) return;
        inputTokens += usage.InputTokenCount ?? 0;
        outputTokens += usage.OutputTokenCount ?? 0;
    }

    private static UsageDetails? MergeUsage(UsageDetails? existing, UsageDetails? incoming)
    {
        if (incoming == null) return existing;
        if (existing == null) return incoming;

        return new UsageDetails
        {
            InputTokenCount = (existing.InputTokenCount ?? 0) + (incoming.InputTokenCount ?? 0),
            OutputTokenCount = (existing.OutputTokenCount ?? 0) + (incoming.OutputTokenCount ?? 0),
            TotalTokenCount = (existing.TotalTokenCount ?? 0) + (incoming.TotalTokenCount ?? 0)
        };
    }
}
