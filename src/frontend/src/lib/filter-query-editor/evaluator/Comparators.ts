/**
 * Type-safe comparison functions for filter evaluation
 * Implements all operators with strict type checking
 */

import { ColumnType } from "../types/column";

/**
 * Operator function signature
 */
export type OperatorFunction = (
  columnValue: unknown,
  args: unknown[],
  columnType: ColumnType,
) => boolean;

/**
 * Check if a value is null or undefined
 */
export function isNullish(value: unknown): boolean {
  return value === null || value === undefined;
}

/**
 * Equals operator - strict equality with no type coercion
 */
export const equals: OperatorFunction = (columnValue, args, _columnType) => {
  if (args.length === 0) return false;
  const compareValue = args[0];

  // Handle null/undefined specially
  if (isNullish(columnValue) && isNullish(compareValue)) {
    return columnValue === compareValue; // null === null, undefined === undefined
  }
  if (isNullish(columnValue) || isNullish(compareValue)) {
    return false;
  }

  // Strict equality - no type coercion
  return columnValue === compareValue;
};

/**
 * Greater than operator - works for numbers and dates
 */
export const greaterThan: OperatorFunction = (columnValue, args, columnType) => {
  if (args.length === 0) return false;
  const compareValue = args[0];

  // Null/undefined always returns false
  if (isNullish(columnValue) || isNullish(compareValue)) {
    return false;
  }

  switch (columnType) {
    case "number":
      if (typeof columnValue !== "number" || typeof compareValue !== "number") {
        return false;
      }
      return columnValue > compareValue;

    case "date":
      // For dates, we expect ISO strings
      if (typeof columnValue !== "string" || typeof compareValue !== "string") {
        return false;
      }
      try {
        const date1 = new Date(columnValue);
        const date2 = new Date(compareValue);
        // Check for valid dates
        if (isNaN(date1.getTime()) || isNaN(date2.getTime())) {
          return false;
        }
        return date1 > date2;
      } catch {
        return false;
      }

    default:
      // Operator not supported for this type
      return false;
  }
};

/**
 * Less than operator - works for numbers and dates
 */
export const lessThan: OperatorFunction = (columnValue, args, columnType) => {
  if (args.length === 0) return false;
  const compareValue = args[0];

  // Null/undefined always returns false
  if (isNullish(columnValue) || isNullish(compareValue)) {
    return false;
  }

  switch (columnType) {
    case "number":
      if (typeof columnValue !== "number" || typeof compareValue !== "number") {
        return false;
      }
      return columnValue < compareValue;

    case "date":
      // For dates, we expect ISO strings
      if (typeof columnValue !== "string" || typeof compareValue !== "string") {
        return false;
      }
      try {
        const date1 = new Date(columnValue);
        const date2 = new Date(compareValue);
        // Check for valid dates
        if (isNaN(date1.getTime()) || isNaN(date2.getTime())) {
          return false;
        }
        return date1 < date2;
      } catch {
        return false;
      }

    default:
      // Operator not supported for this type
      return false;
  }
};

/**
 * Greater than or equal operator - works for numbers and dates
 */
export const greaterThanOrEqual: OperatorFunction = (columnValue, args, columnType) => {
  if (args.length === 0) return false;
  const compareValue = args[0];

  // Null/undefined always returns false
  if (isNullish(columnValue) || isNullish(compareValue)) {
    return false;
  }

  switch (columnType) {
    case "number":
      if (typeof columnValue !== "number" || typeof compareValue !== "number") {
        return false;
      }
      return columnValue >= compareValue;

    case "date":
      // For dates, we expect ISO strings
      if (typeof columnValue !== "string" || typeof compareValue !== "string") {
        return false;
      }
      try {
        const date1 = new Date(columnValue);
        const date2 = new Date(compareValue);
        // Check for valid dates
        if (isNaN(date1.getTime()) || isNaN(date2.getTime())) {
          return false;
        }
        return date1 >= date2;
      } catch {
        return false;
      }

    default:
      // Operator not supported for this type
      return false;
  }
};

