/**
 * Semantic validator for filter AST
 * Validates column existence, type compatibility, and argument types
 */

import { FilterGroup, Filter, Condition } from "../types/filter";
import { ColumnDef, ColumnType } from "../types/column";
import { ParseError } from "../types/parser";
import {
  isOperatorCompatible,
  isBlankOperatorCompatible,
  validateValueType,
  getOperatorDisplayName,
} from "./TypeChecker";

/**
 * Validation context with column definitions
 */
export interface ValidationContext {
  columns: ColumnDef[];
  errors: ParseError[];
}

/**
 * Semantic validator for filter expressions
 */
export class SemanticValidator {
  private columns: Map<string, ColumnDef>;
  private errors: ParseError[];

  constructor(columns: ColumnDef[]) {
    this.columns = new Map(columns.map((col) => [col.name, col]));
    this.errors = [];
  }

  /**
   * Validate a FilterGroup and return errors
   */
  validate(filterGroup: FilterGroup): ParseError[] {
    this.errors = [];
    this.validateFilterGroup(filterGroup);
    return this.errors;
  }

  /**
   * Validate a FilterGroup recursively
   */
  private validateFilterGroup(group: FilterGroup): void {
    for (const filter of group.filters) {
      this.validateFilter(filter);
    }
  }

  /**
   * Validate a single Filter
   */
  private validateFilter(filter: Filter): void {
    if (filter.condition) {
      this.validateCondition(filter.condition);
    } else if (filter.group) {
      this.validateFilterGroup(filter.group);
    }
  }

  /**
   * Validate a Condition
   */
  private validateCondition(condition: Condition): void {
    // Check if column exists
    const column = this.columns.get(condition.column);
    if (!column) {
      this.addError(
        `Column '${condition.column}' does not exist`,
        0, // Position will be set by parser
        0,
      );
      return; // Can't validate further without column definition
    }

    // Check operator compatibility
    if (condition.function === "isBlank" || condition.function === "isNotBlank") {
      if (!isBlankOperatorCompatible(column.type as ColumnType, condition.function)) {
        this.addError(
          `Operator '${getOperatorDisplayName(condition.function)}' is not compatible with type '${column.type}'`,
          0,
          0,
        );
      }
      // Blank operators don't have arguments
      return;
    }

    if (!isOperatorCompatible(column.type as ColumnType, condition.function)) {
      this.addError(
        `Operator '${getOperatorDisplayName(condition.function)}' is not compatible with type '${column.type}'`,
        0,
        0,
      );
      return;
    }

    // Validate argument types
    if (condition.args && condition.args.length > 0) {
      for (const arg of condition.args) {
        const validation = validateValueType(arg, column);
        if (!validation.valid) {
          this.addError(validation.error || "Invalid value type", 0, 0);
        }
      }
    } else {
      // Non-blank operators should have arguments
      // (we've already handled blank operators above)
      this.addError(
        `Operator '${getOperatorDisplayName(condition.function)}' requires a value`,
        0,
        0,
      );
    }
  }

  /**
   * Add an error to the collection
   */
  private addError(message: string, start: number, end: number): void {
    this.errors.push({
      message,
      start,
      end,
      severity: "error",
    });
  }
}

/**
 * Validate a filter group and return all semantic errors
 */
export function validateFilterGroup(filterGroup: FilterGroup, columns: ColumnDef[]): ParseError[] {
  const validator = new SemanticValidator(columns);
  return validator.validate(filterGroup);
}
