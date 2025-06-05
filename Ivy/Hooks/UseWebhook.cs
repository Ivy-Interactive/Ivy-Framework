using System.Collections.Concurrent;
using Ivy.Apps;
using Ivy.Core;
using Ivy.Core.Hooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Hooks;

public static class UseWebhookExtensions
{
    public static Uri UseWebhook<TView>(this TView view, Func<HttpRequest, IActionResult> handler) where TView : ViewBase =>
        view.Context.UseWebhook(handler);
    
    public static Uri UseWebhook<TView>(this TView view, Action<HttpRequest> handler) where TView : ViewBase =>
        view.Context.UseWebhook(e =>
        {
            handler(e);
            return new OkResult();
        });

    public static Uri UseWebhook(this IViewContext context, Action<HttpRequest> handler) =>
        context.UseWebhook(e =>
        {
            handler(e);
            return new OkResult();
        });

    public static Uri UseWebhook(this IViewContext context, Func<HttpRequest, IActionResult> handler)
    {
        var webhookId = context.UseState(() => Guid.NewGuid().ToString(), false);
        var webhookController = context.UseService<IWebhookRegistry>();
        var args = context.UseService<AppArgs>();
        
        context.UseEffect(() => webhookController.Register(webhookId.Value, handler), [EffectTrigger.AfterInit()]);
        
        return new Uri($"{args.Scheme}://{args.Host}/webhook/{webhookId.Value}");
    }
}

public interface IWebhookRegistry
{
    IDisposable Register(string id, Func<HttpRequest, IActionResult> handler);
}

public class WebhookController : Controller, IWebhookRegistry
{
    private static readonly ConcurrentDictionary<string, Func<HttpRequest, IActionResult>> Handlers = new();

    [Route("webhook/{id}")]
    [HttpGet, HttpPost]
    public IActionResult HandleWebhook(string id)
    {
        if (Handlers.TryGetValue(id, out var handler))
        {
            return handler(Request);
        }
        return NotFound();
    }

    public IDisposable Register(string id, Func<HttpRequest, IActionResult> handler)
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
