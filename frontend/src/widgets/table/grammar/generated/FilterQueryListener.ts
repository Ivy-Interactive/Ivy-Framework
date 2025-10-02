// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-nocheck
/* eslint-disable */
import {
  ErrorNode,
  ParseTreeListener,
  ParserRuleContext,
  TerminalNode,
} from 'antlr4ng';

import { QueryContext } from './FilterQueryParser.js';
import { ParenExpressionContext } from './FilterQueryParser.js';
import { NotExpressionContext } from './FilterQueryParser.js';
import { ConditionExpressionContext } from './FilterQueryParser.js';
import { AndExpressionContext } from './FilterQueryParser.js';
import { OrExpressionContext } from './FilterQueryParser.js';
import { EqualsConditionContext } from './FilterQueryParser.js';
import { NotEqualsConditionContext } from './FilterQueryParser.js';
import { GreaterThanConditionContext } from './FilterQueryParser.js';
import { GreaterThanEqualsConditionContext } from './FilterQueryParser.js';
import { LessThanConditionContext } from './FilterQueryParser.js';
import { LessThanEqualsConditionContext } from './FilterQueryParser.js';
import { ContainsConditionContext } from './FilterQueryParser.js';
import { StartsWithConditionContext } from './FilterQueryParser.js';
import { EndsWithConditionContext } from './FilterQueryParser.js';
import { InConditionContext } from './FilterQueryParser.js';
import { NotInConditionContext } from './FilterQueryParser.js';
import { IsNullConditionContext } from './FilterQueryParser.js';
import { IsNotNullConditionContext } from './FilterQueryParser.js';
import { ColumnNameContext } from './FilterQueryParser.js';
import { ValueContext } from './FilterQueryParser.js';
import { ValueListContext } from './FilterQueryParser.js';

/**
 * This interface defines a complete listener for a parse tree produced by
 * `FilterQueryParser`.
 */
