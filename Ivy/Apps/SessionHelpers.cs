using System.Text.Json.Nodes;
using Ivy.Auth;
using Ivy.Client;
using Ivy.Core;
using Ivy.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ivy.Apps;

public static class SessionHelpers
{
    // Replace connection's widget tree with an error view, so an unauthenticated user cannot interact with the real app.
    // This is intended mainly as a safeguard against malicious clients (e.g., those which ignore messages that should trigger a page reload and/or cookie updates).
    // The error page this provides is not very user-friendly, but in practice it should very rarely appear for a legitimate user.
    public static async Task AbandonSessionAsync(
        AppSession session,
        IContentBuilder contentBuilder,
        bool resetTokenAndReload,
        bool triggerRecursiveReload,
        ILogger logger,
        string logContext = "AbandonSession")
    {
        try
        {
            var displayException = new Exception("Your session is no longer valid. Please log in again.");
            var clientProvider = session.AppServices.GetRequiredService<IClientProvider>();

            if (resetTokenAndReload)
            {
                var tokenRegistry = session.AppServices.GetRequiredService<IAuthTokenRegistry>();
                var tokenId = tokenRegistry.Register(null);
                clientProvider.SetAuthToken(tokenId, reloadPage: true, triggerRecursiveReload: triggerRecursiveReload);
            }

            session.WidgetTree = new WidgetTree(new ErrorView(displayException), contentBuilder, session.AppServices);
            await session.WidgetTree.BuildAsync();
            JsonNode widgets;
            try
            {
                session.WidgetTree.GetWidgets().Serialize();
            }
            catch (NotSupportedException)
            {
                widgets = JsonValue.Create("Error: Unable to serialize widgets due to unsupported content.");
            }
            clientProvider.Sender.Send("Refresh", new
            {
                Widgets = session.WidgetTree.GetWidgets().Serialize()
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Context}: Error sending session expired message to {ConnectionId}", logContext, session.ConnectionId);
        }
    }
}
