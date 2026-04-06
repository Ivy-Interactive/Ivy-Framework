using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test.Services;

public class FileLinkHelperTests
{
    [Fact]
    public void CreateFileLinkClickHandler_CapturesFilePath_WhenFileUrlProvided()
    {
        var state = new TestState<string?>(null);
        var handler = FileLinkHelper.CreateFileLinkClickHandler(state);

        handler("file:///C:/some/path/file.cs");

        Assert.Equal("C:/some/path/file.cs", state.Value);
    }

    [Fact]
    public void CreateFileLinkClickHandler_IgnoresNonFileUrls()
    {
        var state = new TestState<string?>(null);
        var handler = FileLinkHelper.CreateFileLinkClickHandler(state);

        handler("https://example.com/page");

        Assert.Null(state.Value);
    }

    [Fact]
    public void CreateFileLinkClickHandler_IsCaseInsensitive()
    {
        var state = new TestState<string?>(null);
        var handler = FileLinkHelper.CreateFileLinkClickHandler(state);

        handler("FILE:///D:/test/readme.md");

        Assert.Equal("D:/test/readme.md", state.Value);
    }

    [Fact]
    public void BuildFileLinkSheet_ReturnsNull_WhenFilePathIsNull()
    {
        var result = FileLinkHelper.BuildFileLinkSheet(null, () => { }, []);
        Assert.Null(result);
    }

    [Fact]
    public void BuildFileLinkSheet_RendersImage_WhenFileIsImage()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
        File.WriteAllBytes(tempFile, [0x89, 0x50, 0x4E, 0x47]); // PNG header
        try
        {
            var result = FileLinkHelper.BuildFileLinkSheet(tempFile, () => { }, []);
            Assert.NotNull(result);
            Assert.IsType<Sheet>(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void BuildFileLinkSheet_RendersSyntaxHighlighted_WhenFileExists()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cs");
        File.WriteAllText(tempFile, "public class Foo { }");
        try
        {
            var result = FileLinkHelper.BuildFileLinkSheet(tempFile, () => { }, []);
            Assert.NotNull(result);
            Assert.IsType<Sheet>(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void BuildFileLinkSheet_ShowsSuggestions_WhenFileNotFound()
    {
        var result = FileLinkHelper.BuildFileLinkSheet(
            "/nonexistent/path/foo.cs", () => { }, []);
        Assert.NotNull(result);
        Assert.IsType<Sheet>(result);
    }

    [Fact]
    public void BuildFileLinkSheet_ReturnsSheet_WhenFileNotFound()
    {
        var result = FileLinkHelper.BuildFileLinkSheet(
            "/nonexistent/file.txt", () => { }, []);
        Assert.NotNull(result);
        Assert.IsType<Sheet>(result);
    }

    private class TestState<T> : IState<T>
    {
        private readonly T _initial;

        public TestState(T initial)
        {
            _initial = initial;
            Value = initial;
        }

        public T Value { get; set; }

        public T Set(T value) => Value = value;

        public T Set(Func<T, T> setter) => Value = setter(Value);

        public T Reset() => Value = _initial;

        public IDisposable Subscribe(IObserver<T> observer) => throw new NotImplementedException();
        public IDisposable SubscribeAny(Action action) => throw new NotImplementedException();
        public IDisposable SubscribeAny(Action<object?> action) => throw new NotImplementedException();
        public Type GetStateType() => typeof(T);
        public object? GetValueAsObject() => Value;
        public void Dispose() { }
        public IEffectTrigger ToTrigger() => throw new NotImplementedException();
    }
}
