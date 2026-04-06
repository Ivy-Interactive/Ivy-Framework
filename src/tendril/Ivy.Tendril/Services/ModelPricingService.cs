using System.Reflection;
using YamlDotNet.Serialization;

namespace Ivy.Tendril.Services;

public record ModelPricing
{
    public double Input { get; init; }
    public double Output { get; init; }
    public double CacheWrite { get; init; }
    public double CacheRead { get; init; }
}

public record CostCalculation
{
    public int TotalTokens { get; init; }
    public double TotalCost { get; init; }
}

public class ModelPricingService : IModelPricingService
{
    private static readonly IDeserializer DefaultDeserializer = new DeserializerBuilder().Build();

    private readonly Dictionary<string, ModelPricing> _pricing;

    public ModelPricingService()
    {
        _pricing = LoadEmbeddedPricing();
    }

    internal Dictionary<string, ModelPricing> Pricing => _pricing;

    private static Dictionary<string, ModelPricing> LoadEmbeddedPricing()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Ivy.Tendril.Assets.models.yaml";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found");
        }

        using var reader = new StreamReader(stream);
        var yaml = reader.ReadToEnd();

        var config = DefaultDeserializer.Deserialize<Dictionary<string, object>>(yaml);

        var result = new Dictionary<string, ModelPricing>();
        if (config.TryGetValue("models", out var modelsObj) && modelsObj is Dictionary<object, object> models)
        {
            foreach (var kvp in models)
            {
                var modelName = kvp.Key.ToString() ?? "";
                if (kvp.Value is Dictionary<object, object> props)
                {
                    result[modelName] = new ModelPricing
                    {
                        Input = Convert.ToDouble(props["input"]),
                        Output = Convert.ToDouble(props["output"]),
                        CacheWrite = Convert.ToDouble(props["cacheWrite"]),
                        CacheRead = Convert.ToDouble(props["cacheRead"])
                    };
                }
            }
        }

        return result;
    }

    public ModelPricing GetPricing(string modelName)
    {
        foreach (var key in _pricing.Keys)
        {
            if (modelName.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return _pricing[key];
            }
        }

        // Fallback to Opus 4
        return _pricing.TryGetValue("claude-opus-4", out var fallback)
            ? fallback
            : new ModelPricing { Input = 15.0, Output = 75.0, CacheWrite = 18.75, CacheRead = 1.50 };
    }

    public CostCalculation CalculateSessionCost(string sessionId)
    {
        var claudeProjectsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude", "projects"
        );

        var sessionFile = FindSessionFile(claudeProjectsDir, sessionId);
        if (sessionFile == null) return new CostCalculation();

        var totalCost = 0.0;
        var totalTokens = 0;

        ProcessSessionFile(sessionFile, ref totalCost, ref totalTokens);

        // Parse subagent files
        var subagentDir = Path.Combine(
            Path.GetDirectoryName(sessionFile)!,
            Path.GetFileNameWithoutExtension(sessionFile),
            "subagents"
        );

        if (Directory.Exists(subagentDir))
        {
            foreach (var subFile in Directory.GetFiles(subagentDir, "*.jsonl"))
            {
                ProcessSessionFile(subFile, ref totalCost, ref totalTokens);
            }
        }

        return new CostCalculation { TotalTokens = totalTokens, TotalCost = totalCost };
    }

    internal CostCalculation CalculateFromFile(string filePath)
    {
        var totalCost = 0.0;
        var totalTokens = 0;
        ProcessSessionFile(filePath, ref totalCost, ref totalTokens);
        return new CostCalculation { TotalTokens = totalTokens, TotalCost = totalCost };
    }

    private static string? FindSessionFile(string claudeProjectsDir, string sessionId)
    {
        if (!Directory.Exists(claudeProjectsDir)) return null;

        return Directory.GetFiles(claudeProjectsDir, $"{sessionId}.jsonl", SearchOption.AllDirectories)
            .FirstOrDefault(f => !f.Contains("\\subagents\\") && !f.Contains("/subagents/"));
    }

    private void ProcessSessionFile(string filePath, ref double totalCost, ref int totalTokens)
    {
        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var obj = System.Text.Json.JsonDocument.Parse(line);
                var root = obj.RootElement;

                if (root.GetProperty("type").GetString() != "assistant") continue;
                if (!root.TryGetProperty("message", out var message)) continue;
                if (!message.TryGetProperty("usage", out var usage)) continue;

                var model = message.TryGetProperty("model", out var m)
                    ? m.GetString() ?? "claude-opus-4"
                    : "claude-opus-4";

                var pricing = GetPricing(model);

                var priceInput = pricing.Input * 1e-6;
                var priceOutput = pricing.Output * 1e-6;
                var priceCacheWrite = pricing.CacheWrite * 1e-6;
                var priceCacheRead = pricing.CacheRead * 1e-6;

                var inputTokens = usage.TryGetProperty("input_tokens", out var it) ? it.GetInt32() : 0;
                var outputTokens = usage.TryGetProperty("output_tokens", out var ot) ? ot.GetInt32() : 0;
                var cacheReadTokens = usage.TryGetProperty("cache_read_input_tokens", out var cr) ? cr.GetInt32() : 0;

                totalTokens += inputTokens + outputTokens + cacheReadTokens;
                totalCost += inputTokens * priceInput;
                totalCost += outputTokens * priceOutput;
                totalCost += cacheReadTokens * priceCacheRead;

                if (usage.TryGetProperty("cache_creation", out var cacheCreation))
                {
                    var cache5m = cacheCreation.TryGetProperty("ephemeral_5m_input_tokens", out var c5) ? c5.GetInt32() : 0;
                    var cache1h = cacheCreation.TryGetProperty("ephemeral_1h_input_tokens", out var c1) ? c1.GetInt32() : 0;
                    totalTokens += cache5m + cache1h;
                    totalCost += (cache5m + cache1h) * priceCacheWrite;
                }
                else if (usage.TryGetProperty("cache_creation_input_tokens", out var ccTokens))
                {
                    var cacheCreationTokens = ccTokens.GetInt32();
                    totalTokens += cacheCreationTokens;
                    totalCost += cacheCreationTokens * priceCacheWrite;
                }
            }
            catch { /* Skip malformed lines */ }
        }
    }
}
