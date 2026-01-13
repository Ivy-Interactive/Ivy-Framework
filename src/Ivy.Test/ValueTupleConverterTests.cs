using System.Text.Json;
using Ivy.Core.Helpers;

namespace Ivy.Test;

public class ValueTupleConverterTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new ValueTupleConverterFactory()
        }
    };

    [Fact]
    public void SerializeDeserialize_Tuple_WithMultipleTypes()
    {
        (int, string, bool) original = (42, "Hello", true);
        string json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<(int, string, bool)>(json, _options);
        Assert.Equal(original, deserialized);
    }

    [Fact]
    public void Serialize_Tuple_HasExpectedPropertyNames()
    {
        (int, string) tuple = (100, "Test");
        string json = JsonSerializer.Serialize(tuple, _options);
        Assert.Contains("item1", json);
        Assert.Contains("item2", json);
    }
}
