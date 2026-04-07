using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class ModelPricingServiceTests
{
    private readonly ModelPricingService _service = new();

    [Fact]
    public void LoadsEmbeddedModelsYaml()
    {
        Assert.NotEmpty(_service.Pricing);
        Assert.True(_service.Pricing.Count >= 3);
    }

    [Theory]
    [InlineData("claude-opus-4", 15.00, 75.00)]
    [InlineData("claude-sonnet-4", 3.00, 15.00)]
    [InlineData("claude-haiku-4", 0.80, 4.00)]
    public void GetPricing_ReturnsCorrectRates(string model, double expectedInput, double expectedOutput)
    {
        var pricing = _service.GetPricing(model);
        Assert.Equal(expectedInput, pricing.Input);
        Assert.Equal(expectedOutput, pricing.Output);
    }

    [Theory]
    [InlineData("us.anthropic.claude-opus-4-6-v1", 15.00)]
    [InlineData("claude-opus-4-20260301", 15.00)]
    [InlineData("claude-sonnet-4-latest", 3.00)]
    public void GetPricing_MatchesSubstrings(string model, double expectedInput)
    {
        var pricing = _service.GetPricing(model);
        Assert.Equal(expectedInput, pricing.Input);
    }

    [Fact]
    public void GetPricing_FallsBackToOpus4ForUnknownModels()
    {
        var pricing = _service.GetPricing("gpt-4o-unknown");
        Assert.Equal(15.00, pricing.Input);
        Assert.Equal(75.00, pricing.Output);
    }

    [Fact]
    public void CalculateFromFile_BasicUsage()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, """
                                        {"type":"assistant","message":{"model":"claude-sonnet-4","usage":{"input_tokens":1000,"output_tokens":500,"cache_read_input_tokens":200}}}
                                        """);

            var result = _service.CalculateFromFile(tempFile);

            Assert.True(result.TotalTokens > 0);
            Assert.Equal(1700, result.TotalTokens);

            // Expected: 1000 * 3.00/1M + 500 * 15.00/1M + 200 * 0.30/1M
            var expectedCost = 1000 * 3.00e-6 + 500 * 15.00e-6 + 200 * 0.30e-6;
            Assert.Equal(expectedCost, result.TotalCost, 10);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void CalculateFromFile_CacheCreationTokens_NewFormat()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, """
                                        {"type":"assistant","message":{"model":"claude-opus-4","usage":{"input_tokens":500,"output_tokens":100,"cache_read_input_tokens":0,"cache_creation":{"ephemeral_5m_input_tokens":300,"ephemeral_1h_input_tokens":200}}}}
                                        """);

            var result = _service.CalculateFromFile(tempFile);

            // 500 input + 100 output + 0 cache_read + 300 + 200 cache_creation = 1100
            Assert.Equal(1100, result.TotalTokens);

            var expectedCost = 500 * 15.00e-6 + 100 * 75.00e-6 + (300 + 200) * 18.75e-6;
            Assert.Equal(expectedCost, result.TotalCost, 10);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void CalculateFromFile_CacheCreationTokens_LegacyFormat()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, """
                                        {"type":"assistant","message":{"model":"claude-opus-4","usage":{"input_tokens":500,"output_tokens":100,"cache_read_input_tokens":0,"cache_creation_input_tokens":400}}}
                                        """);

            var result = _service.CalculateFromFile(tempFile);

            Assert.Equal(1000, result.TotalTokens);

            var expectedCost = 500 * 15.00e-6 + 100 * 75.00e-6 + 400 * 18.75e-6;
            Assert.Equal(expectedCost, result.TotalCost, 10);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void CalculateFromFile_SkipsMalformedLines()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, """
                                        {"type":"assistant","message":{"model":"claude-sonnet-4","usage":{"input_tokens":1000,"output_tokens":500,"cache_read_input_tokens":0}}}
                                        this is not json
                                        {"type":"user","message":"hello"}
                                        {"type":"assistant","message":{"model":"claude-sonnet-4","usage":{"input_tokens":2000,"output_tokens":1000,"cache_read_input_tokens":0}}}
                                        """);

            var result = _service.CalculateFromFile(tempFile);

            // Should process both valid assistant lines
            Assert.Equal(4500, result.TotalTokens);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void CalculateSessionCost_ReturnsEmptyForInvalidSessionId()
    {
        var result = _service.CalculateSessionCost("nonexistent-session-id-12345");
        Assert.Equal(0, result.TotalTokens);
        Assert.Equal(0.0, result.TotalCost);
    }

    [Fact]
    public void CalculateFromFile_AggregatesMultipleMessages()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var lines = string.Join("\n",
                """{"type":"assistant","message":{"model":"claude-opus-4","usage":{"input_tokens":1000,"output_tokens":200,"cache_read_input_tokens":0}}}""",
                """{"type":"assistant","message":{"model":"claude-opus-4","usage":{"input_tokens":500,"output_tokens":100,"cache_read_input_tokens":300}}}"""
            );
            File.WriteAllText(tempFile, lines);

            var result = _service.CalculateFromFile(tempFile);

            Assert.Equal(2100, result.TotalTokens);

            var expectedCost =
                1000 * 15.00e-6 + 200 * 75.00e-6 +
                500 * 15.00e-6 + 100 * 75.00e-6 + 300 * 1.50e-6;
            Assert.Equal(expectedCost, result.TotalCost, 10);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseCodexSessionFile_ParsesTokenUsage()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var lines = string.Join("\n",
                """{"timestamp":"2026-03-24T12:11:51.485Z","type":"session_meta","payload":{"id":"test-session","model_provider":"openai"}}""",
                """{"timestamp":"2026-03-24T12:11:51.489Z","type":"turn_context","payload":{"model":"o4-mini"}}""",
                """{"timestamp":"2026-03-24T12:11:52.000Z","type":"event_msg","payload":{"type":"token_count","info":null}}""",
                """{"timestamp":"2026-03-24T12:11:53.000Z","type":"event_msg","payload":{"type":"token_count","info":{"total_token_usage":{"input_tokens":5000,"cached_input_tokens":2000,"output_tokens":500,"reasoning_output_tokens":100,"total_tokens":7600},"model_context_window":258400}}}"""
            );
            File.WriteAllText(tempFile, lines);

            var result = _service.ParseCodexSessionFile(tempFile);

            // 5000 input + 500 output + 100 reasoning + 2000 cached = 7600
            Assert.Equal(7600, result.TotalTokens);

            // o4-mini pricing: input=1.10, output=4.40, cacheRead=0.275
            // output includes reasoning: 500 + 100 = 600
            var expectedCost = 5000 * 1.10e-6 + 600 * 4.40e-6 + 2000 * 0.275e-6;
            Assert.Equal(expectedCost, result.TotalCost, 10);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseCodexSessionFile_UsesLastTokenCountEntry()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var lines = string.Join("\n",
                """{"timestamp":"2026-03-24T12:11:51.489Z","type":"turn_context","payload":{"model":"o3"}}""",
                """{"timestamp":"2026-03-24T12:11:52.000Z","type":"event_msg","payload":{"type":"token_count","info":{"total_token_usage":{"input_tokens":1000,"cached_input_tokens":0,"output_tokens":100,"reasoning_output_tokens":0,"total_tokens":1100},"model_context_window":258400}}}""",
                """{"timestamp":"2026-03-24T12:11:53.000Z","type":"event_msg","payload":{"type":"token_count","info":{"total_token_usage":{"input_tokens":3000,"cached_input_tokens":1000,"output_tokens":300,"reasoning_output_tokens":50,"total_tokens":4350},"model_context_window":258400}}}"""
            );
            File.WriteAllText(tempFile, lines);

            var result = _service.ParseCodexSessionFile(tempFile);

            // Should use the LAST token_count entry (cumulative): 3000 + 350 + 1000 = 4350
            Assert.Equal(4350, result.TotalTokens);

            // o3 pricing: input=10.00, output=40.00, cacheRead=2.50
            var expectedCost = 3000 * 10.00e-6 + 350 * 40.00e-6 + 1000 * 2.50e-6;
            Assert.Equal(expectedCost, result.TotalCost, 10);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseGeminiSessionFile_ParsesTokenUsage()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = """
                       {
                         "sessionId": "test-session",
                         "messages": [
                           {"type": "user", "content": [{"text": "hello"}]},
                           {
                             "type": "gemini",
                             "model": "gemini-2.5-pro",
                             "tokens": {"input": 1000, "output": 200, "cached": 500, "thoughts": 50, "tool": 0, "total": 1750},
                             "content": "response"
                           },
                           {
                             "type": "gemini",
                             "model": "gemini-2.5-pro",
                             "tokens": {"input": 2000, "output": 400, "cached": 300, "thoughts": 100, "tool": 0, "total": 2800},
                             "content": "another response"
                           }
                         ]
                       }
                       """;
            File.WriteAllText(tempFile, json);

            var result = _service.ParseGeminiSessionFile(tempFile);

            // (1000 + 200 + 500) + (2000 + 400 + 300) = 4400
            Assert.Equal(4400, result.TotalTokens);

            // gemini-2.5-pro pricing: input=1.25, output=10.00, cacheRead=0.315
            var expectedCost =
                (1000 + 2000) * 1.25e-6 +
                (200 + 400) * 10.00e-6 +
                (500 + 300) * 0.315e-6;
            Assert.Equal(expectedCost, result.TotalCost, 10);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseGeminiSessionFile_SkipsNonGeminiMessages()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = """
                       {
                         "sessionId": "test-session",
                         "messages": [
                           {"type": "user", "content": [{"text": "hello"}]},
                           {
                             "type": "gemini",
                             "model": "gemini-2.5-flash",
                             "tokens": {"input": 500, "output": 100, "cached": 0, "thoughts": 10, "tool": 0, "total": 610},
                             "content": "response"
                           }
                         ]
                       }
                       """;
            File.WriteAllText(tempFile, json);

            var result = _service.ParseGeminiSessionFile(tempFile);

            Assert.Equal(600, result.TotalTokens);

            // gemini-2.5-flash pricing: input=0.15, output=0.60, cacheRead=0.0375
            var expectedCost = 500 * 0.15e-6 + 100 * 0.60e-6;
            Assert.Equal(expectedCost, result.TotalCost, 10);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void CalculateSessionCost_RoutesToCodex()
    {
        var result = _service.CalculateSessionCost("nonexistent-codex-session-12345", "codex");
        Assert.Equal(0, result.TotalTokens);
        Assert.Equal(0.0, result.TotalCost);
    }

    [Fact]
    public void CalculateSessionCost_RoutesToGemini()
    {
        var result = _service.CalculateSessionCost("nonexistent-gemini-session-12345", "gemini");
        Assert.Equal(0, result.TotalTokens);
        Assert.Equal(0.0, result.TotalCost);
    }
}