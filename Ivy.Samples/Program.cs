using Ivy.Samples.Apps.Demos;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
server.UseHotReload();
server.AddAppsFromAssembly();

var version = typeof(Server).Assembly.GetName().Version!.ToString().EatRight(".0");
server.SetMetaTitle($"Ivy Samples {version}");

var chromeSettings = new ChromeSettings()
    .Header(
        Layout.Vertical().Padding(2)
            | new IvyLogo()
            | Text.Muted($"Version {version}")
    )
    .DefaultApp<HelloApp>()
    .UseTabs(preventDuplicates: true);
server.UseChrome(() => new DefaultSidebarChrome(chromeSettings));
server.Services.AddSingleton<SampleDbContextFactory>();
await server.RunAsync();