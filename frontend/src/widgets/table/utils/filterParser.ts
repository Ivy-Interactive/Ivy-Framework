import { Filter, Condition } from '@/services/grpcTableService';

/**
 * Token types for the filter query lexer
 */
enum TokenType {
  // Literals
  STRING = 'STRING',
  NUMBER = 'NUMBER',
  BOOLEAN = 'BOOLEAN',
  NULL = 'NULL',
  IDENTIFIER = 'IDENTIFIER',

  // Operators
  EQUALS = 'EQUALS',
  NOT_EQUALS = 'NOT_EQUALS',
  GREATER_THAN = 'GREATER_THAN',
  GREATER_THAN_EQUALS = 'GREATER_THAN_EQUALS',
  LESS_THAN = 'LESS_THAN',
  LESS_THAN_EQUALS = 'LESS_THAN_EQUALS',

  // Keywords
  AND = 'AND',
  OR = 'OR',
  NOT = 'NOT',
  IN = 'IN',
  IS = 'IS',
  CONTAINS = 'CONTAINS',
  STARTS_WITH = 'STARTS_WITH',
  ENDS_WITH = 'ENDS_WITH',

  // Punctuation
  LPAREN = 'LPAREN',
  RPAREN = 'RPAREN',
  COMMA = 'COMMA',

  // Special
  EOF = 'EOF',
}

interface Token {
  type: TokenType;
  value: string;
  position: number;
}

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
 * Lexer: Converts input string into tokens
 */
class FilterLexer {
  private input: string;
  private position: number = 0;
  private currentChar: string | null;

  constructor(input: string) {
    this.input = input;
    this.currentChar = input.length > 0 ? input[0] : null;
  }

  private advance(): void {
    this.position++;
    this.currentChar =
      this.position < this.input.length ? this.input[this.position] : null;
  }

  private peek(offset: number = 1): string | null {
    const pos = this.position + offset;
    return pos < this.input.length ? this.input[pos] : null;
  }

  private skipWhitespace(): void {
    while (this.currentChar !== null && /\s/.test(this.currentChar)) {
      this.advance();
    }
  }

  private readString(quote: string): string {
    let result = '';
    this.advance(); // skip opening quote

    while (this.currentChar !== null && this.currentChar !== quote) {
      if (this.currentChar === '\\' && this.peek() === quote) {
        this.advance();
        result += this.currentChar;
      } else {
        result += this.currentChar;
      }
      this.advance();
    }

    if (this.currentChar !== quote) {
      throw new FilterParseError('Unterminated string literal', this.position);
    }

    this.advance(); // skip closing quote
    return result;
  }

  private readNumber(): string {
    let result = '';

    if (this.currentChar === '-') {
      result += this.currentChar;
      this.advance();
    }

    while (this.currentChar !== null && /[0-9.]/.test(this.currentChar)) {
      result += this.currentChar;
      this.advance();
    }

    return result;
  }

  private readIdentifier(): string {
    let result = '';

    while (this.currentChar !== null && /[a-zA-Z0-9_]/.test(this.currentChar)) {
      result += this.currentChar;
      this.advance();
    }

    return result;
  }

  private readQuotedIdentifier(closingChar: string): string {
    let result = '';
    this.advance(); // skip opening bracket/backtick

    while (this.currentChar !== null && this.currentChar !== closingChar) {
      result += this.currentChar;
      this.advance();
    }

    if (this.currentChar !== closingChar) {
      throw new FilterParseError(
        'Unterminated quoted identifier',
        this.position
      );
    }

    this.advance(); // skip closing bracket/backtick
    return result;
  }

