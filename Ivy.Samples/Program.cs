CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
server.UseHotReload();
server.AddAppsFromAssembly();
var chromeSettings = new ChromeSettings()
    .Header(
        Layout.Vertical().Padding(2) | new IvyLogo()
    )
    .PreventTabDuplicates();
server.UseChrome(() => new DefaultSidebarChrome(chromeSettings));
server.Services.AddSingleton<SampleDbContextFactory>();
await server.RunAsync();