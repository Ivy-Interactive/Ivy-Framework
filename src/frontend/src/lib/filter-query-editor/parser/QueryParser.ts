/**
 * Main query parser that orchestrates lexing, parsing, and validation
 */

import { CharStream, CommonTokenStream } from "antlr4ng";
import { FiltersLexer } from "../generated/FiltersLexer";
import { FiltersParser } from "../generated/FiltersParser";
import { ASTBuilder } from "./ASTBuilder";
import { ErrorCollector } from "./ErrorCollector";
import { validateFilterGroup } from "../validator/SemanticValidator";
import { FilterGroup } from "../types/filter";
import { ColumnDef } from "../types/column";
import { ParseResult } from "../types/parser";

/**
 * Parse a query string into a FilterGroup AST.
 * Performs syntax and semantic validation.
 *
 * @param query - The query string to parse
 * @param columns - Schema of available columns for validation
 * @returns ParseResult with either filters or errors
 */
export function parseQuery(query: string, columns: ColumnDef[]): ParseResult {
  // Handle empty or whitespace-only input
  if (!query || query.trim().length === 0) {
    return {
      filters: { op: "AND", filters: [] },
    };
  }

  // Create error collector
  const errorCollector = new ErrorCollector();

  try {
    // Step 1: Create lexer
    const charStream = CharStream.fromString(query);
    const lexer = new FiltersLexer(charStream);

    // Remove default error listeners and add our custom one
    lexer.removeErrorListeners();
    lexer.addErrorListener(errorCollector);

    // Step 2: Create token stream
    const tokenStream = new CommonTokenStream(lexer);

    // Step 3: Create parser
    const parser = new FiltersParser(tokenStream);

    // Remove default error listeners and add our custom one
    parser.removeErrorListeners();
    parser.addErrorListener(errorCollector);

    // Step 4: Parse the query
    const tree = parser.formula();

    // Check for syntax errors
    const syntaxErrors = errorCollector.getErrors();
    if (syntaxErrors.length > 0) {
      return { errors: syntaxErrors };
    }

    // Step 5: Build AST using visitor
    const astBuilder = new ASTBuilder();
    const filterGroup = astBuilder.visit(tree) as FilterGroup;

    // Step 6: Perform semantic validation
    const semanticErrors = validateFilterGroup(filterGroup, columns);

    // Check for semantic errors
    if (semanticErrors.length > 0) {
      // Update error positions if needed (semantic validator returns 0,0 positions)
      const updatedErrors = semanticErrors.map((error) => {
        // In a real implementation, we'd track token positions during AST building
        // For now, we'll use the provided positions
        return error;
      });
      return { errors: updatedErrors };
    }

    // Success - return the filter group
    return { filters: filterGroup };
  } catch (error) {
    // Unexpected error during parsing
    const message = error instanceof Error ? error.message : "Unknown parsing error";
    return {
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
 * Parse a query and throw if there are errors (for testing/debugging)
 */
export function parseQueryOrThrow(query: string, columns: ColumnDef[]): FilterGroup {
  const result = parseQuery(query, columns);

  if (result.errors) {
    const errorMessages = result.errors.map((e) => e.message).join("; ");
    throw new Error(`Parse errors: ${errorMessages}`);
  }

  if (!result.filters) {
    throw new Error("No filters returned from parser");
  }

  return result.filters;
}