  public tokenize(): Token[] {
    const tokens: Token[] = [];

    while (this.currentChar !== null) {
      this.skipWhitespace();

      if (this.currentChar === null) break;

      const tokenStart = this.position;

      // String literals
      if (this.currentChar === '"' || this.currentChar === "'") {
        const quote = this.currentChar;
        const value = this.readString(quote);
        tokens.push({ type: TokenType.STRING, value, position: tokenStart });
        continue;
      }

      // Numbers
      if (/[0-9-]/.test(this.currentChar)) {
        const value = this.readNumber();
        tokens.push({ type: TokenType.NUMBER, value, position: tokenStart });
        continue;
      }

      // Quoted identifiers
      if (this.currentChar === '[') {
        const value = this.readQuotedIdentifier(']');
        tokens.push({
          type: TokenType.IDENTIFIER,
          value,
          position: tokenStart,
        });
        continue;
      }

      if (this.currentChar === '`') {
        const value = this.readQuotedIdentifier('`');
        tokens.push({
          type: TokenType.IDENTIFIER,
          value,
          position: tokenStart,
        });
        continue;
      }

      // Operators
      if (this.currentChar === '=' && this.peek() === '=') {
        tokens.push({
          type: TokenType.EQUALS,
          value: '==',
          position: tokenStart,
        });
        this.advance();
        this.advance();
        continue;
      }

      if (this.currentChar === '=') {
        tokens.push({
          type: TokenType.EQUALS,
          value: '=',
          position: tokenStart,
        });
        this.advance();
        continue;
      }

      if (this.currentChar === '!' && this.peek() === '=') {
        tokens.push({
          type: TokenType.NOT_EQUALS,
          value: '!=',
          position: tokenStart,
        });
        this.advance();
        this.advance();
        continue;
      }

      if (this.currentChar === '<' && this.peek() === '>') {
        tokens.push({
          type: TokenType.NOT_EQUALS,
          value: '<>',
          position: tokenStart,
        });
        this.advance();
        this.advance();
        continue;
      }

      if (this.currentChar === '>' && this.peek() === '=') {
        tokens.push({
          type: TokenType.GREATER_THAN_EQUALS,
          value: '>=',
          position: tokenStart,
        });
        this.advance();
        this.advance();
        continue;
      }

      if (this.currentChar === '>') {
        tokens.push({
          type: TokenType.GREATER_THAN,
          value: '>',
          position: tokenStart,
        });
        this.advance();
        continue;
      }

      if (this.currentChar === '<' && this.peek() === '=') {
        tokens.push({
          type: TokenType.LESS_THAN_EQUALS,
          value: '<=',
          position: tokenStart,
        });
        this.advance();
        this.advance();
        continue;
      }

      if (this.currentChar === '<') {
        tokens.push({
          type: TokenType.LESS_THAN,
          value: '<',
          position: tokenStart,
        });
        this.advance();
        continue;
      }

      // Punctuation
      if (this.currentChar === '(') {
        tokens.push({
          type: TokenType.LPAREN,
          value: '(',
          position: tokenStart,
        });
        this.advance();
        continue;
      }

      if (this.currentChar === ')') {
        tokens.push({
          type: TokenType.RPAREN,
          value: ')',
          position: tokenStart,
        });
        this.advance();
        continue;
      }

      if (this.currentChar === ',') {
        tokens.push({
          type: TokenType.COMMA,
          value: ',',
          position: tokenStart,
        });
        this.advance();
        continue;
      }

      // Identifiers and keywords
      if (/[a-zA-Z_]/.test(this.currentChar)) {
        const value = this.readIdentifier();
        const upperValue = value.toUpperCase();

        // Check for keywords
        if (upperValue === 'AND') {
          tokens.push({ type: TokenType.AND, value, position: tokenStart });
        } else if (upperValue === 'OR') {
          tokens.push({ type: TokenType.OR, value, position: tokenStart });
        } else if (upperValue === 'NOT') {
          tokens.push({ type: TokenType.NOT, value, position: tokenStart });
        } else if (upperValue === 'IN') {
          tokens.push({ type: TokenType.IN, value, position: tokenStart });
        } else if (upperValue === 'IS') {
          tokens.push({ type: TokenType.IS, value, position: tokenStart });
        } else if (upperValue === 'CONTAINS') {
          tokens.push({
            type: TokenType.CONTAINS,
            value,
            position: tokenStart,
          });
        } else if (
          upperValue === 'STARTS_WITH' ||
          upperValue === 'STARTSWITH'
        ) {
          tokens.push({
            type: TokenType.STARTS_WITH,
            value,
            position: tokenStart,
          });
        } else if (upperValue === 'ENDS_WITH' || upperValue === 'ENDSWITH') {
          tokens.push({
            type: TokenType.ENDS_WITH,
            value,
            position: tokenStart,
          });
        } else if (upperValue === 'TRUE' || upperValue === 'FALSE') {
          tokens.push({ type: TokenType.BOOLEAN, value, position: tokenStart });
        } else if (upperValue === 'NULL') {
          tokens.push({ type: TokenType.NULL, value, position: tokenStart });
        } else {
          tokens.push({
            type: TokenType.IDENTIFIER,
            value,
            position: tokenStart,
          });
        }
        continue;
      }

      throw new FilterParseError(
        `Unexpected character: ${this.currentChar}`,
        this.position
      );
    }

    tokens.push({ type: TokenType.EOF, value: '', position: this.position });
    return tokens;
  }
}

