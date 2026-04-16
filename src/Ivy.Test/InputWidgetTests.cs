using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Ivy.Test;

public class InputWidgetTests
{
    private class MockState<T>(T value) : IState<T>
    {
        private readonly Subject<T> _subject = new();
        public T Value { get; set; } = value;

        [OverloadResolutionPriority(1)]
        public T Set(T value) { Value = value; return Value; }
        public T Set(Func<T, T> setter) { Value = setter(Value); return Value; }
        public T Reset() => Set(default(T)!);
        public Type GetStateType() => typeof(T);

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(Value);
            return _subject.Subscribe(observer);
        }

        public void Dispose() => _subject.Dispose();
        public IDisposable SubscribeAny(Action action) => _subject.Subscribe(_ => action());
        public IDisposable SubscribeAny(Action<object?> action) => _subject.Subscribe(x => action(x));
        public IEffectTrigger ToTrigger() => EffectTrigger.OnStateChange(this);
        public object? GetValueAsObject() => Value;
    }

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

public class InputPrefixSuffixSlotTests
{
    private class MockState<T>(T value) : IState<T>
    {
        private readonly Subject<T> _subject = new();
        public T Value { get; set; } = value;

        [OverloadResolutionPriority(1)]
        public T Set(T value) { Value = value; return Value; }
        public T Set(Func<T, T> setter) { Value = setter(Value); return Value; }
        public T Reset() => Set(default(T)!);
        public Type GetStateType() => typeof(T);

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(Value);
            return _subject.Subscribe(observer);
        }

