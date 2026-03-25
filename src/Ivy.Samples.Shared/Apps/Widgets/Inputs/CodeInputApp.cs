
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Code, group: ["Widgets", "Inputs"], searchHints: ["editor", "syntax", "programming", "code", "highlighting", "monaco"])]
public class CodeInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var csharpCode = UseState(
            """
            public class Program
            {
                public static void Main(string[] args)
                {
                    Console.WriteLine("Hello, World!");
                }
            }
            """);

        var jsonCode = UseState(
            """
            {
              "name": "John Doe",
              "age": 30,
              "email": "john@example.com"
            }
            """);

        var sqlCode = UseState(
            """
            SELECT u.name, u.email, p.title
            FROM users u
            JOIN posts p ON u.id = p.user_id
            WHERE u.active = true
            ORDER BY p.created_at DESC;
            """);

        var htmlCode = UseState(
            """
            <!DOCTYPE html>
            <html>
            <head>
                <title>My Page</title>
            </head>
            <body>
                <h1>Hello World</h1>
            </body>
            </html>
            """);

        var yamlCode = UseState(
            """
            name: my-app
            version: 1.0.0
            services:
              web:
                image: nginx:latest
                ports:
                  - "80:80"
              database:
                image: postgres:15
                environment:
                  POSTGRES_USER: admin
                  POSTGRES_PASSWORD: secret
            """);

        var emptyCsharpState = UseState("");
        var emptyJsonState = UseState("");
        var emptySqlState = UseState("");
        var emptyHtmlState = UseState("");
        var emptyYamlState = UseState("");

        var onBlurState = UseState("");
        var onBlurLabel = UseState("");
        var onFocusState = UseState("");
        var onFocusLabel = UseState("");

        var cardCode = UseState(
            """
            public class Example
            {
                public void DoWork()
                {
                    Console.WriteLine("Code inside Card");
                }
            }
            """);

        var firstGrid = Layout.Grid().Columns(4)
               | null!
               | Text.Monospaced("Default")
               | Text.Monospaced("Disabled")
               | Text.Monospaced("Invalid")

               | Text.Monospaced("C#")
               | csharpCode.ToCodeInput().Language(Languages.Csharp)
               | csharpCode.ToCodeInput().Language(Languages.Csharp).Disabled()
               | csharpCode.ToCodeInput().Language(Languages.Csharp).Invalid("Invalid code")

               | Text.Monospaced("JSON")
               | jsonCode.ToCodeInput().Language(Languages.Json)
               | jsonCode.ToCodeInput().Language(Languages.Json).Disabled()
               | jsonCode.ToCodeInput().Language(Languages.Json).Invalid("Invalid JSON")

               | Text.Monospaced("SQL")
               | sqlCode.ToCodeInput().Language(Languages.Sql)
               | sqlCode.ToCodeInput().Language(Languages.Sql).Disabled()
               | sqlCode.ToCodeInput().Language(Languages.Sql).Invalid("Invalid SQL")

               | Text.Monospaced("HTML")
               | htmlCode.ToCodeInput().Language(Languages.Html)
               | htmlCode.ToCodeInput().Language(Languages.Html).Disabled()
               | htmlCode.ToCodeInput().Language(Languages.Html).Invalid("Invalid HTML")

               | Text.Monospaced("YAML")
               | yamlCode.ToCodeInput().Language(Languages.Yaml)
               | yamlCode.ToCodeInput().Language(Languages.Yaml).Disabled()
               | yamlCode.ToCodeInput().Language(Languages.Yaml).Invalid("Invalid YAML")
            ;

        var secondGrid = Layout.Grid().Columns(4)
               | null!
               | Text.Monospaced("With Placeholder")
               | Text.Monospaced("Empty State")
               | Text.Monospaced("With Copy Button")

               | Text.Monospaced("C#")
               | csharpCode.ToCodeInput().Language(Languages.Csharp).Placeholder("Enter C# code here...")
               | emptyCsharpState.ToCodeInput().Language(Languages.Csharp).Placeholder("Enter C# code here...")
               | csharpCode.ToCodeInput().Language(Languages.Csharp).ShowCopyButton()

               | Text.Monospaced("JSON")
               | jsonCode.ToCodeInput().Language(Languages.Json).Placeholder("Enter JSON here...")
               | emptyJsonState.ToCodeInput().Language(Languages.Json).Placeholder("Enter JSON here...")
               | jsonCode.ToCodeInput().Language(Languages.Json).ShowCopyButton()

               | Text.Monospaced("SQL")
               | sqlCode.ToCodeInput().Language(Languages.Sql).Placeholder("Enter SQL query here...")
               | emptySqlState.ToCodeInput().Language(Languages.Sql).Placeholder("Enter SQL query here...")
               | sqlCode.ToCodeInput().Language(Languages.Sql).ShowCopyButton()

               | Text.Monospaced("HTML")
               | htmlCode.ToCodeInput().Language(Languages.Html).Placeholder("Enter HTML here...")
               | emptyHtmlState.ToCodeInput().Language(Languages.Html).Placeholder("Enter HTML here...")
               | htmlCode.ToCodeInput().Language(Languages.Html).ShowCopyButton()

               | Text.Monospaced("YAML")
               | yamlCode.ToCodeInput().Language(Languages.Yaml).Placeholder("Enter YAML here...")
               | emptyYamlState.ToCodeInput().Language(Languages.Yaml).Placeholder("Enter YAML here...")
               | yamlCode.ToCodeInput().Language(Languages.Yaml).ShowCopyButton()
            ;

        var thirdGrid = Layout.Grid().Columns(4)
               | null!
               | Text.Monospaced("Invalid + Copy")
               | null!
               | null!

               | Text.Monospaced("C#")
               | csharpCode.ToCodeInput().Language(Languages.Csharp).Invalid("Invalid code").ShowCopyButton()
               | null!
               | null!

               | Text.Monospaced("JSON")
               | jsonCode.ToCodeInput().Language(Languages.Json).Invalid("Invalid JSON").ShowCopyButton()
               | null!
               | null!

               | Text.Monospaced("SQL")
               | sqlCode.ToCodeInput().Language(Languages.Sql).Invalid("Invalid SQL").ShowCopyButton()
               | null!
               | null!

               | Text.Monospaced("HTML")
               | htmlCode.ToCodeInput().Language(Languages.Html).Invalid("Invalid HTML").ShowCopyButton()
               | null!
               | null!

               | Text.Monospaced("YAML")
               | yamlCode.ToCodeInput().Language(Languages.Yaml).Invalid("Invalid YAML").ShowCopyButton()
               | null!
               | null!
            ;

        var sizeGrid = Layout.Grid().Columns(4)
               | null!
               | Text.Monospaced("Small")
               | Text.Monospaced("Medium")
               | Text.Monospaced("Large")

               | Text.Monospaced("C#")
               | csharpCode.ToCodeInput().Language(Languages.Csharp).Small()
               | csharpCode.ToCodeInput().Language(Languages.Csharp)
               | csharpCode.ToCodeInput().Language(Languages.Csharp).Large()

               | Text.Monospaced("JSON")
               | jsonCode.ToCodeInput().Language(Languages.Json).Small()
               | jsonCode.ToCodeInput().Language(Languages.Json)
               | jsonCode.ToCodeInput().Language(Languages.Json).Large()

               | Text.Monospaced("SQL")
               | sqlCode.ToCodeInput().Language(Languages.Sql).Small()
               | sqlCode.ToCodeInput().Language(Languages.Sql)
               | sqlCode.ToCodeInput().Language(Languages.Sql).Large()

               | Text.Monospaced("HTML")
               | htmlCode.ToCodeInput().Language(Languages.Html).Small()
               | htmlCode.ToCodeInput().Language(Languages.Html)
               | htmlCode.ToCodeInput().Language(Languages.Html).Large()

               | Text.Monospaced("YAML")
               | yamlCode.ToCodeInput().Language(Languages.Yaml).Small()
               | yamlCode.ToCodeInput().Language(Languages.Yaml)
               | yamlCode.ToCodeInput().Language(Languages.Yaml).Large()
            ;

        var dataBinding = new CodeInputDataBindings();



        // Links with copy functionality using one Code block inside Card
        var socialMediaLinksContent = """
            discord.gg: https://discord.gg/62DYrqEX
            github.com: https://github.com/Ivy-Interactive/Ivy-Framework
            linkedin.com: https://www.linkedin.com/company/ivy-interactive/posts/?feedView=all
            discord.gg: https://discord.gg/rVcUVZPG
            youtube.com: https://www.youtube.com/@IvyInteractive
            github.com: https://github.com/Ivy-Interactive/Ivy-Framework
            x.com: https://x.com/ivy_interactive
            """;

        var socialMediaLinks = new Card(
            new CodeBlock(socialMediaLinksContent, Languages.Text).ShowCopyButton().ShowBorder(false)
        );

        return Layout.Vertical()
               | Text.H1("Code Input")
               | Text.H2("Sizes")
               | sizeGrid
               | Text.H2("Variants")
               | firstGrid
               | secondGrid
               | thirdGrid
               | Text.H2("Data Binding")
               | dataBinding
               | Text.H2("CodeInput in Card")
               | new Card(
                   cardCode.ToCodeInput().Language(Languages.Csharp).ShowCopyButton().Height(Size.Auto())
               ).Title("Code Example").Description("Testing copy button visibility with card background")
               | socialMediaLinks
               | Text.H2("Events")
               | (Layout.Vertical()
                   | new Card(
                       Layout.Vertical().Gap(2)
                           | Text.P("The blur event fires when the code input loses focus.").Small()
                           | onBlurState.ToCodeInput().OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
                           | (onBlurLabel.Value != ""
                               ? Callout.Success(onBlurLabel.Value)
                               : Callout.Info("Interact then click away to see blur events"))
                   ).Title("OnBlur Handler")
                   | new Card(
                       Layout.Vertical().Gap(2)
                           | Text.P("The focus event fires when you click on or tab into the code input.").Small()
                           | onFocusState.ToCodeInput().OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
                           | (onFocusLabel.Value != ""
                               ? Callout.Success(onFocusLabel.Value)
                               : Callout.Info("Click or tab into the input to see focus events"))
                   ).Title("OnFocus Handler")
               )
               ;
    }

    // Helper methods moved to CodeInputDataBindings class
}

