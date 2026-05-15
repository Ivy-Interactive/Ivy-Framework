/**
 * Custom hook for managing CodeMirror 6 lifecycle in React
 */

import { useEffect, useRef, useState } from "react";
import { EditorState, Extension } from "@codemirror/state";
import { EditorView, keymap, ViewUpdate } from "@codemirror/view";
import { defaultKeymap, history, historyKeymap } from "@codemirror/commands";
import { startCompletion } from "@codemirror/autocomplete";
import { ColumnDef } from "../types/column";
import { createBaseExtensions } from "./extensions/base";
import { createHighlightingExtension } from "./extensions/highlighting";
import { createValidationExtension } from "./extensions/validation";
import { createFormattingExtension } from "./extensions/formatting";
import { createAutocompleteExtension } from "./extensions/autocomplete";

interface UseCodeMirrorOptions {
  container: HTMLElement | null;
  value: string;
  columns: ColumnDef[];
  onChange?: (text: string) => void;
  onApply?: () => void;
  theme?: "light" | "dark";
  readOnly?: boolean;
  placeholder?: string;
  autoFocus?: boolean;
}

/**
 * Custom hook that manages the CodeMirror editor lifecycle
 */
export function useCodeMirror({
  container,
  value,
  columns,
  onChange,
  onApply,
  theme = "light",
  readOnly = false,
  placeholder,
  autoFocus = false,
}: UseCodeMirrorOptions) {
  const [view, setView] = useState<EditorView | null>(null);
  const [containerEl, setContainerEl] = useState<HTMLElement | null>(null);
  const onChangeRef = useRef(onChange);
  const onApplyRef = useRef(onApply);

  // Keep onChange ref up to date
  useEffect(() => {
    onChangeRef.current = onChange;
  }, [onChange]);

  // Keep onApply ref up to date
  useEffect(() => {
    onApplyRef.current = onApply;
  }, [onApply]);

  // Initialize editor
  useEffect(() => {
    if (!container) return;

    // Create extensions array
    const extensions: Extension[] = [
      // Basic setup
      history(),
      keymap.of([
        // Custom keymaps for applying filter
        {
          key: "Enter",
          run: () => {
            if (onApplyRef.current) {
              onApplyRef.current();
              return true;
            }
            return false;
          },
        },
        {
          key: "Mod-Enter", // Cmd+Enter on Mac, Ctrl+Enter on Windows/Linux
          run: () => {
            if (onApplyRef.current) {
              onApplyRef.current();
              return true;
            }
            return false;
          },
        },
        ...defaultKeymap,
        ...historyKeymap,
      ]),
      EditorView.editable.of(!readOnly),

      // Base extensions (placeholder, etc.)
      ...createBaseExtensions({ placeholder }),

      // Syntax highlighting
      createHighlightingExtension(theme),

      // Validation (linting)
      createValidationExtension(columns),

      // Auto-formatting
      createFormattingExtension(columns),

      // Autocomplete
      createAutocompleteExtension(columns),

      // Update listener for onChange
      EditorView.updateListener.of((update: ViewUpdate) => {
        if (update.docChanged && onChangeRef.current) {
          const text = update.state.doc.toString();
          onChangeRef.current(text);
        }

        // Trigger autocomplete on focus
        if (update.focusChanged && update.view.hasFocus) {
          // Use setTimeout to ensure focus is complete before triggering
          setTimeout(() => {
            startCompletion(update.view);
          }, 10);
        }
      }),

      // Prevent line breaks - make it single line
      EditorState.transactionFilter.of((tr) => {
        if (!tr.docChanged) return tr;

        let text = tr.newDoc.toString();
        if (text.includes("\n")) {
          // Remove all line breaks. The replacement change applies against the
          // transaction's start state, so the range must span the *old*
          // document length (tr.startState.doc.length). Using tr.newDoc.length
          // here builds a range past the end of the start-state doc and throws
          // a RangeError whenever a change both grows the doc and adds a newline
          // (e.g. pasting multi-line text).
          text = text.replace(/\n/g, " ");
          return [
            {
              changes: {
                from: 0,
                to: tr.startState.doc.length,
                insert: text,
              },
              selection: tr.selection,
            },
          ];
        }
        return tr;
      }),

      // Theme
      EditorView.theme({
        "&": {
          fontSize: "12px",
          fontFamily: '"Monaco", "Consolas", "Courier New", monospace',
        },
        ".cm-editor": {
          borderRadius: "4px",
          height: "100%",
        },
        ".cm-editor.cm-focused": {
          outline: "none",
        },
        ".cm-content": {
          padding: "16px 16px",
          minHeight: "auto",
          cursor: "text",
        },
        ".cm-line": {
          padding: "0",
        },
        ".cm-placeholder": {
          color: "#999999",
        },
        ".cm-scroller": {
          fontFamily: "inherit",
          overflow: "hidden",
        },
      }),

      // Additional theme-specific styles
      EditorView.theme({}, { dark: theme === "dark" }),
    ];

    // Create editor state
    const state = EditorState.create({
      doc: value,
      extensions,
    });

    // Create editor view
    const editorView = new EditorView({
      state,
      parent: container,
    });

    // Disable Grammarly and spellcheck on the contenteditable element
    const contentElement = editorView.contentDOM;
    contentElement.setAttribute("data-gramm", "false");
    contentElement.setAttribute("data-gramm_editor", "false");
    contentElement.setAttribute("spellcheck", "false");

    // Auto-focus if requested
    if (autoFocus) {
      editorView.focus();
    }

    // Make the editor focusable and clickable
    container.addEventListener("click", () => {
      editorView.focus();
    });

    setView(editorView);
    setContainerEl(container);

    // Cleanup on unmount
    return () => {
      editorView.destroy();
      setView(null);
      setContainerEl(null);
    };
  }, [container, theme, readOnly, placeholder, autoFocus, columns]); // Note: value and onChange are not dependencies here

  // Handle external value changes
  useEffect(() => {
    if (view && view.state.doc.toString() !== value) {
      // Save current cursor position
      const cursorPos = view.state.selection.main.head;
      const oldLength = view.state.doc.length;
      const newLength = value.length;

      // Calculate new cursor position (proportional mapping as fallback)
      // If cursor was at end, keep it at end
      // Otherwise, try to maintain relative position
      let newCursorPos = cursorPos;
      if (cursorPos === oldLength) {
        newCursorPos = newLength;
      } else if (oldLength > 0) {
        const ratio = cursorPos / oldLength;
        newCursorPos = Math.floor(ratio * newLength);
      }

      // Ensure cursor is within bounds
      newCursorPos = Math.max(0, Math.min(newCursorPos, newLength));

      view.dispatch({
        changes: {
          from: 0,
          to: view.state.doc.length,
          insert: value,
        },
        selection: { anchor: newCursorPos, head: newCursorPos },
      });
    }
  }, [value, view]);

  return { view, containerEl };
}
