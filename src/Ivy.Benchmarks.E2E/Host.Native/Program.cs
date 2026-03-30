using Ivy;
using Ivy.Core;

// Workaround for MSBuild Top Level statement compiler bug
namespace Host.Native
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new Server(new ServerArgs { Port = 5010 });
            server.AddAppsFromAssembly(typeof(BananaApp).Assembly);
            await server.RunAsync();
        }
    }

    [App("hello")]
    public class BananaApp : ViewBase
    {
        public override object? Build()
        {
            var textState = this.UseState<string>();

            return Layout.Vertical()
                | Text.H2("Hello App")
                | textState.ToInput(placeholder: "Type fruit...")
                | Text.Markdown("You typed: " + textState.Value);
        }
    }
}
