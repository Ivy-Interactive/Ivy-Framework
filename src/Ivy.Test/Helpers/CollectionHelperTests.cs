using System.Dynamic;

namespace Ivy.Test.Helpers;

public class CollectionHelperTests
{
    [Fact]
    public void ToExpando_Dictionary_CreatesExpandoObject()
    {
        var dict = new Dictionary<string, object>
        {
            ["Name"] = "Test",
            ["Value"] = 42
        };

        var expando = dict.ToExpando();

        Assert.IsType<ExpandoObject>(expando);
        var expandoDict = (IDictionary<string, object?>)expando!;
        Assert.Equal("Test", expandoDict["Name"]);
        Assert.Equal(42, expandoDict["Value"]);
    }

    [Fact]
    public void ToExpando_NestedDictionary_CreatesNestedExpando()
    {
        var dict = new Dictionary<string, object>
        {
            ["Inner"] = new Dictionary<string, object> { ["Key"] = "Value" }
        };

        var expando = dict.ToExpando();
        var expandoDict = (IDictionary<string, object?>)expando!;
        Assert.IsType<ExpandoObject>(expandoDict["Inner"]);
    }

    [Fact]
    public void ToExpando_EnumerableOfDictionaries_CreatesArray()
    {
        var records = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { ["Id"] = 1 },
            new Dictionary<string, object> { ["Id"] = 2 }
        };

        var result = records.ToExpando();
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public async Task ToArrayAsync_AsyncEnumerable_ReturnsArray()
    {
        var items = ToAsyncEnumerable(1, 2, 3);
        var result = await items.ToArrayAsync();
        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public void Before_ExecutesBeforeAction()
    {
        var order = new List<string>();
        Action action = () => order.Add("main");
        var composed = action.Before(() => order.Add("before"));

        composed();

        Assert.Equal(["before", "main"], order);
    }

    [Fact]
    public void After_ExecutesAfterAction()
    {
        var order = new List<string>();
        Action action = () => order.Add("main");
        var composed = action.After(() => order.Add("after"));

        composed();

        Assert.Equal(["main", "after"], order);
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }
}
