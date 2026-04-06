using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Ivy.Test;

public class ContentInputTests
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
    public void ToContentInput_SetsPropsFromUploadContext()
    {
        var textState = new MockState<string>("hello");
        var uploadContext = new MockState<UploadContext>(new UploadContext("/upload", _ => { })
        {
            Accept = "image/*",
            MaxFileSize = 1024 * 1024,
            MaxFiles = 5
        });

        var widget = textState.ToContentInput(uploadContext);

        Assert.Equal("/upload", widget.UploadUrl);
        Assert.Equal("image/*", widget.Accept);
        Assert.Equal(1024 * 1024, widget.MaxFileSize);
        Assert.Equal(5, widget.MaxFiles);
    }

    [Fact]
    public void Files_ProjectsFileUploadMetadata()
    {
        var textState = new MockState<string>("");
        var uploadContext = new MockState<UploadContext>(new UploadContext("/upload", _ => { }));

        var files = new[]
        {
            new FileUpload
            {
                Id = Guid.NewGuid(),
                FileName = "test.png",
                ContentType = "image/png",
                Length = 1234,
                Progress = 0.5f,
                Status = FileUploadStatus.Loading
            }
        };

        var widget = textState.ToContentInput(uploadContext).Files(files);

        Assert.NotNull(widget.Files);
        var fileList = widget.Files!.ToList();
        Assert.Single(fileList);
        Assert.Equal("test.png", fileList[0].FileName);
        Assert.Equal("image/png", fileList[0].ContentType);
        Assert.Equal(1234, fileList[0].Length);
    }

    [Fact]
    public void OnCancel_CallsUploadContextCancel()
    {
        var textState = new MockState<string>("");
        Guid? cancelledFileId = null;
        var uploadContext = new MockState<UploadContext>(new UploadContext("/upload", fileId => cancelledFileId = fileId));

        var widget = textState.ToContentInput(uploadContext);

        Assert.NotNull(widget.OnCancel);
        var testFileId = Guid.NewGuid();
        widget.OnCancel!.Invoke(new Event<IAnyInput, Guid>("OnCancel", widget, testFileId));

        Assert.Equal(testFileId, cancelledFileId);
    }

    [Fact]
    public void DefaultValue_IsEmptyString()
    {
        var textState = new MockState<string>("");
        var uploadContext = new MockState<UploadContext>(new UploadContext("/upload", _ => { }));

        var widget = textState.ToContentInput(uploadContext);

        Assert.IsType<ContentInput<string>>(widget);
        var typed = (ContentInput<string>)widget;
        Assert.Equal("", typed.Value);
    }

    [Fact]
    public void ExtensionMethods_SetProperties()
    {
        var textState = new MockState<string>("");
        var uploadContext = new MockState<UploadContext>(new UploadContext("/upload", _ => { }));

        var widget = textState.ToContentInput(uploadContext)
            .Placeholder("Type here...")
            .Rows(5)
            .MaxLength(500)
            .Accept("image/*,.pdf")
            .MaxFileSize(10 * 1024 * 1024)
            .MaxFiles(3)
            .Disabled();

        Assert.Equal("Type here...", widget.Placeholder);
        Assert.Equal(5, widget.Rows);
        Assert.Equal(500, widget.MaxLength);
        Assert.Equal("image/*,.pdf", widget.Accept);
        Assert.Equal(10 * 1024 * 1024, widget.MaxFileSize);
        Assert.Equal(3, widget.MaxFiles);
        Assert.True(widget.Disabled);
    }

    [Fact]
    public void Files_GenericOverload_ProjectsFileUploadOfT()
    {
        var textState = new MockState<string>("");
        var uploadContext = new MockState<UploadContext>(new UploadContext("/upload", _ => { }));

        var files = new[]
        {
            new FileUpload<byte[]>
            {
                Id = Guid.NewGuid(),
                FileName = "doc.pdf",
                ContentType = "application/pdf",
                Length = 5678,
                Progress = 1.0f,
                Status = FileUploadStatus.Finished,
                Content = new byte[] { 1, 2, 3 }
            }
        };

        var widget = textState.ToContentInput(uploadContext).Files(files);

        Assert.NotNull(widget.Files);
        var fileList = widget.Files!.ToList();
        Assert.Single(fileList);
        Assert.Equal("doc.pdf", fileList[0].FileName);
        Assert.IsType<FileUpload>(fileList[0]); // Should be projected, not FileUpload<byte[]>
    }
}
