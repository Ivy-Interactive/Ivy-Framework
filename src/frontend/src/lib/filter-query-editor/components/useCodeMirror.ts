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
  const columnsRef = useRef(columns);

  // Keep columns ref up to date. The editor reads columns through this ref
  // (not a closure) so that column loads/changes do NOT recreate the editor.
  // Recreating it mid-typing was resetting validation state and made valid
  // queries (e.g. "[Age] = 32") momentarily parse against an empty column
  // set, falsely reporting them invalid and triggering the LLM fallback.
  useEffect(() => {
    columnsRef.current = columns;
  }, [columns]);

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

    // Pending autocomplete timer, cleared when the view is torn down so it
    // can't fire startCompletion() against a destroyed EditorView.
    let completionTimer: ReturnType<typeof setTimeout> | undefined;

    // Create extensions array
    const extensions: Extension[] = [
      // Basic setup
      history(),
      keymap.of([
        // Custom keymaps for applying filter. Enter is always consumed so a
        // newline is never inserted into this single-line editor: the host
        // component handles Enter (submitting the filter) at the React level.
        // Returning false here would let defaultKeymap insert "\n", which the
        // transaction filter then turned into a stray trailing space.
        {
          key: "Enter",
          run: () => {
            onApplyRef.current?.();
            return true;
          },
        },
        {
          key: "Mod-Enter", // Cmd+Enter on Mac, Ctrl+Enter on Windows/Linux
          run: () => {
            onApplyRef.current?.();
            return true;
          },
        },
        {
          key: "Shift-Enter",
          run: () => {
            onApplyRef.current?.();
            return true;
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
      createFormattingExtension(() => columnsRef.current),

      // Autocomplete
      createAutocompleteExtension(() => columnsRef.current),

      // Update listener for onChange
      EditorView.updateListener.of((update: ViewUpdate) => {
        if (update.docChanged && onChangeRef.current) {
          const text = update.state.doc.toString();
          onChangeRef.current(text);
        }

        // Trigger autocomplete on focus
        if (update.focusChanged && update.view.hasFocus) {
          // Use setTimeout to ensure focus is complete before triggering
          clearTimeout(completionTimer);
          completionTimer = setTimeout(() => {
            startCompletion(update.view);
          }, 10);
        }
      }),

      // Keep this a single-line editor.
      EditorState.transactionFilter.of((tr) => {
        if (!tr.docChanged) return tr;

        const newText = tr.newDoc.toString();
        if (!newText.includes("\n")) return tr;

        // Strip carriage returns, then collapse newlines. If removing the
        // newline(s) yields exactly the previous document, the only thing the
        // change added was a line break (e.g. pressing Enter) — drop the
        // transaction entirely so no character (not even a space) is inserted.
        // Pressing Enter previously fell through to defaultKeymap, inserting
        // "\n", which this filter turned into a stray trailing space.
        const oldText = tr.startState.doc.toString();
        const withoutNewlines = newText.replace(/\r/g, "").replace(/\n/g, "");
        if (withoutNewlines === oldText) {
          // No-op transaction: keep the document and selection unchanged.
          return [];
        }

        // Otherwise this is real multi-line content (e.g. a paste): flatten it
        // to a single line, using a space so adjacent line tokens stay
        // separated, and collapse the runs that creates.
        const flattened = newText.replace(/\r/g, "").replace(/\n+/g, " ").replace(/ {2,}/g, " ");
        return [
          {
            // The replacement change applies against the transaction's start
            // state, so the range spans the *old* document length. Using
            // tr.newDoc.length would build a range past the end of the
            // start-state doc and throw a RangeError on growing pastes.
            changes: {
              from: 0,
              to: tr.startState.doc.length,
              insert: flattened,
            },
            selection: { anchor: flattened.length },
          },
        ];
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
      clearTimeout(completionTimer);
      editorView.destroy();
      setView(null);
      setContainerEl(null);
    };
    // `columns`, `value` and `onChange` are intentionally NOT dependencies:
    // they are read through refs so the editor is created once and is not
    // torn down when columns load or the parent re-renders.
    // oxlint-disable-next-line react-hooks/exhaustive-deps
  }, [container, theme, readOnly, placeholder, autoFocus]);

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
