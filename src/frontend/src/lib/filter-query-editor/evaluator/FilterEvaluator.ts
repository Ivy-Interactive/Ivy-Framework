/**
 * Filter Evaluator - Evaluates FilterGroup AST against row data
 * This is a utility function exported for consumer use.
 * The query editor does not perform filtering itself.
 */

import { FilterGroup, Filter, Condition } from "../types/filter";
import { ColumnDef, ColumnType } from "../types/column";
import { getOperator } from "./Comparators";

/**
 * Maximum recursion depth to prevent stack overflow
 */
const MAX_RECURSION_DEPTH = 100;

/**
 * Evaluate a FilterGroup against a row object.
 * Returns true if row matches filter, false otherwise.
 *
 * **Usage Note:** This is a utility function exported for consumer use.
 * The query editor does not perform filtering itself. Consumers import
 * this function to filter their own datasets.
 *
 * @example
 * ```typescript
 * import { evaluateFilter } from '@/lib/filter-query-editor';
 *
 * const filteredRows = rows.filter(row =>
 *   evaluateFilter(filterGroup, row, columns)
 * );
 * ```
 *
 * @param filter - The FilterGroup AST to evaluate
 * @param row - The row data to test
 * @param columns - Schema of columns for type information
 * @returns boolean indicating match
 */
export function evaluateFilter(
  filter: FilterGroup,
  row: Record<string, unknown>,
  columns: ColumnDef[],
): boolean {
  try {
    // Create column map for quick lookup
    const columnMap = new Map(columns.map((col) => [col.name, col]));

    // Evaluate with depth tracking
    return evaluateFilterGroup(filter, row, columnMap, 0);
  } catch (error) {
    // Return false for any errors (safe default)
    console.warn("Error evaluating filter:", error);
    return false;
  }
}

/**
 * Evaluate a FilterGroup recursively
 */
function evaluateFilterGroup(
  group: FilterGroup,
  row: Record<string, unknown>,
  columnMap: Map<string, ColumnDef>,
  depth: number,
): boolean {
  // Check recursion depth
  if (depth > MAX_RECURSION_DEPTH) {
    throw new Error(`Maximum recursion depth (${MAX_RECURSION_DEPTH}) exceeded`);
  }

  // Empty filter group returns true (vacuous truth)
  if (!group.filters || group.filters.length === 0) {
    return true;
  }

  // Evaluate based on operator
  if (group.op === "AND") {
    // All filters must be true
    return group.filters.every((filter) => evaluateSingleFilter(filter, row, columnMap, depth + 1));
  } else if (group.op === "OR") {
    // At least one filter must be true
    return group.filters.some((filter) => evaluateSingleFilter(filter, row, columnMap, depth + 1));
  }

  // Unknown operator - return false
  return false;
}

/**
 * Evaluate a single Filter
 */
function evaluateSingleFilter(
  filter: Filter,
  row: Record<string, unknown>,
  columnMap: Map<string, ColumnDef>,
  depth: number,
): boolean {
  let result = false;

  if (filter.condition) {
    // Evaluate the condition
    result = evaluateCondition(filter.condition, row, columnMap);
  } else if (filter.group) {
    // Recursively evaluate nested group
    result = evaluateFilterGroup(filter.group, row, columnMap, depth);
  } else {
    // Invalid filter structure
    return false;
  }

  // Apply negation if present
  if (filter.negate) {
    result = !result;
  }

  return result;
}

/**
 * Evaluate a Condition
 */
function evaluateCondition(
  condition: Condition,
  row: Record<string, unknown>,
  columnMap: Map<string, ColumnDef>,
): boolean {
  // Get column definition
  const column = columnMap.get(condition.column);
  if (!column) {
    // Unknown column - return false
    return false;
  }

  // Get the operator function
  const operatorFunc = getOperator(condition.function);
  if (!operatorFunc) {
    // Unknown operator - return false
    return false;
  }

  // Get the column value from the row
  const columnValue = row[condition.column];

  // Call the operator function
  return operatorFunc(columnValue, condition.args || [], column.type as ColumnType);
}

/**
 * Evaluate multiple rows in batch
 * Useful for filtering large datasets
 *
 * @param filter - The FilterGroup to evaluate
 * @param rows - Array of row objects
 * @param columns - Column definitions
 * @returns Array of rows that match the filter
 */
export function evaluateFilterBatch<T extends Record<string, unknown>>(
  filter: FilterGroup,
  rows: T[],
  columns: ColumnDef[],
): T[] {
  return rows.filter((row) => evaluateFilter(filter, row, columns));
}

/**
 * Count matching rows without creating a new array
 * More memory efficient for large datasets
 *
 * @param filter - The FilterGroup to evaluate
 * @param rows - Array of row objects
 * @param columns - Column definitions
 * @returns Number of matching rows
 */
export function countMatches(
  filter: FilterGroup,
  rows: Record<string, unknown>[],
  columns: ColumnDef[],
): number {
  let count = 0;
  for (const row of rows) {
    if (evaluateFilter(filter, row, columns)) {
      count++;
    }
  }
  return count;
}

/**
 * Get the first matching row
 * Useful for finding a single match efficiently
 *
 * @param filter - The FilterGroup to evaluate
 * @param rows - Array of row objects
 * @param columns - Column definitions
 * @returns First matching row or undefined
 */
export function findFirstMatch<T extends Record<string, unknown>>(
  filter: FilterGroup,
  rows: T[],
  columns: ColumnDef[],
): T | undefined {
  for (const row of rows) {
    if (evaluateFilter(filter, row, columns)) {
      return row;
    }
  }
  return undefined;
}
