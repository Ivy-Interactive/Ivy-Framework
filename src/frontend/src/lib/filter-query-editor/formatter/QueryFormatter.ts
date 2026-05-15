/**
 * Query Formatter - Transforms queries to canonical form
 * Ensures idempotent formatting with consistent rules
 */

import { parseQuery } from "../parser/QueryParser";
import { ASTPrinter, canonicalOptions } from "./ASTPrinter";
import { ColumnDef } from "../types/column";
import { ParseError } from "../types/parser";

/**
 * Result of formatting a query
 */
export interface FormatResult {
  /** The formatted query string */
  formatted: string;
  /** Errors if the query couldn't be parsed */
  errors?: ParseError[];
}

/**
 * Format a query string to canonical form.
 * Returns original query + errors if unparseable.
 *
 * Formatting rules:
 * 1. Bracket columns: status → [status]
 * 2. Uppercase logical operators: and → AND
 * 3. Space binary operators: [price]>100 → [price] > 100
 * 4. Double-quote strings: 'value' → "value"
 * 5. Normalize parentheses: not [x] → NOT ([x])
 * 6. Normalize operator synonyms: greaterThan → >
 *
 * @param query - The query string to format
 * @param columns - Schema of available columns for validation
 * @returns FormatResult with formatted string or errors
 */
export function formatQuery(query: string, columns: ColumnDef[]): FormatResult {
  // Handle empty or whitespace-only input
  if (!query || query.trim().length === 0) {
    return { formatted: "" };
  }

  try {
    // Step 1: Parse the query to get AST
    const parseResult = parseQuery(query, columns);

    // If there are parse errors, return original query with errors
    if (parseResult.errors && parseResult.errors.length > 0) {
      return {
        formatted: query,
        errors: parseResult.errors,
      };
    }

    // If no filters were parsed (shouldn't happen), return empty
    if (!parseResult.filters) {
      return { formatted: "" };
    }

    // Step 2: Print the AST back to string with canonical formatting
    const printer = new ASTPrinter(canonicalOptions);
    const formatted = printer.print(parseResult.filters);

    return { formatted };
  } catch (error) {
    // If any unexpected error occurs, return original with error
    const message = error instanceof Error ? error.message : "Unknown formatting error";
    return {
      formatted: query,
      errors: [
        {
          message,
          start: 0,
          end: query.length,
          severity: "error",
        },
      ],
    };
  }
}

/**
 * Check if a query is already in canonical form
 *
 * @param query - The query to check
 * @param columns - Column definitions
 * @returns true if the query is already canonical
 */
export function isCanonical(query: string, columns: ColumnDef[]): boolean {
  const result = formatQuery(query, columns);
  return !result.errors && result.formatted === query;
}

/**
 * Format a query multiple times to verify idempotence
 * Useful for testing
 *
 * @param query - The query to format
 * @param columns - Column definitions
 * @param iterations - Number of times to format (default 2)
 * @returns Array of formatted results
 */
export function formatQueryMultiple(
  query: string,
  columns: ColumnDef[],
  iterations: number = 2,
): string[] {
  const results: string[] = [];
  let current = query;

  for (let i = 0; i < iterations; i++) {
    const result = formatQuery(current, columns);
    if (result.errors) {
      // If there are errors, stop formatting
      results.push(current);
      break;
    }
    current = result.formatted;
    results.push(current);
  }

  return results;
}

/**
 * Verify that formatting is idempotent
 *
 * @param query - The query to test
 * @param columns - Column definitions
 * @returns true if formatting is idempotent
 */
export function isIdempotent(query: string, columns: ColumnDef[]): boolean {
  const results = formatQueryMultiple(query, columns, 2);

  // If we got less than 2 results, there was an error
  if (results.length < 2) {
    return false;
  }

  // Check if first and second formatting are identical
  return results[0] === results[1];
}

/**
 * Simple wrapper for formatQuery that returns just the formatted string.
 * Throws an error if the query cannot be formatted.
 *
 * @param query - The query string to format
 * @param columns - Schema of available columns for validation
 * @returns The formatted string
 * @throws Error if the query cannot be formatted
 */
export function formatQueryString(query: string, columns: ColumnDef[]): string {
  const result = formatQuery(query, columns);
  if (result.errors && result.errors.length > 0) {
    throw new Error(result.errors[0].message);
  }
  return result.formatted;
}