/**
 * Parser: Converts tokens into Filter AST
 */
class FilterParser {
  private tokens: Token[];
  private position: number = 0;
  private currentToken: Token;

  constructor(tokens: Token[]) {
    this.tokens = tokens;
    this.currentToken = tokens[0];
  }

  private advance(): void {
    this.position++;
    this.currentToken = this.tokens[this.position];
  }

  private expect(type: TokenType): Token {
    if (this.currentToken.type !== type) {
      throw new FilterParseError(
        `Expected ${type} but got ${this.currentToken.type}`,
        this.currentToken.position
      );
    }
    const token = this.currentToken;
    this.advance();
    return token;
  }

  private parseValue(): unknown {
    if (this.currentToken.type === TokenType.STRING) {
      const value = this.currentToken.value;
      this.advance();
      return value;
    }

    if (this.currentToken.type === TokenType.NUMBER) {
      const value = parseFloat(this.currentToken.value);
      this.advance();
      return value;
    }

    if (this.currentToken.type === TokenType.BOOLEAN) {
      const value = this.currentToken.value.toUpperCase() === 'TRUE';
      this.advance();
      return value;
    }

    if (this.currentToken.type === TokenType.NULL) {
      this.advance();
      return null;
    }

    throw new FilterParseError(
      `Expected value but got ${this.currentToken.type}`,
      this.currentToken.position
    );
  }

  private parseValueList(): unknown[] {
    const values: unknown[] = [];

    values.push(this.parseValue());

    while (this.currentToken.type === TokenType.COMMA) {
      this.advance();
      values.push(this.parseValue());
    }

    return values;
  }

  private parseColumnName(): string {
    const token = this.expect(TokenType.IDENTIFIER);
    return token.value;
  }

