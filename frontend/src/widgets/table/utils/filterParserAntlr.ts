import { CharStream, CommonTokenStream } from 'antlr4ng';
import { FilterQueryLexer } from '../grammar/generated/FilterQueryLexer';
import { FilterQueryParser } from '../grammar/generated/FilterQueryParser';
import { FilterQueryVisitor } from '../grammar/generated/FilterQueryVisitor';
import { Filter, Condition } from '@/services/grpcTableService';
import {
  QueryContext,
  ParenExpressionContext,
  NotExpressionContext,
  ConditionExpressionContext,
  AndExpressionContext,
  OrExpressionContext,
  EqualsConditionContext,
  NotEqualsConditionContext,
  GreaterThanConditionContext,
  GreaterThanEqualsConditionContext,
  LessThanConditionContext,
  LessThanEqualsConditionContext,
  ContainsConditionContext,
  StartsWithConditionContext,
  EndsWithConditionContext,
  InConditionContext,
  NotInConditionContext,
  IsNullConditionContext,
  IsNotNullConditionContext,
  ColumnNameContext,
  ValueContext,
  ValueListContext,
} from '../grammar/generated/FilterQueryParser';

/**
 * Error thrown during parsing
 */
export class FilterParseError extends Error {
  constructor(
    message: string,
    public position: number
  ) {
    super(`Parse error at position ${position}: ${message}`);
    this.name = 'FilterParseError';
  }
}

/**
 * Visitor that transforms ANTLR4 parse tree into Filter objects
 */
class FilterQueryTransformVisitor extends FilterQueryVisitor<Filter> {
  visitQuery = (ctx: QueryContext): Filter => {
    const expr = ctx.expression();
    if (!expr) throw new Error('Missing expression');
    return this.visit(expr) as Filter;
  };

  visitParenExpression = (ctx: ParenExpressionContext): Filter => {
    const expr = ctx.expression();
    if (!expr) throw new Error('Missing expression');
    return this.visit(expr) as Filter;
  };

  visitNotExpression = (ctx: NotExpressionContext): Filter => {
    const expr = ctx.expression();
    if (!expr) throw new Error('Missing expression');
    const innerFilter = this.visit(expr) as Filter;
    // Toggle the negate flag
    return { ...innerFilter, negate: !innerFilter.negate };
  };

  visitConditionExpression = (ctx: ConditionExpressionContext): Filter => {
    const cond = ctx.condition();
    if (!cond) throw new Error('Missing condition');
    return this.visit(cond) as Filter;
  };

  visitAndExpression = (ctx: AndExpressionContext): Filter => {
    const expr0 = ctx.expression(0);
    const expr1 = ctx.expression(1);
    if (!expr0 || !expr1) throw new Error('Missing expression');

    const left = this.visit(expr0) as Filter;
    const right = this.visit(expr1) as Filter;

    return {
      group: {
        op: 'AND',
        filters: [left, right],
      },
    };
  };

  visitOrExpression = (ctx: OrExpressionContext): Filter => {
    const expr0 = ctx.expression(0);
    const expr1 = ctx.expression(1);
    if (!expr0 || !expr1) throw new Error('Missing expression');

    const left = this.visit(expr0) as Filter;
    const right = this.visit(expr1) as Filter;

    return {
      group: {
        op: 'OR',
        filters: [left, right],
      },
    };
  };

  visitEqualsCondition = (ctx: EqualsConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const value = this.getValue(ctx.value());

    const condition: Condition = {
      column,
      function: 'equals',
      args: [value],
    };

    return { condition };
  };

  visitNotEqualsCondition = (ctx: NotEqualsConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const value = this.getValue(ctx.value());

    const condition: Condition = {
      column,
      function: 'equals',
      args: [value],
    };

    return { condition, negate: true };
  };

  visitGreaterThanCondition = (ctx: GreaterThanConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const value = this.getValue(ctx.value());

    const condition: Condition = {
      column,
      function: 'greaterThan',
      args: [value],
    };

    return { condition };
  };

  visitGreaterThanEqualsCondition = (
    ctx: GreaterThanEqualsConditionContext
  ): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const value = this.getValue(ctx.value());

    const condition: Condition = {
      column,
      function: 'greaterThanOrEquals',
      args: [value],
    };

