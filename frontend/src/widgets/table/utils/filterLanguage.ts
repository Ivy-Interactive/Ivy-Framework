import { StreamLanguage } from '@codemirror/language';
import { CharStream } from 'antlr4ng';
import { FilterQueryLexer } from '../grammar/generated/FilterQueryLexer';

/**
 * CodeMirror language extension for Filter Query syntax highlighting
 * Uses ANTLR4 lexer to tokenize the input
 */

// Map ANTLR4 token types to CodeMirror styles
const tokenTypeToStyle: Record<number, string> = {
  [FilterQueryLexer.AND]: 'keyword',
  [FilterQueryLexer.OR]: 'keyword',
  [FilterQueryLexer.NOT]: 'keyword',
  [FilterQueryLexer.IN]: 'keyword',
  [FilterQueryLexer.IS]: 'keyword',
  [FilterQueryLexer.CONTAINS]: 'keyword',
  [FilterQueryLexer.STARTS_WITH]: 'keyword',
  [FilterQueryLexer.ENDS_WITH]: 'keyword',

  [FilterQueryLexer.EQUALS]: 'operator',
  [FilterQueryLexer.NOT_EQUALS]: 'operator',
  [FilterQueryLexer.GREATER_THAN]: 'operator',
  [FilterQueryLexer.GREATER_THAN_EQUALS]: 'operator',
  [FilterQueryLexer.LESS_THAN]: 'operator',
  [FilterQueryLexer.LESS_THAN_EQUALS]: 'operator',

  [FilterQueryLexer.STRING_LITERAL]: 'string',
  [FilterQueryLexer.NUMBER_LITERAL]: 'number',
  [FilterQueryLexer.BOOLEAN_LITERAL]: 'atom',
  [FilterQueryLexer.NULL_LITERAL]: 'atom',

  [FilterQueryLexer.IDENTIFIER]: 'variableName',
  [FilterQueryLexer.QUOTED_IDENTIFIER]: 'variableName',

  [FilterQueryLexer.T__0]: 'paren', // (
  [FilterQueryLexer.T__1]: 'paren', // )
  [FilterQueryLexer.T__2]: 'punctuation', // ,
};

type AntlrToken = {
  type: number;
  text: string;
  start: number;
  stop: number;
};

const filterQueryMode = {
  startState: () => ({
    lexer: null as FilterQueryLexer | null,
    tokens: [] as AntlrToken[],
    tokenIndex: 0,
  }),

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  token: (stream: any, state: any) => {
    // Initialize lexer on first call or when starting a new line
    if (!state.lexer || stream.sol()) {
      const input = stream.string;
      const charStream = CharStream.fromString(input);
      state.lexer = new FilterQueryLexer(charStream);
      state.tokens = [];
      state.tokenIndex = 0;

      // Tokenize entire line
      let token = state.lexer.nextToken();
      while (token.type !== FilterQueryLexer.EOF) {
        if (token.type !== FilterQueryLexer.WS) {
          // Skip whitespace tokens
          state.tokens.push(token);
        }
        token = state.lexer.nextToken();
      }
    }

    // Handle whitespace
    if (stream.eatSpace()) {
      return null;
    }

    // Find the next token that matches the current position
    while (state.tokenIndex < state.tokens.length) {
      const token = state.tokens[state.tokenIndex];
      const tokenStart = token.column;
      const tokenEnd = tokenStart + token.text.length;

      if (stream.pos >= tokenStart && stream.pos < tokenEnd) {
        // Advance stream to end of this token
        stream.pos = tokenEnd;
        state.tokenIndex++;
        return tokenTypeToStyle[token.type] || null;
      } else if (stream.pos < tokenStart) {
        // We're before this token, advance to it
        stream.pos = tokenStart;
        return null;
      } else {
        // We're past this token, try the next one
        state.tokenIndex++;
      }
    }

    // No more tokens, consume rest of line
    stream.skipToEnd();
    return null;
  },
};

export const filterQueryLanguage = () => StreamLanguage.define(filterQueryMode);
