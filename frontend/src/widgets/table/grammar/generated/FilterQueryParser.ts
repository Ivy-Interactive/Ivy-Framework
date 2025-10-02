// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-nocheck
/* eslint-disable */
import * as antlr from 'antlr4ng';

import { FilterQueryListener } from './FilterQueryListener.js';
import { FilterQueryVisitor } from './FilterQueryVisitor.js';

// for running tests with parameters, TODO: discuss strategy for typed parameters in CI

type int = number;

export class FilterQueryParser extends antlr.Parser {
  public static readonly T__0 = 1;
  public static readonly T__1 = 2;
  public static readonly T__2 = 3;
  public static readonly AND = 4;
  public static readonly OR = 5;
  public static readonly NOT = 6;
  public static readonly IN = 7;
  public static readonly IS = 8;
  public static readonly CONTAINS = 9;
  public static readonly STARTS_WITH = 10;
  public static readonly ENDS_WITH = 11;
  public static readonly EQUALS = 12;
  public static readonly NOT_EQUALS = 13;
  public static readonly GREATER_THAN = 14;
  public static readonly GREATER_THAN_EQUALS = 15;
  public static readonly LESS_THAN = 16;
  public static readonly LESS_THAN_EQUALS = 17;
  public static readonly BOOLEAN_LITERAL = 18;
  public static readonly NULL_LITERAL = 19;
  public static readonly STRING_LITERAL = 20;
  public static readonly NUMBER_LITERAL = 21;
  public static readonly IDENTIFIER = 22;
  public static readonly QUOTED_IDENTIFIER = 23;
  public static readonly WS = 24;
  public static readonly RULE_query = 0;
  public static readonly RULE_expression = 1;
  public static readonly RULE_condition = 2;
  public static readonly RULE_columnName = 3;
  public static readonly RULE_value = 4;
  public static readonly RULE_valueList = 5;

  public static readonly literalNames = [
    null,
    "'('",
    "')'",
    "','",
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    "'>'",
    "'>='",
    "'<'",
    "'<='",
  ];

  public static readonly symbolicNames = [
    null,
    null,
    null,
    null,
    'AND',
    'OR',
    'NOT',
    'IN',
    'IS',
    'CONTAINS',
    'STARTS_WITH',
    'ENDS_WITH',
    'EQUALS',
    'NOT_EQUALS',
    'GREATER_THAN',
    'GREATER_THAN_EQUALS',
    'LESS_THAN',
    'LESS_THAN_EQUALS',
    'BOOLEAN_LITERAL',
    'NULL_LITERAL',
    'STRING_LITERAL',
    'NUMBER_LITERAL',
    'IDENTIFIER',
    'QUOTED_IDENTIFIER',
    'WS',
  ];
  public static readonly ruleNames = [
    'query',
    'expression',
    'condition',
    'columnName',
    'value',
    'valueList',
  ];

  public get grammarFileName(): string {
    return 'FilterQuery.g4';
  }
  public get literalNames(): (string | null)[] {
    return FilterQueryParser.literalNames;
  }
  public get symbolicNames(): (string | null)[] {
    return FilterQueryParser.symbolicNames;
  }
  public get ruleNames(): string[] {
    return FilterQueryParser.ruleNames;
  }
  public get serializedATN(): number[] {
    return FilterQueryParser._serializedATN;
  }

  protected createFailedPredicateException(
    predicate?: string,
    message?: string
  ): antlr.FailedPredicateException {
    return new antlr.FailedPredicateException(this, predicate, message);
  }

