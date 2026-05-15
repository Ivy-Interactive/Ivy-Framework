/**
 * AST Printer - Converts FilterGroup AST back to query string
 * This is the foundation for the query formatter
 */

import { FilterGroup, Filter, Condition } from "../types/filter";

/**
 * Options for controlling how the AST is printed
 */
export interface PrintOptions {
  bracketColumns?: boolean; // Wrap column names in brackets
  uppercaseLogicalOps?: boolean; // Uppercase AND/OR/NOT
  spaceBinaryOps?: boolean; // Add spaces around operators
  doubleQuoteStrings?: boolean; // Use double quotes for strings
  normalizeParens?: boolean; // Add parentheses for NOT
  normalizeOperatorSynonyms?: boolean; // Convert synonyms to canonical form
}

/**
 * Default options for canonical formatting
 */
export const canonicalOptions: PrintOptions = {
  bracketColumns: true,
  uppercaseLogicalOps: true,
  spaceBinaryOps: true,
  doubleQuoteStrings: true,
  normalizeParens: true,
  normalizeOperatorSynonyms: true,
};

/**
 * Converts a FilterGroup AST back to a query string
 */
export class ASTPrinter {
  private options: PrintOptions;

  constructor(options: PrintOptions = canonicalOptions) {
    this.options = { ...canonicalOptions, ...options };
  }

  /**
   * Print a FilterGroup to a query string
   */
  print(filterGroup: FilterGroup): string {
    if (!filterGroup || filterGroup.filters.length === 0) {
      return "";
    }

    return this.printFilterGroup(filterGroup, false);
  }

  /**
   * Print a FilterGroup with proper operator spacing
   */
  private printFilterGroup(group: FilterGroup, isNested: boolean): string {
    if (group.filters.length === 0) {
      return "";
    }

    const operator = this.options.uppercaseLogicalOps
      ? group.op.toUpperCase()
      : group.op.toLowerCase();

    const parts = group.filters.map((filter) => this.printFilter(filter));
    const joined = parts.join(` ${operator} `);

    // Add parentheses for nested groups
    return isNested ? `(${joined})` : joined;
  }

  /**
   * Print a single Filter
   */
  private printFilter(filter: Filter): string {
    let result = "";

    if (filter.condition) {
      result = this.printCondition(filter.condition);
    } else if (filter.group) {
      result = this.printFilterGroup(filter.group, true);
    }

    // Handle negation
    if (filter.negate) {
      const notKeyword = this.options.uppercaseLogicalOps ? "NOT" : "not";
      if (this.options.normalizeParens && !result.startsWith("(")) {
        result = `(${result})`;
      }
      result = `${notKeyword} ${result}`;
    }

    return result;
  }

  /**
   * Print a Condition
   */
  private printCondition(condition: Condition): string {
    const column = this.printColumn(condition.column);
    const operator = this.printOperator(condition.function);

    // Handle blank operators (no arguments)
    if (condition.function === "isBlank") {
      const isKeyword = this.options.uppercaseLogicalOps ? "IS" : "is";
      const blankKeyword = this.options.uppercaseLogicalOps ? "BLANK" : "blank";
      return `${column} ${isKeyword} ${blankKeyword}`;
    }

    if (condition.function === "isNotBlank") {
      const isKeyword = this.options.uppercaseLogicalOps ? "IS" : "is";
      const notKeyword = this.options.uppercaseLogicalOps ? "NOT" : "not";
      const blankKeyword = this.options.uppercaseLogicalOps ? "BLANK" : "blank";
      return `${column} ${isKeyword} ${notKeyword} ${blankKeyword}`;
    }

    // Handle regular operators with arguments
    if (!condition.args || condition.args.length === 0) {
      return `${column} ${operator}`;
    }

    const value = this.printValue(condition.args[0]);

    if (this.options.spaceBinaryOps) {
      return `${column} ${operator} ${value}`;
    } else {
      // Special case for symbolic operators without spaces
      if ([">", "<", "=", "==", "!="].includes(operator)) {
        return `${column}${operator}${value}`;
      }
      return `${column} ${operator} ${value}`;
    }
  }

  /**
   * Print a column name
   */
  private printColumn(columnName: string): string {
    if (this.options.bracketColumns && !columnName.startsWith("[")) {
      return `[${columnName}]`;
    }
    return columnName;
  }

  /**
   * Print an operator
   */
  private printOperator(func: Condition["function"]): string {
    if (!this.options.normalizeOperatorSynonyms) {
      // Keep original operator names
      return func;
    }

    // Normalize to canonical form
    switch (func) {
      case "equals":
        return "equals";
      case "greaterThan":
        return ">";
      case "lessThan":
        return "<";
      case "greaterThanOrEqual":
        return ">=";
      case "lessThanOrEqual":
        return "<=";
      case "contains":
        return "contains";
      case "startsWith":
        return this.options.uppercaseLogicalOps ? "STARTS WITH" : "starts with";
      case "endsWith":
        return this.options.uppercaseLogicalOps ? "ENDS WITH" : "ends with";
      default:
        return func;
    }
  }

  /**
   * Print a value (string, number, boolean, etc.)
   */
  private printValue(value: unknown): string {
    if (value === null) {
      return "null";
    }

    if (value === undefined) {
      return "undefined";
    }

    if (typeof value === "string") {
      if (this.options.doubleQuoteStrings) {
        // Escape double quotes and use double quotes
        const escaped = value.replace(/\\/g, "\\\\").replace(/"/g, '\\"');
        return `"${escaped}"`;
      } else {
        // Use single quotes
        const escaped = value.replace(/\\/g, "\\\\").replace(/'/g, "\\'");
        return `'${escaped}'`;
      }
    }

    if (typeof value === "boolean") {
      return value ? "true" : "false";
    }

    if (typeof value === "number") {
      return String(value);
    }

    // For dates and other types, convert to string
    return String(value);
  }
}

/**
 * Helper function to print a FilterGroup with canonical options
 */
export function printFilterGroup(filterGroup: FilterGroup): string {
  const printer = new ASTPrinter(canonicalOptions);
  return printer.print(filterGroup);
}
