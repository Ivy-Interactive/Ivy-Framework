using System.Collections.Generic;
using System.Reflection;
using Ivy.Apps;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views;
using Ivy.Views.Forms;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

[App()]
public class DefaultAuthApp : ViewBase
{
    public override object Build()
    {
        var auth = this.UseService<IAuthService>();
        var errorMessage = this.UseState<string?>();
        var serverArgs = this.UseService<ServerArgs>();
        var appName = serverArgs.MetaTitle.NullIfEmpty() ?? Assembly.GetEntryAssembly()?.GetName().Name.NullIfEmpty() ?? "Ivy";

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

        var flows = renderedOptions
            .SelectMany(x => new[] { x, new Separator("OR") })
            .Take(Math.Max(renderedOptions.Count * 2 - 1, 0))
            .ToArray();

        var flowsLayout = renderedOptions.Count > 0
            ? Layout.Vertical().Gap(6)
                | flows
            : null;

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
        var credentials = this.UseState(() => new LoginFormModel("", ""));
        var loading = this.UseState<bool>();
        var auth = this.UseService<IAuthService>();
        var client = this.UseService<IClientProvider>();

        var formBuilder = credentials.ToForm("Login")
            .Required(m => m.User, m => m.Password)
            .Label(m => m.User, "User")
            .Label(m => m.Password, "Password")
            .Builder(m => m.User, state => state.ToTextInput())
            .Builder(m => m.Password, state => state.ToPasswordInput());

        var (submitForm, formView, _, submitting) = formBuilder.UseForm(this.Context);

        var isBusy = loading.Value || submitting;

        async ValueTask HandleSubmit()
        {
            if (isBusy)
            {
                return;
            }

            var isValid = await submitForm(); // FormBuilder runs validation and updates field errors
            if (!isValid)
            {
                return;
            }

            await HandleLoginAsync();
        }

        async ValueTask HandleLoginAsync()
        {
            try
            {
                loading.Set(true);
                errorMessage.Set((string?)null);

                var token = await TimeoutHelper.WithTimeoutAsync(
                    ct => auth.LoginAsync(credentials.Value.User, credentials.Value.Password, ct));

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

        return Layout.Vertical().Gap(12)
               | formView
               | new Button("Login")
                   .HandleClick(HandleSubmit)
                   .Loading(isBusy)
                   .Disabled(isBusy)
                   .Scale(formBuilder._scale)
                   .Width(Size.Full());
    }
}


public class OAuthFlowView(AuthOption option, IState<string?> errorMessage) : ViewBase
{
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
