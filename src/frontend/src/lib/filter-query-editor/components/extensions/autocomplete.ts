/**
 * Autocomplete extension for CodeMirror
 * Provides context-aware completions for columns, operators, and values
 */

import { Extension } from "@codemirror/state";
import {
  autocompletion,
  CompletionContext,
  CompletionResult,
  Completion,
} from "@codemirror/autocomplete";
import { ColumnDef } from "../../types/column";

/**
 * Format technical type names to user-friendly labels
 */
function formatTypeForDisplay(type: string): string {
  const typeMap: Record<string, string> = {
    Utf8: "Text",
    string: "Text",
    Int32: "Number",
    Int64: "Number",
    Float32: "Number",
    Float64: "Number",
    number: "Number",
    Boolean: "True/False",
    boolean: "True/False",
    Date: "Date",
    date: "Date",
    Timestamp: "Date",
    Enum: "Option",
    enum: "Option",
  };

  return typeMap[type] || type.charAt(0).toUpperCase() + type.slice(1);
}

/**
 * Get the text before the cursor up to the start of a word or token
 */
function getTextBeforeCursor(context: CompletionContext): string {
  const line = context.state.doc.lineAt(context.pos);
  const textBeforeCursor = line.text.slice(0, context.pos - line.from);
  return textBeforeCursor;
}

/**
 * Check if we're after an opening bracket (column reference context)
 */
function isInColumnContext(textBefore: string): boolean {
  // Check if last non-whitespace char is [
  const trimmed = textBefore.trim();
  return trimmed.endsWith("[") || /\[\w*$/.test(textBefore);
}

/**
 * Check if we're after a column reference (operator context)
 */
function isInOperatorContext(textBefore: string): boolean {
  // Check if we have a complete column reference like [columnName]
  return (
    /\]\s*$/.test(textBefore) &&
    !/(equals|contains|starts|ends|greater|less|is)\s*$/i.test(textBefore)
  );
}

/**
 * Create column name completions
 */
function createColumnCompletions(columns: ColumnDef[], context: CompletionContext): Completion[] {
  // Check if there's already a closing bracket after the cursor
  const line = context.state.doc.lineAt(context.pos);
  const textAfterCursor = line.text.slice(context.pos - line.from);
  const hasClosingBracket = textAfterCursor.startsWith("]");

  return columns.map((col) => {
    const insertText = hasClosingBracket ? col.name : `${col.name}]`;

    return {
      label: col.name,
      type: "variable",
      detail: formatTypeForDisplay(col.type),
      // Use a function to control cursor position after insertion
      apply: (view, _completion, from, to) => {
        const cursorPos = hasClosingBracket
          ? from + insertText.length + 1 // Skip past the existing ]
          : from + insertText.length; // Already includes the ]

        view.dispatch({
          changes: { from, to, insert: insertText },
          selection: { anchor: cursorPos },
        });
      },
      boost: 1,
    };
  });
}

/**
 * Normalize column type to standard types
 */
function normalizeColumnType(type: string): string {
  const typeMap: Record<string, string> = {
    Utf8: "string",
    Int32: "number",
    Int64: "number",
    Float32: "number",
    Float64: "number",
    Boolean: "boolean",
    Date: "date",
    Timestamp: "date",
    Enum: "enum",
  };

  return typeMap[type] || type.toLowerCase();
}

/**
 * Create operator completions based on column type
 */
function createOperatorCompletions(columns: ColumnDef[], textBefore: string): Completion[] {
  // Extract column name from text before cursor
  const columnMatch = textBefore.match(/\[([^\]]+)\]\s*$/);
  if (!columnMatch) return [];

  const columnName = columnMatch[1];
  const column = columns.find((c) => c.name === columnName);
  if (!column) return [];

  const operators: Completion[] = [];

  // Normalize the column type
  const normalizedType = normalizeColumnType(column.type);

  // Add operators based on column type
  switch (normalizedType) {
    case "string":
      operators.push(
        { label: "equals", type: "keyword", apply: "equals ", boost: 10 },
        { label: "=", type: "operator", apply: "= ", boost: 9 },
        { label: "contains", type: "keyword", apply: "contains ", boost: 8 },
        { label: "starts with", type: "keyword", apply: "starts with ", boost: 7 },
        { label: "ends with", type: "keyword", apply: "ends with ", boost: 6 },
      );
      break;

    case "number":
      operators.push(
        { label: "equals", type: "keyword", apply: "equals ", boost: 10 },
        { label: "=", type: "operator", apply: "= ", boost: 9 },
        { label: ">", type: "operator", apply: "> ", boost: 8 },
        { label: "<", type: "operator", apply: "< ", boost: 7 },
        { label: ">=", type: "operator", apply: ">= ", boost: 6 },
        { label: "<=", type: "operator", apply: "<= ", boost: 5 },
        { label: "!=", type: "operator", apply: "!= ", boost: 4 },
        { label: "greater than", type: "keyword", apply: "greater than ", boost: 3 },
        { label: "less than", type: "keyword", apply: "less than ", boost: 2 },
      );
      break;

    case "boolean":
      operators.push(
        { label: "equals", type: "keyword", apply: "equals ", boost: 10 },
        { label: "=", type: "operator", apply: "= ", boost: 9 },
      );
      break;

    case "date":
      operators.push(
        { label: "equals", type: "keyword", apply: "equals ", boost: 10 },
        { label: "=", type: "operator", apply: "= ", boost: 9 },
        { label: ">", type: "operator", apply: "> ", boost: 8 },
        { label: "<", type: "operator", apply: "< ", boost: 7 },
        { label: ">=", type: "operator", apply: ">= ", boost: 6 },
        { label: "<=", type: "operator", apply: "<= ", boost: 5 },
      );
      break;

    case "enum":
      operators.push(
        { label: "equals", type: "keyword", apply: "equals ", boost: 10 },
        { label: "=", type: "operator", apply: "= ", boost: 9 },
      );
      break;
  }

  // Add blank operators for applicable types
  if (["string", "date", "enum"].includes(column.type)) {
    operators.push(
      { label: "IS BLANK", type: "keyword", apply: "IS BLANK", boost: 2 },
      { label: "IS NOT BLANK", type: "keyword", apply: "IS NOT BLANK", boost: 1 },
    );
  }

  return operators;
}

