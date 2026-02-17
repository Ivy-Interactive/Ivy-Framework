using Ivy.Core;
using Ivy.Views;
﻿using Ivy.Hooks;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

public class MySignal() : AbstractSignal<int, string> { }

[App(icon: Icons.Signal, searchHints: ["communication", "events", "messaging", "broadcast", "pub-sub", "cross-component"])]
public class SignalApp : SampleBase
{
    protected override object? BuildSample()
    {
        var output = UseState<string>("");
        var signal = UseSignal<MySignal, int, string>();

        return
            Layout.Vertical()
            | new Button("Send Signal", OnClick)
            | (Layout.Horizontal()
               | new ChildView()
               | new ChildView())
            | output
            ;

        async void OnClick(Event<Button> _)
        {
            var results = await signal.Send(1);
            output.Set(string.Join(';', results));
        }
    }
}

public class ChildView : ViewBase
{
    public override object? Build()
    {
        var signal = UseSignal<MySignal, int, string>();
        var counter = UseState(0);

        UseEffect(() => signal.Receive((input) =>
        {
            counter.Set(counter.Value + input);
            return counter.Value.ToString();
        }));

        return new Card(
            Layout.Vertical(
                (Layout.Horizontal()
                    | Icons.Plus.ToButton(_ =>
                    {
                        counter.Set(counter.Value + 1);
                    })
                    | Icons.Minus.ToButton(_ =>
                    {
                        counter.Set(counter.Value - 1);
                    })
                ))
            | counter
        );
    }
}