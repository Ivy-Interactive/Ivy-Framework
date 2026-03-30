using Ivy.Widgets.DiffView;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.GitCompareArrows, group: ["Widgets", "Primitives"], searchHints: ["diff", "git", "compare", "code review", "unified", "split"])]
public class DiffViewApp : SampleBase
{
    protected override object? BuildSample()
    {
        var viewType = UseState(DiffViewType.Unified);

        var diff = @"--- a/src/utils.ts
+++ b/src/utils.ts
@@ -1,8 +1,10 @@
-export function formatDate(date: Date): string {
-  return date.toLocaleDateString();
+export function formatDate(date: Date, locale?: string): string {
+  return date.toLocaleDateString(locale ?? 'en-US', {
+    year: 'numeric',
+    month: 'long',
+    day: 'numeric',
+  });
 }

 export function capitalize(str: string): string {
   return str.charAt(0).toUpperCase() + str.slice(1);
 }";

        return Layout.Vertical()
            | (Layout.Horizontal()
                | new Button("Unified", () => viewType.Set(DiffViewType.Unified))
                    .Variant(viewType.Value == DiffViewType.Unified ? ButtonVariant.Primary : ButtonVariant.Outline)
                | new Button("Split", () => viewType.Set(DiffViewType.Split))
                    .Variant(viewType.Value == DiffViewType.Split ? ButtonVariant.Primary : ButtonVariant.Outline))
            | new DiffView()
                .Diff(diff)
                .ViewType(viewType.Value)
                .OldRevision("a/src/utils.ts")
                .NewRevision("b/src/utils.ts")
                .Language("typescript");
    }
}
