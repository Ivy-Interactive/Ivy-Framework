using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Hooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Ivy;

public abstract partial class ViewBase
{
    protected void CreateContext<T>(Func<T> factory) =>
        this.Context.CreateContext(factory);

    protected T UseContext<T>()
        => this.Context.UseContext<T>();

    protected object UseContext(Type type)
        => this.Context.UseContext(type);

    protected T UseService<T>()
        => this.Context.UseService<T>();

    protected object UseService(Type type)
        => this.Context.UseService(type);

    protected IState<T> UseState<T>(T? initialValue = default(T?), bool buildOnChange = true) =>
        this.Context.UseState(initialValue!, buildOnChange);

    [OverloadResolutionPriority(1)]
    protected IState<T> UseState<T>(Func<T>? buildInitialValue, bool buildOnChange = true) =>
        buildInitialValue is null
            ? this.Context.UseState<T>(initialValue: default, buildOnChange)
            : this.Context.UseState(buildInitialValue, buildOnChange);

    protected IRef<T> UseRef<T>(T? initialValue = default) =>
        this.Context.UseRef(initialValue);

    [OverloadResolutionPriority(1)]
    protected IRef<T> UseRef<T>(Func<T>? buildInitialValue) =>
        buildInitialValue is null
            ? this.Context.UseRef<T>(initialValue: default)
            : this.Context.UseRef(buildInitialValue);

