using System.Runtime.CompilerServices;
using Ivy.Chrome;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Services;
using Ivy.Shared;
using Ivy.Views.Blades;
using Ivy.Views.DataTables;
using Ivy.Views.Alerts;
using Ivy.Views.Forms;
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

    protected IState<T> UseState<T>(Func<T> buildInitialValue, bool buildOnChange = true) =>
        this.Context.UseState(buildInitialValue, buildOnChange);

    protected IState<T> UseRef<T>(T? initialValue = default) =>
        this.Context.UseRef(initialValue);

    protected IState<T> UseRef<T>(Func<T> buildInitialValue) =>
        this.Context.UseRef(buildInitialValue);

    [OverloadResolutionPriority(1)]
    protected void UseEffect(Func<Task> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected void UseEffect(Func<Task<IDisposable>> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected void UseEffect(Func<IDisposable> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected void UseEffect(Action handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected T? UseArgs<T>() where T : class =>
        this.Context.UseArgs<T>();

    public ISignalSender<TInput, TOutput> CreateSignal<T, TInput, TOutput>() where T : AbstractSignal<TInput, TOutput> => this.Context.CreateSignal<T, TInput, TOutput>();

    public ISignalReceiver<TInput, TOutput> UseSignal<T, TInput, TOutput>() where T : AbstractSignal<TInput, TOutput> => this.Context.UseSignal<T, TInput, TOutput>();

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

    protected IState<string?> UseDownload(Func<byte[]> factory, string mimeType, string fileName) =>
        this.Context.UseDownload(() => Task.FromResult(factory()), mimeType, fileName);

    protected IState<string?> UseDownload(Func<Task<byte[]>> factory, string mimeType, string fileName) =>
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

    protected IView UseBlades(Func<IView> rootBlade, string? title = null, Size? width = null) =>
        this.Context.UseBlades(rootBlade, title, width);

    protected (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) UseForm<TModel>(Func<FormBuilder<TModel>> factory) =>
        this.Context.UseForm(factory);

    protected INavigator UseNavigation() =>
        this.Context.UseNavigation();

    protected (IView? alertView, ShowAlertDelegate showAlert) UseAlert() =>
        this.Context.UseAlert();
}
