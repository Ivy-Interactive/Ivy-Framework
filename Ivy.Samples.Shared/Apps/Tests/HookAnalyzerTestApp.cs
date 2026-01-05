using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Tests;

#pragma warning disable IVYHOOK001, IVYHOOK002, IVYHOOK003, IVYHOOK004, IVYHOOK005
[App(icon: Icons.Bug, searchHints: ["analyzer", "hooks", "rules", "warnings", "test"], isVisible: false)]
public class HookAnalyzerTestApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            Text.H2("Hook Analyzer Test"),
            Text.P("This app demonstrates various hook usage violations that trigger analyzer warnings."),
            Text.P("Check the Error List or build output to see the warnings."),
            new Separator(),
            Text.P("⚠️ This file intentionally contains violations for testing purposes.")
        );
    }
}

public class ValidHookUsageView : ViewBase
{
    public override object? Build()
    {
        var state1 = UseState(0);
        var state2 = UseState("hello");
        UseEffect(() => { });

        return Text.P($"State: {state1.Value}, {state2.Value}");
    }
}

public class HookInConditionalView : ViewBase
{
    public override object? Build()
    {
        var condition = UseState(true);

        if (condition.Value)
        {
            var badState = UseState(0);
        }

        return Text.P("This will trigger IVYHOOK002 warning");
    }
}

public class HookInLoopView : ViewBase
{
    public override object? Build()
    {
        var items = new[] { 1, 2, 3 };

        foreach (var item in items)
        {
            var badState = UseState(item);
        }

        return Text.P("This will trigger IVYHOOK003 warning");
    }
}

public class HookNotAtTopView : ViewBase
{
    public override object? Build()
    {
        var x = SomeMethod();
        var badState = UseState(0);

        return Text.P("This will trigger IVYHOOK005 warning");
    }

    private int SomeMethod() => 42;
}
#pragma warning restore IVYHOOK001, IVYHOOK002, IVYHOOK003, IVYHOOK004, IVYHOOK005

