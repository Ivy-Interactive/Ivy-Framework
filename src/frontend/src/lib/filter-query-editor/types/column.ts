/**
 * Column definition types for schema validation
 */

/**
 * Supported column data types
 */
export type ColumnType = "string" | "number" | "boolean" | "date" | "enum";

/**
 * Data type enum for backward compatibility
 */
export enum DataType {
  STRING = "string",
  NUMBER = "number",
  BOOLEAN = "boolean",
  DATE = "date",
  ENUM = "enum",
}

/**
 * Column definition for schema validation
 */
export interface ColumnDef {
  name: string;
  type: string;
  width: number;
}
