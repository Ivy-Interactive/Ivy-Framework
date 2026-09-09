using System.Net;
using Ivy.Agent.Filter.Eval.Console;

namespace Ivy.Agent.Filter.Tests;

public class ModelCostServiceCancellationTests
{
    [Fact]
    public async Task LoadModelCostsFromLiteLLMAsync_PassesCancellationTokenToHttpCalls()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var handlerInvoked = false;
        var handler = new MockHttpMessageHandler
        {
            Handler = (_, token) =>
            {
                handlerInvoked = true;
                // Verifies that the cancellation token passed to HttpClient is linked to cts
                Assert.False(token.IsCancellationRequested);
                cts.Cancel();
                Assert.True(token.IsCancellationRequested);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""
                    {
                        "data": [
                            {
                                "model_group": "gpt-4o",
                                "input_cost_per_token": 0.000005,
                                "output_cost_per_token": 0.000015
                            }
                        ]
                    }
                    """)
                };
                return Task.FromResult(response);
            }
        };

        var httpClient = new HttpClient(handler);
        var costService = new ModelCostService(httpClient);

        // Act
        var result = await costService.LoadModelCostsFromLiteLLMAsync("test-api-key", cts.Token);

        // Assert
        Assert.True(result);
        Assert.True(handlerInvoked);
        Assert.Equal(0.000020m, costService.CalculateCost("gpt-4o", 1, 1));
    }

    [Fact]
    public async Task LoadModelCostsFromLiteLLMAsync_WhenAlreadyCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var costService = new ModelCostService(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            costService.LoadModelCostsFromLiteLLMAsync("test-api-key", cts.Token));
    }

    [Fact]
    public async Task LoadModelCostsFromLiteLLMAsync_WhenHttpThrowsOperationCanceledException_RethrowsException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var handler = new MockHttpMessageHandler
        {
            Handler = (_, token) => throw new OperationCanceledException(token)
        };
        var httpClient = new HttpClient(handler);
        var costService = new ModelCostService(httpClient);

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            costService.LoadModelCostsFromLiteLLMAsync("test-api-key", cts.Token));
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? Handler { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Handler != null)
            {
                return Handler(request, cancellationToken);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"data\": []}")
            });
        }
    }
}
