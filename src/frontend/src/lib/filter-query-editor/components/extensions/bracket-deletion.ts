/**
 * Custom bracket deletion extension for CodeMirror
 * Deletes matching bracket pairs when one bracket is deleted
 */

import { Extension, EditorSelection, Prec } from "@codemirror/state";
import { keymap } from "@codemirror/view";

/**
 * Bracket pairs to handle
 */
const BRACKET_PAIRS: Record<string, string> = {
  "[": "]",
  "(": ")",
  "{": "}",
};

const REVERSE_BRACKET_PAIRS: Record<string, string> = {
  "]": "[",
  ")": "(",
  "}": "{",
};

/**
 * Find matching closing bracket by searching forward
 */
function findMatchingClosing(
  doc: any,
  startPos: number,
  openChar: string,
  closeChar: string,
): number | null {
  const text = doc.sliceString(startPos);
  let depth = 1;

  for (let i = 0; i < text.length; i++) {
    if (text[i] === openChar) {
      depth++;
    } else if (text[i] === closeChar) {
      depth--;
      if (depth === 0) {
        return startPos + i;
      }
    }
  }

  return null;
}

/**
 * Find matching opening bracket by searching backward
 */
function findMatchingOpening(
  doc: any,
  endPos: number,
  openChar: string,
  closeChar: string,
): number | null {
  const text = doc.sliceString(0, endPos);
  let depth = 1;

  for (let i = text.length - 1; i >= 0; i--) {
    if (text[i] === closeChar) {
      depth++;
    } else if (text[i] === openChar) {
      depth--;
      if (depth === 0) {
        return i;
      }
    }
  }

  return null;
}

/**
 * Creates an extension that handles bracket pair deletion
 */
export function bracketDeletion(): Extension {
  return Prec.high(
    keymap.of([
      {
        key: "Backspace",
        run: (view) => {
          const { state } = view;
          const changes: Array<{ from: number; to: number }> = [];

          // Only handle single cursor (not multiple selections)
          if (state.selection.ranges.length > 1 || !state.selection.main.empty) {
            return false;
          }

          const pos = state.selection.main.head;
          const charBefore = state.doc.sliceString(pos - 1, pos);

          // Deleting an opening bracket
          if (BRACKET_PAIRS[charBefore]) {
            const closeChar = BRACKET_PAIRS[charBefore];
            const matchingPos = findMatchingClosing(state.doc, pos, charBefore, closeChar);

            if (matchingPos !== null) {
              // Delete both brackets
              changes.push({ from: pos - 1, to: pos }); // Delete opening bracket
              changes.push({ from: matchingPos, to: matchingPos + 1 }); // Delete closing bracket
            } else {
              return false; // No matching bracket, use default behavior
            }
          }
          // Deleting a closing bracket
          else if (REVERSE_BRACKET_PAIRS[charBefore]) {
            const openChar = REVERSE_BRACKET_PAIRS[charBefore];
            const matchingPos = findMatchingOpening(state.doc, pos - 1, openChar, charBefore);

            if (matchingPos !== null) {
              // Delete both brackets (in reverse order to maintain positions)
              changes.push({ from: pos - 1, to: pos }); // Delete closing bracket
              changes.push({ from: matchingPos, to: matchingPos + 1 }); // Delete opening bracket
            } else {
              return false; // No matching bracket, use default behavior
            }
          } else {
            return false; // Not a bracket, use default behavior
          }

          if (changes.length > 0) {
            // Sort changes in reverse order to maintain positions
            changes.sort((a, b) => b.from - a.from);

            // Calculate new cursor position after deletions
            // We need to account for the character we deleted before the cursor
            let newPos = pos - 1;

            // If we deleted a closing bracket and the opening bracket was before cursor,
            // we need to adjust position by 1 more
            if (REVERSE_BRACKET_PAIRS[charBefore]) {
              const openingBracketPos = changes.find((c) => c.from < pos - 1);
              if (openingBracketPos) {
                newPos = pos - 2; // Account for both deletions before cursor
              }
            }

            view.dispatch({
              changes: changes,
              selection: EditorSelection.cursor(newPos),
            });
            return true;
          }

          return false;
        },
      },
      {
        key: "Delete",
        run: (view) => {
          const { state } = view;
          const changes: Array<{ from: number; to: number }> = [];

          // Only handle single cursor (not multiple selections)
          if (state.selection.ranges.length > 1 || !state.selection.main.empty) {
            return false;
          }

          const pos = state.selection.main.head;
          const charAfter = state.doc.sliceString(pos, pos + 1);

          // Deleting an opening bracket
          if (BRACKET_PAIRS[charAfter]) {
            const closeChar = BRACKET_PAIRS[charAfter];
            const matchingPos = findMatchingClosing(state.doc, pos + 1, charAfter, closeChar);

            if (matchingPos !== null) {
              // Delete both brackets (in reverse order to maintain positions)
              changes.push({ from: matchingPos, to: matchingPos + 1 }); // Delete closing bracket
              changes.push({ from: pos, to: pos + 1 }); // Delete opening bracket
            } else {
              return false; // No matching bracket, use default behavior
            }
          }
          // Deleting a closing bracket
          else if (REVERSE_BRACKET_PAIRS[charAfter]) {
            const openChar = REVERSE_BRACKET_PAIRS[charAfter];
            const matchingPos = findMatchingOpening(state.doc, pos, openChar, charAfter);

            if (matchingPos !== null) {
              // Delete both brackets (in reverse order to maintain positions)
              changes.push({ from: pos, to: pos + 1 }); // Delete closing bracket
              changes.push({ from: matchingPos, to: matchingPos + 1 }); // Delete opening bracket
            } else {
              return false; // No matching bracket, use default behavior
            }
          } else {
            return false; // Not a bracket, use default behavior
          }

          if (changes.length > 0) {
            // Sort changes in reverse order to maintain positions
            changes.sort((a, b) => b.from - a.from);

            // Calculate new cursor position after deletions
            let newPos = pos;

            // If we deleted a closing bracket and the opening bracket was before cursor,
            // we need to adjust position by 1
            if (REVERSE_BRACKET_PAIRS[charAfter]) {
              const openingBracketPos = changes.find((c) => c.from < pos);
              if (openingBracketPos) {
                newPos = pos - 1; // Account for deletion before cursor
              }
            }

            view.dispatch({
              changes: changes,
              selection: EditorSelection.cursor(newPos),
            });
            return true;
          }

          return false;
        },
      },
    ]),
  );
}
