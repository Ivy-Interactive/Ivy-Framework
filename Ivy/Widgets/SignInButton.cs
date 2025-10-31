using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Sign-in button widget using Clerk authentication for secure user login and session management.</summary>
public record SignInButton : WidgetBase<SignInButton>
{
    /// <summary>Initializes a SignInButton with optional children content.</summary>
    public SignInButton()
    {
    }

    /// <summary>Display mode for the sign-in component. "modal" shows a modal dialog, "redirect" redirects to sign-in page. Default is "modal".</summary>
    [Prop] public string? Mode { get; set; }

    /// <summary>Fallback redirect URL after sign-in when primary redirect fails. Default is null.</summary>
    [Prop] public string? FallbackRedirectUrl { get; set; }

    /// <summary>Force redirect URL after successful sign-in authentication. Default is null.</summary>
    [Prop] public string? ForceRedirectUrl { get; set; }

    /// <summary>Force redirect URL after user completes sign-up flow from sign-in button. Default is null.</summary>
    [Prop] public string? SignUpForceRedirectUrl { get; set; }

    /// <summary>Fallback redirect URL after sign-up when user creates account from sign-in button. Default is null.</summary>
    [Prop] public string? SignUpFallbackRedirectUrl { get; set; }

    /// <summary>Initial values for sign-in form fields. Default is null.</summary>
    [Prop] public object? InitialValues { get; set; }

    /// <summary>Custom tag object associated with the button for arbitrary data. Default is null.</summary>
    public object? Tag { get; set; } //not a prop!
}

/// <summary>Extension methods for SignInButton widget providing fluent API for configuring authentication behavior.</summary>
public static class SignInButtonExtensions
{
    /// <summary>Sets the display mode for the sign-in component.</summary>
    /// <param name="button">SignInButton to configure.</param>
    /// <param name="mode">Mode: "modal" for modal dialog, "redirect" for page redirect.</param>
    /// <returns>New SignInButton instance with updated mode.</returns>
    public static SignInButton Mode(this SignInButton button, string mode)
    {
        return button with { Mode = mode };
    }

    /// <summary>Sets modal display mode for the sign-in component.</summary>
    /// <param name="button">SignInButton to configure.</param>
    /// <returns>New SignInButton instance with modal mode.</returns>
    public static SignInButton Modal(this SignInButton button)
    {
        return button with { Mode = "modal" };
    }

    /// <summary>Sets redirect display mode for the sign-in component.</summary>
    /// <param name="button">SignInButton to configure.</param>
    /// <returns>New SignInButton instance with redirect mode.</returns>
    public static SignInButton Redirect(this SignInButton button)
    {
        return button with { Mode = "redirect" };
    }

    /// <summary>Sets the fallback redirect URL after sign-in.</summary>
    /// <param name="button">SignInButton to configure.</param>
    /// <param name="fallbackRedirectUrl">Fallback URL to redirect to if primary redirect fails.</param>
    /// <returns>New SignInButton instance with updated fallback redirect URL.</returns>
    public static SignInButton FallbackRedirectUrl(this SignInButton button, string fallbackRedirectUrl)
    {
        return button with { FallbackRedirectUrl = fallbackRedirectUrl };
    }

    /// <summary>Forces redirect after successful sign-in authentication.</summary>
    /// <param name="button">SignInButton to configure.</param>
    /// <param name="forceRedirectUrl">URL to force redirect to after sign-in.</param>
    /// <returns>New SignInButton instance with updated force redirect URL.</returns>
    public static SignInButton ForceRedirectUrl(this SignInButton button, string forceRedirectUrl)
    {
        return button with { ForceRedirectUrl = forceRedirectUrl };
    }

    /// <summary>Sets force redirect URL for sign-up flow initiated from sign-in button.</summary>
    /// <param name="button">SignInButton to configure.</param>
    /// <param name="signUpForceRedirectUrl">URL to redirect to after completing sign-up from sign-in.</param>
    /// <returns>New SignInButton instance with updated sign-up force redirect URL.</returns>
    public static SignInButton SignUpForceRedirectUrl(this SignInButton button, string signUpForceRedirectUrl)
    {
        return button with { SignUpForceRedirectUrl = signUpForceRedirectUrl };
    }

    /// <summary>Sets fallback redirect URL for sign-up flow initiated from sign-in button.</summary>
    /// <param name="button">SignInButton to configure.</param>
    /// <param name="signUpFallbackRedirectUrl">Fallback URL to redirect to after sign-up from sign-in.</param>
    /// <returns>New SignInButton instance with updated sign-up fallback redirect URL.</returns>
    public static SignInButton SignUpFallbackRedirectUrl(this SignInButton button, string signUpFallbackRedirectUrl)
    {
        return button with { SignUpFallbackRedirectUrl = signUpFallbackRedirectUrl };
    }

    /// <summary>Sets initial values for sign-in form fields.</summary>
    /// <param name="button">SignInButton to configure.</param>
    /// <param name="initialValues">Initial values object for form fields.</param>
    /// <returns>New SignInButton instance with updated initial values.</returns>
    public static SignInButton InitialValues(this SignInButton button, object initialValues)
    {
        return button with { InitialValues = initialValues };
    }

    /// <summary>Sets a custom tag object for the button.</summary>
    /// <param name="button">SignInButton to configure.</param>
    /// <param name="tag">Custom tag object to associate with button.</param>
    /// <returns>New SignInButton instance with updated tag.</returns>
    public static SignInButton Tag(this SignInButton button, object tag)
    {
        return button with { Tag = tag };
    }
}
