using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.GettingStarted.Overview;

[App(order:1, icon:Icons.Book, groupExpanded:true)]
public class _IndexApp(bool onlyBody = false) : ViewBase
{
    public _IndexApp() : this(false)
    {
    }
    public override object? Build()
    {
        return null;
    }
}
