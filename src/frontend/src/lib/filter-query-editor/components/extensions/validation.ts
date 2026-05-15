/**
 * Validation extension for CodeMirror
 * Provides real-time error highlighting and diagnostics
 */

import { Extension } from "@codemirror/state";
import { EditorView, Decoration, DecorationSet, ViewPlugin, ViewUpdate } from "@codemirror/view";
import { ColumnDef } from "../../types/column";

/**
 * Linter functionality disabled
 * Validation now happens through the onChange callback in QueryEditor
 * This keeps the editor clean without visual error decorations
 */

/**
 * ViewPlugin for managing error decorations (underlines)
 */
class ValidationPlugin {
  decorations: DecorationSet;
  private timeoutId: number | null = null;

  constructor(private view: EditorView) {
    this.decorations = this.computeDecorations();
  }

  private computeDecorations(): DecorationSet {
    // Disabled: No visual decorations for errors
    // Validation still works through the linter for error reporting
    return Decoration.none;
  }

  update(update: ViewUpdate) {
    if (update.docChanged) {
      // Debounce validation updates
      if (this.timeoutId !== null) {
        clearTimeout(this.timeoutId);
      }

      this.timeoutId = window.setTimeout(() => {
        this.decorations = this.computeDecorations();
        this.view.update([]); // Force view update
        this.timeoutId = null;
      }, 300);
    }
  }

  destroy() {
    if (this.timeoutId !== null) {
      clearTimeout(this.timeoutId);
    }
  }
}

/**
 * Create validation extension (visual error indicators disabled)
 * Validation happens through onChange callback instead
 */
export function createValidationExtension(_columns: ColumnDef[]): Extension {
  return [
    // Linter disabled - validation happens through onChange callback instead
    // createQueryLinter(columns),

    // ViewPlugin for error underlines (disabled)
    ViewPlugin.define((view) => new ValidationPlugin(view), {
      decorations: (plugin) => plugin.decorations,
    }),

    // CSS styles to hide all error decorations
    EditorView.baseTheme({
      ".cm-query-error": {
        // No styling
      },
      ".cm-lintRange-error": {
        // Hide the SVG underline completely
        backgroundImage: "none !important",
        paddingBottom: "0 !important",
      },
      ".cm-diagnostic-error": {
        // No border
      },
      ".cm-tooltip.cm-tooltip-lint": {
        // Hide lint tooltips
        display: "none !important",
      },
    }),
  ];
}
