namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Terminal, group: ["Widgets", "Primitives"], searchHints: ["terminal", "console", "command", "cli", "shell", "bash"])]
public class TerminalApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Terminal()
            .Title("Installation")
            .AddCommand("dotnet tool install -g Ivy.Console")
            .AddOutput("You can use the following command to install Ivy globally.")
            .AddCommand("ivy init --namespace Acme.InternalProject")
            .AddOutput("Initializing project structure...")
            .ShowCopyButton(true);
    }
}
