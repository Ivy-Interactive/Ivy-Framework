using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.Configuration.Projects;

[App(order:2, icon:Icons.FolderGit, groupExpanded:true)]
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
