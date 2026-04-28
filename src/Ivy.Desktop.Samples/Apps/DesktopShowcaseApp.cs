using Ivy;
using Ivy.Desktop;

namespace Ivy.Desktop.Samples.Apps;

[App(icon: Icons.Monitor, title: "Desktop Showcase", description: "Demonstrates all DesktopWindow capabilities")]
public class DesktopShowcaseApp : ViewBase
{
    public override object? Build()
    {
        var window = UseService<DesktopWindow>();
        var log = UseState("");
        var zoomLevel = UseState(1.0);
        var badgeCount = UseState(0);
        var textA = UseState("");
        var textB = UseState("");

        void Log(string message) => log.Set(s =>
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            return string.IsNullOrEmpty(s) ? line : $"{line}\n{s}";
        });

        return Layout.Vertical().Gap(4).Padding(4)
               | Text.H1("Ivy Desktop Showcase")
               | Text.Markdown("This app demonstrates all `DesktopWindow` capabilities. " +
                               "Each section exercises a different Rustino feature through the Ivy.Desktop API.")

               // ── Window State ──────────────────────────────────
               | new Separator()
               | Text.H2("Window State")
               | Text.Markdown("Control the native window state — minimize, maximize, restore, and fullscreen.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Minimize", () => { window.Minimize(); Log("Window minimized"); })
                  | new Button("Maximize", () => { window.Maximize(); Log("Window maximized"); })
                  | new Button("Restore", () => { window.Restore(); Log("Window restored"); })
                  | new Button("Toggle Fullscreen", () =>
                  {
                      var fs = !window.IsFullscreen;
                      window.SetFullscreen(fs);
                      Log($"Fullscreen: {fs}");
                  })
                  | new Button("Focus", () => { window.Focus(); Log("Window focused"); })
               )

               // ── Window Info ───────────────────────────────────
               | new Separator()
               | Text.H2("Window Info")
               | Text.Markdown("Query the current window position, size, and state flags.")
               | new Button("Get Window Info", () =>
               {
                   var pos = window.GetPosition();
                   var size = window.GetSize();
                   Log($"Position: ({pos.X}, {pos.Y})  Size: {size.Width}x{size.Height}  " +
                       $"Minimized={window.IsMinimized}  Maximized={window.IsMaximized}  Fullscreen={window.IsFullscreen}");
               })

               // ── Dialogs ──────────────────────────────────────
               | new Separator()
               | Text.H2("Native Dialogs")
               | Text.Markdown("Open native file and folder pickers backed by the OS dialog APIs.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Open File...", () =>
                  {
                      var files = window.ShowOpenFileDialog("Open File", filters:
                      [
                          new DesktopFileFilter("All Files", "*"),
                          new DesktopFileFilter("Text Files", "txt", "md", "csv"),
                          new DesktopFileFilter("Images", "png", "jpg", "gif", "bmp"),
                      ]);
                      Log(files is { Length: > 0 }
                          ? $"Opened: {string.Join(", ", files)}"
                          : "Open dialog cancelled");
                  })
                  | new Button("Save File...", () =>
                  {
                      var file = window.ShowSaveFileDialog("Save File", filters:
                      [
                          new DesktopFileFilter("Text File", "txt"),
                          new DesktopFileFilter("JSON File", "json"),
                          new DesktopFileFilter("All Files", "*"),
                      ]);
                      Log(file != null ? $"Save to: {file}" : "Save dialog cancelled");
                  })
                  | new Button("Select Folder...", () =>
                  {
                      var folders = window.ShowSelectFolderDialog("Select Folder");
                      Log(folders is { Length: > 0 }
                          ? $"Selected: {string.Join(", ", folders)}"
                          : "Folder dialog cancelled");
                  })
               )

               // ── Notifications ─────────────────────────────────
               | new Separator()
               | Text.H2("Notifications")
               | Text.Markdown("Send native OS toast notifications. The `appId` parameter controls the sender name on Windows.")
               | new Button("Show Notification", () =>
               {
                   var sent = DesktopWindow.ShowNotification(
                       "Ivy Desktop",
                       $"Hello from Ivy Desktop! Time: {DateTime.Now:T}",
                       appId: "Ivy Desktop Samples");
                   Log(sent ? "Notification sent" : "Notification failed");
               })

               // ── Monitors ─────────────────────────────────────
               | new Separator()
               | Text.H2("Monitors")
               | Text.Markdown("Enumerate all connected displays and identify the current one.")
               | new Button("Detect Monitors", () =>
               {
                   var monitors = window.GetMonitors();
                   foreach (var m in monitors)
                       Log($"Monitor: {m.Name} — {m.Width}x{m.Height} at ({m.X},{m.Y}), " +
                           $"scale={m.ScaleFactor:F2}x{(m.IsPrimary ? " [primary]" : "")}");
                   var current = window.GetCurrentMonitor();
                   if (current != null) Log($"Current monitor: {current.Name}");
               })

               // ── Badge ─────────────────────────────────────────
               | new Separator()
               | Text.H2("Taskbar Badge")
               | Text.Markdown("Set a numeric badge on the taskbar/dock icon.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Badge +1", () =>
                  {
                      var n = badgeCount.Set(c => c + 1);
                      window.SetBadgeCount(n);
                      Log($"Badge set to {n}");
                  })
                  | new Button("Badge 99", () =>
                  {
                      badgeCount.Set(99);
                      window.SetBadgeCount(99);
                      Log("Badge set to 99");
                  })
                  | new Button("Clear Badge", () =>
                  {
                      badgeCount.Set(0);
                      window.ClearBadge();
                      Log("Badge cleared");
                  })
               )

               // ── Context Menu ──────────────────────────────────
               | new Separator()
               | Text.H2("Context Menu")
               | Text.Markdown("Show a native context menu at the cursor position.")
               | new Button("Show Context Menu", () =>
               {
                   var ctx = new DesktopMenu()
                       .AddItem("ctx-copy", "Copy")
                       .AddItem("ctx-paste", "Paste")
                       .AddSeparator()
                       .AddSubmenu("More", sub => sub
                           .AddItem("ctx-select-all", "Select All")
                           .AddItem("ctx-find", "Find..."))
                       .AddSeparator()
                       .AddItem("ctx-about", "About Ivy Desktop");
                   window.ShowContextMenu(ctx);
                   Log("Context menu shown");
               })

               // ── Dynamic Menu ──────────────────────────────────
               | new Separator()
               | Text.H2("Dynamic Menu")
               | Text.Markdown("Replace or remove the application menu bar at runtime.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Set Minimal Menu", () =>
                  {
                      window.SetMenu(new DesktopMenu()
                          .AddSubmenu("App", a => a
                              .AddItem("menu-hello", "Hello!")
                              .AddSeparator()
                              .AddItem("exit", "Exit")));
                      Log("Menu replaced with minimal menu");
                  })
                  | new Button("Remove Menu", () =>
                  {
                      window.RemoveMenu();
                      Log("Menu removed");
                  })
               )

               // ── Tray Icon ─────────────────────────────────────
               | new Separator()
               | Text.H2("System Tray Icon")
               | Text.Markdown("A tray icon is set up in `Program.cs` via `SetTrayIcon()`. Click it to bring the window to focus.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Remove Tray Icon", () =>
                  {
                      window.RemoveTrayIcon();
                      Log("Tray icon removed");
                  })
                  | new Button("Restore Tray Icon", () =>
                  {
                      window.SetTrayIcon(typeof(DesktopWindow), "Ivy.Desktop.ivy.ico", "Ivy Desktop Samples",
                          new DesktopMenu()
                              .AddItem("tray-show", "Show Window")
                              .AddItem("tray-hide", "Hide Window")
                              .AddSeparator()
                              .AddItem("tray-quit", "Quit"));
                      Log("Tray icon restored");
                  })
               )

               // ── Zoom ──────────────────────────────────────────
               | new Separator()
               | Text.H2("Zoom")
               | Text.Markdown($"Control the webview zoom level. Current: **{zoomLevel.Value:F2}x**")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Zoom In (+0.25)", () =>
                  {
                      var z = zoomLevel.Set(v => Math.Min(v + 0.25, 3.0));
                      window.SetZoom(z);
                      Log($"Zoom: {z:F2}x");
                  })
                  | new Button("Zoom Out (-0.25)", () =>
                  {
                      var z = zoomLevel.Set(v => Math.Max(v - 0.25, 0.25));
                      window.SetZoom(z);
                      Log($"Zoom: {z:F2}x");
                  })
                  | new Button("Reset Zoom", () =>
                  {
                      zoomLevel.Set(1.0);
                      window.SetZoom(1.0);
                      Log("Zoom: 1.00x");
                  })
               )

               // ── JavaScript Interop ────────────────────────────
               | new Separator()
               | Text.H2("JavaScript Interop")
               | Text.Markdown("Execute arbitrary JavaScript in the webview.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Execute alert()", () =>
                  {
                      window.ExecuteScript("alert('Hello from Ivy.Desktop!')");
                      Log("Executed JavaScript alert");
                  })
                  | new Button("Get document.title", () =>
                  {
                      window.ExecuteScript("document.title");
                      Log("Executed document.title script");
                  })
               )

               // ── Copy / Paste Test ────────────────────────────
               | new Separator()
               | Text.H2("Copy / Paste Test")
               | Text.Markdown("Use these text fields to verify that Ctrl+C / Ctrl+V (copy/paste) work correctly.")
               | textA.ToTextInput("Type or paste here...")
               | textB.ToTextInput("Paste into this field...")

               // ── Splashscreen ─────────────────────────────────
               | new Separator()
               | Text.H2("Splashscreen")
               | Text.Markdown("A native splash image is shown during startup while the Ivy server initializes. " +
                               "Configured via `.Splash()` on the `DesktopWindow` builder in `Program.cs`.")

               // ── Event Log ─────────────────────────────────────
               | new Separator()
               | Text.H2("Event Log")
               | Text.Markdown("Actions and events are logged here. Menu clicks and window events also appear in the console.")
               | new Card(
                   Text.Literal(string.IsNullOrEmpty(log.Value) ? "No events yet. Click a button above." : log.Value)
               )
        ;
    }
}
