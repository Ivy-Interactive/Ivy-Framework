using System.Reflection;
using Ivy.Chrome;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
server.AddAppsFromAssembly();
server.UseHotReload();

var version = typeof(Server).Assembly.GetName().Version!.ToString();
var chromeSettings = new ChromeSettings()
    .Header(
        Layout.Vertical().Padding(2)
            | new IvyLogo()
            | Text.Muted($"Version {version}")
    )
    .DefaultApp<Ivy.Docs.Apps.Onboarding.GettingStarted.IntroductionApp>()
    .UsePages();
server.UseChrome(() => new DefaultSidebarChrome(chromeSettings));

await server.RunAsync();