using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Code, path: ["Widgets", "Primitives"])]
public class CodeApp : SampleBase
{
    protected override object? BuildSample()
    {
        var variants = CreateLanguageVariants();
        var options = CreateOptionsVariants();

        return Layout.Vertical()
               | Text.H1("Code")
               | Text.H2("Language Variants")
               | variants
               | Text.H2("Options")
               | options
               ;
    }

    private object CreateLanguageVariants()
    {
        var sampleCode = new Dictionary<Languages, string>
        {
            [Languages.Csharp] = """
                public class Fibonacci
                {
                    public static IEnumerable<int> Generate()
                    {
                        int a = 0, b = 1;
                        while (true)
                        {
                            yield return a;
                            (a, b) = (b, a + b);
                        }
                    }
                }
                """,

            [Languages.Javascript] = """
                function* fibonacci() {
                    let a = 0, b = 1;
                    while (true) {
                        yield a;
                        [a, b] = [b, a + b];
                    }
                }
                
                // Usage
                const fibGen = fibonacci();
                console.log(fibGen.next().value); // 0
                """,

            [Languages.Typescript] = """
                function* fibonacci(): Generator<number, void, unknown> {
                    let a = 0, b = 1;
                    while (true) {
                        yield a;
                        [a, b] = [b, a + b];
                    }
                }
                
                // Usage
                const fibGen = fibonacci();
                console.log(fibGen.next().value); // 0
                """,

            [Languages.Python] = """
                def fibonacci():
                    a, b = 0, 1
                    while True:
                        yield a
                        a, b = b, a + b
                
                # Usage
                fib_gen = fibonacci()
                print(next(fib_gen))  # 0
                print(next(fib_gen))  # 1
                """,

            [Languages.Sql] = """
                WITH RECURSIVE fibonacci AS (
                    SELECT 0 as n, 0 as fib, 1 as next_fib
                    UNION ALL
                    SELECT n + 1, next_fib, fib + next_fib
                    FROM fibonacci
                    WHERE n < 10
                )
                SELECT n, fib FROM fibonacci;
                """,

            [Languages.Html] = """
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <title>Fibonacci Calculator</title>
                </head>
                <body>
                    <h1>Fibonacci Sequence</h1>
                    <div id="result"></div>
                    <script src="fibonacci.js"></script>
                </body>
                </html>
                """,

            [Languages.Css] = """
                .fibonacci-container {
                    display: flex;
                    flex-direction: column;
                    gap: 1rem;
                    padding: 2rem;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                }
                
                .fibonacci-item {
                    background: rgba(255, 255, 255, 0.1);
                    border-radius: 8px;
                    padding: 1rem;
                    color: white;
                    font-family: 'Courier New', monospace;
                }
                """,

            [Languages.Json] = """
                {
                  "fibonacci": {
                    "sequence": [0, 1, 1, 2, 3, 5, 8, 13, 21, 34],
                    "metadata": {
                      "description": "First 10 Fibonacci numbers",
                      "generated": "2024-01-15T10:30:00Z",
                      "algorithm": "recursive"
                    }
                  }
                }
                """,

            [Languages.Dbml] = """
                // Database schema for Fibonacci application
                Project fibonacci_app {
                    database_type: 'postgresql'
                    Note: 'Application for calculating Fibonacci sequences'
                }
                
                Table users {
                    id uuid [primary key]
                    name varchar [not null]
                    email varchar [unique, not null]
                    created_at timestamp [default: `now()`]
                }
                
                Table fibonacci_results {
                    id int [primary key, increment]
                    user_id uuid [ref: > users.id]
                    input_number int [not null]
                    result bigint [not null]
                    calculated_at timestamp [default: `now()`]
                }
                """
        };

        var cards = new List<object>();
        foreach (var (language, code) in sampleCode)
        {
            cards.Add(
                Layout.Vertical()
                    | Text.H3(language.ToString())
                    | new Code(code, language)
                        .ShowCopyButton(true)
                        .Height(Size.Units(60))
            );
        }

        // Arrange cards in a grid with 2 columns (adjust as needed)
        return Layout.Grid().Columns(2) | cards.ToArray();
    }

    private object CreateOptionsVariants()
    {
        var sampleCode = """
            public class Example
            {
                public static void Main()
                {
                    Console.WriteLine("Hello, World!");
                    var result = Calculate(42);
                    Console.WriteLine($"Result: {result}");
                }
                
                private static int Calculate(int input)
                {
                    return input * 2 + 1;
                }
            }
            """;

        var optionBlocks = new object[]
        {
            Layout.Vertical()
                | Text.InlineCode("Default")
                | new Code(sampleCode, Languages.Csharp),
            Layout.Vertical()
                | Text.InlineCode("With Line Numbers")
                | new Code(sampleCode, Languages.Csharp).ShowLineNumbers(true),
            Layout.Vertical()
                | Text.InlineCode("No Copy Button")
                | new Code(sampleCode, Languages.Csharp).ShowCopyButton(false),
            Layout.Vertical()
                | Text.InlineCode("No Border")
                | new Code(sampleCode, Languages.Csharp).ShowBorder(false)
        };

        var variants = Layout.Grid().Columns(2) | optionBlocks;
        return variants;
    }
}