/**
 * Create value completions for enum columns
 */
function createValueCompletions(columns: ColumnDef[], textBefore: string): Completion[] {
  // Extract column name and check for operator
  const match = textBefore.match(/\[([^\]]+)\]\s+(?:equals|=)\s*$/i);
  if (!match) return [];

  const columnName = match[1];
  const column = columns.find((c) => c.name === columnName);
  if (!column || column.type !== "enum") return [];

  // enumValues not available in ColumnDef - returning empty array
  return [];
}

/**
 * Create boolean value completions
 */
function createBooleanCompletions(textBefore: string, columns: ColumnDef[]): Completion[] {
  const match = textBefore.match(/\[([^\]]+)\]\s+(?:equals|=)\s*$/i);
  if (!match) return [];

  const columnName = match[1];
  const column = columns.find((c) => c.name === columnName);
  if (!column || column.type !== "boolean") return [];

  return [
    { label: "true", type: "constant", apply: "true", boost: 2 },
    { label: "false", type: "constant", apply: "false", boost: 1 },
  ];
}

/**
 * Create logical operator completions
 */
function createLogicalCompletions(textBefore: string): Completion[] {
  // Check if there's already a space before the cursor
  const hasSpaceBefore = /\s$/.test(textBefore);

  return [
    { label: "AND", type: "keyword", apply: hasSpaceBefore ? "AND " : " AND ", boost: 10 },
    { label: "OR", type: "keyword", apply: hasSpaceBefore ? "OR " : " OR ", boost: 9 },
    { label: "NOT", type: "keyword", apply: "NOT ", boost: 8 },
  ];
}

/**
 * Main completion function
 */
function completeQuery(columns: ColumnDef[]) {
  return (context: CompletionContext): CompletionResult | null => {
    const textBefore = getTextBeforeCursor(context);

    // Show column suggestions at the start or when empty
    if (textBefore.trim() === "" || context.pos === 0) {
      const columnSuggestions = columns.map((col) => ({
        label: col.name,
        type: "variable" as const,
        detail: formatTypeForDisplay(col.type),
        apply: `[${col.name}] `,
        boost: 100,
      }));

      return {
        from: context.pos,
        options: columnSuggestions,
      };
    }

    // Column name completion
    if (isInColumnContext(textBefore)) {
      const completions = createColumnCompletions(columns, context);
      const bracketMatch = textBefore.match(/\[(\w*)$/);
      const from = bracketMatch ? context.pos - bracketMatch[1].length : context.pos;

      return {
        from,
        options: completions,
        validFor: /^\w*$/,
      };
    }

    // Operator completion
    if (isInOperatorContext(textBefore)) {
      const completions = createOperatorCompletions(columns, textBefore);
      return {
        from: context.pos,
        options: completions,
        validFor: /^[\w\s]*$/,
      };
    }

    // Value completions for enums
    const enumCompletions = createValueCompletions(columns, textBefore);
    if (enumCompletions.length > 0) {
      return {
        from: context.pos,
        options: enumCompletions,
      };
    }

    // Boolean completions
    const boolCompletions = createBooleanCompletions(textBefore, columns);
    if (boolCompletions.length > 0) {
      return {
        from: context.pos,
        options: boolCompletions,
      };
    }

    // Logical operators (only if we're not in middle of an expression)

    // Must have a space after the value/closing bracket to trigger AND/OR suggestions
    if (/\]\s+$/.test(textBefore) || /(?:true|false|\d+|"[^"]*")\s+$/.test(textBefore)) {
      const logicalCompletions = createLogicalCompletions(textBefore);
      return {
        from: context.pos,
        options: logicalCompletions,
        validFor: /^[\w\s]*$/,
      };
    }

    return null;
  };
}

/**
 * Create autocomplete extension
 */
export function createAutocompleteExtension(columns: ColumnDef[]): Extension {
  return autocompletion({
    override: [completeQuery(columns)],
    activateOnTyping: true,
    maxRenderedOptions: 20,
    closeOnBlur: true,
    icons: false,
  });
}
