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
    }

    [Fact]
    public void TextInput_ToTextInput_ShouldNotThrow()
    {
        var state = new MockState<string>("test");
        var widget = state.ToTextInput();
        Assert.NotNull(widget);
    }

    [Fact]
    public void DateRangeInput_ToDateRangeInput_ShouldNotThrow()
    {
        var state = new MockState<(DateOnly, DateOnly)>((DateOnly.MinValue, DateOnly.MaxValue));
        var widget = state.ToDateRangeInput();
        Assert.NotNull(widget);
    }

    [Fact]
    public void CodeInput_ToCodeInput_ShouldNotThrow()
    {
        var state = new MockState<string>("{}");
        var widget = state.ToCodeInput();
        Assert.NotNull(widget);
    }

    [Fact]
    public void FeedbackInput_ToFeedbackInput_ShouldNotThrow()
    {
        var state = new MockState<int>(5);
        var widget = state.ToFeedbackInput();
        Assert.NotNull(widget);
    }

    [Fact]
    public void IconInput_ToIconInput_ShouldNotThrow()
    {
        var state = new MockState<Icons>(Icons.Activity);
        var widget = state.ToIconInput();
        Assert.NotNull(widget);
    }

    [Fact]
    public void NumberInput_ToNumberInput_ShouldNotThrow()
    {
        var state = new MockState<int>(42);
        var widget = state.ToNumberInput();
        Assert.NotNull(widget);
    }

    [Fact]
    public void NumberRangeInput_ToNumberRangeInput_ShouldNotThrow()
    {
        var state = new MockState<(int, int)>((0, 100));
        var widget = state.ToNumberRangeInput();
        Assert.NotNull(widget);
    }

    public enum TestEnum { A, B, C }

    [Fact]
    public void Option_ToOptions_ShouldNotThrow()
    {
        var options = typeof(TestEnum).ToOptions();
        Assert.NotNull(options);
        Assert.NotEmpty(options);
    }

    [Fact]
    public void ReadOnlyInput_ToReadOnlyInput_ShouldNotThrow()
    {
        var state = new MockState<string>("readonly text");
        var widget = state.ToReadOnlyInput();
        Assert.NotNull(widget);
    }

    [Fact]
    public void SelectInput_ToSelectInput_ShouldNotThrow()
    {
        var state = new MockState<TestEnum>(TestEnum.A);
        var widget = state.ToSelectInput();
        Assert.NotNull(widget);
    }
}
