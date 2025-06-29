using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Primitives;

[App(icon: Icons.Code, path: ["Widgets", "Primitives"])]
public class CodeApp : SampleBase
{
    protected override object? BuildSample()
    {
        var content =
            """
            function* fibonacci(): Generator<number, void, unknown> {
                let a = 0, b = 1;
                while (true) {
                    yield a;
                    [a, b] = [b, a + b];
                }
            }
            
            // Usage example:
            const fibGen = fibonacci();
            console.log(fibGen.next().value); // 0
            console.log(fibGen.next().value); // 1
            console.log(fibGen.next().value); // 1
            console.log(fibGen.next().value); // 2
            console.log(fibGen.next().value); // 3
            console.log(fibGen.next().value); // 5
            """;

        return new Code(content, "ts")
            .ShowCopyButton(true)
            .ShowLineNumbers()
            ;
    }
}