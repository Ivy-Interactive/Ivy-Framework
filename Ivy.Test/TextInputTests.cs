using System.Reactive.Subjects;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

namespace Ivy.Test;

//Todo: I don't really think we need these tests, as if TextInput<string> works, then TextInput should work too.

public class TextInputTests
{
    [Fact]
    public void TextInput_WithState_BindsCorrectly()
    {
        var state = new MockState<string>("initial");

        var textInput = new TextInput(state);

        Assert.Equal("initial", textInput.Value);
        Assert.Equal(TextInputs.Text, textInput.Variant);
        Assert.NotNull(textInput.OnChange);
    }

    [Fact]
    public void TextInput_WithState_UpdatesStateOnChange()
    {
        var state = new MockState<string>("initial");
        var textInput = new TextInput(state);

        // Simulate OnChange event
        var eventArgs = new Event<IInput<string>, string>("OnChange", textInput, "new value");
        textInput.OnChange!(eventArgs);

        Assert.Equal("new value", state.Value);
    }

    [Fact]
    public void TextInput_WithValueAndOnChange_WorksCorrectly()
    {
        string? capturedValue = null;
        var onChange = new Action<Event<IInput<string>, string>>(e => capturedValue = e.Value);

        var textInput = new TextInput("initial", onChange);

        Assert.Equal("initial", textInput.Value);
        Assert.NotNull(textInput.OnChange);

        // Test the OnChange handler
        var eventArgs = new Event<IInput<string>, string>("OnChange", textInput, "updated");
        textInput.OnChange!(eventArgs);
        Assert.Equal("updated", capturedValue);
    }

    [Fact]
    public void TextInput_WithPlaceholderAndVariant_WorksCorrectly()
    {
        var textInput = new TextInput(placeholder: "Enter text", variant: TextInputs.Password);

        Assert.Equal("Enter text", textInput.Placeholder);
        Assert.Equal(TextInputs.Password, textInput.Variant);
        Assert.False(textInput.Disabled);
    }

    [Fact]
    public void TextInput_GenericVersion_WorksForStringType()
    {
        var state = new MockState<string>("test");

        var textInput = new TextInput<string>(state);

        Assert.Equal("test", textInput.Value);
        Assert.Equal(TextInputs.Text, textInput.Variant);
    }

    [Theory]
    [InlineData(TextInputs.Text)]
    [InlineData(TextInputs.Textarea)]
    [InlineData(TextInputs.Email)]
    [InlineData(TextInputs.Tel)]
    [InlineData(TextInputs.Url)]
    [InlineData(TextInputs.Password)]
    [InlineData(TextInputs.Search)]
    public void TextInput_AllVariants_WorkCorrectly(TextInputs variant)
    {
        var textInput = new TextInput(variant: variant);

        Assert.Equal(variant, textInput.Variant);
    }

    [Fact]
    public void TextInput_Disabled_WorksCorrectly()
    {
        var textInput = new TextInput(disabled: true);

        Assert.True(textInput.Disabled);
    }

    [Fact]
    public void TextInput_WithInvalid_WorksCorrectly()
    {
        var textInput = new TextInput();
        textInput.Invalid = "This field is required";

        Assert.Equal("This field is required", textInput.Invalid);
    }

    [Fact]
    public void TextInput_WithShortcutKey_WorksCorrectly()
    {
        var textInput = new TextInput();
        textInput.ShortcutKey = "Ctrl+K";

        Assert.Equal("Ctrl+K", textInput.ShortcutKey);
    }

    [Fact]
    public void TextInput_ExtensionMethods_WorkCorrectly()
    {
        var state = new MockState<string>("test");

        // Test ToTextInput extension
        var textInput = state.ToTextInput();
        Assert.Equal(TextInputs.Text, textInput.Variant);

        // Test ToPasswordInput extension
        var passwordInput = state.ToPasswordInput();
        Assert.Equal(TextInputs.Password, passwordInput.Variant);

        // Test ToSearchInput extension
        var searchInput = state.ToSearchInput();
        Assert.Equal(TextInputs.Search, searchInput.Variant);

        // Test ToEmailInput extension
        var emailInput = state.ToEmailInput();
        Assert.Equal(TextInputs.Email, emailInput.Variant);

        // Test ToUrlInput extension
        var urlInput = state.ToUrlInput();
        Assert.Equal(TextInputs.Url, urlInput.Variant);

        // Test ToTelInput extension
        var telInput = state.ToTelInput();
        Assert.Equal(TextInputs.Tel, telInput.Variant);
    }

