using Ivy.Desktop;

namespace Ivy.Tendril.Desktop;

public class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var server = TendrilServer.Create(args);

        var window = new DesktopWindow(server)
            .Title("Ivy Tendril — Multi-host AI Tool")
            .Size(1400, 900);

        return window.Run();
    }
}
