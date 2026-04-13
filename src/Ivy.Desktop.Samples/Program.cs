using Ivy;
using Ivy.Desktop;

var server = new Server(new ServerArgs { FindAvailablePort = true });
server.UseHotReload();
server.AddAppsFromAssembly();
server.SetMetaTitle("Ivy Desktop Samples");

var menu = new DesktopMenu()
    .AddSubmenu("File", f => f
        .AddItem("new", "New", "CmdOrCtrl+N")
        .AddItem("open-file", "Open File...", "CmdOrCtrl+O")
        .AddItem("save-file", "Save File...", "CmdOrCtrl+S")
        .AddSeparator()
        .AddItem("exit", "Exit", "Alt+F4"))
    .AddSubmenu("Edit", e => e
        .AddItem("undo", "Undo", "CmdOrCtrl+Z")
        .AddItem("redo", "Redo", "CmdOrCtrl+Y")
        .AddSeparator()
        .AddItem("cut", "Cut", "CmdOrCtrl+X")
        .AddItem("copy", "Copy", "CmdOrCtrl+C")
        .AddItem("paste", "Paste", "CmdOrCtrl+V"))
    .AddSubmenu("View", v => v
        .AddItem("toggle-fullscreen", "Toggle Fullscreen", "F11")
        .AddItem("zoom-in", "Zoom In", "CmdOrCtrl+Plus")
        .AddItem("zoom-out", "Zoom Out", "CmdOrCtrl+Minus")
        .AddItem("zoom-reset", "Reset Zoom", "CmdOrCtrl+0")
        .AddSeparator()
        .AddCheckItem("dark-mode", "Dark Mode", isChecked: false))
    .AddSubmenu("Help", h => h
        .AddItem("about", "About Ivy Desktop"));

var window = new DesktopWindow(server)
    .Title("Ivy Desktop Samples")
    .Size(1400, 900)
    .MinSize(800, 600)
    .UseDevTools()
    .ZoomHotkeys()
    .Menu(menu)
    .OnReady(w =>
    {
        w.MenuItemClicked += (_, id) =>
        {
            switch (id)
            {
                case "exit":
                    w.Close();
                    break;
                case "toggle-fullscreen":
                    w.SetFullscreen(!w.IsFullscreen);
                    break;
                case "zoom-in":
                    w.SetZoom(1.25);
                    break;
                case "zoom-out":
                    w.SetZoom(0.8);
                    break;
                case "zoom-reset":
                    w.SetZoom(1.0);
                    break;
                case "open-file":
                    w.ShowOpenFileDialog("Open File", filters: [
                        new DesktopFileFilter("All Files", "*"),
                        new DesktopFileFilter("Text Files", "txt", "md"),
                    ]);
                    break;
                case "save-file":
                    w.ShowSaveFileDialog("Save File", filters: [
                        new DesktopFileFilter("Text File", "txt"),
                        new DesktopFileFilter("All Files", "*"),
                    ]);
                    break;
                case "about":
                    DesktopWindow.ShowNotification(
                        "Ivy Desktop Samples",
                        "Demonstrates all DesktopWindow capabilities powered by Rustino.");
                    break;
            }
        };

        w.SizeChanged += (_, e) => Console.WriteLine($"[Event] SizeChanged: {e.Width}x{e.Height}");
        w.LocationChanged += (_, e) => Console.WriteLine($"[Event] LocationChanged: {e.X},{e.Y}");
        w.FocusChanged += (_, focused) => Console.WriteLine($"[Event] FocusChanged: {focused}");
        w.WebMessageReceived += (_, msg) => Console.WriteLine($"[Event] WebMessage: {msg}");
    });

return window.Run();
