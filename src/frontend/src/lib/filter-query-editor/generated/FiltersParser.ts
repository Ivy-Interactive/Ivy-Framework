// @ts-nocheck
/* eslint-disable */

import * as antlr from "antlr4ng";

import { FiltersListener } from "./FiltersListener.js";
import { FiltersVisitor } from "./FiltersVisitor.js";

// for running tests with parameters, TODO: discuss strategy for typed parameters in CI
// eslint-disable-next-line no-unused-vars
type int = number;

export class FiltersParser extends antlr.Parser {
  public static readonly FIELD = 1;
  public static readonly STRING = 2;
  public static readonly CONTAINS = 3;
  public static readonly GREATER = 4;
  public static readonly STARTS = 5;
  public static readonly EQUALS = 6;
  public static readonly EQUAL = 7;
  public static readonly BLANK = 8;
  public static readonly LESS = 9;
  public static readonly THAN = 10;
  public static readonly ENDS = 11;
  public static readonly WITH = 12;
  public static readonly NOT = 13;
  public static readonly OR = 14;
  public static readonly IS = 15;
  public static readonly AND = 16;
  public static readonly TRUE = 17;
  public static readonly FALSE = 18;
  public static readonly LPAREN = 19;
  public static readonly RPAREN = 20;
  public static readonly EQ = 21;
  public static readonly EQ2 = 22;
  public static readonly NEQ = 23;
  public static readonly GT = 24;
  public static readonly GE = 25;
  public static readonly LT = 26;
  public static readonly LE = 27;
  public static readonly DOT = 28;
  public static readonly SIGN = 29;
  public static readonly DIGITS = 30;
  public static readonly WS = 31;
  public static readonly RULE_formula = 0;
  public static readonly RULE_expr = 1;
  public static readonly RULE_orExpr = 2;
  public static readonly RULE_andExpr = 3;
  public static readonly RULE_unaryExpr = 4;
  public static readonly RULE_primary = 5;
  public static readonly RULE_group = 6;
  public static readonly RULE_comparison = 7;
  public static readonly RULE_textOperation = 8;
  public static readonly RULE_existenceOperation = 9;
  public static readonly RULE_fieldRef = 10;
  public static readonly RULE_fieldIdentifier = 11;
  public static readonly RULE_compOp = 12;
  public static readonly RULE_textOp = 13;
  public static readonly RULE_operand = 14;
  public static readonly RULE_booleanLiteral = 15;
  public static readonly RULE_number = 16;
  public static readonly RULE_stringLiteral = 17;

  public static readonly literalNames = [
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
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    "'('",
    "')'",
    "'='",
    "'=='",
    "'!='",
    "'>'",
    "'>='",
    "'<'",
    "'<='",
    "'.'",
  ];

  public static readonly symbolicNames = [
    null,
    "FIELD",
    "STRING",
    "CONTAINS",
    "GREATER",
    "STARTS",
    "EQUALS",
    "EQUAL",
    "BLANK",
    "LESS",
    "THAN",
    "ENDS",
    "WITH",
    "NOT",
    "OR",
    "IS",
    "AND",
    "TRUE",
    "FALSE",
    "LPAREN",
    "RPAREN",
    "EQ",
    "EQ2",
    "NEQ",
    "GT",
    "GE",
    "LT",
    "LE",
    "DOT",
    "SIGN",
    "DIGITS",
    "WS",
  ];
  public static readonly ruleNames = [
    "formula",
    "expr",
    "orExpr",
    "andExpr",
    "unaryExpr",
    "primary",
    "group",
    "comparison",
    "textOperation",
    "existenceOperation",
    "fieldRef",
    "fieldIdentifier",
    "compOp",
    "textOp",
    "operand",
    "booleanLiteral",
    "number",
    "stringLiteral",
  ];

  public get grammarFileName(): string {
    return "Filters.g4";
  }
  public get literalNames(): (string | null)[] {
    return FiltersParser.literalNames;
  }
  public get symbolicNames(): (string | null)[] {
    return FiltersParser.symbolicNames;
  }
  public get ruleNames(): string[] {
    return FiltersParser.ruleNames;
  }
  public get serializedATN(): number[] {
    return FiltersParser._serializedATN;
  }

  protected createFailedPredicateException(
    predicate?: string,
    message?: string,
  ): antlr.FailedPredicateException {
    return new antlr.FailedPredicateException(this, predicate, message);
  }

