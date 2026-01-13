using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Tests;

[App(path: ["Tests"], isVisible: false, searchHints: ["url", "validation", "security", "redirect", "link", "button", "markdown", "xss", "phishing"])]
public class UrlValidationTestApp : SampleBase
{
    private static Button SafeButtonWithUrl(string label, string url, ButtonVariant variant = ButtonVariant.Link)
    {
        try
        {
            return new Button(label, variant: variant).Url(url);
        }
        catch (ArgumentException)
        {
            return new Button($"{label} (blocked)", variant: variant).Disabled(true);
        }
    }

    protected override object? BuildSample()
    {
        var validButtons = Layout.Vertical().Gap(8)
            | new Button("HTTPS URL", variant: ButtonVariant.Link)
                .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
            | new Button("HTTP URL", variant: ButtonVariant.Link)
                .Url("http://example.com")
            | new Button("Relative Path", variant: ButtonVariant.Link)
                .Url("/path/to/page")
            | new Button("Relative Path with Query", variant: ButtonVariant.Link)
                .Url("/search?q=test")
            | new Button("App Protocol", variant: ButtonVariant.Link)
                .Url("app://MyApp")
            | new Button("App Protocol with Query", variant: ButtonVariant.Link)
                .Url("app://MyApp?param=value")
            | new Button("Anchor Link", variant: ButtonVariant.Link)
                .Url("#section")
            | new Button("Anchor with Colon", variant: ButtonVariant.Link)
                .Url("#section:value")
            | new Button("External URL with Path", variant: ButtonVariant.Link)
                .Url("https://example.com/path/to/resource")
            | new Button("URL with Query & Fragment", variant: ButtonVariant.Link)
                .Url("https://example.com/search?q=test&sort=date#results");

        var invalidButtons = Layout.Vertical().Gap(8)
            | SafeButtonWithUrl("JavaScript Protocol", "javascript:alert('XSS')")
            | SafeButtonWithUrl("Data Protocol", "data:text/html,<script>alert('XSS')</script>")
            | SafeButtonWithUrl("VBScript Protocol", "vbscript:msgbox('XSS')")
            | SafeButtonWithUrl("File Protocol", "file:///etc/passwd")
            | SafeButtonWithUrl("Malformed URL", "https://example.com:javascript:alert('XSS')")
            | SafeButtonWithUrl("App Protocol with Fragment", "app://MyApp#fragment")
            | SafeButtonWithUrl("Relative Path with Colon", "/path:javascript:alert('XSS')");

        var validMarkdown = """
- [HTTPS Link](https://github.com/Ivy-Interactive/Ivy-Framework)
- [HTTP Link](http://example.com)
- [Relative Path](/path/to/page)
- [Relative with Query](/search?q=test)
- [App Protocol](app://MyApp)
- [App Protocol with Query](app://MyApp?param=value)
- [Anchor Link](#section)
- [Anchor with Colon](#section:value)
- [External with Path](https://example.com/path/to/resource)
- [URL with Query & Fragment](https://example.com/search?q=test&sort=date#results)
""";

        var invalidMarkdown = """
- [JavaScript Protocol](javascript:alert('XSS'))
- [Data Protocol](data:text/html,<script>alert('XSS')</script>)
- [VBScript Protocol](vbscript:msgbox('XSS'))
- [File Protocol](file:///etc/passwd)
- [Malformed URL](https://example.com:javascript:alert('XSS'))
- [App Protocol with Fragment](app://MyApp#fragment)
- [Relative Path with Colon](/path:javascript:alert('XSS'))
""";

        return Layout.Vertical()
               | Text.H1("URL Validation")
               | Text.Markdown("Testing URL validation for button links and markdown links. Valid URLs work normally, invalid URLs are blocked or sanitized.")

               | Layout.Grid().Columns(2).Gap(16)
                   | new Card(validButtons).Title("Valid URLs - Button Links")
                   | new Card(invalidButtons).Title("Invalid URLs - Button Links")

               | Layout.Grid().Columns(2).Gap(16)
                   | new Card(new Markdown(validMarkdown)).Title("Valid URLs - Markdown Links")
                   | new Card(new Markdown(invalidMarkdown)).Title("Invalid URLs - Markdown Links")
            ;
    }
}
