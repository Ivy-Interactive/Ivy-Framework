using Ivy.Shared;

namespace Ivy.Samples.Apps.Concepts;

[App(icon: Icons.Check)]
public class TaskApp : SampleBase
{
    protected override async Task<object?> BuildSample()
    {
        var result = await Task.Run(async () =>
        {
            await Task.Delay(3000);
            return SampleData.GetUsers(10);
        });

        return result;
    }
}