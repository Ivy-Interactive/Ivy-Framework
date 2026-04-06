using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.Apps;

[App(order:4, icon:Icons.LayoutDashboard)]
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
