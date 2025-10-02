import { AbstractParseTreeVisitor } from 'antlr4ng';

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
 * This interface defines a complete generic visitor for a parse tree produced
 * by `FilterQueryParser`.
 *
 * @param <Result> The return type of the visit operation. Use `void` for
 * operations with no return type.
 */
export class FilterQueryVisitor<
  Result,
> extends AbstractParseTreeVisitor<Result> {
  /**
   * Visit a parse tree produced by `FilterQueryParser.query`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitQuery?: (ctx: QueryContext) => Result;
  /**
   * Visit a parse tree produced by the `ParenExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitParenExpression?: (ctx: ParenExpressionContext) => Result;
  /**
   * Visit a parse tree produced by the `NotExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitNotExpression?: (ctx: NotExpressionContext) => Result;
  /**
   * Visit a parse tree produced by the `ConditionExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitConditionExpression?: (ctx: ConditionExpressionContext) => Result;
  /**
   * Visit a parse tree produced by the `AndExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitAndExpression?: (ctx: AndExpressionContext) => Result;
  /**
   * Visit a parse tree produced by the `OrExpression`
   * labeled alternative in `FilterQueryParser.expression`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitOrExpression?: (ctx: OrExpressionContext) => Result;
  /**
   * Visit a parse tree produced by the `EqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitEqualsCondition?: (ctx: EqualsConditionContext) => Result;
  /**
   * Visit a parse tree produced by the `NotEqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitNotEqualsCondition?: (ctx: NotEqualsConditionContext) => Result;
  /**
   * Visit a parse tree produced by the `GreaterThanCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitGreaterThanCondition?: (ctx: GreaterThanConditionContext) => Result;
  /**
   * Visit a parse tree produced by the `GreaterThanEqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitGreaterThanEqualsCondition?: (
    ctx: GreaterThanEqualsConditionContext
  ) => Result;
  /**
   * Visit a parse tree produced by the `LessThanCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitLessThanCondition?: (ctx: LessThanConditionContext) => Result;
  /**
   * Visit a parse tree produced by the `LessThanEqualsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitLessThanEqualsCondition?: (
    ctx: LessThanEqualsConditionContext
  ) => Result;
  /**
   * Visit a parse tree produced by the `ContainsCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitContainsCondition?: (ctx: ContainsConditionContext) => Result;
  /**
   * Visit a parse tree produced by the `StartsWithCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitStartsWithCondition?: (ctx: StartsWithConditionContext) => Result;
  /**
   * Visit a parse tree produced by the `EndsWithCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitEndsWithCondition?: (ctx: EndsWithConditionContext) => Result;
  /**
   * Visit a parse tree produced by the `InCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitInCondition?: (ctx: InConditionContext) => Result;
  /**
   * Visit a parse tree produced by the `NotInCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitNotInCondition?: (ctx: NotInConditionContext) => Result;
  /**
   * Visit a parse tree produced by the `IsNullCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitIsNullCondition?: (ctx: IsNullConditionContext) => Result;
  /**
   * Visit a parse tree produced by the `IsNotNullCondition`
   * labeled alternative in `FilterQueryParser.condition`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitIsNotNullCondition?: (ctx: IsNotNullConditionContext) => Result;
  /**
   * Visit a parse tree produced by `FilterQueryParser.columnName`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitColumnName?: (ctx: ColumnNameContext) => Result;
  /**
   * Visit a parse tree produced by `FilterQueryParser.value`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitValue?: (ctx: ValueContext) => Result;
  /**
   * Visit a parse tree produced by `FilterQueryParser.valueList`.
   * @param ctx the parse tree
   * @return the visitor result
   */
  visitValueList?: (ctx: ValueListContext) => Result;
}
