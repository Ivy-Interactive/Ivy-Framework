using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test.Services;

public class FileLinkHelperTests
{
    private static readonly IConfigService TestConfig = new TestConfigService();

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
        var result = FileLinkHelper.BuildFileLinkSheet(null, () => { }, [], TestConfig);
        Assert.Null(result);
    }

    [Fact]
    public void BuildFileLinkSheet_RendersImage_WhenFileIsImage()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
        File.WriteAllBytes(tempFile, [0x89, 0x50, 0x4E, 0x47]); // PNG header
        try
        {
            var result = FileLinkHelper.BuildFileLinkSheet(tempFile, () => { }, [], TestConfig);
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
            var result = FileLinkHelper.BuildFileLinkSheet(tempFile, () => { }, [], TestConfig);
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
            "/nonexistent/path/foo.cs", () => { }, [], TestConfig);
        Assert.NotNull(result);
        Assert.IsType<Sheet>(result);
    }

    [Fact]
    public void BuildFileLinkSheet_ReturnsSheet_WhenFileNotFound()
    {
        var result = FileLinkHelper.BuildFileLinkSheet(
            "/nonexistent/file.txt", () => { }, [], TestConfig);
        Assert.NotNull(result);
        Assert.IsType<Sheet>(result);
    }

    private class TestConfigService : IConfigService
    {
        public TendrilSettings Settings => new();
        public string TendrilHome => "";
        public string ConfigPath => "";
        public string PlanFolder => "";
        public List<ProjectConfig> Projects => [];
        public List<LevelConfig> Levels => [];
        public string[] LevelNames => [];
        public EditorConfig Editor => new() { Command = "code", Label = "VS Code" };
        public bool NeedsOnboarding => false;

        public ProjectConfig? GetProject(string name) => null;
        public BadgeVariant GetBadgeVariant(string level) => BadgeVariant.Outline;
        public Colors? GetProjectColor(string projectName) => null;
        public void SaveSettings() { }
        public void ReloadSettings() { }
#pragma warning disable CS0067
        public event EventHandler? SettingsReloaded;
#pragma warning restore CS0067
        public void SetPendingCodingAgent(string name) { }
        public string? GetPendingCodingAgent() => null;
        public void SetPendingTendrilHome(string path) { }
        public string? GetPendingTendrilHome() => null;
        public void SetPendingProject(ProjectConfig project) { }
        public ProjectConfig? GetPendingProject() => null;
        public void SetPendingVerificationDefinitions(List<VerificationConfig> definitions) { }
        public List<VerificationConfig>? GetPendingVerificationDefinitions() => null;
        public void CompleteOnboarding(string tendrilHome) { }
        public void OpenInEditor(string path) { }
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

        public T Set(T value)
        {
            return Value = value;
        }

        public T Set(Func<T, T> setter)
        {
            return Value = setter(Value);
        }

        public T Reset()
        {
            return Value = _initial;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            throw new NotImplementedException();
        }

        public IDisposable SubscribeAny(Action action)
        {
            throw new NotImplementedException();
        }

        public IDisposable SubscribeAny(Action<object?> action)
        {
            throw new NotImplementedException();
        }

        public Type GetStateType()
        {
            return typeof(T);
        }

        public object? GetValueAsObject()
        {
            return Value;
        }

        public void Dispose()
        {
        }

        public IEffectTrigger ToTrigger()
        {
            throw new NotImplementedException();
        }
    }
}