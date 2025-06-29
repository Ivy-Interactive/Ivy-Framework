using Ivy.Apps;
using Ivy.Client;
using Ivy.Core;
using Ivy.Helpers;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

[App()]
public class DefaultAuthApp : ViewBase
{
    public override object Build()
    {
        var auth = this.UseService<IAuthService>();

        var options = auth.GetAuthOptions();

        var renderedOptions = new List<object>();

        if (options.Any(e => e.Flow == AuthFlow.EmailPassword))
        {
            var emailOption = options.First(e => e.Flow == AuthFlow.EmailPassword);
            renderedOptions.Add(new PasswordEmailFlowView());
        }

        if (options.Any(e => e.Flow == AuthFlow.OAuth))
        {
            var oAuthOptions = options.Where(e => e.Flow == AuthFlow.OAuth).ToList();
            renderedOptions.Add(Layout.Vertical() | oAuthOptions.Select(e => new OAuthFlowView(e)));
        }

        return
            Layout.Horizontal().Align(Align.Center).Height(Size.Screen())
            | (new Card().Width(100).Title("Login")
               | (Layout.Vertical() | renderedOptions.SelectMany(x => new[] { x, new Separator("OR") }).Take(renderedOptions.Count * 2 - 1)
                   .ToArray())
               );
    }
}

public class PasswordEmailFlowView() : ViewBase
{
    public override object Build()
    {
        var user = this.UseState<string>("");
        var password = this.UseState<string>("");
        var result = this.UseState<string?>();
        var loading = this.UseState<bool>();
        var auth = this.UseService<IAuthService>();
        var client = this.UseService<IClientProvider>();

        var login = async () =>
        {
            try
            {
                loading.Set(true);
                var token = await auth.LoginAsync(user.Value, password.Value);
                if (token != null)
                {
                    client.SetJwt(token);
                }
            }
            finally
            {
                loading.Set(false);
            }
        };

        return Layout.Vertical()
         | Text.Label("User:")
         | user.ToTextInput().Disabled(loading.Value)
         | Text.Label("Password:")
         | password.ToPasswordInput().Disabled(loading.Value)
         | new Button("Login").Width(Size.Full()).HandleClick(login.HandleError(this)).Loading(loading.Value).Disabled(loading.Value)
         | result
         ;
    }
}


public class OAuthFlowView(AuthOption option) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var auth = this.UseService<IAuthService>();
        var callback = this.UseWebhook(async (request) =>
        {
            var token = await auth.HandleOAuthCallbackAsync(request);
            client.SetJwt(token);
            return new RedirectResult("/");
        });

        var login = async () =>
        {
            client.OpenUrl(await auth.GetOAuthUriAsync(option, callback));
        };

        return new Button(option.Name).Secondary().Icon(option.Icon).Width(Size.Full()).HandleClick(login.HandleError(this));
    }
}