    [Fact]
    public void TextInput_ExtensionMethods_WithPlaceholder_WorkCorrectly()
    {
        var state = new MockState<string>("test");

        var textInput = state.ToTextInput(placeholder: "Enter text");
        Assert.Equal("Enter text", textInput.Placeholder);

        var passwordInput = state.ToPasswordInput(placeholder: "Enter password");
        Assert.Equal("Enter password", passwordInput.Placeholder);
    }

    [Fact]
    public void TextInput_ExtensionMethods_WithDisabled_WorkCorrectly()
    {
        var state = new MockState<string>("test");

        var textInput = state.ToTextInput(disabled: true);
        Assert.True(textInput.Disabled);

        var passwordInput = state.ToPasswordInput(disabled: true);
        Assert.True(passwordInput.Disabled);
    }

    [Fact]
    public void TextInput_ChainedExtensionMethods_WorkCorrectly()
    {
        var state = new MockState<string>("test");

        var textInput = state.ToTextInput()
            .Placeholder("Enter text")
            .Disabled(true)
            .Variant(TextInputs.Email)
            .Invalid("Invalid email")
            .ShortcutKey("Ctrl+E");

        Assert.Equal("Enter text", textInput.Placeholder);
        Assert.True(textInput.Disabled);
        Assert.Equal(TextInputs.Email, textInput.Variant);
        Assert.Equal("Invalid email", textInput.Invalid);
        Assert.Equal("Ctrl+E", textInput.ShortcutKey);
    }

    [Fact]
    public void TextInput_WithNullValue_WorksCorrectly()
    {
        var state = new MockState<string?>(null);

        var textInput = new TextInput<string?>(state);

        Assert.Null(textInput.Value);
    }

    [Fact]
    public void TextInput_WithEmptyString_WorksCorrectly()
    {
        var state = new MockState<string>("");

        var textInput = new TextInput(state);

        Assert.Equal("", textInput.Value);
    }

    [Fact]
    public void TextInput_OnBlurEvent_WorksCorrectly()
    {
        var textInput = new TextInput();
        bool onBlurCalled = false;

        textInput.OnBlur = e => onBlurCalled = true;

        Assert.NotNull(textInput.OnBlur);

        // Simulate OnBlur event
        var eventArgs = new Event<IAnyInput>("OnBlur", textInput);
        textInput.OnBlur!(eventArgs);

        Assert.True(onBlurCalled);
    }

    [Fact]
    public void TextInput_SupportedStateTypes_ReturnsEmptyArray()
    {
        var textInput = new TextInput();

        var supportedTypes = textInput.SupportedStateTypes();

        Assert.Empty(supportedTypes);
    }

    [Fact]
    public void TextInput_WithDifferentGenericTypes_WorksCorrectly()
    {
        // Test with string
        var stringState = new MockState<string>("string value");
        var stringInput = new TextInput<string>(stringState);
        Assert.Equal("string value", stringInput.Value);

        // Test with nullable string
        var nullableStringState = new MockState<string?>(null);
        var nullableStringInput = new TextInput<string?>(nullableStringState);
        Assert.Null(nullableStringInput.Value);
    }

    private class MockState<T>(T value) : IState<T>
    {
        private readonly Subject<T> _subject = new();

        public T Value { get; set; } = value;

        public T Set(T value) => Value = value;
        public Type GetStateType() => typeof(T);

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(Value);
            return _subject.Subscribe(observer);
        }

        public void Dispose()
        {
            _subject.Dispose();
        }

        public IDisposable SubscribeAny(Action action)
        {
            return _subject.Subscribe(_ => action());
        }

        public IDisposable SubscribeAny(Action<object?> action)
        {
            return _subject.Subscribe(x => action(x));
        }

        public IEffectTrigger ToTrigger()
        {
            return EffectTrigger.AfterChange(this);
        }
    }
}