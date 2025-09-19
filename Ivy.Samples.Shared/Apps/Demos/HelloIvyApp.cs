using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Star, title: "Hello Ivy")]
public class HelloIvyApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Center()
             | Text.H2("🚀 Hello Ivy!");
    }
}