/**
 * AST Builder - Visitor implementation that builds FilterGroup AST from parse tree
 */

import { AbstractParseTreeVisitor } from "antlr4ng";
import { FilterGroup, Filter, Condition } from "../types/filter";
import { FiltersVisitor } from "../generated/FiltersVisitor";
import {
  FormulaContext,
  ExprContext,
  OrExprContext,
  AndExprContext,
  UnaryExprContext,
  PrimaryContext,
  GroupContext,
  ComparisonContext,
  TextOperationContext,
  ExistenceOperationContext,
  FieldRefContext,
  CompOpContext,
  TextOpContext,
  OperandContext,
  BooleanLiteralContext,
  NumberContext,
  StringLiteralContext,
} from "../generated/FiltersParser";

/**
 * Builds FilterGroup AST from ANTLR parse tree
 */
export class ASTBuilder extends AbstractParseTreeVisitor<any> implements FiltersVisitor<any> {
  /**
   * Entry point - visits the formula rule
   */
  visitFormula(ctx: FormulaContext): FilterGroup {
    const expr = ctx.expr();
    if (!expr) {
      // Empty formula
      return { op: "AND", filters: [] };
    }
    return this.visit(expr);
  }

  /**
   * Visit expression - delegates to orExpr
   */
  visitExpr(ctx: ExprContext): FilterGroup {
    return this.visit(ctx.orExpr());
  }

  /**
   * Visit OR expression - creates FilterGroup with OR operator
   */
  visitOrExpr(ctx: OrExprContext): FilterGroup {
    const andExprs = ctx.andExpr();

    if (andExprs.length === 1) {
      // Single AND expression, visit it directly
      return this.visit(andExprs[0]);
    }

    // Multiple AND expressions connected by OR
    const filters: Filter[] = [];
    for (const andExpr of andExprs) {
      const result = this.visit(andExpr);
      if (result.op && result.filters) {
        // It's a FilterGroup
        if (result.filters.length === 1) {
          filters.push(result.filters[0]);
        } else {
          filters.push({ group: result });
        }
      } else {
        // It's a Filter
        filters.push(result);
      }
    }

    return { op: "OR", filters };
  }

  /**
   * Visit AND expression - creates FilterGroup with AND operator
   */
  visitAndExpr(ctx: AndExprContext): FilterGroup {
    const unaryExprs = ctx.unaryExpr();

    if (unaryExprs.length === 1) {
      // Single unary expression
      const filter = this.visit(unaryExprs[0]);
      if (filter.op && filter.filters) {
        // It's already a FilterGroup
        return filter;
      }
      // Wrap single filter in AND group
      return { op: "AND", filters: [filter] };
    }

    // Multiple unary expressions connected by AND
    const filters: Filter[] = [];
    for (const unaryExpr of unaryExprs) {
      const filter = this.visit(unaryExpr);
      filters.push(filter);
    }

    return { op: "AND", filters };
  }

  /**
   * Visit unary expression - handles NOT operator
   */
  visitUnaryExpr(ctx: UnaryExprContext): Filter {
    const notToken = ctx.NOT();

    if (notToken) {
      // NOT expression
      const inner = ctx.unaryExpr();
      if (inner) {
        const filter = this.visit(inner);
        filter.negate = !filter.negate; // Toggle negation
        return filter;
      }
    }

    // No NOT, visit primary
    const primary = ctx.primary();
    if (primary) {
      return this.visit(primary);
    }

    throw new Error("Invalid unary expression");
  }

  /**
   * Visit primary expression - delegates to specific operation types
   */
  visitPrimary(ctx: PrimaryContext): Filter {
    // Check which type of primary it is
    const group = ctx.group();
    if (group) {
      return this.visit(group);
    }

    const comparison = ctx.comparison();
    if (comparison) {
      return this.visit(comparison);
    }

    const textOp = ctx.textOperation();
    if (textOp) {
      return this.visit(textOp);
    }

    const existenceOp = ctx.existenceOperation();
    if (existenceOp) {
      return this.visit(existenceOp);
    }

    throw new Error("Invalid primary expression");
  }

  /**
   * Visit group (parenthesized expression)
   */
  visitGroup(ctx: GroupContext): Filter {
    const expr = ctx.expr();
    const result = this.visit(expr);

    // If it's a FilterGroup, wrap it as a group filter
    if (result.op && result.filters) {
      return { group: result };
    }

    return result;
  }

