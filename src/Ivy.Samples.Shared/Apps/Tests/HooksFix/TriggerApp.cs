namespace Ivy.Samples.Shared.Apps.Tests.HooksFix;

[App(isVisible: false)]
public class TriggerApp : ViewBase
{
    public override object? Build()
    {
        var (triggerView, showTrigger) = UseTrigger((IState<bool> isOpen, int? id) => new FooView(isOpen, id));

        var body = Layout.Vertical()
                   | DateTime.Now.Ticks
                   | new Button("Show Trigger 1", () => showTrigger(1))
                   | new Button("Show Trigger 2", () => showTrigger(2))
                   | new Button("Show Trigger Null", () => showTrigger(default))
            ;

        return Layout.Vertical()
               | body
               | triggerView;
    }

    public class FooView(IState<bool> show, int? someId) : ViewBase
    {
        public override object? Build()
        {
            if (!show.Value) return null;

            return Layout.Vertical()
                   | DateTime.Now.Ticks
                   | new Button("Close", () => show.Set(false))
                   | someId
                   | "Hello";
        }
    }
}

