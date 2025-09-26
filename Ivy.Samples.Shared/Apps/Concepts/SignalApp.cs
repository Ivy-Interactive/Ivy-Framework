using Ivy.Hooks;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

public class MySignal() : AbstractSignal<int, string> { }

[App(icon: Icons.Signal)]
public class SignalApp : SampleBase
{
    protected override object? BuildSample()
    {
        var signal = Context.CreateSignal<MySignal, int, string>();
        var output = UseState<string>("");

        async void OnClick(Event<Button> _)
        {
            var results = await signal.Send(1);
            output.Set(string.Join(';', results));
        }

        return
            Layout.Vertical()
            | new Button("Send Signal", OnClick)
            | (Layout.Horizontal()
               | new ChildView()
               | new ChildView())
            | output
            ;
    }
}

public class ChildView : ViewBase
{
    public override object? Build()
    {
        var signal = Context.UseSignal<MySignal, int, string>();
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