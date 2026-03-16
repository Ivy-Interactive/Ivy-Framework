using System;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Hooks;
using Xunit;

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
    }

    [Fact]
    public void ColorInput_ToColorInput_ShouldNotThrow()
    {
        var state = new MockState<string>("#ffffff");
        var widget = state.ToColorInput();
        Assert.NotNull(widget);
        Assert.IsType<ColorInput<string>>(widget);
    }



    [Fact]
    public void DateRangeInput_ToDateRangeInput_ShouldNotThrow()
    {
        var state = new MockState<(DateOnly, DateOnly)>((DateOnly.MinValue, DateOnly.MaxValue));
        var widget = state.ToDateRangeInput();
        Assert.NotNull(widget);
        Assert.IsType<DateRangeInput<(DateOnly, DateOnly)>>(widget);
    }

    [Fact]
    public void CodeInput_ToCodeInput_ShouldNotThrow()
    {
        var state = new MockState<string>("{}");
        var widget = state.ToCodeInput();
        Assert.NotNull(widget);
        Assert.IsType<CodeInput<string>>(widget);
    }

    [Fact]
    public void FeedbackInput_ToFeedbackInput_ShouldNotThrow()
    {
        var state = new MockState<int>(5);
        var widget = state.ToFeedbackInput();
        Assert.NotNull(widget);
        Assert.IsType<FeedbackInput<int>>(widget);
    }

    [Fact]
    public void IconInput_ToIconInput_ShouldNotThrow()
    {
        var state = new MockState<Icons>(Icons.Activity);
        var widget = state.ToIconInput();
        Assert.NotNull(widget);
        Assert.IsType<IconInput<Icons>>(widget);
    }



    [Fact]
    public void NumberRangeInput_ToNumberRangeInput_ShouldNotThrow()
    {
        var state = new MockState<(int, int)>((0, 100));
        var widget = state.ToNumberRangeInput();
        Assert.NotNull(widget);
        Assert.IsType<NumberRangeInput<int>>(widget);
    }

    public enum TestEnum { A, B, C }

    [Fact]
    public void Option_ToOptions_ShouldNotThrow()
    {
        var options = typeof(TestEnum).ToOptions();
        Assert.NotNull(options);
        Assert.NotEmpty(options);
        Assert.All(options, opt => Assert.IsType<Option<TestEnum>>(opt));
    }








    [Theory]
    [InlineData(typeof(short), (short)1)]
    [InlineData(typeof(int), 1)]
    [InlineData(typeof(double), 1.0)]
    public void NumberInput_PreservesType(Type type, object value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, value)!;
        var widget = state.ToNumberInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(NumberInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(string), "test")]
    [InlineData(typeof(int), 1)]
    public void TextInput_PreservesType(Type type, object value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, value)!;
        var widget = state.ToTextInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(TextInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(string), "test")]
    public void ReadOnlyInput_PreservesType(Type type, object value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, value)!;
        var widget = state.ToReadOnlyInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(ReadOnlyInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(TestEnum), TestEnum.A)]
    public void SelectInput_PreservesType(Type type, object value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, value)!;
        var widget = state.ToSelectInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(SelectInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(int), 1)]
    public void BoolInput_PreservesType(Type type, object value)
    {
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, value)!;
        var widget = state.ToBoolInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(BoolInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }

    [Theory]
    [InlineData(typeof(DateTime), "2024-01-01T00:00:00")]
    public void DateTimeInput_PreservesType(Type type, string valueStr)
    {
        var value = DateTime.Parse(valueStr);
        var mockStateType = typeof(MockState<>).MakeGenericType(type);
        var state = (IAnyState)Activator.CreateInstance(mockStateType, value)!;
        var widget = state.ToDateTimeInput();
        Assert.NotNull(widget);
        var expectedWidgetType = typeof(DateTimeInput<>).MakeGenericType(type);
        Assert.IsType(expectedWidgetType, widget);
    }
}
