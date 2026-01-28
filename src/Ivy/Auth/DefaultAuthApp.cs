using System.Reflection;
using Ivy.Apps;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views;
using Ivy.Views.Forms;
using Microsoft.AspNetCore.Mvc;
using AppContext = Ivy.Apps.AppContext;

namespace Ivy.Auth;

[App(icon: Icons.Lock, path: ["Demos"], searchHints: ["authentication", "login", "oauth", "password", "email", "auth", "security", "sign-in"])]
public class DefaultAuthApp : ViewBase
{
    public override object Build()
    {
        var auth = UseService<IAuthService>();
        var errorMessage = UseState<string?>();
        var serverArgs = UseService<ServerArgs>();
        var appName = serverArgs.MetaTitle.NullIfEmpty()?.Trim() ?? Assembly.GetEntryAssembly()?.GetName().Name.NullIfEmpty() ?? "Ivy";

        // Check if authentication is configured
        if (auth == null)
        {
            return Layout.Horizontal().Align(Align.Center).Height(Size.Screen())
                | (new Card(
                    Layout.Vertical().Gap(6).Padding(2)
                    | new IvyLogo()
                    | Text.H2($"Welcome to {appName}!")
                    | new Callout("Authentication is not configured for this application. To use authentication, configure an auth provider in your Program.cs using `server.UseAuth<T>()`.")
                        .Variant(CalloutVariant.Info)
                  )
                  .Width(Size.Units(120).Max(500))
                );
        }

        var options = auth.GetAuthOptions() ?? Array.Empty<AuthOption>();

        var renderedOptions = new List<object>();

        if (options.Length > 0 && options.Any(e => e.Flow == AuthFlow.EmailPassword))
        {
            renderedOptions.Add(new PasswordEmailFlowView(errorMessage));
        }

        if (options.Length > 0 && options.Any(e => e.Flow == AuthFlow.OAuth))
        {
            var oAuthOptions = options.Where(e => e.Flow == AuthFlow.OAuth).ToList();
            if (oAuthOptions.Count > 0)
            {
                renderedOptions.Add(Layout.Vertical() | oAuthOptions.Select(e => new OAuthFlowView(e)));
            }
        }

        object? flowsLayout = null;
        if (renderedOptions != null && renderedOptions.Count > 0)
        {
            var flows = new List<object>();
            for (int i = 0; i < renderedOptions.Count; i++)
            {
                flows.Add(renderedOptions[i]);
                if (i < renderedOptions.Count - 1)
                {
                    flows.Add(new Separator("OR"));
                }
            }
            if (flows.Count > 0)
            {
                flowsLayout = Layout.Vertical().Gap(6) | flows.ToArray();
            }
        }

        return
            Layout.Horizontal().Align(Align.Center).Height(Size.Screen())
            | (new Card(
                Layout.Vertical().Gap(6).Padding(2)
                | new IvyLogo()
                | Text.H2($"Welcome to {appName}!")
                | (errorMessage.Value.NullIfEmpty() == null
                    ? Text.Markdown("Enter user credentials for authentication.")
                    : null)
                | (errorMessage.Value.NullIfEmpty() != null ? new Callout(errorMessage.Value).Variant(CalloutVariant.Error) : null)
                | flowsLayout
              )
              .Width(Size.Units(120).Max(500))
            );
    }
}

public class PasswordEmailFlowView(IState<string?> errorMessage) : ViewBase
{
    private record LoginFormModel(string User, string Password);

    public override object Build()
    {
        var credentials = UseState(() => new LoginFormModel("", ""));
        var loading = UseState<bool>();
        var auth = UseService<IAuthService>();
        var client = UseService<IClientProvider>();

        async Task HandleLoginAsync(LoginFormModel model)
        {
            if (auth == null)
            {
                errorMessage.Set("Authentication service is not available.");
                return;
            }

            if (model == null)
            {
                errorMessage.Set("Invalid form data.");
                return;
            }

            try
            {
                loading.Set(true);
                errorMessage.Set((string?)null);

                var username = model.User ?? string.Empty;
                var password = model.Password ?? string.Empty;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    errorMessage.Set("Username and password are required.");
                    return;
                }

                await auth.LoginAsync(username, password);

                if (auth.GetCurrentToken() == null)
                {
                    errorMessage.Set("Login failed. Please check your credentials.");
                }
            }
            catch (Exception ex)
            {
                errorMessage.Set(ex.Message);
            }
            finally
            {
                loading.Set(false);
            }
        }

        var formBuilder = credentials.ToForm("Login")
            .Required(m => m.User, m => m.Password)
            .Label(m => m.User, "User")
            .Label(m => m.Password, "Password")
            .Builder(m => m.User, state => state.ToTextInput())
            .Builder(m => m.Password, state => state.ToPasswordInput())
            .HandleSubmit(HandleLoginAsync);

        var (submitForm, formView, _, submitting) = formBuilder.UseForm(this.Context);

        var isBusy = loading.Value || submitting;

        return Layout.Vertical().Gap(12)
               | formView
               | new Button("Login")
                   .HandleClick(async _ => await submitForm())
                   .Loading(isBusy)
                   .Disabled(isBusy)
                   .Scale(formBuilder._scale)
                   .Width(Size.Full());
    }
}


public class OAuthFlowView(AuthOption option) : ViewBase
{
    public override object? Build()
    {
        var args = this.UseService<AppContext>();
        var auth = this.UseService<IAuthService>();

        var callback = this.UseWebhook(async (request) =>
        {
            var token = await auth.HandleOAuthCallbackAsync(request);
            return new RedirectResult("/");
        });

        // Redirect to our OAuth login endpoint, which will in turn redirect to the provider's OAuth URL.
        // This is done to evade Safari's pop-up blocking feature.
        var oauthUriBuilder = new UriBuilder($"{args.Scheme}://{args.Host}/ivy/auth/oauth-login")
        {
            Query = $"optionId={Uri.EscapeDataString(option.Id ?? "")}&callbackId={Uri.EscapeDataString(callback.Id)}&connectionId={Uri.EscapeDataString(args.ConnectionId)}"
        };
        return new Button(option.Name).Secondary().Icon(option.Icon).Width(Size.Full()).Url(oauthUriBuilder.ToString());
    }
}
