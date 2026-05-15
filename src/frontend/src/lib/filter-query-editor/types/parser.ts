/**
 * Parser result and error types
 */

import { FilterGroup } from "./filter";

/**
 * Error severity levels
 */
export type ErrorSeverity = "error" | "warning";

/**
 * Parse error with position information
 */
export interface ParseError {
  /** Human-readable error message */
  message: string;
  /** Start position in the input (0-indexed) */
  start: number;
  /** End position in the input (exclusive) */
  end: number;
  /** Error severity */
  severity?: ErrorSeverity;
}

/**
 * Result of parsing a query string
 * Contains either a FilterGroup AST or a list of errors
 */
export interface ParseResult {
  /** Successfully parsed filter group */
  filters?: FilterGroup;
  /** List of parse/validation errors */
  errors?: ParseError[];
}