  private parseCondition(): Filter {
    const column = this.parseColumnName();

    // Handle IS NULL / IS NOT NULL
    if (this.currentToken.type === TokenType.IS) {
      this.advance();
      const nextToken = this.currentToken;
      let isNot = false;
      if (nextToken.type === TokenType.NOT) {
        isNot = true;
        this.advance();
      }
      this.expect(TokenType.NULL);

      const condition: Condition = {
        column,
        function: 'isNull',
        args: [],
      };

      return isNot ? { condition, negate: true } : { condition };
    }

    // Handle IN / NOT IN
    if (this.currentToken.type === TokenType.IN) {
      this.advance();
      this.expect(TokenType.LPAREN);
      const values = this.parseValueList();
      this.expect(TokenType.RPAREN);

      const condition: Condition = {
        column,
        function: 'inSet',
        args: [values],
      };

      return { condition };
    }

    if (this.currentToken.type === TokenType.NOT) {
      const nextToken = this.tokens[this.position + 1];
      if (nextToken?.type === TokenType.IN) {
        this.advance(); // skip NOT
        this.advance(); // skip IN
        this.expect(TokenType.LPAREN);
        const values = this.parseValueList();
        this.expect(TokenType.RPAREN);

        const condition: Condition = {
          column,
          function: 'inSet',
          args: [values],
        };

        return { condition, negate: true };
      }
    }

    // Handle string functions
    if (this.currentToken.type === TokenType.CONTAINS) {
      this.advance();
      const value = this.parseValue();

      const condition: Condition = {
        column,
        function: 'contains',
        args: [value],
      };

      return { condition };
    }

    if (this.currentToken.type === TokenType.STARTS_WITH) {
      this.advance();
      const value = this.parseValue();

      const condition: Condition = {
        column,
        function: 'startsWith',
        args: [value],
      };

      return { condition };
    }

    if (this.currentToken.type === TokenType.ENDS_WITH) {
      this.advance();
      const value = this.parseValue();

      const condition: Condition = {
        column,
        function: 'endsWith',
        args: [value],
      };

      return { condition };
    }

    // Handle comparison operators
    let functionName: string;
    if (this.currentToken.type === TokenType.EQUALS) {
      functionName = 'equals';
      this.advance();
    } else if (this.currentToken.type === TokenType.NOT_EQUALS) {
      functionName = 'equals';
      this.advance();
      const value = this.parseValue();

      const condition: Condition = {
        column,
        function: functionName,
        args: [value],
      };

      return { condition, negate: true };
    } else if (this.currentToken.type === TokenType.GREATER_THAN) {
      functionName = 'greaterThan';
      this.advance();
    } else if (this.currentToken.type === TokenType.GREATER_THAN_EQUALS) {
      functionName = 'greaterThanOrEquals';
      this.advance();
    } else if (this.currentToken.type === TokenType.LESS_THAN) {
      functionName = 'lessThan';
      this.advance();
    } else if (this.currentToken.type === TokenType.LESS_THAN_EQUALS) {
      functionName = 'lessThanOrEquals';
      this.advance();
    } else {
      throw new FilterParseError(
        `Expected operator but got ${this.currentToken.type}`,
        this.currentToken.position
      );
    }

    const value = this.parseValue();

    const condition: Condition = {
      column,
      function: functionName,
      args: [value],
    };

    return { condition };
  }

  private parsePrimary(): Filter {
    // Handle parentheses
    if (this.currentToken.type === TokenType.LPAREN) {
      this.advance();
      const expr = this.parseExpression();
      this.expect(TokenType.RPAREN);
      return expr;
    }

    // Handle NOT
    if (this.currentToken.type === TokenType.NOT) {
      this.advance();
      const expr = this.parsePrimary();
      // Toggle negate flag
      return { ...expr, negate: !expr.negate };
    }

    // Parse condition
    return this.parseCondition();
  }

  private parseAndExpression(): Filter {
    let left = this.parsePrimary();

    while (this.currentToken.type === TokenType.AND) {
      this.advance();
      const right = this.parsePrimary();

      // Combine with AND
      left = {
        group: {
          op: 'AND',
          filters: [left, right],
        },
      };
    }

    return left;
  }

  private parseExpression(): Filter {
    let left = this.parseAndExpression();

    while (this.currentToken.type === TokenType.OR) {
      this.advance();
      const right = this.parseAndExpression();

      // Combine with OR
      left = {
        group: {
          op: 'OR',
          filters: [left, right],
        },
      };
    }

    return left;
  }

  public parse(): Filter {
    const result = this.parseExpression();
    this.expect(TokenType.EOF);
    return result;
  }
}

/**
 * Main entry point: Parse a filter query string into a Filter object
 *
 * @param input - The filter query string (e.g., "age > 18 AND status = 'active'")
 * @returns The parsed Filter object
 * @throws FilterParseError if the input is invalid
 *
 * @example
 * ```typescript
 * const filter = parseFilterQuery("age > 18 AND status = 'active'");
 * // Returns:
 * // {
 * //   group: {
 * //     op: 'AND',
 * //     filters: [
 * //       { condition: { column: 'age', function: 'greaterThan', args: [18] } },
 * //       { condition: { column: 'status', function: 'equals', args: ['active'] } }
 * //     ]
 * //   }
 * // }
 * ```
 */
export function parseFilterQuery(input: string): Filter {
  if (!input || input.trim().length === 0) {
    throw new FilterParseError('Empty filter query', 0);
  }

  const lexer = new FilterLexer(input);
  const tokens = lexer.tokenize();

  const parser = new FilterParser(tokens);
  return parser.parse();
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
