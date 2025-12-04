using Ivy.Client;
using Ivy.Apps;
using Ivy.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ivy.Auth;

namespace Ivy.Core.Helpers;

public static class SessionHelpers
{
    // Replace connection's widget tree with an error view, so an unauthenticated user cannot interact with the real app.
    // This is intended mainly as a safeguard against malicious clients (e.g., those which ignore messages that should trigger a page reload and/or cookie updates).
    // The error page this provides is not very user-friendly, but in practice it should very rarely appear for a legitimate user.
    public static async Task AbandonSessionAsync(
        AppSessionStore sessionStore,
        AppSession session,
        IContentBuilder contentBuilder,
        bool resetTokenAndReload,
        bool triggerMachineReload,
        ILogger logger,
        string logContext = "AbandonSession")
    {
        try
        {
            var displayException = new Exception("Your session is no longer valid. Please log in again.");
            var clientProvider = session.AppServices.GetRequiredService<IClientProvider>();
            var authService = session.AppServices.GetRequiredService<IAuthService>();

            if (resetTokenAndReload)
            {
                authService.GetAuthSession().AuthToken = null;
                authService.SetAuthTokenCookies(reloadPage: true, triggerMachineReload: triggerMachineReload);
            }

            session.WidgetTree = new WidgetTree(new ErrorView(displayException), contentBuilder, session.AppServices);
            await session.WidgetTree.BuildAsync();
            try
            {
                session.WidgetTree.GetWidgets().Serialize();
            }
            catch (NotSupportedException)
            {
                logger.LogError("{Context}: Unable to serialize widgets for session {ConnectionId} due to unsupported content.", logContext, session.ConnectionId);
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