    return { condition };
  };

  visitLessThanCondition = (ctx: LessThanConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const value = this.getValue(ctx.value());

    const condition: Condition = {
      column,
      function: 'lessThan',
      args: [value],
    };

    return { condition };
  };

  visitLessThanEqualsCondition = (
    ctx: LessThanEqualsConditionContext
  ): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const value = this.getValue(ctx.value());

    const condition: Condition = {
      column,
      function: 'lessThanOrEquals',
      args: [value],
    };

    return { condition };
  };

  visitContainsCondition = (ctx: ContainsConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const value = this.getValue(ctx.value());

    const condition: Condition = {
      column,
      function: 'contains',
      args: [value],
    };

    return { condition };
  };

  visitStartsWithCondition = (ctx: StartsWithConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const value = this.getValue(ctx.value());

    const condition: Condition = {
      column,
      function: 'startsWith',
      args: [value],
    };

    return { condition };
  };

  visitEndsWithCondition = (ctx: EndsWithConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const value = this.getValue(ctx.value());

    const condition: Condition = {
      column,
      function: 'endsWith',
      args: [value],
    };

    return { condition };
  };

  visitInCondition = (ctx: InConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const values = this.getValueList(ctx.valueList());

    const condition: Condition = {
      column,
      function: 'inSet',
      args: [values],
    };

    return { condition };
  };

  visitNotInCondition = (ctx: NotInConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());
    const values = this.getValueList(ctx.valueList());

    const condition: Condition = {
      column,
      function: 'inSet',
      args: [values],
    };

    return { condition, negate: true };
  };

  visitIsNullCondition = (ctx: IsNullConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());

    const condition: Condition = {
      column,
      function: 'isNull',
      args: [],
    };

    return { condition };
  };

  visitIsNotNullCondition = (ctx: IsNotNullConditionContext): Filter => {
    const column = this.getColumnName(ctx.columnName());

    const condition: Condition = {
      column,
      function: 'isNull',
      args: [],
    };

    return { condition, negate: true };
  };

  // Helper method to get column name as string (not part of visitor pattern)
  private getColumnName(ctx: ColumnNameContext): string {
    const text = ctx.getText();
    // Remove quotes if present
    if (text.startsWith('[') && text.endsWith(']')) {
      return text.slice(1, -1);
    }
    if (text.startsWith('`') && text.endsWith('`')) {
      return text.slice(1, -1);
    }
    return text;
  }

  // Helper method to get value (not part of visitor pattern)
  private getValue(ctx: ValueContext): unknown {
    const text = ctx.getText();

    if (ctx.STRING_LITERAL()) {
      // Remove quotes
      const str = text.slice(1, -1);
      // Handle escaped quotes
      return str.replace(/""/g, '"').replace(/''/g, "'");
    }

    if (ctx.NUMBER_LITERAL()) {
      return parseFloat(text);
    }

    if (ctx.BOOLEAN_LITERAL()) {
      return text.toLowerCase() === 'true';
    }

    if (ctx.NULL_LITERAL()) {
      return null;
    }

    return text;
  }

  // Helper method to get value list (not part of visitor pattern)
  private getValueList(ctx: ValueListContext): unknown[] {
    const values = ctx.value();
    return values.map(v => this.getValue(v));
  }

  protected defaultResult(): Filter {
    return { condition: { column: '', function: 'equals', args: [] } };
  }
}

/**
 * Main entry point: Parse a filter query string into a Filter object using ANTLR4
 *
 * @param input - The filter query string (e.g., "age > 18 AND status = 'active'")
 * @returns The parsed Filter object
 * @throws FilterParseError if the input is invalid
 *
 * @example
 * ```typescript
 * const filter = parseFilterQuery("age > 18 AND status = 'active'");
 * ```
 */
export function parseFilterQuery(input: string): Filter {
  if (!input || input.trim().length === 0) {
    throw new FilterParseError('Empty filter query', 0);
  }

  try {
    const charStream = CharStream.fromString(input);
    const lexer = new FilterQueryLexer(charStream);
    const tokenStream = new CommonTokenStream(lexer);
    const parser = new FilterQueryParser(tokenStream);

    // Remove default error listeners
    parser.removeErrorListeners();

    // Add custom error listener
    parser.addErrorListener({
      syntaxError: (
        _recognizer,
        _offendingSymbol,
        _line,
        charPositionInLine,
        msg
      ) => {
        throw new FilterParseError(msg, charPositionInLine);
      },
      reportAmbiguity: () => {},
      reportAttemptingFullContext: () => {},
      reportContextSensitivity: () => {},
    });

    const tree = parser.query();
    const visitor = new FilterQueryTransformVisitor();
    const result = visitor.visit(tree);
    if (!result) {
      throw new FilterParseError('Failed to parse filter query', 0);
    }
    return result;
  } catch (error) {
    if (error instanceof FilterParseError) {
      throw error;
    }
    throw new FilterParseError(
      error instanceof Error ? error.message : 'Unknown parsing error',
      0
    );
  }
}

/**
 * Validate a filter query string without throwing an error
 *
 * @param input - The filter query string
 * @returns An object with `valid` flag and optional `error` message
 */
export function validateFilterQuery(input: string): {
  valid: boolean;
  error?: string;
} {
  if (!input || input.trim().length === 0) {
    return { valid: false, error: 'Filter query cannot be empty' };
  }

  try {
    parseFilterQuery(input);
    return { valid: true };
  } catch (error) {
    if (error instanceof FilterParseError) {
      return { valid: false, error: error.message };
    }
    return { valid: false, error: 'Unknown parsing error' };
  }
}
