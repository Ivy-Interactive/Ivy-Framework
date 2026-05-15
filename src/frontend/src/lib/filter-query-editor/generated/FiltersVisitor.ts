// @ts-nocheck
/* eslint-disable */

import { AbstractParseTreeVisitor } from "antlr4ng";

import { FormulaContext } from "./FiltersParser.js";
import { ExprContext } from "./FiltersParser.js";
import { OrExprContext } from "./FiltersParser.js";
import { AndExprContext } from "./FiltersParser.js";
import { UnaryExprContext } from "./FiltersParser.js";
import { PrimaryContext } from "./FiltersParser.js";
import { GroupContext } from "./FiltersParser.js";
import { ComparisonContext } from "./FiltersParser.js";
import { TextOperationContext } from "./FiltersParser.js";
import { ExistenceOperationContext } from "./FiltersParser.js";
import { FieldRefContext } from "./FiltersParser.js";
import { FieldIdentifierContext } from "./FiltersParser.js";
import { CompOpContext } from "./FiltersParser.js";
import { TextOpContext } from "./FiltersParser.js";
import { OperandContext } from "./FiltersParser.js";
import { BooleanLiteralContext } from "./FiltersParser.js";
import { NumberContext } from "./FiltersParser.js";
import { StringLiteralContext } from "./FiltersParser.js";

/**
 * This interface defines a complete generic visitor for a parse tree produced
 * by `FiltersParser`.
 *
 * @param <Result> The return type of the visit operation. Use `void` for
 * operations with no return type.
 */
export class FiltersVisitor<Result> extends AbstractParseTreeVisitor<Result> {
  /**
   * Visit a parse tree produced by `FiltersParser.formula`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitFormula?: (ctx: FormulaContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.expr`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitExpr?: (ctx: ExprContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.orExpr`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitOrExpr?: (ctx: OrExprContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.andExpr`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitAndExpr?: (ctx: AndExprContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.unaryExpr`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitUnaryExpr?: (ctx: UnaryExprContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.primary`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitPrimary?: (ctx: PrimaryContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.group`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitGroup?: (ctx: GroupContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.comparison`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitComparison?: (ctx: ComparisonContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.textOperation`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitTextOperation?: (ctx: TextOperationContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.existenceOperation`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitExistenceOperation?: (ctx: ExistenceOperationContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.fieldRef`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitFieldRef?: (ctx: FieldRefContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.fieldIdentifier`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitFieldIdentifier?: (ctx: FieldIdentifierContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.compOp`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitCompOp?: (ctx: CompOpContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.textOp`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitTextOp?: (ctx: TextOpContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.operand`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitOperand?: (ctx: OperandContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.booleanLiteral`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitBooleanLiteral?: (ctx: BooleanLiteralContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.number`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitNumber?: (ctx: NumberContext) => Result;
  /**
   * Visit a parse tree produced by `FiltersParser.stringLiteral`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitStringLiteral?: (ctx: StringLiteralContext) => Result;
}
