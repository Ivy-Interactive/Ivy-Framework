namespace Ivy.Samples.Shared.Apps.Tests;

[App(group: ["Tests"], isVisible: false, searchHints: ["url", "validation", "security", "redirect", "link", "button", "markdown", "xss", "phishing"])]
public class UrlValidationTestApp : SampleBase
{
    private static Baton SafeBatonWithUrl(string label, string url, BatonVariant variant = BatonVariant.Link)
    {
        try
        {
            return new Baton(label, variant: variant).Url(url);
        }
        catch (ArgumentException)
        {
            return new Baton($"{label} (blocked)", variant: variant).Disabled(true);
        }
    }

    protected override object? BuildSample()
    {
        var validBatons = Layout.Vertical().Gap(8)
            | new Baton("HTTPS URL", variant: BatonVariant.Link)
                .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
            | new Baton("HTTP URL", variant: BatonVariant.Link)
                .Url("http://example.com")
            | new Baton("Relative Path", variant: BatonVariant.Link)
                .Url("/path/to/page")
            | new Baton("Relative Path with Query", variant: BatonVariant.Link)
                .Url("/search?q=test")
            | new Baton("App Protocol", variant: BatonVariant.Link)
                .Url("app://MyApp")
            | new Baton("App Protocol with Query", variant: BatonVariant.Link)
                .Url("app://MyApp?param=value")
            | new Baton("Anchor Link", variant: BatonVariant.Link)
                .Url("#section")
            | new Baton("Anchor with Colon", variant: BatonVariant.Link)
                .Url("#section:value")
            | new Baton("External URL with Path", variant: BatonVariant.Link)
                .Url("https://example.com/path/to/resource")
            | new Baton("URL with Query & Fragment", variant: BatonVariant.Link)
                .Url("https://example.com/search?q=test&sort=date#results");

        var invalidBatons = Layout.Vertical().Gap(8)
            | SafeBatonWithUrl("JavaScript Protocol", "javascript:alert('XSS')")
            | SafeBatonWithUrl("Data Protocol", "data:text/html,<script>alert('XSS')</script>")
            | SafeBatonWithUrl("VBScript Protocol", "vbscript:msgbox('XSS')")
            | SafeBatonWithUrl("File Protocol", "file:///etc/passwd")
            | SafeBatonWithUrl("Malformed URL", "https://example.com:javascript:alert('XSS')")
            | SafeBatonWithUrl("App Protocol with Fragment", "app://MyApp#fragment")
            | SafeBatonWithUrl("Relative Path with Colon", "/path:javascript:alert('XSS')");

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
                   | new Card(validBatons).Title("Valid URLs - Baton Links")
                   | new Card(invalidBatons).Title("Invalid URLs - Baton Links")

               | Layout.Grid().Columns(2).Gap(16)
                   | new Card(new Markdown(validMarkdown)).Title("Valid URLs - Markdown Links")
                   | new Card(new Markdown(invalidMarkdown)).Title("Invalid URLs - Markdown Links")
            ;
    }
}
