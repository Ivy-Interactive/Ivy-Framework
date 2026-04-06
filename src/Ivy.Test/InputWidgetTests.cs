namespace Ivy.Test;

public class InputWidgetTests
{

    [Theory]
    [InlineData(typeof(short), (short)1)]
    [InlineData(typeof(short?), null)]
    [InlineData(typeof(int), 1)]
    [InlineData(typeof(int?), null)]
    [InlineData(typeof(int?), 1)]
    [InlineData(typeof(double), 1.0)]
    [InlineData(typeof(double?), null)]
    public void NumberInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToNumberInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(NumberInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(string), "test")]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(int), 1)]
    [InlineData(typeof(int?), null)]
    public void TextInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToTextInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(TextInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(string), "test")]
    [InlineData(typeof(string), null)]
    public void ReadOnlyInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToReadOnlyInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(ReadOnlyInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    public enum TestEnum { A, B, C }

    [Theory]
    [InlineData(typeof(TestEnum), TestEnum.A)]
    [InlineData(typeof(TestEnum?), null)]
    [InlineData(typeof(TestEnum?), TestEnum.B)]
    public void SelectInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToSelectInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(SelectInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(bool?), null)]
    [InlineData(typeof(bool?), false)]
    [InlineData(typeof(int), 1)]
    [InlineData(typeof(int?), null)]
    public void BoolInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToBoolInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(BoolInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(DateTime), "2024-01-01T00:00:00")]
    [InlineData(typeof(DateTime?), null)]
    [InlineData(typeof(DateTime?), "2024-01-01T00:00:00")]
    public void DateTimeInput_PreservesType(Type type, string? valueStr)
    {
        object? value = valueStr == null ? null : DateTime.Parse(valueStr);
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToDateTimeInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(DateTimeInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(string), "#ffffff")]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(Colors), Colors.Blue)]
    [InlineData(typeof(Colors?), null)]
    public void ColorInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToColorInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(ColorInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [MemberData(nameof(DateRangeData))]
    public void DateRangeInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToDateRangeInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(DateRangeInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }
    public static IEnumerable<object?[]> DateRangeData() =>
    [
        [typeof((DateOnly, DateOnly)), (DateOnly.MinValue, DateOnly.MaxValue)],
        [typeof((DateOnly?, DateOnly?)), ((DateOnly?)null, (DateOnly?)null)]
    ];

    [Theory]
    [InlineData(typeof(string), "{}")]
    [InlineData(typeof(string), null)]
    public void CodeInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToCodeInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(CodeInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(int), 5)]
    [InlineData(typeof(int?), null)]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(bool?), null)]
    public void FeedbackInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToFeedbackInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(FeedbackInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(Icons), Icons.Activity)]
    [InlineData(typeof(Icons?), null)]
    public void IconInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToIconInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(IconInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [MemberData(nameof(NumberRangeData))]
    public void NumberRangeInput_PreservesType(Type type, object? value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, new object?[] { value })!;
        var widget = state.ToNumberRangeInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(NumberRangeInput<>).MakeGenericType(type.GetGenericArguments()[0]);
        Assert.IsType(expectedWidgetType, widget);
    }
    public static IEnumerable<object?[]> NumberRangeData() =>
    [
        [typeof((int, int)), (0, 100)],
        [typeof((int?, int?)), ((int?)null, (int?)null)],
        [typeof((double, double)), (0.0, 100.0)],
        [typeof((double?, double?)), ((double?)null, (double?)null)]
    ];
}
