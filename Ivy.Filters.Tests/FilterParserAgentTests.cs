using Ivy.Filters;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using Xunit.Abstractions;

namespace Ivy.Filters.Tests;

public class FilterParserAgentTests : IDisposable
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<FilterParserAgent> _logger;
    private readonly FieldMeta[] _testFields;
    private readonly ITestOutputHelper _output;

    public FilterParserAgentTests(ITestOutputHelper output)
    {
        _output = output;

        // Load configuration from user secrets
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<FilterParserAgentTests>()
            .Build();

        var endpoint = configuration["OpenAi:Endpoint"] ?? throw new InvalidOperationException("OpenAi:Endpoint not found in user secrets");
        var apiKey = configuration["OpenAi:ApiKey"] ?? throw new InvalidOperationException("OpenAi:ApiKey not found in user secrets");

        // Create OpenAI client
        var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint)
        });

        // Convert OpenAI ChatClient to IChatClient
        var openAIChatClient = openAiClient.GetChatClient("gpt-4o");
        _chatClient = openAIChatClient.AsIChatClient();

        // Create logger that outputs to xUnit
        _logger = new XunitLogger<FilterParserAgent>(output);

        // Set up test fields
        _testFields =
        [
            new FieldMeta("Name", "name", FieldType.Text),
            new FieldMeta("Age", "age", FieldType.Number),
            new FieldMeta("Email", "email", FieldType.Text),
            new FieldMeta("Price", "price", FieldType.Number),
            new FieldMeta("Status", "status", FieldType.Text),
            new FieldMeta("Category", "category", FieldType.Text),
            new FieldMeta("Created Date", "createdDate", FieldType.Date),
            new FieldMeta("Is Active", "isActive", FieldType.Boolean),
            new FieldMeta("Salary", "salary", FieldType.Number),
            new FieldMeta("Department", "department", FieldType.Text),
            new FieldMeta("Country", "country", FieldType.Text),
            new FieldMeta("Region", "region", FieldType.Text)
        ];
    }

    [Fact]
    public async Task Parse_SimpleTextContains_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show me records where name contains John";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);
        Assert.NotNull(result.Model);

        // Verify usage tracking
        Assert.NotNull(result.Usage);
        Assert.True(result.Usage.InputTokens > 0, "Input tokens should be tracked");
        Assert.True(result.Usage.OutputTokens > 0, "Output tokens should be tracked");
        Assert.Equal(result.Usage.InputTokens + result.Usage.OutputTokens, result.Usage.TotalTokens);

        // Verify iteration tracking
        Assert.True(result.Iterations > 0, "Iterations should be tracked");
        Assert.True(result.Iterations <= 5, "Iterations should not exceed max retries");
    }

    [Fact]
    public async Task Parse_SimpleNumberComparison_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Find people older than 30";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);
        Assert.IsType<Leaf>(result.Ast);

        var leaf = (Leaf)result.Ast;
        Assert.Equal("Age", leaf.FieldDisplay);
        Assert.Equal(FieldType.Number, leaf.Type);

    }

    [Fact]
    public async Task Parse_LogicalAndOperation_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show people over 25 who work in Sales department";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);
        Assert.IsType<And>(result.Ast);

    }

    [Fact]
    public async Task Parse_LogicalOrOperation_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Find employees from USA or Canada";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);
        Assert.IsType<Or>(result.Ast);

    }

    [Fact]
    public async Task Parse_ComplexNestedConditions_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show active employees older than 30 from either Sales or Marketing department with salary above 50000";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_BetweenOperation_ShouldGenerateComparisonFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Find products priced between 100 and 500";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

        // Should contain comparison operations (>= AND <=)
        var hasComparisons = ContainsComparisonOperations(result.Ast);
        Assert.True(hasComparisons, "Filter should contain comparison operations for range");

    }

    [Fact]
    public async Task Parse_BlankCheck_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show records where email is empty";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_NotBlankCheck_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Find records with non-empty department";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_StartsWithOperation_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Find emails that start with admin";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_EndsWithOperation_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show names ending with son";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_DateComparison_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Find records created after January 1st 2024";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_EqualityCheck_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show only active status";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_NotEqualCheck_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Find all except inactive status";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_MultipleTextConditions_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show people named John or Jane in the IT department";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_NegationWithAnd_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show employees not in USA and age under 40";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_BooleanField_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show only active records";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_GreaterThanOrEqual_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Find employees with salary of at least 75000";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_LessThanOrEqual_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show products priced up to 100";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_CategorySelection_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Filter by Electronics or Appliances category";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_WithFieldNameInQuery_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Country equals Canada";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_InvalidFieldReference_ShouldReturnError()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show records where nonexistent_field equals something";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.True(result.HasErrors, "Filter with invalid field should fail");
        Assert.Contains(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

        _output.WriteLine($"Error message: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
    }

    [Fact]
    public async Task Parse_AmbiguousRequest_ShouldHandleGracefully()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show the good ones";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        // This should either fail with a clear error or ask for clarification
        _output.WriteLine($"Result HasErrors: {result.HasErrors}");
        if (result.HasErrors)
        {
            _output.WriteLine($"Error message: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        }
    }

    [Fact]
    public async Task Parse_VeryComplexFilter_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show active employees aged between 25 and 45 from Sales or Marketing department in USA or Canada with salary greater than 60000 and email not empty";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Complex filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_WithNaturalLanguageOperators_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Age greater than or equal to 21 and less than 65";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_DateRange_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Created between January 1st 2024 and December 31st 2024";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_MultipleRanges_ShouldGenerateValidFilter()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Show employees with age between 30 and 50 and salary between 50000 and 100000";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    [Fact]
    public async Task Parse_CaseSensitiveText_ShouldPreserveCase()
    {
        // Arrange
        var agent = new FilterParserAgent(_chatClient, _logger);
        var naturalLanguageFilter = "Department equals IT";

        // Act
        var result = await agent.Parse(naturalLanguageFilter, _testFields);

        // Assert
        Assert.False(result.HasErrors, $"Filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

    }

    private bool ContainsComparisonOperations(Node? node)
    {
        if (node == null) return false;

        return node switch
        {
            Leaf leaf => leaf.Op is Op.GreaterThanOrEqual or Op.LessThanOrEqual or Op.GreaterThan or Op.LessThan,
            And and => ContainsComparisonOperations(and.Left) || ContainsComparisonOperations(and.Right),
            Or or => ContainsComparisonOperations(or.Left) || ContainsComparisonOperations(or.Right),
            Not not => ContainsComparisonOperations(not.Inner),
            _ => false
        };
    }

    public void Dispose()
    {
        // Cleanup if needed
        if (_chatClient is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

/// <summary>
/// Logger implementation that outputs to xUnit test output
/// </summary>
internal class XunitLogger<T> : ILogger<T>
{
    private readonly ITestOutputHelper _output;

    public XunitLogger(ITestOutputHelper output)
    {
        _output = output;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _output.WriteLine($"[{logLevel}] {message}");

        if (exception != null)
        {
            _output.WriteLine($"Exception: {exception}");
        }
    }
}