  public constructor(input: antlr.TokenStream) {
    super(input);
    this.interpreter = new antlr.ParserATNSimulator(
      this,
      FilterQueryParser._ATN,
      FilterQueryParser.decisionsToDFA,
      new antlr.PredictionContextCache()
    );
  }
  public query(): QueryContext {
    let localContext = new QueryContext(this.context, this.state);
    this.enterRule(localContext, 0, FilterQueryParser.RULE_query);
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 12;
        this.expression(0);
        this.state = 13;
        this.match(FilterQueryParser.EOF);
      }
    } catch (re) {
      if (re instanceof antlr.RecognitionException) {
        this.errorHandler.reportError(this, re);
        this.errorHandler.recover(this, re);
      } else {
        throw re;
      }
    } finally {
      this.exitRule();
    }
    return localContext;
  }

  public expression(): ExpressionContext;
  public expression(_p: number): ExpressionContext;
  public expression(_p?: number): ExpressionContext {
    if (_p === undefined) {
      _p = 0;
    }

    let parentContext = this.context;
    let parentState = this.state;
    let localContext = new ExpressionContext(this.context, parentState);
    let previousContext = localContext;
    let _startState = 2;
    this.enterRecursionRule(
      localContext,
      2,
      FilterQueryParser.RULE_expression,
      _p
    );
    try {
      let alternative: number;
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 23;
        this.errorHandler.sync(this);
        switch (this.tokenStream.LA(1)) {
          case FilterQueryParser.T__0:
            {
              localContext = new ParenExpressionContext(localContext);
              this.context = localContext;
              previousContext = localContext;

              this.state = 16;
              this.match(FilterQueryParser.T__0);
              this.state = 17;
              this.expression(0);
              this.state = 18;
              this.match(FilterQueryParser.T__1);
            }
            break;
          case FilterQueryParser.NOT:
            {
              localContext = new NotExpressionContext(localContext);
              this.context = localContext;
              previousContext = localContext;
              this.state = 20;
              this.match(FilterQueryParser.NOT);
              this.state = 21;
              this.expression(4);
            }
            break;
          case FilterQueryParser.IDENTIFIER:
          case FilterQueryParser.QUOTED_IDENTIFIER:
            {
              localContext = new ConditionExpressionContext(localContext);
              this.context = localContext;
              previousContext = localContext;
              this.state = 22;
              this.condition();
            }
            break;
          default:
            throw new antlr.NoViableAltException(this);
        }
        this.context!.stop = this.tokenStream.LT(-1);
        this.state = 33;
        this.errorHandler.sync(this);
        alternative = this.interpreter.adaptivePredict(
          this.tokenStream,
          2,
          this.context
        );
        while (
          alternative !== 2 &&
          alternative !== antlr.ATN.INVALID_ALT_NUMBER
        ) {
          if (alternative === 1) {
            if (this.parseListeners != null) {
              this.triggerExitRuleEvent();
            }
            previousContext = localContext;
            {
              this.state = 31;
              this.errorHandler.sync(this);
              switch (
                this.interpreter.adaptivePredict(
                  this.tokenStream,
                  1,
                  this.context
                )
              ) {
                case 1:
                  {
                    localContext = new AndExpressionContext(
                      new ExpressionContext(parentContext, parentState)
                    );
                    this.pushNewRecursionContext(
                      localContext,
                      _startState,
                      FilterQueryParser.RULE_expression
                    );
                    this.state = 25;
                    if (!this.precpred(this.context, 3)) {
                      throw this.createFailedPredicateException(
                        'this.precpred(this.context, 3)'
                      );
                    }
                    this.state = 26;
                    this.match(FilterQueryParser.AND);
                    this.state = 27;
                    this.expression(4);
                  }
                  break;
                case 2:
                  {
                    localContext = new OrExpressionContext(
                      new ExpressionContext(parentContext, parentState)
                    );
                    this.pushNewRecursionContext(
                      localContext,
                      _startState,
                      FilterQueryParser.RULE_expression
                    );
                    this.state = 28;
                    if (!this.precpred(this.context, 2)) {
                      throw this.createFailedPredicateException(
                        'this.precpred(this.context, 2)'
                      );
                    }
                    this.state = 29;
                    this.match(FilterQueryParser.OR);
                    this.state = 30;
                    this.expression(3);
                  }
                  break;
              }
            }
          }
          this.state = 35;
          this.errorHandler.sync(this);
          alternative = this.interpreter.adaptivePredict(
            this.tokenStream,
            2,
            this.context
          );
        }
      }
    } catch (re) {
      if (re instanceof antlr.RecognitionException) {
        this.errorHandler.reportError(this, re);
        this.errorHandler.recover(this, re);
      } else {
        throw re;
      }
    } finally {
      this.unrollRecursionContexts(parentContext);
    }
    return localContext;
  }
  public condition(): ConditionContext {
    let localContext = new ConditionContext(this.context, this.state);
    this.enterRule(localContext, 4, FilterQueryParser.RULE_condition);
    try {
      this.state = 94;
      this.errorHandler.sync(this);
      switch (
        this.interpreter.adaptivePredict(this.tokenStream, 3, this.context)
      ) {
        case 1:
          localContext = new EqualsConditionContext(localContext);
          this.enterOuterAlt(localContext, 1);
          {
            this.state = 36;
            this.columnName();
            this.state = 37;
            this.match(FilterQueryParser.EQUALS);
            this.state = 38;
            this.value();
          }
          break;
        case 2:
          localContext = new NotEqualsConditionContext(localContext);
          this.enterOuterAlt(localContext, 2);
          {
            this.state = 40;
            this.columnName();
            this.state = 41;
            this.match(FilterQueryParser.NOT_EQUALS);
            this.state = 42;
            this.value();
          }
          break;
        case 3:
          localContext = new GreaterThanConditionContext(localContext);
          this.enterOuterAlt(localContext, 3);
          {
            this.state = 44;
            this.columnName();
            this.state = 45;
            this.match(FilterQueryParser.GREATER_THAN);
            this.state = 46;
            this.value();
          }
          break;
        case 4:
          localContext = new GreaterThanEqualsConditionContext(localContext);
          this.enterOuterAlt(localContext, 4);
          {
            this.state = 48;
            this.columnName();
            this.state = 49;
            this.match(FilterQueryParser.GREATER_THAN_EQUALS);
            this.state = 50;
            this.value();
          }
          break;
        case 5:
          localContext = new LessThanConditionContext(localContext);
          this.enterOuterAlt(localContext, 5);
          {
            this.state = 52;
            this.columnName();
            this.state = 53;
            this.match(FilterQueryParser.LESS_THAN);
            this.state = 54;
            this.value();
          }
          break;
        case 6:
          localContext = new LessThanEqualsConditionContext(localContext);
          this.enterOuterAlt(localContext, 6);
          {
            this.state = 56;
            this.columnName();
            this.state = 57;
            this.match(FilterQueryParser.LESS_THAN_EQUALS);
            this.state = 58;
            this.value();
          }
          break;
        case 7:
          localContext = new ContainsConditionContext(localContext);
          this.enterOuterAlt(localContext, 7);
          {
            this.state = 60;
            this.columnName();
            this.state = 61;
            this.match(FilterQueryParser.CONTAINS);
            this.state = 62;
            this.value();
          }
          break;
        case 8:
          localContext = new StartsWithConditionContext(localContext);
          this.enterOuterAlt(localContext, 8);
          {
            this.state = 64;
            this.columnName();
            this.state = 65;
            this.match(FilterQueryParser.STARTS_WITH);
            this.state = 66;
            this.value();
          }
          break;
        case 9:
          localContext = new EndsWithConditionContext(localContext);
          this.enterOuterAlt(localContext, 9);
          {
            this.state = 68;
            this.columnName();
            this.state = 69;
            this.match(FilterQueryParser.ENDS_WITH);
            this.state = 70;
            this.value();
          }
          break;
        case 10:
          localContext = new InConditionContext(localContext);
          this.enterOuterAlt(localContext, 10);
          {
            this.state = 72;
            this.columnName();
            this.state = 73;
            this.match(FilterQueryParser.IN);
            this.state = 74;
            this.match(FilterQueryParser.T__0);
            this.state = 75;
            this.valueList();
            this.state = 76;
            this.match(FilterQueryParser.T__1);
          }
          break;
        case 11:
          localContext = new NotInConditionContext(localContext);
          this.enterOuterAlt(localContext, 11);
          {
            this.state = 78;
            this.columnName();
            this.state = 79;
            this.match(FilterQueryParser.NOT);
            this.state = 80;
            this.match(FilterQueryParser.IN);
            this.state = 81;
            this.match(FilterQueryParser.T__0);
            this.state = 82;
            this.valueList();
            this.state = 83;
            this.match(FilterQueryParser.T__1);
          }
          break;
        case 12:
          localContext = new IsNullConditionContext(localContext);
          this.enterOuterAlt(localContext, 12);
          {
            this.state = 85;
            this.columnName();
            this.state = 86;
            this.match(FilterQueryParser.IS);
            this.state = 87;
            this.match(FilterQueryParser.NULL_LITERAL);
          }
          break;
        case 13:
          localContext = new IsNotNullConditionContext(localContext);
          this.enterOuterAlt(localContext, 13);
          {
            this.state = 89;
            this.columnName();
            this.state = 90;
            this.match(FilterQueryParser.IS);
            this.state = 91;
            this.match(FilterQueryParser.NOT);
            this.state = 92;
            this.match(FilterQueryParser.NULL_LITERAL);
          }
          break;
      }
    } catch (re) {
      if (re instanceof antlr.RecognitionException) {
        this.errorHandler.reportError(this, re);
        this.errorHandler.recover(this, re);
      } else {
        throw re;
      }
    } finally {
      this.exitRule();
    }
    return localContext;
  }
  public columnName(): ColumnNameContext {
    let localContext = new ColumnNameContext(this.context, this.state);
    this.enterRule(localContext, 6, FilterQueryParser.RULE_columnName);
    let _la: number;
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 96;
        _la = this.tokenStream.LA(1);
        if (!(_la === 22 || _la === 23)) {
          this.errorHandler.recoverInline(this);
        } else {
          this.errorHandler.reportMatch(this);
          this.consume();
        }
      }
    } catch (re) {
      if (re instanceof antlr.RecognitionException) {
        this.errorHandler.reportError(this, re);
        this.errorHandler.recover(this, re);
      } else {
        throw re;
      }
    } finally {
      this.exitRule();
    }
    return localContext;
  }
  public value(): ValueContext {
    let localContext = new ValueContext(this.context, this.state);
    this.enterRule(localContext, 8, FilterQueryParser.RULE_value);
    let _la: number;
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 98;
        _la = this.tokenStream.LA(1);
        if (!((_la & ~0x1f) === 0 && ((1 << _la) & 3932160) !== 0)) {
          this.errorHandler.recoverInline(this);
        } else {
          this.errorHandler.reportMatch(this);
          this.consume();
        }
      }
    } catch (re) {
      if (re instanceof antlr.RecognitionException) {
        this.errorHandler.reportError(this, re);
        this.errorHandler.recover(this, re);
      } else {
        throw re;
      }
    } finally {
      this.exitRule();
    }
    return localContext;
  }
  public valueList(): ValueListContext {
    let localContext = new ValueListContext(this.context, this.state);
    this.enterRule(localContext, 10, FilterQueryParser.RULE_valueList);
    let _la: number;
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 100;
        this.value();
        this.state = 105;
        this.errorHandler.sync(this);
        _la = this.tokenStream.LA(1);
        while (_la === 3) {
          {
            {
              this.state = 101;
              this.match(FilterQueryParser.T__2);
              this.state = 102;
              this.value();
            }
          }
          this.state = 107;
          this.errorHandler.sync(this);
          _la = this.tokenStream.LA(1);
        }
      }
    } catch (re) {
      if (re instanceof antlr.RecognitionException) {
        this.errorHandler.reportError(this, re);
        this.errorHandler.recover(this, re);
      } else {
        throw re;
      }
    } finally {
      this.exitRule();
    }
    return localContext;
  }

  public override sempred(
    localContext: antlr.ParserRuleContext | null,
    ruleIndex: number,
    predIndex: number
  ): boolean {
    switch (ruleIndex) {
      case 1:
        return this.expression_sempred(
          localContext as ExpressionContext,
          predIndex
        );
    }
    return true;
  }
  private expression_sempred(
    localContext: ExpressionContext | null,
    predIndex: number
  ): boolean {
    switch (predIndex) {
      case 0:
        return this.precpred(this.context, 3);
      case 1:
        return this.precpred(this.context, 2);
    }
    return true;
  }

  public static readonly _serializedATN: number[] = [
    4, 1, 24, 109, 2, 0, 7, 0, 2, 1, 7, 1, 2, 2, 7, 2, 2, 3, 7, 3, 2, 4, 7, 4,
    2, 5, 7, 5, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
    1, 3, 1, 24, 8, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 5, 1, 32, 8, 1, 10,
    1, 12, 1, 35, 9, 1, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1,
    2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2,
    1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1,
    2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2,
    1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 3, 2, 95,
    8, 2, 1, 3, 1, 3, 1, 4, 1, 4, 1, 5, 1, 5, 1, 5, 5, 5, 104, 8, 5, 10, 5, 12,
    5, 107, 9, 5, 1, 5, 0, 1, 2, 6, 0, 2, 4, 6, 8, 10, 0, 2, 1, 0, 22, 23, 1, 0,
    18, 21, 119, 0, 12, 1, 0, 0, 0, 2, 23, 1, 0, 0, 0, 4, 94, 1, 0, 0, 0, 6, 96,
    1, 0, 0, 0, 8, 98, 1, 0, 0, 0, 10, 100, 1, 0, 0, 0, 12, 13, 3, 2, 1, 0, 13,
    14, 5, 0, 0, 1, 14, 1, 1, 0, 0, 0, 15, 16, 6, 1, -1, 0, 16, 17, 5, 1, 0, 0,
    17, 18, 3, 2, 1, 0, 18, 19, 5, 2, 0, 0, 19, 24, 1, 0, 0, 0, 20, 21, 5, 6, 0,
    0, 21, 24, 3, 2, 1, 4, 22, 24, 3, 4, 2, 0, 23, 15, 1, 0, 0, 0, 23, 20, 1, 0,
    0, 0, 23, 22, 1, 0, 0, 0, 24, 33, 1, 0, 0, 0, 25, 26, 10, 3, 0, 0, 26, 27,
    5, 4, 0, 0, 27, 32, 3, 2, 1, 4, 28, 29, 10, 2, 0, 0, 29, 30, 5, 5, 0, 0, 30,
    32, 3, 2, 1, 3, 31, 25, 1, 0, 0, 0, 31, 28, 1, 0, 0, 0, 32, 35, 1, 0, 0, 0,
    33, 31, 1, 0, 0, 0, 33, 34, 1, 0, 0, 0, 34, 3, 1, 0, 0, 0, 35, 33, 1, 0, 0,
    0, 36, 37, 3, 6, 3, 0, 37, 38, 5, 12, 0, 0, 38, 39, 3, 8, 4, 0, 39, 95, 1,
    0, 0, 0, 40, 41, 3, 6, 3, 0, 41, 42, 5, 13, 0, 0, 42, 43, 3, 8, 4, 0, 43,
    95, 1, 0, 0, 0, 44, 45, 3, 6, 3, 0, 45, 46, 5, 14, 0, 0, 46, 47, 3, 8, 4, 0,
    47, 95, 1, 0, 0, 0, 48, 49, 3, 6, 3, 0, 49, 50, 5, 15, 0, 0, 50, 51, 3, 8,
    4, 0, 51, 95, 1, 0, 0, 0, 52, 53, 3, 6, 3, 0, 53, 54, 5, 16, 0, 0, 54, 55,
    3, 8, 4, 0, 55, 95, 1, 0, 0, 0, 56, 57, 3, 6, 3, 0, 57, 58, 5, 17, 0, 0, 58,
    59, 3, 8, 4, 0, 59, 95, 1, 0, 0, 0, 60, 61, 3, 6, 3, 0, 61, 62, 5, 9, 0, 0,
    62, 63, 3, 8, 4, 0, 63, 95, 1, 0, 0, 0, 64, 65, 3, 6, 3, 0, 65, 66, 5, 10,
    0, 0, 66, 67, 3, 8, 4, 0, 67, 95, 1, 0, 0, 0, 68, 69, 3, 6, 3, 0, 69, 70, 5,
    11, 0, 0, 70, 71, 3, 8, 4, 0, 71, 95, 1, 0, 0, 0, 72, 73, 3, 6, 3, 0, 73,
    74, 5, 7, 0, 0, 74, 75, 5, 1, 0, 0, 75, 76, 3, 10, 5, 0, 76, 77, 5, 2, 0, 0,
    77, 95, 1, 0, 0, 0, 78, 79, 3, 6, 3, 0, 79, 80, 5, 6, 0, 0, 80, 81, 5, 7, 0,
    0, 81, 82, 5, 1, 0, 0, 82, 83, 3, 10, 5, 0, 83, 84, 5, 2, 0, 0, 84, 95, 1,
    0, 0, 0, 85, 86, 3, 6, 3, 0, 86, 87, 5, 8, 0, 0, 87, 88, 5, 19, 0, 0, 88,
    95, 1, 0, 0, 0, 89, 90, 3, 6, 3, 0, 90, 91, 5, 8, 0, 0, 91, 92, 5, 6, 0, 0,
    92, 93, 5, 19, 0, 0, 93, 95, 1, 0, 0, 0, 94, 36, 1, 0, 0, 0, 94, 40, 1, 0,
    0, 0, 94, 44, 1, 0, 0, 0, 94, 48, 1, 0, 0, 0, 94, 52, 1, 0, 0, 0, 94, 56, 1,
    0, 0, 0, 94, 60, 1, 0, 0, 0, 94, 64, 1, 0, 0, 0, 94, 68, 1, 0, 0, 0, 94, 72,
    1, 0, 0, 0, 94, 78, 1, 0, 0, 0, 94, 85, 1, 0, 0, 0, 94, 89, 1, 0, 0, 0, 95,
    5, 1, 0, 0, 0, 96, 97, 7, 0, 0, 0, 97, 7, 1, 0, 0, 0, 98, 99, 7, 1, 0, 0,
    99, 9, 1, 0, 0, 0, 100, 105, 3, 8, 4, 0, 101, 102, 5, 3, 0, 0, 102, 104, 3,
    8, 4, 0, 103, 101, 1, 0, 0, 0, 104, 107, 1, 0, 0, 0, 105, 103, 1, 0, 0, 0,
    105, 106, 1, 0, 0, 0, 106, 11, 1, 0, 0, 0, 107, 105, 1, 0, 0, 0, 5, 23, 31,
    33, 94, 105,
  ];

  private static __ATN: antlr.ATN;
  public static get _ATN(): antlr.ATN {
    if (!FilterQueryParser.__ATN) {
      FilterQueryParser.__ATN = new antlr.ATNDeserializer().deserialize(
        FilterQueryParser._serializedATN
      );
    }

    return FilterQueryParser.__ATN;
  }

  private static readonly vocabulary = new antlr.Vocabulary(
    FilterQueryParser.literalNames,
    FilterQueryParser.symbolicNames,
    []
  );

  public override get vocabulary(): antlr.Vocabulary {
    return FilterQueryParser.vocabulary;
  }

  private static readonly decisionsToDFA =
    FilterQueryParser._ATN.decisionToState.map(
      (ds: antlr.DecisionState, index: number) => new antlr.DFA(ds, index)
    );
}

