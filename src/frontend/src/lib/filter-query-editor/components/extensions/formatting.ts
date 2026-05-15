/**
 * Auto-formatting extension for CodeMirror
 * Formats query text on blur or via keyboard shortcut
 */

import { Extension } from "@codemirror/state";
import { EditorView, ViewPlugin, ViewUpdate, keymap } from "@codemirror/view";
import { ColumnDef } from "../../types/column";
import { formatQueryString } from "../../formatter/QueryFormatter";

/**
 * Calculate minimal changes between old and new text
 * This helps CodeMirror's cursor mapping work correctly
 */
function calculateChanges(oldText: string, newText: string) {
  // If texts are identical, no changes needed
  if (oldText === newText) {
    return [];
  }

  // Find common prefix
  let prefixLen = 0;
  const minLen = Math.min(oldText.length, newText.length);
  while (prefixLen < minLen && oldText[prefixLen] === newText[prefixLen]) {
    prefixLen++;
  }

  // Find common suffix
  let suffixLen = 0;
  while (
    suffixLen < minLen - prefixLen &&
    oldText[oldText.length - 1 - suffixLen] === newText[newText.length - 1 - suffixLen]
  ) {
    suffixLen++;
  }

  // Calculate the changed region
  const from = prefixLen;
  const to = oldText.length - suffixLen;
  const insert = newText.slice(prefixLen, newText.length - suffixLen);

  return [{ from, to, insert }];
}

/**
 * Format the current document
 * Preserves cursor position by calculating minimal changes
 */
function formatDocument(view: EditorView, columns: ColumnDef[]): boolean {
  const text = view.state.doc.toString();

  // Skip formatting for empty text
  if (!text.trim()) {
    return false;
  }

  try {
    // Format the query
    const formatted = formatQueryString(text, columns);

    // Only update if the text changed
    if (formatted !== text) {
      // Calculate minimal changes instead of replacing entire document
      const changes = calculateChanges(text, formatted);

      // Dispatch changes - CodeMirror will automatically map the selection
      view.dispatch({
        changes,
        // Selection will be automatically mapped through changes
        // No need to explicitly set it
      });
    }

    return true;
  } catch (error) {
    // Silently ignore formatting errors
    // The validation extension will show the actual errors
    return false;
  }
}

/**
 * ViewPlugin that handles auto-formatting on blur
 */
class FormattingPlugin {
  private formatOnBlur: boolean;
  private lastFormattedText: string = "";

  constructor(
    _view: EditorView,
    private columns: ColumnDef[],
    options: { formatOnBlur?: boolean } = {},
  ) {
    this.formatOnBlur = options.formatOnBlur !== false; // Default to true
  }

  update(update: ViewUpdate) {
    // Format on blur if enabled
    if (this.formatOnBlur && update.focusChanged && !update.view.hasFocus) {
      const currentText = update.view.state.doc.toString();

      // Only format if text changed since last format
      if (currentText !== this.lastFormattedText) {
        if (formatDocument(update.view, this.columns)) {
          this.lastFormattedText = update.view.state.doc.toString();
        }
      }
    }
  }

  destroy() {
    // Cleanup if needed
  }
}

/**
 * Create keyboard shortcuts for formatting
 */
function createFormatKeymap(columns: ColumnDef[]) {
  return keymap.of([
    {
      // Cmd/Ctrl + Shift + F to format
      key: "Mod-Shift-f",
      preventDefault: true,
      run: (view) => {
        formatDocument(view, columns);
        return true;
      },
    },
    {
      // Alt + Shift + F (VSCode format shortcut)
      key: "Alt-Shift-f",
      preventDefault: true,
      run: (view) => {
        formatDocument(view, columns);
        return true;
      },
    },
  ]);
}

/**
 * Create auto-formatting extension
 * Formats on blur and provides keyboard shortcuts
 */
export function createFormattingExtension(
  columns: ColumnDef[],
  options: { formatOnBlur?: boolean } = {},
): Extension {
  return [
    // ViewPlugin for auto-format on blur
    ViewPlugin.define((view) => new FormattingPlugin(view, columns, options)),

    // Keyboard shortcuts for manual formatting
    createFormatKeymap(columns),

    // Visual feedback for formatting
    EditorView.domEventHandlers({
      // Optional: Add visual feedback when formatting
      keydown: (event, _view) => {
        // Check if format shortcut was pressed
        if ((event.ctrlKey || event.metaKey) && event.shiftKey && event.key === "f") {
          // Could add a brief flash or indicator here
          return false;
        }
        return false;
      },
    }),
  ];
}
