/**
 * Error collector for ANTLR parsing errors
 */

import {
  ANTLRErrorListener,
  RecognitionException,
  Recognizer,
  Token,
  ATNSimulator,
  ATNConfigSet,
  DFA,
  BitSet,
} from "antlr4ng";
import { ParseError } from "../types/parser";

/**
 * Custom error listener that collects parse errors
 */
export class ErrorCollector implements ANTLRErrorListener {
  private errors: ParseError[] = [];

  /**
   * Get collected errors
   */
  getErrors(): ParseError[] {
    return this.errors;
  }

  /**
   * Clear all errors
   */
  clear(): void {
    this.errors = [];
  }

  /**
   * Handle syntax errors from ANTLR
   */
  syntaxError<S extends Token, T extends ATNSimulator>(
    _recognizer: Recognizer<T>,
    offendingSymbol: S | null,
    _line: number,
    charPositionInLine: number,
    msg: string,
    _e: RecognitionException | null,
  ): void {
    // Calculate start and end positions
    let start = charPositionInLine;
    let end = charPositionInLine + 1;

    if (offendingSymbol) {
      start = offendingSymbol.start;
      end = offendingSymbol.stop + 1;
    }

    this.errors.push({
      message: msg,
      start,
      end,
      severity: "error",
    });
  }

  /**
   * Report ambiguity in the grammar (required by interface but not used)
   */
  reportAmbiguity<T extends ATNSimulator>(
    _recognizer: Recognizer<T>,
    _dfa: DFA,
    _startIndex: number,
    _stopIndex: number,
    _exact: boolean,
    _ambigAlts: BitSet | undefined,
    _configs: ATNConfigSet,
  ): void {
    // We don't report ambiguities as errors
  }

  /**
   * Report attempting full context (required by interface but not used)
   */
  reportAttemptingFullContext<T extends ATNSimulator>(
    _recognizer: Recognizer<T>,
    _dfa: DFA,
    _startIndex: number,
    _stopIndex: number,
    _conflictingAlts: BitSet | undefined,
    _configs: ATNConfigSet,
  ): void {
    // We don't report full context attempts
  }

  /**
   * Report context sensitivity (required by interface but not used)
   */
  reportContextSensitivity<T extends ATNSimulator>(
    _recognizer: Recognizer<T>,
    _dfa: DFA,
    _startIndex: number,
    _stopIndex: number,
    _prediction: number,
    _configs: ATNConfigSet,
  ): void {
    // We don't report context sensitivity
  }

  /**
   * Add a semantic error with position information
   */
  addSemanticError(message: string, token?: Token): void {
    let start = 0;
    let end = 0;

    if (token) {
      start = token.start;
      end = token.stop + 1;
    }

    this.errors.push({
      message,
      start,
      end,
      severity: "error",
    });
  }

  /**
   * Add a warning
   */
  addWarning(message: string, token?: Token): void {
    let start = 0;
    let end = 0;

    if (token) {
      start = token.start;
      end = token.stop + 1;
    }

    this.errors.push({
      message,
      start,
      end,
      severity: "warning",
    });
  }
}
