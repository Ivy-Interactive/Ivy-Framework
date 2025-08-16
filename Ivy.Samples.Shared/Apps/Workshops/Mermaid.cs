using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Workshops;

[App(icon: Icons.Workflow, path: ["Workshops"], isVisible: true, title: "Mermaid Workshop")]
public class MermaidMarkdownRequirementsApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Article()
            | new Markdown(
                """"
                # Project Idea: Mermaid Diagrams in Markdown (10. August 2025)

                We want to render [Mermaid](https://mermaid.js.org) diagrams inside Markdown content across Ivy apps and documentation.
                """"
            )
            | new Callout("This is a requirements sketch; it does not implement the feature.", icon: Icons.Info)
            | new Markdown(
                """"
                ## Goals

                * Render Mermaid code blocks in Markdown (```mermaid) as interactive diagrams.
                * Support inside `Text.Markdown`, `Markdown` widgets, and docs-generated `Article` content.
                * Automatic dark/light theme alignment with Ivy styles.
                * Lazy-load Mermaid only when content contains Mermaid diagrams.
                * Provide graceful fallback to code block if rendering fails.
                """"
            )
            | new Markdown(
                """"
                ## Non-Goals (initial)

                * Arbitrary Mermaid plugin execution.
                * Server-side diagram rendering.
                * Export/PNG generation (can be future work).
                """"
            )
            | new Markdown(
                """"
                ```csharp demo-below 
                class Diagram {
                }
                ```
                """"
            )
            | new Markdown(
                """"
                ## User Stories

                1. As a docs reader, I can see Mermaid diagrams rendered in documentation pages.
                2. As a developer, I can add a ```mermaid code block to a `Markdown` widget and see it rendered.
                3. As a reader, I get readable fallback content if rendering fails.
                """"
            )
            | new Markdown(
                """"
                ## Examples (expected rendering)

                ```mermaid
                flowchart TD
                  A[User] -->|Clicks| B(Load Page)
                  B --> C{Has Mermaid?}
                  C -- Yes --> D[Load Mermaid]
                  C -- No  --> E[Skip]
                  D --> F[Render Diagrams]
                ```

                ```mermaid
                sequenceDiagram
                  participant U as User
                  participant F as Frontend
                  participant M as Mermaid
                  U->>F: Navigate to docs
                  F->>M: Initialize with theme
                  F->>F: Render diagram
                ```
                """"
            )
            | new Markdown(
                """"
                ## Acceptance Criteria

                * mermaid code fences render as diagrams in:
                  - Widgets using `Markdown` (runtime content)
                  - Docs pages generated via `Ivy.Docs.Tools` (Markdown converted to `Article` and `Markdown` widgets)
                * Theme matches Ivy dark/light mode and text sizing.
                * No layout overflow; large diagrams scroll within their container.
                * Mermaid script is loaded on-demand and cached.
                * Rendering is resilient: shows original fenced code when errors occur.
                * Safe by default: no remote includes or script injection via Mermaid config.
                """"
            )
            | new Markdown(
                """"
                ## Technical Notes (proposed)

                * Frontend (`frontend/src/components/MarkdownRenderer.tsx`):
                  - Add Mermaid support via a remark/rehype integration (e.g., `remark-mermaidjs` + post-render init) or a custom renderer for code blocks with `language-mermaid`.
                  - Lazy-load `mermaid` package only when needed; initialize once per theme.
                  - Observe theme changes to re-render or update Mermaid theme variables.
                * Docs generator (`Ivy.Docs.Tools/MarkdownConverter.cs`):
                  - No server-side rendering needed. Ensure fenced ```mermaid blocks pass through into `new Markdown("…")` so frontend can render.
                * Widgets: ensure `Article` and `Markdown` containers allow overflow handling (scroll) for large diagrams.
                """"
            )
            | new Markdown(
                """"
                ## Risks / Constraints

                * Mermaid parsing errors must not crash the page.
                * Potential CSS conflicts; isolate Mermaid styles.
                * Performance on pages with many diagrams; batch initialize and avoid repeated reflows.
                """"
            )
            | new Markdown(
                """"
                ## Open Questions

                * Should we expose a widget-level option to disable Mermaid for specific views?
                * Do we need print/export support in v1?
                * How do we snapshot-test rendered diagrams reliably in CI?
                """"
            )
            ;
    }
}


