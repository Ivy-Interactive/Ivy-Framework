/**
 * Filter Query Parser, Formatter, and Evaluator
 *
 * Vendored from the `filter-query-editor` package (formerly Ivy-Query-Editor,
 * v2.2.0). Ported into Ivy-Framework so it is no longer a separate npm
 * dependency. The ANTLR4 parser under `generated/` is checked in (the source
 * repo regenerated it at build time; this build does not run ANTLR).
 */

import "./styles.css";

// Component exports
export { QueryEditor } from "./components/QueryEditor";
export type { QueryEditorProps, QueryEditorChangeEvent } from "./components/types";

// Hooks exports
export { useDropdownState } from "./hooks/useDropdownState";
export type { DropdownState } from "./hooks/useDropdownState";

// Parser exports
export { parseQuery, parseQueryOrThrow } from "./parser/QueryParser";

// Formatter exports
export {
  formatQuery,
  formatQueryString,
  isCanonical,
  isIdempotent,
} from "./formatter/QueryFormatter";
export type { FormatResult } from "./formatter/QueryFormatter";

// Evaluator exports (utility functions for consumer use)
export {
  evaluateFilter,
  evaluateFilterBatch,
  countMatches,
  findFirstMatch,
} from "./evaluator/FilterEvaluator";

// Type exports
export type { FilterGroup, Filter, Condition } from "./types/filter";
export type { ColumnDef, ColumnType } from "./types/column";
export { DataType } from "./types/column";
export type { ParseResult, ParseError, ErrorSeverity } from "./types/parser";
