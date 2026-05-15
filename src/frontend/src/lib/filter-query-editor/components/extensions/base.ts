/**
 * Base CodeMirror extensions for the query editor
 */

import { Extension } from "@codemirror/state";
import { placeholder as placeholderExt } from "@codemirror/view";
import { indentOnInput, bracketMatching } from "@codemirror/language";
import { closeBrackets } from "@codemirror/autocomplete";
import { bracketDeletion } from "./bracket-deletion";

interface BaseExtensionsOptions {
  placeholder?: string;
}

/**
 * Creates base extensions for the editor
 * Includes placeholder, bracket matching, etc.
 */
export function createBaseExtensions(options: BaseExtensionsOptions): Extension[] {
  const extensions: Extension[] = [
    // Bracket deletion (must come before closeBrackets for key priority)
    bracketDeletion(),
    // Bracket matching and closing
    bracketMatching(),
    closeBrackets(),

    // Auto-indent
    indentOnInput(),
  ];

  // Add placeholder if provided
  if (options.placeholder) {
    extensions.push(placeholderExt(options.placeholder));
  }

  return extensions;
}
