using System.Collections.Concurrent;
using Ivy.Apps;
using Ivy.Core;
using Ivy.Core.Hooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Hooks;

public static class UseWebhookExtensions
{
    // Synchronous

    public static WebhookEndpoint UseWebhook<TView>(this TView view, Func<HttpRequest, IActionResult> handler) where TView : ViewBase =>
        view.Context.UseWebhook(handler);

    public static WebhookEndpoint UseWebhook<TView>(this TView view, Action<HttpRequest> handler) where TView : ViewBase =>
        view.Context.UseWebhook(handler);

    public static WebhookEndpoint UseWebhook(this IViewContext context, Action<HttpRequest> handler) =>
        context.UseWebhook(e =>
        {
            handler(e);
            return Task.CompletedTask;
        });

    public static WebhookEndpoint UseWebhook(this IViewContext context, Func<HttpRequest, IActionResult> handler) =>
        context.UseWebhook(e => Task.FromResult(handler(e)));

    // Asynchronous

    public static WebhookEndpoint UseWebhook<TView>(this TView view, Func<HttpRequest, Task<IActionResult>> handler) where TView : ViewBase =>
        view.Context.UseWebhook(handler);

    public static WebhookEndpoint UseWebhook<TView>(this TView view, Func<HttpRequest, Task> handler) where TView : ViewBase =>
        view.Context.UseWebhook(async e =>
        {
            await handler(e);
            return new OkResult();
        });

    public static WebhookEndpoint UseWebhook(this IViewContext context, Func<HttpRequest, Task> handler) =>
        context.UseWebhook(async e =>
        {
            await handler(e);
            return new OkResult();
        });

    public static WebhookEndpoint UseWebhook(this IViewContext context, Func<HttpRequest, Task<IActionResult>> handler)
    {
        var webhookId = context.UseState(() => Guid.NewGuid().ToString(), false);
        var webhookController = context.UseService<IWebhookRegistry>();
        var args = context.UseService<AppArgs>();

        context.UseEffect(() => webhookController.Register(webhookId.Value, handler), [EffectTrigger.AfterInit()]);

        return new WebhookEndpoint($"{args.Scheme}://{args.Host}/webhook", webhookId.Value);
    }
}

public record WebhookEndpoint(string BaseUrl, string Id)
{
    public Uri GetUri(bool includeIdInPath = true) => includeIdInPath
        ? new Uri($"{BaseUrl}/{Id}")
        : new Uri(BaseUrl);
}

public interface IWebhookRegistry
{
    IDisposable Register(string id, Func<HttpRequest, Task<IActionResult>> handler);
}

public class WebhookController : Controller, IWebhookRegistry
{
    private static readonly ConcurrentDictionary<string, Func<HttpRequest, Task<IActionResult>>> Handlers = new();

    [Route("webhook/{id}")]
    [HttpGet, HttpPost]
    public Task<IActionResult> HandleWebhookWithIdInPath(string id)
    {
        if (Handlers.TryGetValue(id, out var handler))
        {
            return handler(Request);
        }
        return Task.FromResult<IActionResult>(NotFound());
    }

    [Route("webhook")]
    [HttpGet, HttpPost]
    public Task<IActionResult> HandleWebhookWithIdInStateQueryParameter([FromQuery(Name = "state")] string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Task.FromResult<IActionResult>(BadRequest("The 'state' query parameter is required."));
        }

        if (Handlers.TryGetValue(id, out var handler))
        {
            return handler(Request);
        }
        return Task.FromResult<IActionResult>(NotFound());
    }

    public IDisposable Register(string id, Func<HttpRequest, Task<IActionResult>> handler)
    {
        if (!Handlers.TryAdd(id, handler))
            throw new InvalidOperationException($"Handler already registered for id '{id}'");

        return new HandlerUnsubscriber(id);
    }

    private sealed class HandlerUnsubscriber(string id) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                Handlers.TryRemove(id, out _);
                _disposed = true;
            }
        }
    }
}
