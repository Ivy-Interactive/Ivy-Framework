using Ivy;
using Ivy.Widgets.DiffView;

var server = new Server();
server.AddApp<DiffViewDemo>();
await server.RunAsync();

[App]
class DiffViewDemo : ViewBase
{
    public override object Build()
    {
        var diff = @"--- a/file.txt
+++ b/file.txt
@@ -1,3 +1,4 @@
 line 1
-line 2
+line 2 modified
+line 2.5 added
 line 3";

        return new DiffView()
            .Diff(diff)
            .Split()
            .OldRevision("a/file.txt")
            .NewRevision("b/file.txt");
    }
}