export class FilterQueryListener implements ParseTreeListener {
  /**
   * Enter a parse tree produced by `FilterQueryParser.query`.
   * @param ctx the parse tree
   */
  enterQuery?: (ctx: QueryContext) => void;
  /**
   * Exit a parse tree produced by `FilterQueryParser.query`.
   * @param ctx the parse tree
   */
  exitQuery?: (ctx: QueryContext) => void;
  /**
   * Enter a parse tree produced by the `ParenExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   */
  enterParenExpression?: (ctx: ParenExpressionContext) => void;
  /**
   * Exit a parse tree produced by the `ParenExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   */
  exitParenExpression?: (ctx: ParenExpressionContext) => void;
  /**
   * Enter a parse tree produced by the `NotExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   */
  enterNotExpression?: (ctx: NotExpressionContext) => void;
  /**
   * Exit a parse tree produced by the `NotExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   */
  exitNotExpression?: (ctx: NotExpressionContext) => void;
  /**
   * Enter a parse tree produced by the `ConditionExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   */
  enterConditionExpression?: (ctx: ConditionExpressionContext) => void;
  /**
   * Exit a parse tree produced by the `ConditionExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   */
  exitConditionExpression?: (ctx: ConditionExpressionContext) => void;
  /**
   * Enter a parse tree produced by the `AndExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   */
  enterAndExpression?: (ctx: AndExpressionContext) => void;
  /**
   * Exit a parse tree produced by the `AndExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   */
  exitAndExpression?: (ctx: AndExpressionContext) => void;
  /**
   * Enter a parse tree produced by the `OrExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   */
  enterOrExpression?: (ctx: OrExpressionContext) => void;
  /**
   * Exit a parse tree produced by the `OrExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   */
  exitOrExpression?: (ctx: OrExpressionContext) => void;
  /**
   * Enter a parse tree produced by the `EqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterEqualsCondition?: (ctx: EqualsConditionContext) => void;
  /**
   * Exit a parse tree produced by the `EqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitEqualsCondition?: (ctx: EqualsConditionContext) => void;
  /**
   * Enter a parse tree produced by the `NotEqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterNotEqualsCondition?: (ctx: NotEqualsConditionContext) => void;
  /**
   * Exit a parse tree produced by the `NotEqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitNotEqualsCondition?: (ctx: NotEqualsConditionContext) => void;
  /**
   * Enter a parse tree produced by the `GreaterThanCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterGreaterThanCondition?: (ctx: GreaterThanConditionContext) => void;
  /**
   * Exit a parse tree produced by the `GreaterThanCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitGreaterThanCondition?: (ctx: GreaterThanConditionContext) => void;
  /**
   * Enter a parse tree produced by the `GreaterThanEqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterGreaterThanEqualsCondition?: (
    ctx: GreaterThanEqualsConditionContext
  ) => void;
  /**
   * Exit a parse tree produced by the `GreaterThanEqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitGreaterThanEqualsCondition?: (
    ctx: GreaterThanEqualsConditionContext
  ) => void;
  /**
   * Enter a parse tree produced by the `LessThanCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterLessThanCondition?: (ctx: LessThanConditionContext) => void;
  /**
   * Exit a parse tree produced by the `LessThanCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitLessThanCondition?: (ctx: LessThanConditionContext) => void;
  /**
   * Enter a parse tree produced by the `LessThanEqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterLessThanEqualsCondition?: (ctx: LessThanEqualsConditionContext) => void;
  /**
   * Exit a parse tree produced by the `LessThanEqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitLessThanEqualsCondition?: (ctx: LessThanEqualsConditionContext) => void;
  /**
   * Enter a parse tree produced by the `ContainsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterContainsCondition?: (ctx: ContainsConditionContext) => void;
  /**
   * Exit a parse tree produced by the `ContainsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitContainsCondition?: (ctx: ContainsConditionContext) => void;
  /**
   * Enter a parse tree produced by the `StartsWithCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterStartsWithCondition?: (ctx: StartsWithConditionContext) => void;
  /**
   * Exit a parse tree produced by the `StartsWithCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitStartsWithCondition?: (ctx: StartsWithConditionContext) => void;
  /**
   * Enter a parse tree produced by the `EndsWithCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterEndsWithCondition?: (ctx: EndsWithConditionContext) => void;
  /**
   * Exit a parse tree produced by the `EndsWithCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitEndsWithCondition?: (ctx: EndsWithConditionContext) => void;
  /**
   * Enter a parse tree produced by the `InCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterInCondition?: (ctx: InConditionContext) => void;
  /**
   * Exit a parse tree produced by the `InCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitInCondition?: (ctx: InConditionContext) => void;
  /**
   * Enter a parse tree produced by the `NotInCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterNotInCondition?: (ctx: NotInConditionContext) => void;
  /**
   * Exit a parse tree produced by the `NotInCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitNotInCondition?: (ctx: NotInConditionContext) => void;
  /**
   * Enter a parse tree produced by the `IsNullCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterIsNullCondition?: (ctx: IsNullConditionContext) => void;
  /**
   * Exit a parse tree produced by the `IsNullCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitIsNullCondition?: (ctx: IsNullConditionContext) => void;
  /**
   * Enter a parse tree produced by the `IsNotNullCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  enterIsNotNullCondition?: (ctx: IsNotNullConditionContext) => void;
  /**
   * Exit a parse tree produced by the `IsNotNullCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   */
  exitIsNotNullCondition?: (ctx: IsNotNullConditionContext) => void;
  /**
   * Enter a parse tree produced by `FilterQueryParser.columnName`.
   * @param ctx the parse tree
   */
  enterColumnName?: (ctx: ColumnNameContext) => void;
  /**
   * Exit a parse tree produced by `FilterQueryParser.columnName`.
   * @param ctx the parse tree
   */
  exitColumnName?: (ctx: ColumnNameContext) => void;
  /**
   * Enter a parse tree produced by `FilterQueryParser.value`.
   * @param ctx the parse tree
   */
  enterValue?: (ctx: ValueContext) => void;
  /**
   * Exit a parse tree produced by `FilterQueryParser.value`.
   * @param ctx the parse tree
   */
  exitValue?: (ctx: ValueContext) => void;
  /**
   * Enter a parse tree produced by `FilterQueryParser.valueList`.
   * @param ctx the parse tree
   */
  enterValueList?: (ctx: ValueListContext) => void;
  /**
   * Exit a parse tree produced by `FilterQueryParser.valueList`.
   * @param ctx the parse tree
   */
  exitValueList?: (ctx: ValueListContext) => void;

  visitTerminal(node: TerminalNode): void {}
  visitErrorNode(node: ErrorNode): void {}
  enterEveryRule(node: ParserRuleContext): void {}
  exitEveryRule(node: ParserRuleContext): void {}
}
