using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivy.Filters.Eval.Console;

public class ModelCostService
{
    private readonly Dictionary<string, ModelCostInfo> _modelCosts = new();
    private readonly HttpClient _httpClient;

    public ModelCostService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoadModelCostsFromLiteLLMAsync(string apiKey)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("x-litellm-api-key", apiKey);

            var response = await _httpClient.GetAsync("https://llmproxy.ivy.app/model_group/info");
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LiteLLMResponse>(content);

            if (result?.Data != null)
            {
                foreach (var model in result.Data)
                {
                    if (!string.IsNullOrEmpty(model.ModelGroup))
                    {
                        _modelCosts[model.ModelGroup] = new ModelCostInfo
                        {
                            InputCostPerToken = model.InputCostPerToken ?? 0,
                            OutputCostPerToken = model.OutputCostPerToken ?? 0
                        };
                    }
                }
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public decimal CalculateCost(string modelName, long inputTokens, long outputTokens)
    {
        if (_modelCosts.TryGetValue(modelName, out var costInfo))
        {
            var inputCost = inputTokens * (decimal)costInfo.InputCostPerToken;
            var outputCost = outputTokens * (decimal)costInfo.OutputCostPerToken;
            return inputCost + outputCost;
        }

        // Fallback costs for common models (OpenAI pricing as of Dec 2024)
        return modelName.ToLowerInvariant() switch
        {
            "gpt-4o" => (inputTokens * 0.0000025m) + (outputTokens * 0.00001m),
            "gpt-4o-mini" => (inputTokens * 0.00000015m) + (outputTokens * 0.0000006m),
            "gpt-4-turbo" => (inputTokens * 0.00001m) + (outputTokens * 0.00003m),
            "gpt-3.5-turbo" => (inputTokens * 0.0000005m) + (outputTokens * 0.0000015m),
            _ when modelName.Contains("claude-3-5-sonnet") => (inputTokens * 0.000003m) + (outputTokens * 0.000015m),
            _ when modelName.Contains("claude-3-opus") => (inputTokens * 0.000015m) + (outputTokens * 0.000075m),
            _ when modelName.Contains("claude-3-haiku") => (inputTokens * 0.00000025m) + (outputTokens * 0.00000125m),
            _ => 0m // Unknown model
        };
    }

    private class LiteLLMResponse
    {
        [JsonPropertyName("data")]
        public List<LiteLLMModel>? Data { get; set; }
    }

    private class LiteLLMModel
    {
        [JsonPropertyName("model_group")]
        public string? ModelGroup { get; set; }

        [JsonPropertyName("input_cost_per_token")]
        public double? InputCostPerToken { get; set; }

        [JsonPropertyName("output_cost_per_token")]
        public double? OutputCostPerToken { get; set; }
    }
}

public class ModelCostInfo
{
    public double InputCostPerToken { get; set; }
    public double OutputCostPerToken { get; set; }
}