public class CodeInputDataBindings : ViewBase
{
    public override object Build()
    {
        var stringState = UseState("");
        var stringNullState = UseState((string?)null);

        var stringTypes = new (string TypeName, object NonNullableState, object NullableState)[]
        {
            ("string", stringState, stringNullState)
        };

        var gridItems = new List<object>
        {
            Text.Monospaced("Type"),
            Text.Monospaced("Non-Nullable"),
            Text.Monospaced("State"),
            Text.Monospaced("Type"),
            Text.Monospaced("Nullable"),
            Text.Monospaced("State")
        };

        foreach (var (typeName, nonNullableState, nullableState) in stringTypes)
        {
            // Non-nullable columns (first 3)
            gridItems.Add(Text.Monospaced(typeName));
            gridItems.Add(CreateCodeInputVariants(nonNullableState));

            var nonNullableAnyState = nonNullableState as IAnyState;
            object? nonNullableValue = null;
            if (nonNullableAnyState != null)
            {
                var prop = nonNullableAnyState.GetType().GetProperty("Value");
                nonNullableValue = prop?.GetValue(nonNullableAnyState);
            }
            gridItems.Add(FormatStateValue(typeName, nonNullableValue, false));

            // Nullable columns (next 3)
            gridItems.Add(Text.Monospaced($"{typeName}?"));
            gridItems.Add(CreateCodeInputVariants(nullableState));

            var anyState = nullableState as IAnyState;
            object? value = null;
            if (anyState != null)
            {
                var prop = anyState.GetType().GetProperty("Value");
                value = prop?.GetValue(anyState);
            }
            gridItems.Add(FormatStateValue(typeName, value, true));
        }

        return Layout.Grid().Columns(6) | gridItems.ToArray();
    }

    private static object CreateCodeInputVariants(object state)
    {
        if (state is not IAnyState anyState)
            return Text.Block("Not an IAnyState");

        var stateType = anyState.GetStateType();
        var isNullable = stateType.IsNullableType();

        if (isNullable)
        {
            // For nullable states, show with placeholder
            return anyState.ToCodeInput().Placeholder("Enter code here...");
        }

        // For non-nullable states, show all variants
        return Layout.Vertical()
               | anyState.ToCodeInput()
               | anyState.ToCodeInput().Language(Languages.Csharp)
               | anyState.ToCodeInput().Language(Languages.Csharp).ShowCopyButton();
    }

    private object FormatStateValue(string typeName, object? value, bool isNullable)
    {
        return value switch
        {
            null => isNullable ? Text.Monospaced("Null") : Text.Monospaced("Empty"),
            string s => s.Length == 0 ? Text.Monospaced("Empty") : Text.Monospaced($"\"{s}\""),
            _ => Text.Monospaced(value?.ToString() ?? "null")
        };
    }
}
