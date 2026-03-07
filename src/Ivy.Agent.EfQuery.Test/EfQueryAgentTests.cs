using Ivy.Samples.Shared.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using Xunit.Abstractions;

namespace Ivy.Agent.EfQuery.Test;

public class EfQueryAgentTests : IDisposable
{
    private readonly IChatClient _chatClient;
    private readonly SampleDbContextFactory _contextFactory;
    private readonly ITestOutputHelper _output;

    public EfQueryAgentTests(ITestOutputHelper output)
    {
        _output = output;

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<EfQueryAgentTests>()
            .Build();

        var endpoint = configuration["OpenAi:Endpoint"]
            ?? throw new InvalidOperationException("OpenAi:Endpoint not found in user secrets");
        var apiKey = configuration["OpenAi:ApiKey"]
            ?? throw new InvalidOperationException("OpenAi:ApiKey not found in user secrets");

        var openAiClient = new OpenAIClient(
            new System.ClientModel.ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint) });

        _chatClient = openAiClient.GetChatClient("gpt-4o").AsIChatClient();
        _contextFactory = new SampleDbContextFactory();
    }

    [Fact]
    public async Task QueryAsync_ProductsAbove50_ReturnsValidResult()
    {
        var agent = new EfQueryAgent<SampleDbContext>(_chatClient, _contextFactory);

        var result = await agent.QueryAsync("Show me all products with price above 50");

        _output.WriteLine($"Plan: {result.Plan}");
        _output.WriteLine($"SQL: {result.Sql}");
        _output.WriteLine($"XAML: {result.Xaml}");
        _output.WriteLine($"Input tokens: {result.InputTokens}");
        _output.WriteLine($"Output tokens: {result.OutputTokens}");
        _output.WriteLine($"Iterations: {result.Iterations}");

        Assert.False(string.IsNullOrWhiteSpace(result.Plan), "Plan should not be empty");
        Assert.False(string.IsNullOrWhiteSpace(result.Sql), "SQL should not be empty");
        Assert.False(string.IsNullOrWhiteSpace(result.Xaml), "XAML should not be empty");
        Assert.True(result.InputTokens > 0, "Input tokens should be tracked");
        Assert.True(result.OutputTokens > 0, "Output tokens should be tracked");
        Assert.True(result.Iterations > 0, "Iterations should be tracked");
    }

    public void Dispose()
    {
        if (_chatClient is IDisposable disposable)
            disposable.Dispose();
    }
}
