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

namespace Ivy.Auth;

[App()]
public class DefaultAuthApp : ViewBase
{
    public override object Build()
    {
        var auth = UseService<IAuthService>();
        var errorMessage = UseState<string?>();
        var serverArgs = UseService<ServerArgs>();
        var formSettings = UseService<AuthFormSettings>();

        var title = formSettings.Title
            ?? (serverArgs.MetaTitle.NullIfEmpty() != null
                ? $"Welcome to {serverArgs.MetaTitle}!"
                : "Welcome");

        var subtitle = formSettings.Subtitle ?? "Enter user credentials for authentication.";
        var logo = formSettings.ShowLogo ? (formSettings.Logo ?? new IvyLogo()) : null;
        var cardWidth = formSettings.CardWidth ?? Size.Units(120).Max(500);
        var cardPadding = formSettings.CardPadding ?? 2;
        var gap = formSettings.Gap ?? 6;

        var options = auth.GetAuthOptions();

        var renderedOptions = new List<object>();

        if (options.Any(e => e.Flow == AuthFlow.EmailPassword))
        {
            renderedOptions.Add(new PasswordEmailFlowView(errorMessage, formSettings));
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
            ? Layout.Vertical().Gap(gap)
                | flows
            : null;

        var cardContent = Layout.Vertical().Gap(gap).Padding(cardPadding)
            | logo
            | Text.H2(title)
            | (errorMessage.Value.NullIfEmpty() == null ? Text.Markdown(subtitle) : null)
            | (errorMessage.Value.NullIfEmpty() != null ? new Callout(errorMessage.Value).Variant(CalloutVariant.Error) : null)
            | flowsLayout
            | formSettings.Footer;

        var card = new Card(cardContent).Width(cardWidth);
        if (formSettings.CardHeight != null)
        {
            card = card.Height(formSettings.CardHeight);
        }

        object pageContent = Layout.Horizontal().Align(Align.Center).Height(Size.Screen()) | card;

        return pageContent;
    }
}

public class PasswordEmailFlowView(IState<string?> errorMessage, AuthFormSettings formSettings) : ViewBase
{
    private record LoginFormModel(string User, string Password);

    public override object Build()
    {
        var credentials = this.UseState(() => new LoginFormModel("", ""));
        var loading = this.UseState<bool>();
        var auth = this.UseService<IAuthService>();
        var client = this.UseService<IClientProvider>();

        var userLabel = formSettings.UserLabel ?? "User";
        var passwordLabel = formSettings.PasswordLabel ?? "Password";
        var buttonText = formSettings.ButtonText ?? "Login";

        var formBuilder = credentials.ToForm(buttonText)
            .Required(m => m.User, m => m.Password)
            .Label(m => m.User, userLabel)
            .Label(m => m.Password, passwordLabel)
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

            var isValid = await submitForm();
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

                await auth.LoginAsync(credentials.Value.User, credentials.Value.Password);

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

        return Layout.Vertical().Gap(12)
               | formView
               | new Button(buttonText)
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
            var authSession = auth.GetAuthSession();
            var token = await auth.HandleOAuthCallbackAsync(request);
            return new RedirectResult("/");
        });

        async ValueTask Login()
        {
            try
            {
                var authSession = auth.GetAuthSession();
                var uri = await auth.GetOAuthUriAsync(option, callback);
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
