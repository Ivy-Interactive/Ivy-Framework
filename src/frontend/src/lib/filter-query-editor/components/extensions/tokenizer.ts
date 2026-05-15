/**
 * ANTLR lexer integration for tokenizing query text
 */

import { CharStream, CommonTokenStream, Token } from "antlr4ng";
import { FiltersLexer } from "../../generated/FiltersLexer";

/**
 * Token type enum matching ANTLR token types
 */
export enum TokenType {
  FIELD = "field",
  STRING = "string",
  NUMBER = "number",
  BOOLEAN = "boolean",
  KEYWORD = "keyword",
  OPERATOR = "operator",
  BRACKET = "bracket",
  UNKNOWN = "unknown",
}

/**
 * Simplified token for highlighting
 */
export interface HighlightToken {
  type: TokenType;
  start: number;
  end: number;
  text: string;
}

/**
 * Map ANTLR token types to our token types
 */
const TOKEN_TYPE_MAP: Record<number, TokenType> = {
  [FiltersLexer.FIELD]: TokenType.FIELD,
  [FiltersLexer.STRING]: TokenType.STRING,
  [FiltersLexer.DIGITS]: TokenType.NUMBER,
  [FiltersLexer.TRUE]: TokenType.BOOLEAN,
  [FiltersLexer.FALSE]: TokenType.BOOLEAN,
  [FiltersLexer.AND]: TokenType.KEYWORD,
  [FiltersLexer.OR]: TokenType.KEYWORD,
  [FiltersLexer.NOT]: TokenType.KEYWORD,
  [FiltersLexer.IS]: TokenType.KEYWORD,
  [FiltersLexer.BLANK]: TokenType.KEYWORD,
  [FiltersLexer.EQUALS]: TokenType.OPERATOR,
  [FiltersLexer.EQUAL]: TokenType.OPERATOR,
  [FiltersLexer.EQ]: TokenType.OPERATOR,
  [FiltersLexer.EQ2]: TokenType.OPERATOR,
  [FiltersLexer.NEQ]: TokenType.OPERATOR,
  [FiltersLexer.GT]: TokenType.OPERATOR,
  [FiltersLexer.GE]: TokenType.OPERATOR,
  [FiltersLexer.LT]: TokenType.OPERATOR,
  [FiltersLexer.LE]: TokenType.OPERATOR,
  [FiltersLexer.GREATER]: TokenType.OPERATOR,
  [FiltersLexer.LESS]: TokenType.OPERATOR,
  [FiltersLexer.THAN]: TokenType.OPERATOR,
  [FiltersLexer.CONTAINS]: TokenType.OPERATOR,
  [FiltersLexer.STARTS]: TokenType.OPERATOR,
  [FiltersLexer.ENDS]: TokenType.OPERATOR,
  [FiltersLexer.WITH]: TokenType.OPERATOR,
  [FiltersLexer.LPAREN]: TokenType.BRACKET,
  [FiltersLexer.RPAREN]: TokenType.BRACKET,
};

/**
 * Tokenize query text using ANTLR lexer
 */
export function tokenizeQuery(text: string): HighlightToken[] {
  if (!text) return [];

  try {
    // Create lexer
    const charStream = CharStream.fromString(text);
    const lexer = new FiltersLexer(charStream);

    // Remove error listeners to prevent console spam
    lexer.removeErrorListeners();

    // Get all tokens
    const tokenStream = new CommonTokenStream(lexer);
    tokenStream.fill();
    const tokens = tokenStream.getTokens();

    // Convert to highlight tokens
    const highlightTokens: HighlightToken[] = [];

    for (const token of tokens) {
      // Skip EOF token
      if (token.type === Token.EOF) continue;

      // Skip whitespace (not in our token map)
      if (token.type === FiltersLexer.WS) continue;

      const tokenType = TOKEN_TYPE_MAP[token.type] || TokenType.UNKNOWN;

      highlightTokens.push({
        type: tokenType,
        start: token.start,
        end: token.stop + 1,
        text: token.text || "",
      });
    }

    return highlightTokens;
  } catch (error) {
    // If tokenization fails, return empty array
    console.warn("Tokenization failed:", error);
    return [];
  }
}

/**
 * Get CSS class for token type
 */
export function getTokenClass(type: TokenType, theme: "light" | "dark"): string {
  const baseClass = `cm-${type}`;
  return `${baseClass} ${baseClass}--${theme}`;
}