  public constructor(input: antlr.TokenStream) {
    super(input);
    this.interpreter = new antlr.ParserATNSimulator(
      this,
      FiltersParser._ATN,
      FiltersParser.decisionsToDFA,
      new antlr.PredictionContextCache(),
    );
  }
  public formula(): FormulaContext {
    let localContext = new FormulaContext(this.context, this.state);
    this.enterRule(localContext, 0, FiltersParser.RULE_formula);
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 36;
        this.expr();
        this.state = 37;
        this.match(FiltersParser.EOF);
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
  public expr(): ExprContext {
    let localContext = new ExprContext(this.context, this.state);
    this.enterRule(localContext, 2, FiltersParser.RULE_expr);
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 39;
        this.orExpr();
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
  public orExpr(): OrExprContext {
    let localContext = new OrExprContext(this.context, this.state);
    this.enterRule(localContext, 4, FiltersParser.RULE_orExpr);
    let _la: number;
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 41;
        this.andExpr();
        this.state = 46;
        this.errorHandler.sync(this);
        _la = this.tokenStream.LA(1);
        while (_la === 14) {
          {
            {
              this.state = 42;
              this.match(FiltersParser.OR);
              this.state = 43;
              this.andExpr();
            }
          }
          this.state = 48;
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
  public andExpr(): AndExprContext {
    let localContext = new AndExprContext(this.context, this.state);
    this.enterRule(localContext, 6, FiltersParser.RULE_andExpr);
    let _la: number;
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 49;
        this.unaryExpr();
        this.state = 54;
        this.errorHandler.sync(this);
        _la = this.tokenStream.LA(1);
        while (_la === 16) {
          {
            {
              this.state = 50;
              this.match(FiltersParser.AND);
              this.state = 51;
              this.unaryExpr();
            }
          }
          this.state = 56;
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
  public unaryExpr(): UnaryExprContext {
    let localContext = new UnaryExprContext(this.context, this.state);
    this.enterRule(localContext, 8, FiltersParser.RULE_unaryExpr);
    try {
      this.state = 60;
      this.errorHandler.sync(this);
      switch (this.tokenStream.LA(1)) {
        case FiltersParser.NOT:
          this.enterOuterAlt(localContext, 1);
          {
            this.state = 57;
            this.match(FiltersParser.NOT);
            this.state = 58;
            this.unaryExpr();
          }
          break;
        case FiltersParser.FIELD:
        case FiltersParser.LPAREN:
          this.enterOuterAlt(localContext, 2);
          {
            this.state = 59;
            this.primary();
          }
          break;
        default:
          throw new antlr.NoViableAltException(this);
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
  public primary(): PrimaryContext {
    let localContext = new PrimaryContext(this.context, this.state);
    this.enterRule(localContext, 10, FiltersParser.RULE_primary);
    try {
      this.state = 66;
      this.errorHandler.sync(this);
      switch (this.interpreter.adaptivePredict(this.tokenStream, 3, this.context)) {
        case 1:
          this.enterOuterAlt(localContext, 1);
          {
            this.state = 62;
            this.group();
          }
          break;
        case 2:
          this.enterOuterAlt(localContext, 2);
          {
            this.state = 63;
            this.comparison();
          }
          break;
        case 3:
          this.enterOuterAlt(localContext, 3);
          {
            this.state = 64;
            this.textOperation();
          }
          break;
        case 4:
          this.enterOuterAlt(localContext, 4);
          {
            this.state = 65;
            this.existenceOperation();
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
  public group(): GroupContext {
    let localContext = new GroupContext(this.context, this.state);
    this.enterRule(localContext, 12, FiltersParser.RULE_group);
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 68;
        this.match(FiltersParser.LPAREN);
        this.state = 69;
        this.expr();
        this.state = 70;
        this.match(FiltersParser.RPAREN);
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
  public comparison(): ComparisonContext {
    let localContext = new ComparisonContext(this.context, this.state);
    this.enterRule(localContext, 14, FiltersParser.RULE_comparison);
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 72;
        this.fieldRef();
        this.state = 73;
        this.compOp();
        this.state = 74;
        this.operand();
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
  public textOperation(): TextOperationContext {
    let localContext = new TextOperationContext(this.context, this.state);
    this.enterRule(localContext, 16, FiltersParser.RULE_textOperation);
    try {
      this.state = 85;
      this.errorHandler.sync(this);
      switch (this.interpreter.adaptivePredict(this.tokenStream, 4, this.context)) {
        case 1:
          this.enterOuterAlt(localContext, 1);
          {
            this.state = 76;
            this.fieldRef();
            this.state = 77;
            this.textOp();
            this.state = 78;
            this.stringLiteral();
          }
          break;
        case 2:
          this.enterOuterAlt(localContext, 2);
          {
            this.state = 80;
            this.fieldRef();
            this.state = 81;
            this.match(FiltersParser.NOT);
            this.state = 82;
            this.textOp();
            this.state = 83;
            this.stringLiteral();
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
  public existenceOperation(): ExistenceOperationContext {
    let localContext = new ExistenceOperationContext(this.context, this.state);
    this.enterRule(localContext, 18, FiltersParser.RULE_existenceOperation);
    try {
      this.state = 96;
      this.errorHandler.sync(this);
      switch (this.interpreter.adaptivePredict(this.tokenStream, 5, this.context)) {
        case 1:
          this.enterOuterAlt(localContext, 1);
          {
            this.state = 87;
            this.fieldRef();
            this.state = 88;
            this.match(FiltersParser.IS);
            this.state = 89;
            this.match(FiltersParser.BLANK);
          }
          break;
        case 2:
          this.enterOuterAlt(localContext, 2);
          {
            this.state = 91;
            this.fieldRef();
            this.state = 92;
            this.match(FiltersParser.IS);
            this.state = 93;
            this.match(FiltersParser.NOT);
            this.state = 94;
            this.match(FiltersParser.BLANK);
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
  public fieldRef(): FieldRefContext {
    let localContext = new FieldRefContext(this.context, this.state);
    this.enterRule(localContext, 20, FiltersParser.RULE_fieldRef);
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 98;
        this.match(FiltersParser.FIELD);
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
  public fieldIdentifier(): FieldIdentifierContext {
    let localContext = new FieldIdentifierContext(this.context, this.state);
    this.enterRule(localContext, 22, FiltersParser.RULE_fieldIdentifier);
    try {
      this.enterOuterAlt(localContext, 1);
      // tslint:disable-next-line:no-empty
      {
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
  public compOp(): CompOpContext {
    let localContext = new CompOpContext(this.context, this.state);
    this.enterRule(localContext, 24, FiltersParser.RULE_compOp);
    try {
      this.state = 126;
      this.errorHandler.sync(this);
      switch (this.interpreter.adaptivePredict(this.tokenStream, 6, this.context)) {
        case 1:
          this.enterOuterAlt(localContext, 1);
          {
            this.state = 102;
            this.match(FiltersParser.EQ);
          }
          break;
        case 2:
          this.enterOuterAlt(localContext, 2);
          {
            this.state = 103;
            this.match(FiltersParser.EQ2);
          }
          break;
        case 3:
          this.enterOuterAlt(localContext, 3);
          {
            this.state = 104;
            this.match(FiltersParser.NEQ);
          }
          break;
        case 4:
          this.enterOuterAlt(localContext, 4);
          {
            this.state = 105;
            this.match(FiltersParser.GT);
          }
          break;
        case 5:
          this.enterOuterAlt(localContext, 5);
          {
            this.state = 106;
            this.match(FiltersParser.GE);
          }
          break;
        case 6:
          this.enterOuterAlt(localContext, 6);
          {
            this.state = 107;
            this.match(FiltersParser.LT);
          }
          break;
        case 7:
          this.enterOuterAlt(localContext, 7);
          {
            this.state = 108;
            this.match(FiltersParser.LE);
          }
          break;
        case 8:
          this.enterOuterAlt(localContext, 8);
          {
            this.state = 109;
            this.match(FiltersParser.EQUALS);
          }
          break;
        case 9:
          this.enterOuterAlt(localContext, 9);
          {
            this.state = 110;
            this.match(FiltersParser.NOT);
            this.state = 111;
            this.match(FiltersParser.EQUALS);
          }
          break;
        case 10:
          this.enterOuterAlt(localContext, 10);
          {
            this.state = 112;
            this.match(FiltersParser.NOT);
            this.state = 113;
            this.match(FiltersParser.EQUAL);
          }
          break;
        case 11:
          this.enterOuterAlt(localContext, 11);
          {
            this.state = 114;
            this.match(FiltersParser.GREATER);
            this.state = 115;
            this.match(FiltersParser.THAN);
          }
          break;
        case 12:
          this.enterOuterAlt(localContext, 12);
          {
            this.state = 116;
            this.match(FiltersParser.GREATER);
            this.state = 117;
            this.match(FiltersParser.THAN);
            this.state = 118;
            this.match(FiltersParser.OR);
            this.state = 119;
            this.match(FiltersParser.EQUAL);
          }
          break;
        case 13:
          this.enterOuterAlt(localContext, 13);
          {
            this.state = 120;
            this.match(FiltersParser.LESS);
            this.state = 121;
            this.match(FiltersParser.THAN);
          }
          break;
        case 14:
          this.enterOuterAlt(localContext, 14);
          {
            this.state = 122;
            this.match(FiltersParser.LESS);
            this.state = 123;
            this.match(FiltersParser.THAN);
            this.state = 124;
            this.match(FiltersParser.OR);
            this.state = 125;
            this.match(FiltersParser.EQUAL);
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
  public textOp(): TextOpContext {
    let localContext = new TextOpContext(this.context, this.state);
    this.enterRule(localContext, 26, FiltersParser.RULE_textOp);
    try {
      this.state = 133;
      this.errorHandler.sync(this);
      switch (this.tokenStream.LA(1)) {
        case FiltersParser.CONTAINS:
          this.enterOuterAlt(localContext, 1);
          {
            this.state = 128;
            this.match(FiltersParser.CONTAINS);
          }
          break;
        case FiltersParser.STARTS:
          this.enterOuterAlt(localContext, 2);
          {
            this.state = 129;
            this.match(FiltersParser.STARTS);
            this.state = 130;
            this.match(FiltersParser.WITH);
          }
          break;
        case FiltersParser.ENDS:
          this.enterOuterAlt(localContext, 3);
          {
            this.state = 131;
            this.match(FiltersParser.ENDS);
            this.state = 132;
            this.match(FiltersParser.WITH);
          }
          break;
        default:
          throw new antlr.NoViableAltException(this);
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
  public operand(): OperandContext {
    let localContext = new OperandContext(this.context, this.state);
    this.enterRule(localContext, 28, FiltersParser.RULE_operand);
    try {
      this.state = 138;
      this.errorHandler.sync(this);
      switch (this.tokenStream.LA(1)) {
        case FiltersParser.SIGN:
        case FiltersParser.DIGITS:
          this.enterOuterAlt(localContext, 1);
          {
            this.state = 135;
            this.number_();
          }
          break;
        case FiltersParser.STRING:
          this.enterOuterAlt(localContext, 2);
          {
            this.state = 136;
            this.stringLiteral();
          }
          break;
        case FiltersParser.TRUE:
        case FiltersParser.FALSE:
          this.enterOuterAlt(localContext, 3);
          {
            this.state = 137;
            this.booleanLiteral();
          }
          break;
        default:
          throw new antlr.NoViableAltException(this);
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
  public booleanLiteral(): BooleanLiteralContext {
    let localContext = new BooleanLiteralContext(this.context, this.state);
    this.enterRule(localContext, 30, FiltersParser.RULE_booleanLiteral);
    let _la: number;
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 140;
        _la = this.tokenStream.LA(1);
        if (!(_la === 17 || _la === 18)) {
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
  public number_(): NumberContext {
    let localContext = new NumberContext(this.context, this.state);
    this.enterRule(localContext, 32, FiltersParser.RULE_number);
    let _la: number;
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 143;
        this.errorHandler.sync(this);
        _la = this.tokenStream.LA(1);
        if (_la === 29) {
          {
            this.state = 142;
            this.match(FiltersParser.SIGN);
          }
        }

        this.state = 145;
        this.match(FiltersParser.DIGITS);
        this.state = 148;
        this.errorHandler.sync(this);
        _la = this.tokenStream.LA(1);
        if (_la === 28) {
          {
            this.state = 146;
            this.match(FiltersParser.DOT);
            this.state = 147;
            this.match(FiltersParser.DIGITS);
          }
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
  public stringLiteral(): StringLiteralContext {
    let localContext = new StringLiteralContext(this.context, this.state);
    this.enterRule(localContext, 34, FiltersParser.RULE_stringLiteral);
    try {
      this.enterOuterAlt(localContext, 1);
      {
        this.state = 150;
        this.match(FiltersParser.STRING);
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

  public static readonly _serializedATN: number[] = [
    4, 1, 31, 153, 2, 0, 7, 0, 2, 1, 7, 1, 2, 2, 7, 2, 2, 3, 7, 3, 2, 4, 7, 4, 2, 5, 7, 5, 2, 6, 7,
    6, 2, 7, 7, 7, 2, 8, 7, 8, 2, 9, 7, 9, 2, 10, 7, 10, 2, 11, 7, 11, 2, 12, 7, 12, 2, 13, 7, 13,
    2, 14, 7, 14, 2, 15, 7, 15, 2, 16, 7, 16, 2, 17, 7, 17, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 2, 1,
    2, 1, 2, 5, 2, 45, 8, 2, 10, 2, 12, 2, 48, 9, 2, 1, 3, 1, 3, 1, 3, 5, 3, 53, 8, 3, 10, 3, 12, 3,
    56, 9, 3, 1, 4, 1, 4, 1, 4, 3, 4, 61, 8, 4, 1, 5, 1, 5, 1, 5, 1, 5, 3, 5, 67, 8, 5, 1, 6, 1, 6,
    1, 6, 1, 6, 1, 7, 1, 7, 1, 7, 1, 7, 1, 8, 1, 8, 1, 8, 1, 8, 1, 8, 1, 8, 1, 8, 1, 8, 1, 8, 3, 8,
    86, 8, 8, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 3, 9, 97, 8, 9, 1, 10, 1, 10, 1,
    11, 1, 11, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12,
    1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 1, 12, 3, 12, 127,
    8, 12, 1, 13, 1, 13, 1, 13, 1, 13, 1, 13, 3, 13, 134, 8, 13, 1, 14, 1, 14, 1, 14, 3, 14, 139, 8,
    14, 1, 15, 1, 15, 1, 16, 3, 16, 144, 8, 16, 1, 16, 1, 16, 1, 16, 3, 16, 149, 8, 16, 1, 17, 1,
    17, 1, 17, 0, 0, 18, 0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 0, 1, 1,
    0, 17, 18, 161, 0, 36, 1, 0, 0, 0, 2, 39, 1, 0, 0, 0, 4, 41, 1, 0, 0, 0, 6, 49, 1, 0, 0, 0, 8,
    60, 1, 0, 0, 0, 10, 66, 1, 0, 0, 0, 12, 68, 1, 0, 0, 0, 14, 72, 1, 0, 0, 0, 16, 85, 1, 0, 0, 0,
    18, 96, 1, 0, 0, 0, 20, 98, 1, 0, 0, 0, 22, 100, 1, 0, 0, 0, 24, 126, 1, 0, 0, 0, 26, 133, 1, 0,
    0, 0, 28, 138, 1, 0, 0, 0, 30, 140, 1, 0, 0, 0, 32, 143, 1, 0, 0, 0, 34, 150, 1, 0, 0, 0, 36,
    37, 3, 2, 1, 0, 37, 38, 5, 0, 0, 1, 38, 1, 1, 0, 0, 0, 39, 40, 3, 4, 2, 0, 40, 3, 1, 0, 0, 0,
    41, 46, 3, 6, 3, 0, 42, 43, 5, 14, 0, 0, 43, 45, 3, 6, 3, 0, 44, 42, 1, 0, 0, 0, 45, 48, 1, 0,
    0, 0, 46, 44, 1, 0, 0, 0, 46, 47, 1, 0, 0, 0, 47, 5, 1, 0, 0, 0, 48, 46, 1, 0, 0, 0, 49, 54, 3,
    8, 4, 0, 50, 51, 5, 16, 0, 0, 51, 53, 3, 8, 4, 0, 52, 50, 1, 0, 0, 0, 53, 56, 1, 0, 0, 0, 54,
    52, 1, 0, 0, 0, 54, 55, 1, 0, 0, 0, 55, 7, 1, 0, 0, 0, 56, 54, 1, 0, 0, 0, 57, 58, 5, 13, 0, 0,
    58, 61, 3, 8, 4, 0, 59, 61, 3, 10, 5, 0, 60, 57, 1, 0, 0, 0, 60, 59, 1, 0, 0, 0, 61, 9, 1, 0, 0,
    0, 62, 67, 3, 12, 6, 0, 63, 67, 3, 14, 7, 0, 64, 67, 3, 16, 8, 0, 65, 67, 3, 18, 9, 0, 66, 62,
    1, 0, 0, 0, 66, 63, 1, 0, 0, 0, 66, 64, 1, 0, 0, 0, 66, 65, 1, 0, 0, 0, 67, 11, 1, 0, 0, 0, 68,
    69, 5, 19, 0, 0, 69, 70, 3, 2, 1, 0, 70, 71, 5, 20, 0, 0, 71, 13, 1, 0, 0, 0, 72, 73, 3, 20, 10,
    0, 73, 74, 3, 24, 12, 0, 74, 75, 3, 28, 14, 0, 75, 15, 1, 0, 0, 0, 76, 77, 3, 20, 10, 0, 77, 78,
    3, 26, 13, 0, 78, 79, 3, 34, 17, 0, 79, 86, 1, 0, 0, 0, 80, 81, 3, 20, 10, 0, 81, 82, 5, 13, 0,
    0, 82, 83, 3, 26, 13, 0, 83, 84, 3, 34, 17, 0, 84, 86, 1, 0, 0, 0, 85, 76, 1, 0, 0, 0, 85, 80,
    1, 0, 0, 0, 86, 17, 1, 0, 0, 0, 87, 88, 3, 20, 10, 0, 88, 89, 5, 15, 0, 0, 89, 90, 5, 8, 0, 0,
    90, 97, 1, 0, 0, 0, 91, 92, 3, 20, 10, 0, 92, 93, 5, 15, 0, 0, 93, 94, 5, 13, 0, 0, 94, 95, 5,
    8, 0, 0, 95, 97, 1, 0, 0, 0, 96, 87, 1, 0, 0, 0, 96, 91, 1, 0, 0, 0, 97, 19, 1, 0, 0, 0, 98, 99,
    5, 1, 0, 0, 99, 21, 1, 0, 0, 0, 100, 101, 1, 0, 0, 0, 101, 23, 1, 0, 0, 0, 102, 127, 5, 21, 0,
    0, 103, 127, 5, 22, 0, 0, 104, 127, 5, 23, 0, 0, 105, 127, 5, 24, 0, 0, 106, 127, 5, 25, 0, 0,
    107, 127, 5, 26, 0, 0, 108, 127, 5, 27, 0, 0, 109, 127, 5, 6, 0, 0, 110, 111, 5, 13, 0, 0, 111,
    127, 5, 6, 0, 0, 112, 113, 5, 13, 0, 0, 113, 127, 5, 7, 0, 0, 114, 115, 5, 4, 0, 0, 115, 127, 5,
    10, 0, 0, 116, 117, 5, 4, 0, 0, 117, 118, 5, 10, 0, 0, 118, 119, 5, 14, 0, 0, 119, 127, 5, 7, 0,
    0, 120, 121, 5, 9, 0, 0, 121, 127, 5, 10, 0, 0, 122, 123, 5, 9, 0, 0, 123, 124, 5, 10, 0, 0,
    124, 125, 5, 14, 0, 0, 125, 127, 5, 7, 0, 0, 126, 102, 1, 0, 0, 0, 126, 103, 1, 0, 0, 0, 126,
    104, 1, 0, 0, 0, 126, 105, 1, 0, 0, 0, 126, 106, 1, 0, 0, 0, 126, 107, 1, 0, 0, 0, 126, 108, 1,
    0, 0, 0, 126, 109, 1, 0, 0, 0, 126, 110, 1, 0, 0, 0, 126, 112, 1, 0, 0, 0, 126, 114, 1, 0, 0, 0,
    126, 116, 1, 0, 0, 0, 126, 120, 1, 0, 0, 0, 126, 122, 1, 0, 0, 0, 127, 25, 1, 0, 0, 0, 128, 134,
    5, 3, 0, 0, 129, 130, 5, 5, 0, 0, 130, 134, 5, 12, 0, 0, 131, 132, 5, 11, 0, 0, 132, 134, 5, 12,
    0, 0, 133, 128, 1, 0, 0, 0, 133, 129, 1, 0, 0, 0, 133, 131, 1, 0, 0, 0, 134, 27, 1, 0, 0, 0,
    135, 139, 3, 32, 16, 0, 136, 139, 3, 34, 17, 0, 137, 139, 3, 30, 15, 0, 138, 135, 1, 0, 0, 0,
    138, 136, 1, 0, 0, 0, 138, 137, 1, 0, 0, 0, 139, 29, 1, 0, 0, 0, 140, 141, 7, 0, 0, 0, 141, 31,
    1, 0, 0, 0, 142, 144, 5, 29, 0, 0, 143, 142, 1, 0, 0, 0, 143, 144, 1, 0, 0, 0, 144, 145, 1, 0,
    0, 0, 145, 148, 5, 30, 0, 0, 146, 147, 5, 28, 0, 0, 147, 149, 5, 30, 0, 0, 148, 146, 1, 0, 0, 0,
    148, 149, 1, 0, 0, 0, 149, 33, 1, 0, 0, 0, 150, 151, 5, 2, 0, 0, 151, 35, 1, 0, 0, 0, 11, 46,
    54, 60, 66, 85, 96, 126, 133, 138, 143, 148,
  ];

  private static __ATN: antlr.ATN;
  public static get _ATN(): antlr.ATN {
    if (!FiltersParser.__ATN) {
      FiltersParser.__ATN = new antlr.ATNDeserializer().deserialize(FiltersParser._serializedATN);
    }

    return FiltersParser.__ATN;
  }

  private static readonly vocabulary = new antlr.Vocabulary(
    FiltersParser.literalNames,
    FiltersParser.symbolicNames,
    [],
  );

  public override get vocabulary(): antlr.Vocabulary {
    return FiltersParser.vocabulary;
  }

  private static readonly decisionsToDFA = FiltersParser._ATN.decisionToState.map(
    (ds: antlr.DecisionState, index: number) => new antlr.DFA(ds, index),
  );
}

export class FormulaContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public expr(): ExprContext {
    return this.getRuleContext(0, ExprContext)!;
  }
  public EOF(): antlr.TerminalNode {
    return this.getToken(FiltersParser.EOF, 0)!;
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_formula;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterFormula) {
      listener.enterFormula(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitFormula) {
      listener.exitFormula(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitFormula) {
      return visitor.visitFormula(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class ExprContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public orExpr(): OrExprContext {
    return this.getRuleContext(0, OrExprContext)!;
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_expr;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterExpr) {
      listener.enterExpr(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitExpr) {
      listener.exitExpr(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitExpr) {
      return visitor.visitExpr(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class OrExprContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public andExpr(): AndExprContext[];
  public andExpr(i: number): AndExprContext | null;
  public andExpr(i?: number): AndExprContext[] | AndExprContext | null {
    if (i === undefined) {
      return this.getRuleContexts(AndExprContext);
    }

    return this.getRuleContext(i, AndExprContext);
  }
  public OR(): antlr.TerminalNode[];
  public OR(i: number): antlr.TerminalNode | null;
  public OR(i?: number): antlr.TerminalNode | null | antlr.TerminalNode[] {
    if (i === undefined) {
      return this.getTokens(FiltersParser.OR);
    } else {
      return this.getToken(FiltersParser.OR, i);
    }
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_orExpr;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterOrExpr) {
      listener.enterOrExpr(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitOrExpr) {
      listener.exitOrExpr(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitOrExpr) {
      return visitor.visitOrExpr(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class AndExprContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public unaryExpr(): UnaryExprContext[];
  public unaryExpr(i: number): UnaryExprContext | null;
  public unaryExpr(i?: number): UnaryExprContext[] | UnaryExprContext | null {
    if (i === undefined) {
      return this.getRuleContexts(UnaryExprContext);
    }

    return this.getRuleContext(i, UnaryExprContext);
  }
  public AND(): antlr.TerminalNode[];
  public AND(i: number): antlr.TerminalNode | null;
  public AND(i?: number): antlr.TerminalNode | null | antlr.TerminalNode[] {
    if (i === undefined) {
      return this.getTokens(FiltersParser.AND);
    } else {
      return this.getToken(FiltersParser.AND, i);
    }
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_andExpr;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterAndExpr) {
      listener.enterAndExpr(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitAndExpr) {
      listener.exitAndExpr(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitAndExpr) {
      return visitor.visitAndExpr(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class UnaryExprContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public NOT(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.NOT, 0);
  }
  public unaryExpr(): UnaryExprContext | null {
    return this.getRuleContext(0, UnaryExprContext);
  }
  public primary(): PrimaryContext | null {
    return this.getRuleContext(0, PrimaryContext);
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_unaryExpr;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterUnaryExpr) {
      listener.enterUnaryExpr(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitUnaryExpr) {
      listener.exitUnaryExpr(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitUnaryExpr) {
      return visitor.visitUnaryExpr(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class PrimaryContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public group(): GroupContext | null {
    return this.getRuleContext(0, GroupContext);
  }
  public comparison(): ComparisonContext | null {
    return this.getRuleContext(0, ComparisonContext);
  }
  public textOperation(): TextOperationContext | null {
    return this.getRuleContext(0, TextOperationContext);
  }
  public existenceOperation(): ExistenceOperationContext | null {
    return this.getRuleContext(0, ExistenceOperationContext);
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_primary;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterPrimary) {
      listener.enterPrimary(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitPrimary) {
      listener.exitPrimary(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitPrimary) {
      return visitor.visitPrimary(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class GroupContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public LPAREN(): antlr.TerminalNode {
    return this.getToken(FiltersParser.LPAREN, 0)!;
  }
  public expr(): ExprContext {
    return this.getRuleContext(0, ExprContext)!;
  }
  public RPAREN(): antlr.TerminalNode {
    return this.getToken(FiltersParser.RPAREN, 0)!;
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_group;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterGroup) {
      listener.enterGroup(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitGroup) {
      listener.exitGroup(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitGroup) {
      return visitor.visitGroup(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class ComparisonContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public fieldRef(): FieldRefContext {
    return this.getRuleContext(0, FieldRefContext)!;
  }
  public compOp(): CompOpContext {
    return this.getRuleContext(0, CompOpContext)!;
  }
  public operand(): OperandContext {
    return this.getRuleContext(0, OperandContext)!;
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_comparison;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterComparison) {
      listener.enterComparison(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitComparison) {
      listener.exitComparison(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitComparison) {
      return visitor.visitComparison(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class TextOperationContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public fieldRef(): FieldRefContext {
    return this.getRuleContext(0, FieldRefContext)!;
  }
  public textOp(): TextOpContext {
    return this.getRuleContext(0, TextOpContext)!;
  }
  public stringLiteral(): StringLiteralContext {
    return this.getRuleContext(0, StringLiteralContext)!;
  }
  public NOT(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.NOT, 0);
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_textOperation;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterTextOperation) {
      listener.enterTextOperation(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitTextOperation) {
      listener.exitTextOperation(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitTextOperation) {
      return visitor.visitTextOperation(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class ExistenceOperationContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public fieldRef(): FieldRefContext {
    return this.getRuleContext(0, FieldRefContext)!;
  }
  public IS(): antlr.TerminalNode {
    return this.getToken(FiltersParser.IS, 0)!;
  }
  public BLANK(): antlr.TerminalNode {
    return this.getToken(FiltersParser.BLANK, 0)!;
  }
  public NOT(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.NOT, 0);
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_existenceOperation;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterExistenceOperation) {
      listener.enterExistenceOperation(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitExistenceOperation) {
      listener.exitExistenceOperation(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitExistenceOperation) {
      return visitor.visitExistenceOperation(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class FieldRefContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public FIELD(): antlr.TerminalNode {
    return this.getToken(FiltersParser.FIELD, 0)!;
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_fieldRef;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterFieldRef) {
      listener.enterFieldRef(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitFieldRef) {
      listener.exitFieldRef(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitFieldRef) {
      return visitor.visitFieldRef(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class FieldIdentifierContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_fieldIdentifier;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterFieldIdentifier) {
      listener.enterFieldIdentifier(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitFieldIdentifier) {
      listener.exitFieldIdentifier(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitFieldIdentifier) {
      return visitor.visitFieldIdentifier(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class CompOpContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public EQ(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.EQ, 0);
  }
  public EQ2(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.EQ2, 0);
  }
  public NEQ(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.NEQ, 0);
  }
  public GT(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.GT, 0);
  }
  public GE(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.GE, 0);
  }
  public LT(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.LT, 0);
  }
  public LE(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.LE, 0);
  }
  public EQUALS(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.EQUALS, 0);
  }
  public NOT(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.NOT, 0);
  }
  public EQUAL(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.EQUAL, 0);
  }
  public GREATER(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.GREATER, 0);
  }
  public THAN(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.THAN, 0);
  }
  public OR(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.OR, 0);
  }
  public LESS(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.LESS, 0);
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_compOp;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterCompOp) {
      listener.enterCompOp(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitCompOp) {
      listener.exitCompOp(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitCompOp) {
      return visitor.visitCompOp(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class TextOpContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public CONTAINS(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.CONTAINS, 0);
  }
  public STARTS(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.STARTS, 0);
  }
  public WITH(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.WITH, 0);
  }
  public ENDS(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.ENDS, 0);
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_textOp;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterTextOp) {
      listener.enterTextOp(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitTextOp) {
      listener.exitTextOp(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitTextOp) {
      return visitor.visitTextOp(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class OperandContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public number(): NumberContext | null {
    return this.getRuleContext(0, NumberContext);
  }
  public stringLiteral(): StringLiteralContext | null {
    return this.getRuleContext(0, StringLiteralContext);
  }
  public booleanLiteral(): BooleanLiteralContext | null {
    return this.getRuleContext(0, BooleanLiteralContext);
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_operand;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterOperand) {
      listener.enterOperand(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitOperand) {
      listener.exitOperand(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitOperand) {
      return visitor.visitOperand(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class BooleanLiteralContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public TRUE(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.TRUE, 0);
  }
  public FALSE(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.FALSE, 0);
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_booleanLiteral;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterBooleanLiteral) {
      listener.enterBooleanLiteral(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitBooleanLiteral) {
      listener.exitBooleanLiteral(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitBooleanLiteral) {
      return visitor.visitBooleanLiteral(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class NumberContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public DIGITS(): antlr.TerminalNode[];
  public DIGITS(i: number): antlr.TerminalNode | null;
  public DIGITS(i?: number): antlr.TerminalNode | null | antlr.TerminalNode[] {
    if (i === undefined) {
      return this.getTokens(FiltersParser.DIGITS);
    } else {
      return this.getToken(FiltersParser.DIGITS, i);
    }
  }
  public SIGN(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.SIGN, 0);
  }
  public DOT(): antlr.TerminalNode | null {
    return this.getToken(FiltersParser.DOT, 0);
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_number;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterNumber) {
      listener.enterNumber(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitNumber) {
      listener.exitNumber(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitNumber) {
      return visitor.visitNumber(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}

export class StringLiteralContext extends antlr.ParserRuleContext {
  public constructor(parent: antlr.ParserRuleContext | null, invokingState: number) {
    super(parent, invokingState);
  }
  public STRING(): antlr.TerminalNode {
    return this.getToken(FiltersParser.STRING, 0)!;
  }
  public override get ruleIndex(): number {
    return FiltersParser.RULE_stringLiteral;
  }
  public override enterRule(listener: FiltersListener): void {
    if (listener.enterStringLiteral) {
      listener.enterStringLiteral(this);
    }
  }
  public override exitRule(listener: FiltersListener): void {
    if (listener.exitStringLiteral) {
      listener.exitStringLiteral(this);
    }
  }
  public override accept<Result>(visitor: FiltersVisitor<Result>): Result | null {
    if (visitor.visitStringLiteral) {
      return visitor.visitStringLiteral(this);
    } else {
      return visitor.visitChildren(this);
    }
  }
}