  /**
   * Visit comparison operation
   */
  visitComparison(ctx: ComparisonContext): Filter {
    const fieldRef = ctx.fieldRef();
    const compOp = ctx.compOp();
    const operand = ctx.operand();

    const column = this.extractFieldName(fieldRef);
    const func = this.mapComparisonOperator(compOp);
    const value = this.extractOperandValue(operand);

    const condition: Condition = {
      column,
      function: func,
      args: [value],
    };

    // Check if this is a negated operator (!=, not equals)
    const shouldNegate =
      compOp.NEQ() !== null || compOp.getText().toLowerCase().includes("notequal");

    return {
      condition,
      negate: shouldNegate || undefined,
    };
  }

  /**
   * Visit text operation (contains, starts with, ends with)
   */
  visitTextOperation(ctx: TextOperationContext): Filter {
    const fieldRef = ctx.fieldRef();
    const textOp = ctx.textOp();
    const stringLiteral = ctx.stringLiteral();
    const notToken = ctx.NOT();

    const column = this.extractFieldName(fieldRef);
    const func = this.mapTextOperator(textOp);
    const value = this.extractStringValue(stringLiteral);

    const condition: Condition = {
      column,
      function: func,
      args: [value],
    };

    return {
      condition,
      negate: notToken !== null,
    };
  }

  /**
   * Visit existence operation (IS BLANK, IS NOT BLANK)
   */
  visitExistenceOperation(ctx: ExistenceOperationContext): Filter {
    const fieldRef = ctx.fieldRef();
    const notToken = ctx.NOT();

    const column = this.extractFieldName(fieldRef);

    const condition: Condition = {
      column,
      function: notToken ? "isNotBlank" : "isBlank",
      args: [],
    };

    return { condition };
  }

  /**
   * Extract field name from field reference
   */
  private extractFieldName(ctx: FieldRefContext): string {
    const fieldToken = ctx.FIELD();
    const text = fieldToken.getText();
    // Remove brackets: [fieldName] -> fieldName
    return text.substring(1, text.length - 1);
  }

  /**
   * Map comparison operator to function name
   */
  private mapComparisonOperator(ctx: CompOpContext): Condition["function"] {
    // Check symbolic operators first
    if (ctx.EQ() || ctx.EQ2()) return "equals";
    if (ctx.GT()) return "greaterThan";
    if (ctx.LT()) return "lessThan";
    if (ctx.NEQ()) return "equals"; // Will need to negate
    if (ctx.GE()) return "greaterThanOrEqual";
    if (ctx.LE()) return "lessThanOrEqual";

    // Check word operators
    if (ctx.EQUALS()) return "equals";

    // Multi-word operators need special handling
    const text = ctx.getText().toLowerCase();
    if (text === "notequals" || text === "notequal") {
      return "equals"; // Will be negated
    }
    if (text.includes("greater")) {
      if (text.includes("equal")) {
        return "greaterThanOrEqual";
      }
      return "greaterThan";
    }
    if (text.includes("less")) {
      if (text.includes("equal")) {
        return "lessThanOrEqual";
      }
      return "lessThan";
    }

    return "equals"; // Default
  }

  /**
   * Map text operator to function name
   */
  private mapTextOperator(ctx: TextOpContext): Condition["function"] {
    if (ctx.CONTAINS()) return "contains";

    const text = ctx.getText().toLowerCase().replace(/\s+/g, "");
    if (text === "startswith") return "startsWith";
    if (text === "endswith") return "endsWith";

    return "contains"; // Default
  }

  /**
   * Extract value from operand
   */
  private extractOperandValue(ctx: OperandContext): unknown {
    const number = ctx.number();
    if (number) {
      return this.extractNumberValue(number);
    }

    const stringLiteral = ctx.stringLiteral();
    if (stringLiteral) {
      return this.extractStringValue(stringLiteral);
    }

    const booleanLiteral = ctx.booleanLiteral();
    if (booleanLiteral) {
      return this.extractBooleanValue(booleanLiteral);
    }

    return null;
  }

  /**
   * Extract number value
   */
  private extractNumberValue(ctx: NumberContext): number {
    const sign = ctx.SIGN();
    const digits = ctx.DIGITS();
    const dot = ctx.DOT();

    let value = digits[0].getText();
    if (dot && digits[1]) {
      value += "." + digits[1].getText();
    }

    const num = parseFloat(value);
    return sign && sign.getText() === "-" ? -num : num;
  }

  /**
   * Extract string value
   */
  private extractStringValue(ctx: StringLiteralContext): string {
    const text = ctx.STRING().getText();
    // Remove quotes and handle escape sequences
    const unquoted = text.substring(1, text.length - 1);
    // Replace escaped quotes and backslashes
    return unquoted.replace(/\\"/g, '"').replace(/\\'/g, "'").replace(/\\\\/g, "\\");
  }

  /**
   * Extract boolean value
   */
  private extractBooleanValue(ctx: BooleanLiteralContext): boolean {
    return ctx.TRUE() !== null;
  }
}
