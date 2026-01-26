using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.CircleQuestionMark, path: ["Tests"])]
public class SingleAnswerQuestionApp : SampleBase
{
    protected override object? BuildSample()
    {
        var answer = UseState((bool?)null);

        return Layout.Vertical().Gap(6)
            | new SingleAnswerQuestion("Create Database Connection?", value => answer.Set(value), yesText: "Connect", noText: "Cancel");
    }
}