export class QueryContext extends antlr.ParserRuleContext {
  public constructor(
    parent: antlr.ParserRuleContext | null,
    invokingState: number
  ) {
    super(parent, invokingState);
  }
  public expression(): ExpressionContext {
    return this.getRuleContext(0, ExpressionContext)!;
  }
  public EOF(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.EOF, 0)!;
  }
  public override get ruleIndex(): number {
    return FilterQueryParser.RULE_query;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterQuery) {
      listener.enterQuery(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitQuery) {
      listener.exitQuery(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitQuery) {
      return visitor.visitQuery(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class ExpressionContext extends antlr.ParserRuleContext {
  public constructor(
    parent: antlr.ParserRuleContext | null,
    invokingState: number
  ) {
    super(parent, invokingState);
  }
  public override get ruleIndex(): number {
    return FilterQueryParser.RULE_expression;
  }
  public override copyFrom(ctx: ExpressionContext): void {
    super.copyFrom(ctx);
  }
}
export class ParenExpressionContext extends ExpressionContext {
  public constructor(ctx: ExpressionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public expression(): ExpressionContext {
    return this.getRuleContext(0, ExpressionContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterParenExpression) {
      listener.enterParenExpression(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitParenExpression) {
      listener.exitParenExpression(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitParenExpression) {
      return visitor.visitParenExpression(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class NotExpressionContext extends ExpressionContext {
  public constructor(ctx: ExpressionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public NOT(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.NOT, 0)!;
  }
  public expression(): ExpressionContext {
    return this.getRuleContext(0, ExpressionContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterNotExpression) {
      listener.enterNotExpression(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitNotExpression) {
      listener.exitNotExpression(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitNotExpression) {
      return visitor.visitNotExpression(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class ConditionExpressionContext extends ExpressionContext {
  public constructor(ctx: ExpressionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public condition(): ConditionContext {
    return this.getRuleContext(0, ConditionContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterConditionExpression) {
      listener.enterConditionExpression(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitConditionExpression) {
      listener.exitConditionExpression(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitConditionExpression) {
      return visitor.visitConditionExpression(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class AndExpressionContext extends ExpressionContext {
  public constructor(ctx: ExpressionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public expression(): ExpressionContext[];
  public expression(i: number): ExpressionContext | null;
  public expression(
    i?: number
  ): ExpressionContext[] | ExpressionContext | null {
    if (i === undefined) {
      return this.getRuleContexts(ExpressionContext);
    }

    return this.getRuleContext(i, ExpressionContext);
  }
  public AND(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.AND, 0)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterAndExpression) {
      listener.enterAndExpression(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitAndExpression) {
      listener.exitAndExpression(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitAndExpression) {
      return visitor.visitAndExpression(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class OrExpressionContext extends ExpressionContext {
  public constructor(ctx: ExpressionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public expression(): ExpressionContext[];
  public expression(i: number): ExpressionContext | null;
  public expression(
    i?: number
  ): ExpressionContext[] | ExpressionContext | null {
    if (i === undefined) {
      return this.getRuleContexts(ExpressionContext);
    }

    return this.getRuleContext(i, ExpressionContext);
  }
  public OR(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.OR, 0)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterOrExpression) {
      listener.enterOrExpression(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitOrExpression) {
      listener.exitOrExpression(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitOrExpression) {
      return visitor.visitOrExpression(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class ConditionContext extends antlr.ParserRuleContext {
  public constructor(
    parent: antlr.ParserRuleContext | null,
    invokingState: number
  ) {
    super(parent, invokingState);
  }
  public override get ruleIndex(): number {
    return FilterQueryParser.RULE_condition;
  }
  public override copyFrom(ctx: ConditionContext): void {
    super.copyFrom(ctx);
  }
}
export class EqualsConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public EQUALS(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.EQUALS, 0)!;
  }
  public value(): ValueContext {
    return this.getRuleContext(0, ValueContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterEqualsCondition) {
      listener.enterEqualsCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitEqualsCondition) {
      listener.exitEqualsCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitEqualsCondition) {
      return visitor.visitEqualsCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class NotEqualsConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public NOT_EQUALS(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.NOT_EQUALS, 0)!;
  }
  public value(): ValueContext {
    return this.getRuleContext(0, ValueContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterNotEqualsCondition) {
      listener.enterNotEqualsCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitNotEqualsCondition) {
      listener.exitNotEqualsCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitNotEqualsCondition) {
      return visitor.visitNotEqualsCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class GreaterThanConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public GREATER_THAN(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.GREATER_THAN, 0)!;
  }
  public value(): ValueContext {
    return this.getRuleContext(0, ValueContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterGreaterThanCondition) {
      listener.enterGreaterThanCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitGreaterThanCondition) {
      listener.exitGreaterThanCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitGreaterThanCondition) {
      return visitor.visitGreaterThanCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class GreaterThanEqualsConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public GREATER_THAN_EQUALS(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.GREATER_THAN_EQUALS, 0)!;
  }
  public value(): ValueContext {
    return this.getRuleContext(0, ValueContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterGreaterThanEqualsCondition) {
      listener.enterGreaterThanEqualsCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitGreaterThanEqualsCondition) {
      listener.exitGreaterThanEqualsCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitGreaterThanEqualsCondition) {
      return visitor.visitGreaterThanEqualsCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class LessThanConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public LESS_THAN(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.LESS_THAN, 0)!;
  }
  public value(): ValueContext {
    return this.getRuleContext(0, ValueContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterLessThanCondition) {
      listener.enterLessThanCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitLessThanCondition) {
      listener.exitLessThanCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitLessThanCondition) {
      return visitor.visitLessThanCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class LessThanEqualsConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public LESS_THAN_EQUALS(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.LESS_THAN_EQUALS, 0)!;
  }
  public value(): ValueContext {
    return this.getRuleContext(0, ValueContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterLessThanEqualsCondition) {
      listener.enterLessThanEqualsCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitLessThanEqualsCondition) {
      listener.exitLessThanEqualsCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitLessThanEqualsCondition) {
      return visitor.visitLessThanEqualsCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class ContainsConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public CONTAINS(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.CONTAINS, 0)!;
  }
  public value(): ValueContext {
    return this.getRuleContext(0, ValueContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterContainsCondition) {
      listener.enterContainsCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitContainsCondition) {
      listener.exitContainsCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitContainsCondition) {
      return visitor.visitContainsCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class StartsWithConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public STARTS_WITH(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.STARTS_WITH, 0)!;
  }
  public value(): ValueContext {
    return this.getRuleContext(0, ValueContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterStartsWithCondition) {
      listener.enterStartsWithCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitStartsWithCondition) {
      listener.exitStartsWithCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitStartsWithCondition) {
      return visitor.visitStartsWithCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class EndsWithConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public ENDS_WITH(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.ENDS_WITH, 0)!;
  }
  public value(): ValueContext {
    return this.getRuleContext(0, ValueContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterEndsWithCondition) {
      listener.enterEndsWithCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitEndsWithCondition) {
      listener.exitEndsWithCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitEndsWithCondition) {
      return visitor.visitEndsWithCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class InConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public IN(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.IN, 0)!;
  }
  public valueList(): ValueListContext {
    return this.getRuleContext(0, ValueListContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterInCondition) {
      listener.enterInCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitInCondition) {
      listener.exitInCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitInCondition) {
      return visitor.visitInCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class NotInConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public NOT(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.NOT, 0)!;
  }
  public IN(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.IN, 0)!;
  }
  public valueList(): ValueListContext {
    return this.getRuleContext(0, ValueListContext)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterNotInCondition) {
      listener.enterNotInCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitNotInCondition) {
      listener.exitNotInCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitNotInCondition) {
      return visitor.visitNotInCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class IsNullConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public IS(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.IS, 0)!;
  }
  public NULL_LITERAL(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.NULL_LITERAL, 0)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterIsNullCondition) {
      listener.enterIsNullCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitIsNullCondition) {
      listener.exitIsNullCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitIsNullCondition) {
      return visitor.visitIsNullCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
export class IsNotNullConditionContext extends ConditionContext {
  public constructor(ctx: ConditionContext) {
    super(ctx.parent, ctx.invokingState);
    super.copyFrom(ctx);
  }
  public columnName(): ColumnNameContext {
    return this.getRuleContext(0, ColumnNameContext)!;
  }
  public IS(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.IS, 0)!;
  }
  public NOT(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.NOT, 0)!;
  }
  public NULL_LITERAL(): antlr.TerminalNode {
    return this.getToken(FilterQueryParser.NULL_LITERAL, 0)!;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterIsNotNullCondition) {
      listener.enterIsNotNullCondition(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitIsNotNullCondition) {
      listener.exitIsNotNullCondition(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitIsNotNullCondition) {
      return visitor.visitIsNotNullCondition(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class ColumnNameContext extends antlr.ParserRuleContext {
  public constructor(
    parent: antlr.ParserRuleContext | null,
    invokingState: number
  ) {
    super(parent, invokingState);
  }
  public IDENTIFIER(): antlr.TerminalNode | null {
    return this.getToken(FilterQueryParser.IDENTIFIER, 0);
  }
  public QUOTED_IDENTIFIER(): antlr.TerminalNode | null {
    return this.getToken(FilterQueryParser.QUOTED_IDENTIFIER, 0);
  }
  public override get ruleIndex(): number {
    return FilterQueryParser.RULE_columnName;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterColumnName) {
      listener.enterColumnName(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitColumnName) {
      listener.exitColumnName(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitColumnName) {
      return visitor.visitColumnName(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class ValueContext extends antlr.ParserRuleContext {
  public constructor(
    parent: antlr.ParserRuleContext | null,
    invokingState: number
  ) {
    super(parent, invokingState);
  }
  public STRING_LITERAL(): antlr.TerminalNode | null {
    return this.getToken(FilterQueryParser.STRING_LITERAL, 0);
  }
  public NUMBER_LITERAL(): antlr.TerminalNode | null {
    return this.getToken(FilterQueryParser.NUMBER_LITERAL, 0);
  }
  public BOOLEAN_LITERAL(): antlr.TerminalNode | null {
    return this.getToken(FilterQueryParser.BOOLEAN_LITERAL, 0);
  }
  public NULL_LITERAL(): antlr.TerminalNode | null {
    return this.getToken(FilterQueryParser.NULL_LITERAL, 0);
  }
  public override get ruleIndex(): number {
    return FilterQueryParser.RULE_value;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterValue) {
      listener.enterValue(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitValue) {
      listener.exitValue(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitValue) {
      return visitor.visitValue(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class ValueListContext extends antlr.ParserRuleContext {
  public constructor(
    parent: antlr.ParserRuleContext | null,
    invokingState: number
  ) {
    super(parent, invokingState);
  }
  public value(): ValueContext[];
  public value(i: number): ValueContext | null;
  public value(i?: number): ValueContext[] | ValueContext | null {
    if (i === undefined) {
      return this.getRuleContexts(ValueContext);
    }

    return this.getRuleContext(i, ValueContext);
  }
  public override get ruleIndex(): number {
    return FilterQueryParser.RULE_valueList;
  }
  public override enterRule(listener: FilterQueryListener): void {
    if (listener.enterValueList) {
      listener.enterValueList(this);
    }
  }
  public override exitRule(listener: FilterQueryListener): void {
    if (listener.exitValueList) {
      listener.exitValueList(this);
    }
  }
  public override accept<Result>(
    visitor: FilterQueryVisitor<Result>
  ): Result | null {
    if (visitor.visitValueList) {
      return visitor.visitValueList(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
