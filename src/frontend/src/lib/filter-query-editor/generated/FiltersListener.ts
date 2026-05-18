// @ts-nocheck
/* eslint-disable */

import { ErrorNode, ParseTreeListener, ParserRuleContext, TerminalNode } from "antlr4ng";

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
 * This interface defines a complete listener for a parse tree produced by
 * `FiltersParser`.
 */
export class FiltersListener implements ParseTreeListener {
  /**
   * Enter a parse tree produced by `FiltersParser.formula`.
   * @param ctx the parse tree
   */
  enterFormula?: (ctx: FormulaContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.formula`.
   * @param ctx the parse tree
   */
  exitFormula?: (ctx: FormulaContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.expr`.
   * @param ctx the parse tree
   */
  enterExpr?: (ctx: ExprContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.expr`.
   * @param ctx the parse tree
   */
  exitExpr?: (ctx: ExprContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.orExpr`.
   * @param ctx the parse tree
   */
  enterOrExpr?: (ctx: OrExprContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.orExpr`.
   * @param ctx the parse tree
   */
  exitOrExpr?: (ctx: OrExprContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.andExpr`.
   * @param ctx the parse tree
   */
  enterAndExpr?: (ctx: AndExprContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.andExpr`.
   * @param ctx the parse tree
   */
  exitAndExpr?: (ctx: AndExprContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.unaryExpr`.
   * @param ctx the parse tree
   */
  enterUnaryExpr?: (ctx: UnaryExprContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.unaryExpr`.
   * @param ctx the parse tree
   */
  exitUnaryExpr?: (ctx: UnaryExprContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.primary`.
   * @param ctx the parse tree
   */
  enterPrimary?: (ctx: PrimaryContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.primary`.
   * @param ctx the parse tree
   */
  exitPrimary?: (ctx: PrimaryContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.group`.
   * @param ctx the parse tree
   */
  enterGroup?: (ctx: GroupContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.group`.
   * @param ctx the parse tree
   */
  exitGroup?: (ctx: GroupContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.comparison`.
   * @param ctx the parse tree
   */
  enterComparison?: (ctx: ComparisonContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.comparison`.
   * @param ctx the parse tree
   */
  exitComparison?: (ctx: ComparisonContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.textOperation`.
   * @param ctx the parse tree
   */
  enterTextOperation?: (ctx: TextOperationContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.textOperation`.
   * @param ctx the parse tree
   */
  exitTextOperation?: (ctx: TextOperationContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.existenceOperation`.
   * @param ctx the parse tree
   */
  enterExistenceOperation?: (ctx: ExistenceOperationContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.existenceOperation`.
   * @param ctx the parse tree
   */
  exitExistenceOperation?: (ctx: ExistenceOperationContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.fieldRef`.
   * @param ctx the parse tree
   */
  enterFieldRef?: (ctx: FieldRefContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.fieldRef`.
   * @param ctx the parse tree
   */
  exitFieldRef?: (ctx: FieldRefContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.fieldIdentifier`.
   * @param ctx the parse tree
   */
  enterFieldIdentifier?: (ctx: FieldIdentifierContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.fieldIdentifier`.
   * @param ctx the parse tree
   */
  exitFieldIdentifier?: (ctx: FieldIdentifierContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.compOp`.
   * @param ctx the parse tree
   */
  enterCompOp?: (ctx: CompOpContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.compOp`.
   * @param ctx the parse tree
   */
  exitCompOp?: (ctx: CompOpContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.textOp`.
   * @param ctx the parse tree
   */
  enterTextOp?: (ctx: TextOpContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.textOp`.
   * @param ctx the parse tree
   */
  exitTextOp?: (ctx: TextOpContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.operand`.
   * @param ctx the parse tree
   */
  enterOperand?: (ctx: OperandContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.operand`.
   * @param ctx the parse tree
   */
  exitOperand?: (ctx: OperandContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.booleanLiteral`.
   * @param ctx the parse tree
   */
  enterBooleanLiteral?: (ctx: BooleanLiteralContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.booleanLiteral`.
   * @param ctx the parse tree
   */
  exitBooleanLiteral?: (ctx: BooleanLiteralContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.number`.
   * @param ctx the parse tree
   */
  enterNumber?: (ctx: NumberContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.number`.
   * @param ctx the parse tree
   */
  exitNumber?: (ctx: NumberContext) => void;
  /**
   * Enter a parse tree produced by `FiltersParser.stringLiteral`.
   * @param ctx the parse tree
   */
  enterStringLiteral?: (ctx: StringLiteralContext) => void;
  /**
   * Exit a parse tree produced by `FiltersParser.stringLiteral`.
   * @param ctx the parse tree
   */
  exitStringLiteral?: (ctx: StringLiteralContext) => void;

  visitTerminal(node: TerminalNode): void {}
  visitErrorNode(node: ErrorNode): void {}
  enterEveryRule(node: ParserRuleContext): void {}
  exitEveryRule(node: ParserRuleContext): void {}
}
