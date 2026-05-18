/**
 * Syntax highlighting extension for CodeMirror
 * Uses ANTLR tokenizer to provide accurate syntax highlighting
 */

import { Extension } from "@codemirror/state";
import { Decoration, DecorationSet, EditorView, ViewPlugin, ViewUpdate } from "@codemirror/view";
import { tokenizeQuery } from "./tokenizer";

/**
 * Theme colors for syntax highlighting
 */
const LIGHT_THEME = {
  field: "#0066cc", // Blue
  string: "#22863a", // Green
  number: "#6f42c1", // Purple
  boolean: "#d73a49", // Red
  keyword: "#d73a49", // Red
  operator: "#005cc5", // Dark blue
  bracket: "#586069", // Gray
  unknown: "#24292e", // Default text
};

const DARK_THEME = {
  field: "#79b8ff", // Light blue
  string: "#9ecbff", // Light green
  number: "#b392f0", // Light purple
  boolean: "#f97583", // Light red
  keyword: "#f97583", // Light red
  operator: "#79b8ff", // Light blue
  bracket: "#959da5", // Light gray
  unknown: "#e1e4e8", // Default light text
};

/**
 * Check if text has at least one matching pair of square brackets
 */
function hasMatchingBracketPairs(text: string): boolean {
  let openCount = 0;
  let matchedPairs = 0;

  for (const char of text) {
    if (char === "[") {
      openCount++;
    } else if (char === "]") {
      if (openCount > 0) {
        matchedPairs++;
        openCount--;
      }
    }
  }

  return matchedPairs > 0;
}

/**
 * Create decoration marks for tokens
 */
function createDecorations(view: EditorView, theme: "light" | "dark"): DecorationSet {
  const text = view.state.doc.toString();

  // Don't apply coloring if there are no matching bracket pairs
  if (!hasMatchingBracketPairs(text)) {
    return Decoration.none;
  }

  const tokens = tokenizeQuery(text);
  const decorations: any[] = [];
  const colors = theme === "light" ? LIGHT_THEME : DARK_THEME;

  for (const token of tokens) {
    const color = colors[token.type] || colors.unknown;
    const decoration = Decoration.mark({
      class: `cm-query-${token.type}`,
      attributes: {
        style: `color: ${color}`,
      },
    });

    decorations.push(decoration.range(token.start, token.end));
  }

  return Decoration.set(decorations);
}

/**
 * ViewPlugin that manages syntax highlighting decorations
 */
class HighlightingPlugin {
  decorations: DecorationSet;

  constructor(
    view: EditorView,
    private theme: "light" | "dark",
  ) {
    this.decorations = createDecorations(view, theme);
  }

  update(update: ViewUpdate) {
    if (update.docChanged || update.viewportChanged) {
      this.decorations = createDecorations(update.view, this.theme);
    }
  }

  destroy() {
    // Cleanup if needed
  }
}

/**
 * Create syntax highlighting extension for the query language
 */
export function createHighlightingExtension(theme: "light" | "dark" = "light"): Extension {
  return [
    // ViewPlugin for managing decorations
    ViewPlugin.define((view) => new HighlightingPlugin(view, theme), {
      decorations: (plugin) => plugin.decorations,
    }),

    // CSS styles for the tokens
    EditorView.baseTheme({
      ".cm-query-field": {
        fontWeight: "600",
      },
      ".cm-query-keyword": {
        fontWeight: "600",
      },
      ".cm-query-operator": {
        fontWeight: "500",
      },
      ".cm-query-bracket": {
        fontWeight: "600",
      },
    }),
  ];
}
