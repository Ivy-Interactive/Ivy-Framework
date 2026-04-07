using System.Reflection;
using System.Text.Json;
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

    public ModelPricingService()
    {
        Pricing = LoadEmbeddedPricing();
    }

    internal Dictionary<string, ModelPricing> Pricing { get; }

    public ModelPricing GetPricing(string modelName)
    {
        foreach (var key in Pricing.Keys)
            if (modelName.Contains(key, StringComparison.OrdinalIgnoreCase))
                return Pricing[key];

        // Fallback to Opus 4
        return Pricing.TryGetValue("claude-opus-4", out var fallback)
            ? fallback
            : new ModelPricing { Input = 15.0, Output = 75.0, CacheWrite = 18.75, CacheRead = 1.50 };
    }

    public CostCalculation CalculateSessionCost(string sessionId)
    {
        return CalculateSessionCost(sessionId, "claude");
    }

    public CostCalculation CalculateSessionCost(string sessionId, string provider)
    {
        return provider.ToLower() switch
        {
            "claude" => CalculateClaudeCost(sessionId),
            "codex" => CalculateCodexCost(sessionId),
            "gemini" => CalculateGeminiCost(sessionId),
            _ => CalculateClaudeCost(sessionId)
        };
    }

    private static Dictionary<string, ModelPricing> LoadEmbeddedPricing()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Ivy.Tendril.Assets.models.yaml";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new InvalidOperationException($"Embedded resource '{resourceName}' not found");

        using var reader = new StreamReader(stream);
        var yaml = reader.ReadToEnd();

        var config = DefaultDeserializer.Deserialize<Dictionary<string, object>>(yaml);

        var result = new Dictionary<string, ModelPricing>();
        if (config.TryGetValue("models", out var modelsObj) && modelsObj is Dictionary<object, object> models)
            foreach (var kvp in models)
            {
                var modelName = kvp.Key.ToString() ?? "";
                if (kvp.Value is Dictionary<object, object> props)
                    result[modelName] = new ModelPricing
                    {
                        Input = Convert.ToDouble(props["input"]),
                        Output = Convert.ToDouble(props["output"]),
                        CacheWrite = Convert.ToDouble(props["cacheWrite"]),
                        CacheRead = Convert.ToDouble(props["cacheRead"])
                    };
            }

        return result;
    }

    private CostCalculation CalculateClaudeCost(string sessionId)
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
            foreach (var subFile in Directory.GetFiles(subagentDir, "*.jsonl"))
                ProcessSessionFile(subFile, ref totalCost, ref totalTokens);

        return new CostCalculation { TotalTokens = totalTokens, TotalCost = totalCost };
    }

    private CostCalculation CalculateCodexCost(string sessionId)
    {
        var codexSessionsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".codex", "sessions"
        );

        if (!Directory.Exists(codexSessionsDir)) return new CostCalculation();

        var sessionFile = Directory.GetFiles(codexSessionsDir, "*.jsonl", SearchOption.AllDirectories)
            .FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).EndsWith(sessionId, StringComparison.OrdinalIgnoreCase));

        if (sessionFile == null) return new CostCalculation();

        return ParseCodexSessionFile(sessionFile);
    }

    private CostCalculation CalculateGeminiCost(string sessionId)
    {
        var geminiDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".gemini", "tmp"
        );

        if (!Directory.Exists(geminiDir)) return new CostCalculation();

        var sessionFile = Directory.GetFiles(geminiDir, "*.json", SearchOption.AllDirectories)
            .FirstOrDefault(f => Path.GetFileName(f).Contains(sessionId, StringComparison.OrdinalIgnoreCase));

        if (sessionFile == null) return new CostCalculation();

        return ParseGeminiSessionFile(sessionFile);
    }

    internal CostCalculation ParseCodexSessionFile(string filePath)
    {
        var model = "o4-mini";
        var totalInputTokens = 0;
        var totalOutputTokens = 0;
        var totalCachedTokens = 0;

        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;

                var entryType = root.TryGetProperty("type", out var t) ? t.GetString() : null;

                if (entryType == "turn_context" &&
                    root.TryGetProperty("payload", out var turnPayload) &&
                    turnPayload.TryGetProperty("model", out var turnModel))
                    model = turnModel.GetString() ?? model;

                if (entryType == "event_msg" &&
                    root.TryGetProperty("payload", out var payload) &&
                    payload.TryGetProperty("type", out var payloadType) &&
                    payloadType.GetString() == "token_count" &&
                    payload.TryGetProperty("info", out var info) &&
                    info.ValueKind != JsonValueKind.Null &&
                    info.TryGetProperty("total_token_usage", out var usage))
                {
                    totalInputTokens = usage.TryGetProperty("input_tokens", out var it) ? it.GetInt32() : 0;
                    totalOutputTokens = usage.TryGetProperty("output_tokens", out var ot) ? ot.GetInt32() : 0;
                    totalCachedTokens = usage.TryGetProperty("cached_input_tokens", out var ct) ? ct.GetInt32() : 0;
                    var reasoningTokens =
                        usage.TryGetProperty("reasoning_output_tokens", out var rt) ? rt.GetInt32() : 0;
                    totalOutputTokens += reasoningTokens;
                }
            }
            catch
            {
                /* Skip malformed lines */
            }
        }

        var pricing = GetPricing(model);
        var totalTokens = totalInputTokens + totalOutputTokens + totalCachedTokens;
        var totalCost = totalInputTokens * pricing.Input * 1e-6
                        + totalOutputTokens * pricing.Output * 1e-6
                        + totalCachedTokens * pricing.CacheRead * 1e-6;

        return new CostCalculation { TotalTokens = totalTokens, TotalCost = totalCost };
    }

    internal CostCalculation ParseGeminiSessionFile(string filePath)
    {
        var totalCost = 0.0;
        var totalTokens = 0;

        try
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("messages", out var messages)) return new CostCalculation();

            foreach (var msg in messages.EnumerateArray())
            {
                var msgType = msg.TryGetProperty("type", out var mt) ? mt.GetString() : null;
                if (msgType != "gemini") continue;
                if (!msg.TryGetProperty("tokens", out var tokens)) continue;

                var model = msg.TryGetProperty("model", out var m)
                    ? m.GetString() ?? "gemini-2.5-flash"
                    : "gemini-2.5-flash";
                var pricing = GetPricing(model);

                var inputTokens = tokens.TryGetProperty("input", out var it) ? it.GetInt32() : 0;
                var outputTokens = tokens.TryGetProperty("output", out var ot) ? ot.GetInt32() : 0;
                var cachedTokens = tokens.TryGetProperty("cached", out var ct) ? ct.GetInt32() : 0;

                totalTokens += inputTokens + outputTokens + cachedTokens;
                totalCost += inputTokens * pricing.Input * 1e-6;
                totalCost += outputTokens * pricing.Output * 1e-6;
                totalCost += cachedTokens * pricing.CacheRead * 1e-6;
            }
        }
        catch
        {
            /* Return empty on parse failure */
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
        foreach (var line in FileHelper.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var obj = JsonDocument.Parse(line);
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
                    var cache5m = cacheCreation.TryGetProperty("ephemeral_5m_input_tokens", out var c5)
                        ? c5.GetInt32()
                        : 0;
                    var cache1h = cacheCreation.TryGetProperty("ephemeral_1h_input_tokens", out var c1)
                        ? c1.GetInt32()
                        : 0;
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
            catch
            {
                /* Skip malformed lines */
            }
        }
    }
}