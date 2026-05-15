/**
 * Filter model types for the AST representation
 */

/**
 * A condition represents a single filter expression
 * e.g., [status] equals "open"
 */
export interface Condition {
  /** The column identifier */
  column: string;
  /** The comparison or text operation function */
  function:
    | "equals"
    | "greaterThan"
    | "lessThan"
    | "greaterThanOrEqual"
    | "lessThanOrEqual"
    | "contains"
    | "startsWith"
    | "endsWith"
    | "isBlank"
    | "isNotBlank";
  /** Arguments for the function (values to compare against) */
  args: unknown[];
}

/**
 * A filter group represents a collection of filters combined with AND/OR
 */
export interface FilterGroup {
  /** Logical operator to combine filters */
  op: "AND" | "OR";
  /** Array of filters in this group */
  filters: Filter[];
}

/**
 * A filter can be either a condition, a nested group, or have a NOT modifier
 */
export interface Filter {
  /** A single condition */
  condition?: Condition;
  /** A nested filter group */
  group?: FilterGroup;
  /** NOT modifier - negates the filter */
  negate?: boolean;
}
