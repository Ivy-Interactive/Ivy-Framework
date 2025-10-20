using Ivy.Apps;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

/// <summary>
/// Default authentication application that provides login UI for various auth flows.
/// </summary>
[App()]
public class DefaultAuthApp : ViewBase
{
    /// <summary>
    /// Builds the authentication UI based on available auth options.
    /// </summary>
    /// <returns>The authentication UI layout</returns>
    public override object Build()
    {
        var auth = this.UseService<IAuthService>();
        var errorMessage = this.UseState<string?>();

        var options = auth.GetAuthOptions();

        var renderedOptions = new List<object>();

        if (options.Any(e => e.Flow == AuthFlow.EmailPassword))
        {
            renderedOptions.Add(new PasswordEmailFlowView(errorMessage));
        }

        if (options.Any(e => e.Flow == AuthFlow.OAuth))
        {
            var oAuthOptions = options.Where(e => e.Flow == AuthFlow.OAuth).ToList();
            renderedOptions.Add(Layout.Vertical() | oAuthOptions.Select(e => new OAuthFlowView(e, errorMessage)));
        }

        return
            Layout.Horizontal().Align(Align.Center).Height(Size.Screen())
            | (new Card().Width(100).Title("Login")
               | (Layout.Vertical()
                  | new Spacer().Height(2)
                  | (errorMessage.Value.NullIfEmpty() != null ? new Callout(errorMessage.Value).Variant(CalloutVariant.Error) : null)
                  | renderedOptions.SelectMany(x => new[] { x, new Separator("OR") }).Take(renderedOptions.Count * 2 - 1)
                   .ToArray())
               );
    }
}

/// <summary>
/// View component for email/password authentication flow.
/// </summary>
/// <param name="errorMessage">State for displaying error messages</param>
public class PasswordEmailFlowView(IState<string?> errorMessage) : ViewBase
{
    /// <summary>
    /// Builds the email/password login form.
    /// </summary>
    /// <returns>The login form UI</returns>
    public override object Build()
    {
        var user = this.UseState<string>("");
        var password = this.UseState<string>("");
        var result = this.UseState<string?>();
        var loading = this.UseState<bool>();
        var auth = this.UseService<IAuthService>();
        var client = this.UseService<IClientProvider>();

        async ValueTask Login()
        {
            try
            {
                loading.Set(true);

                var token = await TimeoutHelper.WithTimeoutAsync(
                    ct => auth.LoginAsync(user.Value, password.Value, ct));

                if (token != null)
                {
                    client.SetAuthToken(token);
                }
                else
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

        return Layout.Vertical()
         | Text.Label("User:")
         | user.ToTextInput().Disabled(loading.Value)
         | Text.Label("Password:")
         | password.ToPasswordInput().Disabled(loading.Value)
         | new Button("Login").Width(Size.Full()).HandleClick(Login).Loading(loading.Value).Disabled(loading.Value)
         | result
         ;
    }
}


/// <summary>
/// View component for OAuth authentication flow.
/// </summary>
/// <param name="option">The OAuth authentication option</param>
/// <param name="errorMessage">State for displaying error messages</param>
public class OAuthFlowView(AuthOption option, IState<string?> errorMessage) : ViewBase
{
    /// <summary>
    /// Builds the OAuth authentication button.
    /// </summary>
    /// <returns>The OAuth login button UI</returns>
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var auth = this.UseService<IAuthService>();
        var callback = this.UseWebhook(async (request) =>
        {
            var token = await TimeoutHelper.WithTimeoutAsync(
                ct => auth.HandleOAuthCallbackAsync(request, ct));
            client.SetAuthToken(token);
            return new RedirectResult("/");
        });

        async ValueTask Login()
        {
            try
            {
                var uri = await TimeoutHelper.WithTimeoutAsync(
                    ct => auth.GetOAuthUriAsync(option, callback, ct));
                client.OpenUrl(uri);
            }
            catch (Exception e)
            {
                errorMessage.Set(e.Message);
            }
        }

        return new Button(option.Name).Secondary().Icon(option.Icon).Width(Size.Full()).HandleClick(Login);
    }
}
