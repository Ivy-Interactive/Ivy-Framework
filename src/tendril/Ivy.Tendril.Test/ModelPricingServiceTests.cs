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
            Assert.Equal(expectedCost, result.TotalCost, precision: 10);
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
            Assert.Equal(expectedCost, result.TotalCost, precision: 10);
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
            Assert.Equal(expectedCost, result.TotalCost, precision: 10);
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
            Assert.Equal(expectedCost, result.TotalCost, precision: 10);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
