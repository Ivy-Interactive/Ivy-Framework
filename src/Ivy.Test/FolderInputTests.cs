using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Ivy.Test;

public class FolderInputTests
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

    [Fact]
    public void FolderInput_DefaultProperties()
    {
        var input = new FolderInput();

        Assert.Null(input.Value);
        Assert.False(input.Disabled);
        Assert.Null(input.Invalid);
        Assert.Null(input.Placeholder);
        Assert.True(input.Nullable);
        Assert.False(input.AutoFocus);
        Assert.Null(input.OnBlur);
        Assert.Null(input.OnFocus);
        Assert.Null(input.OnChange);
    }

    [Fact]
    public void FolderInput_SupportedStateTypes_ReturnsStringType()
    {
        var input = new FolderInput();

        var types = input.SupportedStateTypes();

        Assert.Single(types);
        Assert.Equal(typeof(string), types[0]);
    }

    [Fact]
    public void FolderInput_ValidateValue_ReturnsSuccess()
    {
        var input = new FolderInput();

        var result = input.ValidateValue("any-folder");

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void FolderInput_ValidateValue_WithNull_ReturnsSuccess()
    {
        var input = new FolderInput();

        var result = input.ValidateValue(null);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void FolderInputExtensions_Placeholder_SetsPlaceholder()
    {
        var input = new FolderInput();

        var result = input.Placeholder("Choose directory");

        Assert.Equal("Choose directory", result.Placeholder);
    }

    [Fact]
    public void FolderInputExtensions_Disabled_SetsDisabled()
    {
        var input = new FolderInput();

        var result = input.Disabled();

        Assert.True(result.Disabled);
    }

    [Fact]
    public void FolderInputExtensions_Disabled_False_SetsDisabledFalse()
    {
        var input = new FolderInput { Disabled = true };

        var result = input.Disabled(false);

        Assert.False(result.Disabled);
    }

    [Fact]
    public void FolderInputExtensions_Invalid_SetsInvalid()
    {
        var input = new FolderInput();

        var result = input.Invalid("Required");

        Assert.Equal("Required", result.Invalid);
    }

    [Fact]
    public void FolderInputExtensions_Invalid_Null_ClearsInvalid()
    {
        var input = new FolderInput { Invalid = "Error" };

        var result = input.Invalid(null);

        Assert.Null(result.Invalid);
    }

    [Fact]
    public void FolderInput_DefaultMode_IsName()
    {
        var input = new FolderInput();
        Assert.Equal(FolderInputMode.Name, input.Mode);
    }

    [Fact]
    public void FolderInputExtensions_Mode_SetsMode()
    {
        var input = new FolderInput().Mode(FolderInputMode.FullPath);
        Assert.Equal(FolderInputMode.FullPath, input.Mode);
    }

    [Fact]
    public void FolderInput_ToFolderInput_WithMode_SetsMode()
    {
        var state = new MockState<string?>(null);
        var input = state.ToFolderInput(mode: FolderInputMode.FullPath);
        Assert.Equal(FolderInputMode.FullPath, input.Mode);
    }

    [Fact]
    public void ValidatePath_FullPathMode_NonExistentPath_SetsInvalid()
    {
        var input = new FolderInput { Value = @"C:\NonExistent\Path\That\Does\Not\Exist", Mode = FolderInputMode.FullPath };

        var result = input.ValidatePath();

        Assert.Equal("Directory does not exist", result.Invalid);
    }

    [Fact]
    public void ValidatePath_FullPathMode_ExistingPath_NoInvalid()
    {
        var input = new FolderInput { Value = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), Mode = FolderInputMode.FullPath };

        var result = input.ValidatePath();

        Assert.Null(result.Invalid);
    }

    [Fact]
    public void ValidatePath_NameMode_DoesNotValidate()
    {
        var input = new FolderInput { Value = @"C:\NonExistent\Path", Mode = FolderInputMode.Name };

        var result = input.ValidatePath();

        Assert.Null(result.Invalid);
    }

    [Fact]
    public void ValidatePath_NullValue_DoesNotValidate()
    {
        var input = new FolderInput { Value = null, Mode = FolderInputMode.FullPath };

        var result = input.ValidatePath();

        Assert.Null(result.Invalid);
    }

    [Fact]
    public void ValidatePath_EmptyValue_DoesNotValidate()
    {
        var input = new FolderInput { Value = "", Mode = FolderInputMode.FullPath };

        var result = input.ValidatePath();

        Assert.Null(result.Invalid);
    }

    [Fact]
    public void ValidateValue_FullPathMode_NonExistentPath_ReturnsError()
    {
        var input = new FolderInput { Mode = FolderInputMode.FullPath };

        var result = input.ValidateValue(@"C:\NonExistent\Path\That\Does\Not\Exist");

        Assert.False(result.IsValid);
        Assert.Equal("Directory does not exist", result.ErrorMessage);
    }

    [Fact]
    public void ValidateValue_FullPathMode_ExistingPath_ReturnsSuccess()
    {
        var input = new FolderInput { Mode = FolderInputMode.FullPath };

        var result = input.ValidateValue(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateValue_NameMode_AlwaysReturnsSuccess()
    {
        var input = new FolderInput { Mode = FolderInputMode.Name };

        var result = input.ValidateValue(@"C:\NonExistent\Path");

        Assert.True(result.IsValid);
    }
}
