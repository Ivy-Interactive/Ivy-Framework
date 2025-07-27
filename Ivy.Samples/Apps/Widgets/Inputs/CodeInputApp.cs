using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Inputs;

[App(icon: Icons.Code, path: ["Widgets", "Inputs"])]
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

        var firstGrid = Layout.Grid().Columns(4)
               | null!
               | Text.InlineCode("Default")
               | Text.InlineCode("Disabled")
               | Text.InlineCode("Invalid")

               | Text.InlineCode("C#")
               | csharpCode.ToCodeInput().Language(Languages.Csharp)
               | csharpCode.ToCodeInput().Language(Languages.Csharp).Disabled()
               | csharpCode.ToCodeInput().Language(Languages.Csharp).Invalid("Invalid code")

               | Text.InlineCode("JSON")
               | jsonCode.ToCodeInput().Language(Languages.Json)
               | jsonCode.ToCodeInput().Language(Languages.Json).Disabled()
               | jsonCode.ToCodeInput().Language(Languages.Json).Invalid("Invalid JSON")

               | Text.InlineCode("SQL")
               | sqlCode.ToCodeInput().Language(Languages.Sql)
               | sqlCode.ToCodeInput().Language(Languages.Sql).Disabled()
               | sqlCode.ToCodeInput().Language(Languages.Sql).Invalid("Invalid SQL")

               | Text.InlineCode("HTML")
               | htmlCode.ToCodeInput().Language(Languages.Html)
               | htmlCode.ToCodeInput().Language(Languages.Html).Disabled()
               | htmlCode.ToCodeInput().Language(Languages.Html).Invalid("Invalid HTML")
            ;

        var secondGrid = Layout.Grid().Columns(4)
               | null!
               | Text.InlineCode("With Placeholder")
               | Text.InlineCode("Empty State")
               | Text.InlineCode("With Copy Button")

               | Text.InlineCode("C#")
               | csharpCode.ToCodeInput().Language(Languages.Csharp).Placeholder("Enter C# code here...")
               | UseState("").ToCodeInput().Language(Languages.Csharp).Placeholder("Enter C# code here...")
               | csharpCode.ToCodeInput().Language(Languages.Csharp).ShowCopyButton()

               | Text.InlineCode("JSON")
               | jsonCode.ToCodeInput().Language(Languages.Json).Placeholder("Enter JSON here...")
               | UseState("").ToCodeInput().Language(Languages.Json).Placeholder("Enter JSON here...")
               | jsonCode.ToCodeInput().Language(Languages.Json).ShowCopyButton()

               | Text.InlineCode("SQL")
               | sqlCode.ToCodeInput().Language(Languages.Sql).Placeholder("Enter SQL query here...")
               | UseState("").ToCodeInput().Language(Languages.Sql).Placeholder("Enter SQL query here...")
               | sqlCode.ToCodeInput().Language(Languages.Sql).ShowCopyButton()

               | Text.InlineCode("HTML")
               | htmlCode.ToCodeInput().Language(Languages.Html).Placeholder("Enter HTML here...")
               | UseState("").ToCodeInput().Language(Languages.Html).Placeholder("Enter HTML here...")
               | htmlCode.ToCodeInput().Language(Languages.Html).ShowCopyButton()
            ;

        var thirdGrid = Layout.Grid().Columns(4)
               | null!
               | Text.InlineCode("Invalid + Copy")
               | null!
               | null!

               | Text.InlineCode("C#")
               | csharpCode.ToCodeInput().Language(Languages.Csharp).Invalid("Invalid code").ShowCopyButton()
               | null!
               | null!

               | Text.InlineCode("JSON")
               | jsonCode.ToCodeInput().Language(Languages.Json).Invalid("Invalid JSON").ShowCopyButton()
               | null!
               | null!

               | Text.InlineCode("SQL")
               | sqlCode.ToCodeInput().Language(Languages.Sql).Invalid("Invalid SQL").ShowCopyButton()
               | null!
               | null!

               | Text.InlineCode("HTML")
               | htmlCode.ToCodeInput().Language(Languages.Html).Invalid("Invalid HTML").ShowCopyButton()
               | null!
               | null!
            ;

        var dataBinding = CreateStringTypeTests();

        return Layout.Vertical()
               | Text.H1("CodeInput")
               | Text.H2("Variants")
               | firstGrid
               | secondGrid
               | thirdGrid
               | Text.H2("Data Binding")
               | dataBinding
               ;
    }

    private object CreateStringTypeTests()
    {
        var stringTypes = new (string TypeName, object NonNullableState, object NullableState)[]
        {
            ("string", UseState(""), UseState((string?)null))
        };

        var gridItems = new List<object>
        {
            Text.InlineCode("Type"),
            Text.InlineCode("Non-Nullable"),
            Text.InlineCode("State"),
            Text.InlineCode("Type"),
            Text.InlineCode("Nullable"),
            Text.InlineCode("State")
        };

        foreach (var (typeName, nonNullableState, nullableState) in stringTypes)
        {
            // Non-nullable columns (first 3)
            gridItems.Add(Text.InlineCode(typeName));
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
            gridItems.Add(Text.InlineCode($"{typeName}?"));
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

        object FormatStateValue(string typeName, object? value, bool isNullable)
        {
            return value switch
            {
                null => isNullable ? Text.InlineCode("Null") : Text.InlineCode("Empty"),
                string s => s.Length == 0 ? Text.InlineCode("Empty") : Text.InlineCode($"\"{s}\""),
                _ => Text.InlineCode(value?.ToString() ?? "null")
            };
        }
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
}