using System.Reflection;
using Ivy.Core.Auth;

// ReSharper disable once CheckNamespace
namespace Ivy;

[App()]
public class DefaultAuthApp : ViewBase
{
    public override object Build()
    {
        var auth = UseService<IAuthService>();
        var errorMessage = UseState<string?>();
        var serverArgs = UseService<ServerArgs>();
        var appName = serverArgs.Metadata.Title.NullIfEmpty()?.Trim() ?? Assembly.GetEntryAssembly()?.GetName().Name.NullIfEmpty() ?? "Ivy";

        var options = auth.GetAuthOptions();

        var renderedOptions = new List<object>();

        if (options.Any(e => e.Flow == AuthFlow.EmailPassword))
        {
            renderedOptions.Add(new PasswordEmailFlowView(errorMessage));
        }

        if (options.Any(e => e.Flow == AuthFlow.OAuth))
        {
            var oAuthOptions = options.Where(e => e.Flow == AuthFlow.OAuth).ToList();
            renderedOptions.Add(Layout.Vertical() | oAuthOptions.Select(e => new OAuthFlowView(e)));
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
            Layout.Horizontal().AlignContent(Align.Center).Height(Size.Screen())
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
            try
            {
                loading.Set(true);
                errorMessage.Set((string?)null);

                await auth.LoginAsync(model.User, model.Password);

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
            .SubmitStrategy(FormSubmitStrategy.OnSubmit)
            .SubmitTitle("Login")
            .OnSubmit(async model => await HandleLoginAsync(model));

        var (submitForm, formView, _, submitting) = formBuilder.UseForm(this.Context);

        var isBusy = loading.Value || submitting;

        async ValueTask HandleSubmit()
        {
            if (isBusy)
            {
                return;
            }

            var isValid = await submitForm(); // FormBuilder runs validation and updates field errors and calls OnSubmit
            if (!isValid)
            {
                return;
            }
        }

        return Layout.Vertical().Gap(12)
               | formView
               | new Button("Login")
                   .OnClick(HandleSubmit)
                   .Loading(isBusy)
                   .Disabled(isBusy)
                   .Density(formBuilder._density)
                   .Width(Size.Full());
    }
}


public class OAuthFlowView(AuthOption option) : ViewBase
{
    public override object? Build()
    {
        var args = this.UseService<AppContext>();
        var registry = this.UseService<IOAuthCallbackRegistry>();
        var auth = this.UseService<IAuthProvider>();

        var state = this.UseState(() => registry.RegisterPending(args.ConnectionId, option.Id ?? ""));

        var oauthUriBuilder = new UriBuilder($"{args.BaseUrl.TrimEnd('/')}/ivy/auth/oauth-login")
        {
            Query = $"optionId={Uri.EscapeDataString(option.Id ?? "")}&callbackId={Uri.EscapeDataString(state.Value)}&connectionId={Uri.EscapeDataString(args.ConnectionId)}"
        };
        return new Button(option.Name)
            .Secondary()
            .Icon(option.Icon)
            .Width(Size.Full())
            .Url(oauthUriBuilder.ToString())
            .Target(LinkTarget.Self)
            .OpenInNewTab(auth.OpenOAuthLoginInNewTab);
    }
}
