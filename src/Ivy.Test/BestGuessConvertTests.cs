namespace Ivy.Test;

public class BestGuessConvertTests
{
    public enum TestEnum
    {
        Value1,
        Value2
    }

    [Fact] public void IntToInt() => Test(123, typeof(int), 123);
    [Fact] public void IntToNullInt() => Test(123, typeof(int?), 123);
    [Fact] public void DoubleToInt() => Test(123.0, typeof(int), 123);
    [Fact] public void DoubleToNullInt() => Test(123.0, typeof(int?), 123);
    [Fact] public void StringToNullInt() => Test("123", typeof(int?), 123);
    [Fact] public void StringToInt() => Test("123", typeof(int), 123);
    [Fact] public void StringToDateTime() => Test("2021-01-01", typeof(DateTime), new DateTime(2021, 1, 1));
    [Fact] public void StringToEnum() => Test("Value1", typeof(TestEnum), TestEnum.Value1);
    [Fact] public void StringToNullEnum() => Test("Value1", typeof(TestEnum?), TestEnum.Value1);
    [Fact] public void DoubleToBool0() => Test(0, typeof(bool), false);
    [Fact] public void DoubleToBool1() => Test(1, typeof(bool), true);
    [Fact] public void DoubleToBool2() => Test(2, typeof(bool), true);
    [Fact] public void StringToGuid() => Test("6d4a97d1-472a-4c50-b0fd-47f5a1e1acf6", typeof(Guid), Guid.Parse("6d4a97d1-472a-4c50-b0fd-47f5a1e1acf6"));
    [Fact] public void IntToBoolTrue() => Test(3, typeof(bool), true);
    [Fact] public void IntToBoolFalse() => Test(0, typeof(bool), false);
    [Fact] public void BoolToIntFalse() => Test(false, typeof(int), 0);
    [Fact] public void BoolToIntTrue() => Test(true, typeof(int), 1);

    [Fact]
    public void DateRangeConversion() => Test(new Dictionary<string, object?> { ["item1"] = "2021-01-01", ["item2"] = "2022-01-01" },
        typeof((DateTime, DateTime)), (DateTime.Parse("2021-01-01"), DateTime.Parse("2022-01-01")));

    private void Test(object? input, Type type, object? expectedResult)
    {
        var result = Core.Utils.BestGuessConvert(input, type);
        Assert.Equal(expectedResult, result);
    }
}
