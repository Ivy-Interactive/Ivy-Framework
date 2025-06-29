using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ivy.Test;

public class ConvertJsonNodeTests
{
    public enum TestEnum
    {
        Value1,
        Value2
    }
    [Fact] public void StringToEnum() => Test("Value1", typeof(TestEnum), TestEnum.Value1);
    [Fact] public void StringToNullEnum() => Test("Value1", typeof(TestEnum?), TestEnum.Value1);

    [Fact] public void IntToInt() => Test(123, typeof(int), 123);
    [Fact] public void IntToNullInt() => Test(123, typeof(int?), 123);
    [Fact] public void DoubleToInt() => Test(123.0, typeof(int), 123);
    [Fact] public void DoubleToNullInt() => Test(123.0, typeof(int?), 123);
    [Fact] public void StringToNullInt() => Test("123", typeof(int?), 123);
    [Fact] public void StringToInt() => Test("123", typeof(int), 123);
    [Fact] public void StringToDateTime() => Test("2021-01-01", typeof(DateTime), new DateTime(2021, 1, 1));
    [Fact] public void DoubleToBool0() => Test(0, typeof(bool), false);
    [Fact] public void DoubleToBool1() => Test(1, typeof(bool), true);
    [Fact] public void DoubleToBool2() => Test(2, typeof(bool), true);
    [Fact] public void StringToGuid() => Test("6d4a97d1-472a-4c50-b0fd-47f5a1e1acf6", typeof(Guid), Guid.Parse("6d4a97d1-472a-4c50-b0fd-47f5a1e1acf6"));
    [Fact] public void IntToBoolTrue() => Test(3, typeof(bool), true);
    [Fact] public void IntToBoolFalse() => Test(0, typeof(bool), false);
    [Fact] public void BoolToIntFalse() => Test(false, typeof(int), 0);
    [Fact] public void BoolToIntTrue() => Test(true, typeof(int), 1);


    [Fact]
    public void DateRangeConversion() => Test(JsonNode.Parse(
            """
            {"item1" : "2021-01-01", "item2" : "2022-01-01"}
            """),
        typeof((DateTime, DateTime)), (DateTime.Parse("2021-01-01"), DateTime.Parse("2022-01-01")));

    public record FooBar
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
    }


    [Fact]
    public void Foo()
    {
        var json = """{"content":"SGVsbG8="}""";
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
        var type = typeof(FooBar);
        var jsonNode = JsonNode.Parse(json);
        var deserializedInput = (FooBar)jsonNode.Deserialize(type, options);
        Console.WriteLine(BitConverter.ToString(deserializedInput.Content));
    }

    [Fact]
    public void TestEnumArray()
    {
        var json = """["Value1", "Value2"]""";
        JsonArray jsonArray = JsonNode.Parse(json) as JsonArray;
        var result = Core.Utils.ConvertJsonNode(jsonArray, typeof(TestEnum[]));
        Assert.NotNull(result);
        //make sure result is of type TestEnum[]
        Assert.IsType<TestEnum[]>(result);
    }

    [Fact]
    public void TestEnumList()
    {
        var json = """["Value1", "Value2"]""";
        JsonArray jsonArray = JsonNode.Parse(json) as JsonArray;
        var result = Core.Utils.ConvertJsonNode(jsonArray, typeof(List<TestEnum>));
        Assert.NotNull(result);
        //make sure result is of type TestEnum[]
        Assert.IsType<List<TestEnum>>(result);
    }

    [Fact]
    public void ConvertFileInput()
    {
        var json = JsonNode.Parse("""
                                  {
                                      "name": "myfile.txt",
                                      "size": 123,
                                      "type": "text/plain",
                                      "lastModified": "2023-03-14T09:30:00+01:00",
                                      "content": "SGVsbG8="
                                  }
                                  """);

        var result = (FileInput)Core.Utils.ConvertJsonNode(json!, typeof(FileInput))!;

        Assert.Equal("myfile.txt", result.Name);
        Assert.Equal("text/plain", result.Type);
        Assert.Equal(123, result.Size);
        Assert.Equal(DateTime.Parse("2023-03-14T09:30:00+01:00"), result.LastModified);
        Assert.Equal("Hello", Encoding.UTF8.GetString(result.Content!));
    }

    private void Test(JsonNode? input, Type type, object? expectedResult)
    {
        var result = Core.Utils.ConvertJsonNode(input, type);
        Assert.Equal(expectedResult, result);
    }
}
