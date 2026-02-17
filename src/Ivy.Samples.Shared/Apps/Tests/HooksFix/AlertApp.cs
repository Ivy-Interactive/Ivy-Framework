using Ivy.Views;
﻿using Ivy.Hooks;
using Ivy.Views.Alerts;

namespace Ivy.Samples.Shared.Apps.Tests.HooksFix;

[App(isVisible: false)]
public class AlertApp : ViewBase
{
    public override object? Build()
    {
        var (alertView, showAlert) = UseAlert();

        var body = Layout.Vertical()
                   | DateTime.Now.Ticks
                   | new Button("Show Alert", () => showAlert("Hello", (x) =>
                   {

                   }))
                   | new Button("WithConfirm").WithConfirm("Are you sure?")
                   | new Button("WithPrompt").WithPrompt<int>((result) =>
                   {

                   }, defaultValue: 42)
                   | new Button("WithSheet").WithSheet(() => new FooView(99), "", "")
            ;

        return Layout.Vertical()
               | body
               | alertView;
    }

    public class FooView(int someId) : ViewBase
    {
        public override object? Build()
        {
            return Layout.Vertical()
                   | DateTime.Now.Ticks
                   | someId;
        }
    }
}