/**
 * Less than or equal operator - works for numbers and dates
 */
export const lessThanOrEqual: OperatorFunction = (columnValue, args, columnType) => {
  if (args.length === 0) return false;
  const compareValue = args[0];

  // Null/undefined always returns false
  if (isNullish(columnValue) || isNullish(compareValue)) {
    return false;
  }

  switch (columnType) {
    case "number":
      if (typeof columnValue !== "number" || typeof compareValue !== "number") {
        return false;
      }
      return columnValue <= compareValue;

    case "date":
      // For dates, we expect ISO strings
      if (typeof columnValue !== "string" || typeof compareValue !== "string") {
        return false;
      }
      try {
        const date1 = new Date(columnValue);
        const date2 = new Date(compareValue);
        // Check for valid dates
        if (isNaN(date1.getTime()) || isNaN(date2.getTime())) {
          return false;
        }
        return date1 <= date2;
      } catch {
        return false;
      }

    default:
      // Operator not supported for this type
      return false;
  }
};

/**
 * Contains operator - case-sensitive substring match for strings
 */
export const contains: OperatorFunction = (columnValue, args, columnType) => {
  if (args.length === 0) return false;
  const searchValue = args[0];

  // Only works with strings
  if (columnType !== "string") {
    return false;
  }

  // Null/undefined always returns false
  if (isNullish(columnValue) || isNullish(searchValue)) {
    return false;
  }

  // Both must be strings
  if (typeof columnValue !== "string" || typeof searchValue !== "string") {
    return false;
  }

  // Case-sensitive substring search
  return columnValue.includes(searchValue);
};

/**
 * Starts with operator - case-sensitive prefix match for strings
 */
export const startsWith: OperatorFunction = (columnValue, args, columnType) => {
  if (args.length === 0) return false;
  const searchValue = args[0];

  // Only works with strings
  if (columnType !== "string") {
    return false;
  }

  // Null/undefined always returns false
  if (isNullish(columnValue) || isNullish(searchValue)) {
    return false;
  }

  // Both must be strings
  if (typeof columnValue !== "string" || typeof searchValue !== "string") {
    return false;
  }

  // Case-sensitive prefix search
  return columnValue.startsWith(searchValue);
};

/**
 * Ends with operator - case-sensitive suffix match for strings
 */
export const endsWith: OperatorFunction = (columnValue, args, columnType) => {
  if (args.length === 0) return false;
  const searchValue = args[0];

  // Only works with strings
  if (columnType !== "string") {
    return false;
  }

  // Null/undefined always returns false
  if (isNullish(columnValue) || isNullish(searchValue)) {
    return false;
  }

  // Both must be strings
  if (typeof columnValue !== "string" || typeof searchValue !== "string") {
    return false;
  }

  // Case-sensitive suffix search
  return columnValue.endsWith(searchValue);
};

/**
 * Is blank operator - checks for null, undefined, or empty string
 */
export const isBlank: OperatorFunction = (columnValue, _args, columnType) => {
  // isBlank doesn't use args
  if (isNullish(columnValue)) {
    return true;
  }

  // For strings, also check for empty string
  if (columnType === "string" && columnValue === "") {
    return true;
  }

  return false;
};

/**
 * Is not blank operator - checks for non-null, non-undefined, non-empty values
 */
export const isNotBlank: OperatorFunction = (columnValue, args, columnType) => {
  // Simply the inverse of isBlank
  return !isBlank(columnValue, args, columnType);
};

/**
 * Map of operator names to functions
 */
export const operators: Record<string, OperatorFunction> = {
  equals,
  greaterThan,
  lessThan,
  greaterThanOrEqual,
  lessThanOrEqual,
  contains,
  startsWith,
  endsWith,
  isBlank,
  isNotBlank,
};

/**
 * Get an operator function by name
 */
export function getOperator(name: string): OperatorFunction | undefined {
  return operators[name];
}

/**
 * Check if an operator is supported
 */
export function isOperatorSupported(name: string): boolean {
  return name in operators;
}
