using Ivy.Shared;

namespace Ivy.Samples.Apps.Concepts;

[App(icon: Icons.Paintbrush)]
public class DefaultContentBuilderApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical(
            null!,
            "HelloWorld",
            123_456.78,
            false,
            true,
            DateTime.Now,
            new int[] { 1, 2, 3, 4 },
            new List<int> { 1, 2, 3, 4 },
            new string[] { "a", "b", "c" },
            new List<string> { "a", "b", "c" }
        );
    }
}