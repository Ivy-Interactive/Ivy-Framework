/**
 * Type checking utilities for filter validation
 */

import { ColumnType, ColumnDef } from "../types/column";
import { Condition } from "../types/filter";

/**
 * Normalize .NET backend types to generic column types
 */
function normalizeColumnType(type: string): ColumnType {
  const upperType = type.toUpperCase();

  // Number types
  if (["INT32", "INT64", "DOUBLE", "DECIMAL", "NUMBER"].includes(upperType)) {
    return "number";
  }

  // String types
  if (["TEXT", "STRING", "ICON"].includes(upperType)) {
    return "string";
  }

  // Boolean type
  if (upperType === "BOOLEAN") {
    return "boolean";
  }

  // Date types
  if (["DATE", "DATETIME"].includes(upperType)) {
    return "date";
  }

  // If already a valid ColumnType, return as-is
  if (["string", "number", "boolean", "date", "enum"].includes(type)) {
    return type as ColumnType;
  }

  // Default to string for unknown types
  return "string";
}

/**
 * Check if an operator is compatible with a column type
 */
export function isOperatorCompatible(
  columnType: ColumnType,
  operator: Condition["function"],
): boolean {
  const normalizedType = normalizeColumnType(columnType);

  switch (normalizedType) {
    case "string":
      return ["equals", "contains", "startsWith", "endsWith"].includes(operator);

    case "number":
      return [
        "equals",
        "greaterThan",
        "lessThan",
        "greaterThanOrEqual",
        "lessThanOrEqual",
      ].includes(operator);

    case "boolean":
      return operator === "equals";

    case "date":
      return [
        "equals",
        "greaterThan",
        "lessThan",
        "greaterThanOrEqual",
        "lessThanOrEqual",
      ].includes(operator);

    case "enum":
      return operator === "equals";

    default:
      return false;
  }
}

/**
 * Check if blank operators are compatible with a column type
 */
export function isBlankOperatorCompatible(
  columnType: ColumnType,
  _operator: "isBlank" | "isNotBlank",
): boolean {
  const normalizedType = normalizeColumnType(columnType);
  // Blank operators can be used with string, date, and potentially other nullable types
  return ["string", "date", "enum"].includes(normalizedType);
}

/**
 * Validate value type against column type
 */
export function validateValueType(
  value: unknown,
  column: ColumnDef,
): { valid: boolean; error?: string } {
  const normalizedType = normalizeColumnType(column.type);

  switch (normalizedType) {
    case "string":
      if (typeof value !== "string") {
        return {
          valid: false,
          error: `Expected string for column '${column.name}', got ${typeof value}`,
        };
      }
      break;

    case "number":
      if (typeof value !== "number") {
        return {
          valid: false,
          error: `Expected number for column '${column.name}', got ${typeof value}`,
        };
      }
      break;

    case "boolean":
      if (typeof value !== "boolean") {
        return {
          valid: false,
          error: `Expected boolean for column '${column.name}', got ${typeof value}`,
        };
      }
      break;

    case "date":
      if (typeof value !== "string") {
        return {
          valid: false,
          error: `Expected date string for column '${column.name}', got ${typeof value}`,
        };
      }
      // Basic ISO date/datetime format check - allow both YYYY-MM-DD and ISO datetime
      const datePattern = /^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2}(\.\d{3})?Z?)?$/;
      if (!datePattern.test(value)) {
        return {
          valid: false,
          error: `Invalid date format for column '${column.name}'. Expected YYYY-MM-DD or ISO datetime`,
        };
      }
      break;

    case "enum":
      if (typeof value !== "string") {
        return {
          valid: false,
          error: `Expected string for enum column '${column.name}', got ${typeof value}`,
        };
      }
      // enumValues not available in ColumnDef - skipping enum validation
      break;
  }

  return { valid: true };
}

/**
 * Get human-readable operator name
 */
export function getOperatorDisplayName(operator: Condition["function"]): string {
  const displayNames: Record<Condition["function"], string> = {
    equals: "equals",
    greaterThan: "greater than",
    lessThan: "less than",
    greaterThanOrEqual: "greater than or equal",
    lessThanOrEqual: "less than or equal",
    contains: "contains",
    startsWith: "starts with",
    endsWith: "ends with",
    isBlank: "is blank",
    isNotBlank: "is not blank",
  };

  return displayNames[operator] || operator;
}