        public void Dispose() => _subject.Dispose();
        public IDisposable SubscribeAny(Action action) => _subject.Subscribe(_ => action());
        public IDisposable SubscribeAny(Action<object?> action) => _subject.Subscribe(x => action(x));
        public IEffectTrigger ToTrigger() => EffectTrigger.OnStateChange(this);
        public object? GetValueAsObject() => Value;
    }

    private static Slot? FindSlot(WidgetBase widget, string name)
        => widget.Children.OfType<Slot>().FirstOrDefault(s => s.Name == name);

    [Fact]
    public void TextInput_Prefix_AddsPrefixSlot()
    {
        TextInputBase input = new TextInput<string>();
        var withPrefix = input.Prefix("$");

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Single(slot!.Children);
        Assert.Equal("$", slot.Children[0]);
    }

    [Fact]
    public void TextInput_Suffix_AddsSuffixSlot()
    {
        TextInputBase input = new TextInput<string>();
        var withSuffix = input.Suffix("USD");

        var slot = FindSlot(withSuffix, "Suffix");
        Assert.NotNull(slot);
        Assert.Single(slot!.Children);
        Assert.Equal("USD", slot.Children[0]);
    }

    [Fact]
    public void TextInput_Prefix_ReplacesPreviousPrefix()
    {
        TextInputBase input = new TextInput<string>();
        var replaced = input.Prefix("$").Prefix("€");

        var prefixSlots = replaced.Children.OfType<Slot>().Where(s => s.Name == "Prefix").ToArray();
        Assert.Single(prefixSlots);
        Assert.Equal("€", prefixSlots[0].Children[0]);
    }

    [Fact]
    public void TextInput_Prefix_AcceptsWidgetContent()
    {
        TextInputBase input = new TextInput<string>();
        var button = new Button("Click");
        var withPrefix = input.Prefix(button);

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Same(button, slot!.Children[0]);
    }

    [Fact]
    public void TextInput_PrefixAndSuffix_Coexist()
    {
        TextInputBase input = new TextInput<string>();
        var both = input.Prefix("$").Suffix("USD");

        Assert.NotNull(FindSlot(both, "Prefix"));
        Assert.NotNull(FindSlot(both, "Suffix"));
    }

    [Fact]
    public void NumberInput_Prefix_AddsPrefixSlot()
    {
        NumberInputBase input = new NumberInput<int>();
        var withPrefix = input.Prefix("$");

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("$", slot!.Children[0]);
    }

    [Fact]
    public void NumberInput_Suffix_AddsSuffixSlot()
    {
        NumberInputBase input = new NumberInput<int>();
        var withSuffix = input.Suffix("kg");

        var slot = FindSlot(withSuffix, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal("kg", slot!.Children[0]);
    }

    [Fact]
    public void SliderInput_Prefix_AddsPrefixSlot()
    {
        var state = new MockState<int>(50);
        var input = state.ToSliderInput().Prefix("$");

        var slot = FindSlot(input, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("$", slot!.Children[0]);
    }

    [Fact]
    public void SliderInput_Suffix_AddsSuffixSlot()
    {
        var state = new MockState<int>(50);
        var input = state.ToSliderInput().Suffix("kg");

        var slot = FindSlot(input, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal("kg", slot!.Children[0]);
    }

    [Fact]
    public void NumberRangeInput_Prefix_AddsPrefixSlot()
    {
        NumberRangeInputBase input = new NumberRangeInput<int>();
        var withPrefix = input.Prefix("$");

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("$", slot!.Children[0]);
    }

    [Fact]
    public void NumberRangeInput_Suffix_AddsSuffixSlot()
    {
        NumberRangeInputBase input = new NumberRangeInput<int>();
        var withSuffix = input.Suffix("USD");

        var slot = FindSlot(withSuffix, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal("USD", slot!.Children[0]);
    }

    [Fact]
    public void SelectInput_Prefix_AddsPrefixSlot()
    {
        var state = new MockState<string>("a");
        var input = state.ToSelectInput().Prefix("$");
        var slot = FindSlot(input, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("$", slot!.Children[0]);
    }

    [Fact]
    public void SelectInput_Suffix_AddsSuffixSlot()
    {
        var state = new MockState<string>("a");
        var input = state.ToSelectInput().Suffix(".00");
        var slot = FindSlot(input, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal(".00", slot!.Children[0]);
    }

    [Fact]
    public void SelectInput_PrefixAndSuffix_Coexist()
    {
        var state = new MockState<string>("a");
        var input = state.ToSelectInput().Prefix("$").Suffix("USD");
        Assert.NotNull(FindSlot(input, "Prefix"));
        Assert.NotNull(FindSlot(input, "Suffix"));
    }

    [Fact]
    public void SelectInput_Prefix_AcceptsWidgetContent()
    {
        var state = new MockState<string>("a");
        var button = new Button("Click");
        var input = state.ToSelectInput().Prefix(button);
        var slot = FindSlot(input, "Prefix");
        Assert.NotNull(slot);
        Assert.Same(button, slot!.Children[0]);
    }

    [Fact]
    public void BoolInput_Prefix_AddsPrefixSlot()
    {
        var state = new MockState<bool>(false);
        var input = state.ToBoolInput().Prefix("On");
        var slot = FindSlot(input, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("On", slot!.Children[0]);
    }

    [Fact]
    public void BoolInput_Suffix_AddsSuffixSlot()
    {
        var state = new MockState<bool>(false);
        var input = state.ToBoolInput().Suffix("Off");
        var slot = FindSlot(input, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal("Off", slot!.Children[0]);
    }

    [Fact]
    public void BoolInput_PrefixAndSuffix_Coexist()
    {
        var state = new MockState<bool>(false);
        var input = state.ToBoolInput().Prefix("On").Suffix("Off");
        Assert.NotNull(FindSlot(input, "Prefix"));
        Assert.NotNull(FindSlot(input, "Suffix"));
    }

    [Fact]
    public void BoolInput_Prefix_AcceptsWidgetContent()
    {
        var state = new MockState<bool>(false);
        var button = new Button("Click");
        var input = state.ToBoolInput().Prefix(button);
        var slot = FindSlot(input, "Prefix");
        Assert.NotNull(slot);
        Assert.Same(button, slot!.Children[0]);
    }

    [Fact]
    public void DateRangeInput_Prefix_AddsPrefixSlot()
    {
        var state = new MockState<(DateOnly, DateOnly)>((DateOnly.MinValue, DateOnly.MaxValue));
        var input = state.ToDateRangeInput().Prefix("$");
        var slot = FindSlot(input, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("$", slot!.Children[0]);
    }

    [Fact]
    public void DateRangeInput_Suffix_AddsSuffixSlot()
    {
        var state = new MockState<(DateOnly, DateOnly)>((DateOnly.MinValue, DateOnly.MaxValue));
        var input = state.ToDateRangeInput().Suffix("days");
        var slot = FindSlot(input, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal("days", slot!.Children[0]);
    }

    [Fact]
    public void DateRangeInput_PrefixAndSuffix_Coexist()
    {
        var state = new MockState<(DateOnly, DateOnly)>((DateOnly.MinValue, DateOnly.MaxValue));
        var input = state.ToDateRangeInput().Prefix("From").Suffix("To");
        Assert.NotNull(FindSlot(input, "Prefix"));
        Assert.NotNull(FindSlot(input, "Suffix"));
    }

    [Fact]
    public void DateRangeInput_Prefix_AcceptsWidgetContent()
    {
        var state = new MockState<(DateOnly, DateOnly)>((DateOnly.MinValue, DateOnly.MaxValue));
        var button = new Button("Click");
        var input = state.ToDateRangeInput().Prefix(button);
        var slot = FindSlot(input, "Prefix");
        Assert.NotNull(slot);
        Assert.Same(button, slot!.Children[0]);
    }

    [Fact]
    public void DateTimeInput_Prefix_AddsPrefixSlot()
    {
        DateTimeInputBase input = new DateTimeInput<DateTime>();
        var withPrefix = input.Prefix("$");

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("$", slot!.Children[0]);
    }

    [Fact]
    public void DateTimeInput_Suffix_AddsSuffixSlot()
    {
        DateTimeInputBase input = new DateTimeInput<DateTime>();
        var withSuffix = input.Suffix("UTC");

        var slot = FindSlot(withSuffix, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal("UTC", slot!.Children[0]);
    }

    [Fact]
    public void DateTimeInput_PrefixAndSuffix_Coexist()
    {
        DateTimeInputBase input = new DateTimeInput<DateTime>();
        var both = input.Prefix("From:").Suffix("UTC");

        Assert.NotNull(FindSlot(both, "Prefix"));
        Assert.NotNull(FindSlot(both, "Suffix"));
    }

    [Fact]
    public void DateTimeInput_Prefix_AcceptsWidgetContent()
    {
        DateTimeInputBase input = new DateTimeInput<DateTime>();
        var button = new Button("Pick");
        var withPrefix = input.Prefix(button);

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Same(button, slot!.Children[0]);
    }

    [Fact]
    public void ColorInput_Prefix_AddsPrefixSlot()
    {
        var state = new MockState<string>("#ff0000");
        var input = state.ToColorInput().Prefix("$");
        var slot = FindSlot(input, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("$", slot!.Children[0]);
    }

    [Fact]
    public void ColorInput_Suffix_AddsSuffixSlot()
    {
        var state = new MockState<string>("#ff0000");
        var input = state.ToColorInput().Suffix(".00");
        var slot = FindSlot(input, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal(".00", slot!.Children[0]);
    }

    [Fact]
    public void ColorInput_PrefixAndSuffix_Coexist()
    {
        var state = new MockState<string>("#ff0000");
        var input = state.ToColorInput().Prefix("$").Suffix("USD");
        Assert.NotNull(FindSlot(input, "Prefix"));
        Assert.NotNull(FindSlot(input, "Suffix"));
    }

    [Fact]
    public void ColorInput_Prefix_AcceptsWidgetContent()
    {
        var state = new MockState<string>("#ff0000");
        var button = new Button("Click");
        var input = state.ToColorInput().Prefix(button);
        var slot = FindSlot(input, "Prefix");
        Assert.NotNull(slot);
        Assert.Same(button, slot!.Children[0]);
    }

    [Fact]
    public void CodeInput_Prefix_AddsPrefixSlot()
    {
        CodeInputBase input = new CodeInput<string>();
        var withPrefix = input.Prefix("$");

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("$", slot!.Children[0]);
    }

    [Fact]
    public void CodeInput_Suffix_AddsSuffixSlot()
    {
        CodeInputBase input = new CodeInput<string>();
        var withSuffix = input.Suffix("lang");

        var slot = FindSlot(withSuffix, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal("lang", slot!.Children[0]);
    }

    [Fact]
    public void CodeInput_PrefixAndSuffix_Coexist()
    {
        CodeInputBase input = new CodeInput<string>();
        var both = input.Prefix("//").Suffix("lang");

        Assert.NotNull(FindSlot(both, "Prefix"));
        Assert.NotNull(FindSlot(both, "Suffix"));
    }

    [Fact]
    public void CodeInput_Prefix_AcceptsWidgetContent()
    {
        CodeInputBase input = new CodeInput<string>();
        var button = new Button("Click");
        var withPrefix = input.Prefix(button);

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Same(button, slot!.Children[0]);
    }

    [Fact]
    public void FeedbackInput_Prefix_AddsPrefixSlot()
    {
        FeedbackInputBase input = new FeedbackInput<int>();
        var withPrefix = input.Prefix("Rate:");

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("Rate:", slot!.Children[0]);
    }

    [Fact]
    public void FeedbackInput_Suffix_AddsSuffixSlot()
    {
        FeedbackInputBase input = new FeedbackInput<int>();
        var withSuffix = input.Suffix("/5");

        var slot = FindSlot(withSuffix, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal("/5", slot!.Children[0]);
    }

    [Fact]
    public void FeedbackInput_PrefixAndSuffix_Coexist()
    {
        FeedbackInputBase input = new FeedbackInput<int>();
        var both = input.Prefix("Rate:").Suffix("/5");

        Assert.NotNull(FindSlot(both, "Prefix"));
        Assert.NotNull(FindSlot(both, "Suffix"));
    }

    [Fact]
    public void FeedbackInput_Prefix_AcceptsWidgetContent()
    {
        FeedbackInputBase input = new FeedbackInput<int>();
        var button = new Button("Click");
        var withPrefix = input.Prefix(button);

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Same(button, slot!.Children[0]);
    }

    [Fact]
    public void IconInput_Prefix_AddsPrefixSlot()
    {
        IconInputBase input = new IconInput<Icons>();
        var withPrefix = input.Prefix("Icon:");

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Equal("Icon:", slot!.Children[0]);
    }

    [Fact]
    public void IconInput_Suffix_AddsSuffixSlot()
    {
        IconInputBase input = new IconInput<Icons>();
        var withSuffix = input.Suffix("selected");

        var slot = FindSlot(withSuffix, "Suffix");
        Assert.NotNull(slot);
        Assert.Equal("selected", slot!.Children[0]);
    }

    [Fact]
    public void IconInput_PrefixAndSuffix_Coexist()
    {
        IconInputBase input = new IconInput<Icons>();
        var both = input.Prefix("Icon:").Suffix("selected");

        Assert.NotNull(FindSlot(both, "Prefix"));
        Assert.NotNull(FindSlot(both, "Suffix"));
    }

    [Fact]
    public void IconInput_Prefix_AcceptsWidgetContent()
    {
        IconInputBase input = new IconInput<Icons>();
        var button = new Button("Click");
        var withPrefix = input.Prefix(button);

        var slot = FindSlot(withPrefix, "Prefix");
        Assert.NotNull(slot);
        Assert.Same(button, slot!.Children[0]);
    }
}
