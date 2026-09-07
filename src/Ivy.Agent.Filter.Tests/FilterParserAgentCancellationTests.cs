using Microsoft.Extensions.AI;

namespace Ivy.Agent.Filter.Tests;

using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

public class FilterParserAgentCancellationTests
{
    private readonly FieldMeta[] _testFields =
    [
        new FieldMeta("Name", "name", FieldType.Text),
        new FieldMeta("Age", "age", FieldType.Number)
    ];

    [Fact]
    public async Task Parse_PassesCancellationTokenToChatClient()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var fakeClient = new FakeChatClient();
        var agent = new FilterParserAgent(fakeClient);

        // Act
        var result = await agent.Parse("test filter", _testFields, cts.Token);

        // Assert
        Assert.Equal(cts.Token, fakeClient.LastCancellationToken);
    }

    [Fact]
    public async Task Parse_WhenAlreadyCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var fakeClient = new FakeChatClient();
        var agent = new FilterParserAgent(fakeClient);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            agent.Parse("test filter", _testFields, cts.Token));
    }

    [Fact]
    public async Task Parse_WhenChatClientThrowsOperationCanceledException_RethrowsException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var fakeClient = new FakeChatClient
        {
            Handler = (_, _, token) => throw new OperationCanceledException(token)
        };
        var agent = new FilterParserAgent(fakeClient);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            agent.Parse("test filter", _testFields, cts.Token));
    }

    private class FakeChatClient : IChatClient
    {
        public CancellationToken LastCancellationToken { get; private set; }
        public Func<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken, Task<ChatResponse>>? Handler { get; set; }

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            cancellationToken.ThrowIfCancellationRequested();

            if (Handler != null)
            {
                return Handler(chatMessages, options, cancellationToken);
            }

            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "[Name] = \"Test\"")));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}