    [OverloadResolutionPriority(1)]
    protected void UseEffect(Func<Task> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    [OverloadResolutionPriority(1)]
    protected void UseEffect(Func<Task<IDisposable?>> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected void UseEffect(Func<Task<IAsyncDisposable?>> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    [OverloadResolutionPriority(1)]
    protected void UseEffect(Func<IDisposable?> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected void UseEffect(Func<IAsyncDisposable?> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected void UseEffect(Action handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected T? UseArgs<T>() where T : class =>
        this.Context.UseArgs<T>();

    public ISignal<TInput, Unit> UseSignal<T, TInput>() where T : AbstractSignal<TInput, Unit> => this.Context.UseSignal<T, TInput>();

    public ISignal<TInput, TOutput> UseSignal<T, TInput, TOutput>() where T : AbstractSignal<TInput, TOutput> => this.Context.UseSignal<T, TInput, TOutput>();

    protected QueryResult<TValue> UseQuery<TValue, TKey>(TKey? key,
        Func<TKey, CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull =>
        this.Context.UseQuery(key, fetcher, options, initialValue, tags);

    protected QueryResult<TValue> UseQuery<TValue, TKey>(TKey? key,
        Func<CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull =>
        this.Context.UseQuery<TValue, TKey>(key, fetcher, options, initialValue, tags);

    protected QueryResult<TValue> UseQuery<TValue, TKey>(TKey? key,
        Func<Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull =>
        this.Context.UseQuery<TValue, TKey>(key, fetcher, options, initialValue, tags);

    protected QueryResult<TValue> UseQuery<TValue>(
        Func<CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null,
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0) =>
        this.Context.UseQuery(fetcher, options, initialValue, tags, callerFile, callerLine);

    protected QueryResult<TValue> UseQuery<TValue, TKey>(
        Func<TKey?> keyFactory,
        Func<TKey, CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull =>
        this.Context.UseQuery(keyFactory, fetcher, options, initialValue, tags);

    protected QueryResult<TValue> UseQuery<TValue, TKey>(
        Func<TKey?> keyFactory,
        Func<CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull =>
        this.Context.UseQuery<TValue, TKey>(keyFactory, fetcher, options, initialValue, tags);

    protected QueryResult<TValue> UseQuery<TValue, TKey>(
        Func<TKey?> keyFactory,
        Func<Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull =>
        this.Context.UseQuery<TValue, TKey>(keyFactory, fetcher, options, initialValue, tags);

    protected QueryMutator UseMutation(object key, QueryOptions? options = null) =>
        this.Context.UseMutation(key, options);

    protected QueryMutator<TValue> UseMutation<TValue, TKey>(TKey key, QueryOptions? options = null)
        where TKey : notnull =>
        this.Context.UseMutation<TValue, TKey>(key, options);

    [OverloadResolutionPriority(1)]
    protected IState<string?> UseDownload(Func<byte[]> factory, string mimeType, string fileName) =>
        this.Context.UseDownload(() => Task.FromResult(factory()), mimeType, fileName);

    protected IState<string?> UseDownload(Func<Task<byte[]>> factory, string mimeType, string fileName) =>
        this.Context.UseDownload(factory, mimeType, fileName);

    [OverloadResolutionPriority(1)]
    protected IState<string?> UseDownload(Func<Stream> factory, string mimeType, string fileName) =>
        this.Context.UseDownload(() => Task.FromResult(factory()), mimeType, fileName);

    protected IState<string?> UseDownload(Func<Task<Stream>> factory, string mimeType, string fileName) =>
        this.Context.UseDownload(factory, mimeType, fileName);

    protected RefreshToken UseRefreshToken() =>
        this.Context.UseRefreshToken();

    protected T UseMemo<T>(Func<T> factory, params object?[] deps) =>
        this.Context.UseMemo(factory, deps);

    protected T UseMemo<T>(Func<T> factory) =>
        this.Context.UseMemo(factory);

    protected (T value, Func<string, T> dispatch) UseReducer<T>(Func<T, string, T> reducer, T initialState) =>
        this.Context.UseReducer(reducer, initialState);

    protected (object? triggerView, Action<T> triggerCallback) UseTrigger<T>(Func<IState<bool>, T, object?> viewFactory) =>
        this.Context.UseTrigger(viewFactory);

    protected (object? triggerView, Action triggerCallback) UseTrigger(Func<IState<bool>, object?> viewFactory) =>
        this.Context.UseTrigger(viewFactory);

    protected IState<UploadContext> UseUpload(UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null) =>
        this.Context.UseUpload(handler, defaultContentType, defaultFileName);

    protected IState<UploadContext> UseUpload(IUploadHandler handler, string? defaultContentType = null, string? defaultFileName = null) =>
        this.Context.UseUpload(handler, defaultContentType, defaultFileName);

    protected WebhookEndpoint UseWebhook(Func<HttpRequest, IActionResult> handler) =>
        this.Context.UseWebhook(handler);

    protected WebhookEndpoint UseWebhook(Action<HttpRequest> handler) =>
        this.Context.UseWebhook(handler);

    protected WebhookEndpoint UseWebhook(Func<HttpRequest, Task<IActionResult>> handler) =>
        this.Context.UseWebhook(handler);

    protected WebhookEndpoint UseWebhook(Func<HttpRequest, Task> handler) =>
        this.Context.UseWebhook(handler);

    protected DataTableConnection? UseDataTable(IQueryable queryable, RefreshToken? refreshToken = null) =>
        this.Context.UseDataTable(queryable, refreshToken);

    protected DataTableConnection? UseDataTable(IQueryable queryable, Func<object, object?>? idSelector, RefreshToken? refreshToken = null) =>
        this.Context.UseDataTable(queryable, idSelector, refreshToken);

    protected DataTableConnection? UseDataTable(IQueryable queryable, Func<object, object?>? idSelector, DataTableColumn[]? columns, RefreshToken? refreshToken = null) =>
        this.Context.UseDataTable(queryable, idSelector, columns, refreshToken);

    protected IView UseBlades(Func<IView> rootBlade, string? title = null, Size? width = null) =>
        this.Context.UseBlades(rootBlade, title, width);

    protected (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) UseForm<TModel>(Func<FormBuilder<TModel>> factory) =>
        this.Context.UseForm(factory);

    protected INavigator UseNavigation() =>
        this.Context.UseNavigation();

    protected NavigationBeacon<T>? UseNavigationBeacon<T>() =>
        this.Context.UseNavigationBeacon<T>();

    protected (IView? alertView, ShowAlertDelegate showAlert) UseAlert() =>
        this.Context.UseAlert();

    protected (IView? loadingView, ShowLoadingDelegate showLoading) UseLoading() =>
        this.Context.UseLoading();
    protected (object? dialogView, ShowFileDialogDelegate showFileDialog) UseFileDialog(
        IUploadHandler handler,
        string? accept = null,
        bool multiple = false,
        long? maxFileSize = null,
        long? minFileSize = null) =>
        this.Context.UseFileDialog(handler, accept, multiple, maxFileSize, minFileSize);

    protected (object? dialogView, ShowFileDialogDelegate showFileDialog) UseFileDialog(
        string? accept = null,
        bool multiple = false) =>
        this.Context.UseFileDialog(accept, multiple);

    protected (object? dialogView, ShowSaveDialogDelegate showSaveDialog) UseSaveDialog(
        Func<Task<byte[]>> contentFactory,
        string mimeType,
        string suggestedName) =>
        this.Context.UseSaveDialog(contentFactory, mimeType, suggestedName);

    protected (object? dialogView, ShowFolderDialogDelegate showFolderDialog) UseFolderDialog() =>
        this.Context.UseFolderDialog();

    protected IWriteStream<T> UseStream<T>() =>
        this.Context.UseStream<T>();

    protected void UseInterval(Action callback, TimeSpan? interval) =>
        this.Context.UseInterval(callback, interval);

    protected Action<string> UseClipboard() =>
        this.Context.UseClipboard();

    protected static EffectTrigger OnMount() =>
        EffectTrigger.OnMount();
}
