using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.Configuration.Setup;

[App(order:1, icon:Icons.Wrench, groupExpanded:true)]
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
