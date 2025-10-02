/**
 * FilterInput Component
 *
 * A controlled contentEditable input for building filter queries with auto-formatting.
 *
 * INTERACTION REQUIREMENTS:
 *
 * 1. AUTO-SPACING:
 *    - Spaces are automatically inserted for columns, operators, and separators
 *    - Manual spaces are ONLY allowed when typing values (for multi-word values)
 *    - Manual spaces are prevented in all other states
 *    - This ensures consistent word parsing and token recognition
 *
 * 2. TOKEN RECOGNITION & SAVING (STRICT STATE MACHINE):
 *    - Tokens follow a strict pattern: column → operator → value → separator → column...
 *    - Position % 4 determines expected token type (0=column, 1=operator, 2=value, 3=separator)
 *    - Only tokens matching the expected type are recognized and saved
 *    - Invalid tokens at wrong positions are ignored
 *    - Each token includes type information: 'column', 'operator', 'separator', or 'value'
 *    - Complete queries (columnName operator value) are matched and saved separately
 *
 * 3. EXPECTED INPUT FORMAT:
 *    - Single query: columnName operator value [Enter]
 *    - Example: age >= 18 [press Enter]
 *    - Multiple queries: columnName operator value [Enter] separator columnName operator value [Enter]
 *    - Example: name = John Doe [Enter] && age >= 25 [Enter]
 *    - Values support spaces for multi-word entries (e.g., "John Doe")
 *    - Press Enter or blur input to save value tokens
 *
 * 4. TOKEN REMOVAL:
 *    - Users can remove tokens by backspacing over them
 *    - Removed tokens are automatically cleaned from saved state
 *    - Incomplete queries are removed from matched queries
 *
 * 5. VISUAL STYLING:
 *    - Column names: blue (rgb(59, 130, 246))
 *    - Operators: purple (rgb(168, 85, 247))
 *    - Separators: green (rgb(34, 197, 94))
 *    - Values: default color (unstyled)
 *
 * 6. QUERY MATCHING:
 *    - Complete queries follow the pattern: column operator value
 *    - Queries are matched when all three parts are present consecutively
 *    - Matched queries are provided via onQueriesSaved callback
 *
 * 7. VALUE COMPLETION:
 *    - Values are saved when:
 *      a) User presses Enter key
 *      b) Input loses focus (blur event)
 *    - After saving, a space is auto-added and input is ready for separator token
 */

import React, {
  useRef,
  useEffect,
  useState,
  useMemo,
  useCallback,
} from 'react';

export type TokenType = 'column' | 'operator' | 'value' | 'separator';

export interface Token {
  value: string;
  type: TokenType;
  position: number;
}

export interface Query {
  column: string;
  operator: string;
  value: string;
}

interface FilterInputProps {
  columns: string[];
  onTokensSaved: (tokens: Token[]) => void;
  onQueriesSaved?: (queries: Query[]) => void;
}

export const FilterInput: React.FC<FilterInputProps> = ({
  columns,
  onTokensSaved,
  onQueriesSaved,
}) => {
  const [inputValue, setInputValue] = useState('');
  const [savedTokens, setSavedTokens] = useState<Token[]>([]);
  const [tokenPositions, setTokenPositions] = useState<Map<number, Token>>(
    new Map()
  );
  const inputRef = useRef<HTMLDivElement>(null);

  const OPERATORS = useMemo(() => ['=', '!=', '<', '>', '<=', '>='], []);
  const SEPARATORS = useMemo(() => ['&&', '||'], []);

  const isMatchingColumn = useCallback(
    (word: string): boolean => {
      return columns.some(col => col.toLowerCase() === word.toLowerCase());
    },
    [columns]
  );

  const isOperator = useCallback(
    (word: string): boolean => {
      return OPERATORS.includes(word);
    },
    [OPERATORS]
  );

  const isSeparator = useCallback(
    (word: string): boolean => {
      return SEPARATORS.includes(word);
    },
    [SEPARATORS]
  );

  const getExpectedTokenType = useCallback((tokenCount: number): TokenType => {
    const position = tokenCount % 4;
    switch (position) {
      case 0:
        return 'column';
      case 1:
        return 'operator';
      case 2:
        return 'value';
      case 3:
        return 'separator';
      default:
        return 'column';
    }
  }, []);

  const isTokenValid = useCallback(
    (word: string, expectedType: TokenType): boolean => {
      switch (expectedType) {
        case 'column':
          return isMatchingColumn(word);
        case 'operator':
          return isOperator(word);
        case 'separator':
          return isSeparator(word);
        case 'value':
          // Values are always valid (any text)
          return true;
        default:
          return false;
      }
    },
    [isMatchingColumn, isOperator, isSeparator]
  );

  const matchQueries = useCallback((tokens: Token[]): Query[] => {
    const queries: Query[] = [];

    // Look for pattern: column operator value
    for (let i = 0; i < tokens.length - 2; i++) {
      const token1 = tokens[i];
      const token2 = tokens[i + 1];
      const token3 = tokens[i + 2];

      if (
        token1.type === 'column' &&
        token2.type === 'operator' &&
        token3.type === 'value' &&
        token1.position + 1 === token2.position &&
        token2.position + 1 === token3.position
      ) {
        queries.push({
          column: token1.value,
          operator: token2.value,
          value: token3.value,
        });
      }
    }

    return queries;
  }, []);

  const handleInput = (e: React.FormEvent<HTMLDivElement>) => {
    const text = e.currentTarget.textContent || '';
    setInputValue(text);

    // Split text but keep track of unsaved value being typed
    const words = text
      .trim()
      .split(/\s+/)
      .filter(w => w);

    // Check if user is deleting tokens (words count is less than saved tokens)
    if (words.length < savedTokens.length) {
      // User deleted some tokens, update saved tokens to match
      const newTokens = savedTokens.slice(0, words.length);
      setSavedTokens(newTokens);

      const newPositions = new Map<number, Token>();
      newTokens.forEach((token, index) => {
        newPositions.set(index, token);
      });
      setTokenPositions(newPositions);

      onTokensSaved(newTokens);

      // Update queries
      const queries = matchQueries(newTokens);
      if (onQueriesSaved) {
        onQueriesSaved(queries);
      }

      renderStyledText(text);
      return;
    }

    // Check if user is editing an existing value token
    // This happens when the last saved token is a value and there are more words in input
    if (
      savedTokens.length > 0 &&
      savedTokens[savedTokens.length - 1].type === 'value' &&
      words.length >= savedTokens.length
    ) {
      // User is editing the last value token
      // Update the value token with the new text
      const valueWords = words.slice(savedTokens.length - 1);
      const newValueText = valueWords.join(' ');

      // Check if the value has changed
      if (newValueText !== savedTokens[savedTokens.length - 1].value) {
        const updatedToken: Token = {
          ...savedTokens[savedTokens.length - 1],
          value: newValueText,
        };

        const newTokens = [...savedTokens.slice(0, -1), updatedToken];
        setSavedTokens(newTokens);

        const newPositions = new Map(tokenPositions);
        newPositions.set(savedTokens.length - 1, updatedToken);
        setTokenPositions(newPositions);

        onTokensSaved(newTokens);

        // Update queries
        const queries = matchQueries(newTokens);
        if (onQueriesSaved) {
          onQueriesSaved(queries);
        }

        renderStyledText(text);
        return;
      }
    }

    // Determine what token type we're expecting based on saved token count
    const expectedType = getExpectedTokenType(savedTokens.length);

    // Check if current word being typed matches expected type
    const currentWord = words[words.length - 1];

    // Special case: if we're expecting a value but haven't saved it yet
    // AND the user types a separator, save the value first
    if (expectedType === 'value' && words.length > savedTokens.length) {
      // Check if any of the remaining words is a separator
      const remainingWords = words.slice(savedTokens.length);
      const separatorIndex = remainingWords.findIndex(word =>
        isTokenValid(word, 'separator')
      );

      if (separatorIndex !== -1) {
        // Found a separator, save the value before it
        const valueWords = remainingWords.slice(0, separatorIndex);
        const valueText = valueWords.join(' ');
        const separatorWord = remainingWords[separatorIndex];

        if (valueText && separatorWord) {
          // Save value token
          const valueToken: Token = {
            value: valueText,
            type: 'value',
            position: savedTokens.length,
          };

          // Save separator token
          const separatorToken: Token = {
            value: separatorWord,
            type: 'separator',
            position: savedTokens.length + 1,
          };

          e.preventDefault();
          const newText = text + ' ';
          if (inputRef.current) {
            inputRef.current.textContent = newText;
          }
          setInputValue(newText);

          const newTokens = [...savedTokens, valueToken, separatorToken];
          setSavedTokens(newTokens);

          const newPositions = new Map(tokenPositions);
          newPositions.set(savedTokens.length, valueToken);
          newPositions.set(savedTokens.length + 1, separatorToken);
          setTokenPositions(newPositions);

          onTokensSaved(newTokens);

          // Check for complete queries and notify
          const queries = matchQueries(newTokens);
          if (onQueriesSaved) {
            onQueriesSaved(queries);
          }

          // Move cursor to end
          const range = document.createRange();
          const selection = window.getSelection();
          range.selectNodeContents(inputRef.current as Node);
          range.collapse(false);
          selection?.removeAllRanges();
          selection?.addRange(range);

          renderStyledText(newText);
          return;
        }
      }
    }

    // For non-value tokens, check if they match and auto-space
    if (expectedType !== 'value' && currentWord) {
      const isValid = isTokenValid(currentWord, expectedType);

      if (isValid) {
        // Auto-save and add space for column/operator/separator
        e.preventDefault();
        const newText = text + ' ';
        if (inputRef.current) {
          inputRef.current.textContent = newText;
        }
        setInputValue(newText);

        const newToken: Token = {
          value: currentWord,
          type: expectedType,
          position: savedTokens.length,
        };

        const newTokens = [...savedTokens, newToken];
        setSavedTokens(newTokens);

        const newPositions = new Map(tokenPositions);
        newPositions.set(savedTokens.length, newToken);
        setTokenPositions(newPositions);

        onTokensSaved(newTokens);

        // Check for complete queries and notify
        const queries = matchQueries(newTokens);
        if (onQueriesSaved) {
          onQueriesSaved(queries);
        }

        // Move cursor to end
        const range = document.createRange();
        const selection = window.getSelection();
        range.selectNodeContents(inputRef.current as Node);
        range.collapse(false);
        selection?.removeAllRanges();
        selection?.addRange(range);

        renderStyledText(newText);
        return;
      }
    }

    // For values, just update the text (don't save until Enter or blur)
    renderStyledText(text);
  };

  const saveCurrentValue = useCallback(() => {
    const text = inputRef.current?.textContent || '';
    const words = text
      .trim()
      .split(/\s+/)
      .filter(w => w);

    const expectedType = getExpectedTokenType(savedTokens.length);

    // Only save if we're expecting a value and there's text to save
    if (expectedType === 'value' && words.length > 0) {
      // Get all the words after the last saved token
      const valueWords = words.slice(savedTokens.length);
      const valueText = valueWords.join(' ');

      if (valueText) {
        const newToken: Token = {
          value: valueText,
          type: 'value',
          position: savedTokens.length,
        };

        const newTokens = [...savedTokens, newToken];
        setSavedTokens(newTokens);

        const newPositions = new Map(tokenPositions);
        newPositions.set(savedTokens.length, newToken);
        setTokenPositions(newPositions);

        onTokensSaved(newTokens);

        // Check for complete queries and notify
        const queries = matchQueries(newTokens);
        if (onQueriesSaved) {
          onQueriesSaved(queries);
        }

        // Add space after value
        const newText = text + ' ';
        if (inputRef.current) {
          inputRef.current.textContent = newText;
        }
        setInputValue(newText);
        renderStyledText(newText);

        // Move cursor to end
        const range = document.createRange();
        const selection = window.getSelection();
        if (inputRef.current) {
          range.selectNodeContents(inputRef.current);
          range.collapse(false);
          selection?.removeAllRanges();
          selection?.addRange(range);
        }
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    savedTokens,
    tokenPositions,
    onTokensSaved,
    onQueriesSaved,
    matchQueries,
    getExpectedTokenType,
  ]);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>) => {
    const expectedType = getExpectedTokenType(savedTokens.length);

    // Allow manual spaces only when expecting a value (for multi-word values)
    if (e.key === ' ' && expectedType !== 'value') {
      e.preventDefault();
      return;
    }

    // Handle Enter key to save value
    if (e.key === 'Enter') {
      e.preventDefault();
      saveCurrentValue();
    }
  };

  const renderStyledText = useCallback(
    (text: string) => {
      if (!inputRef.current) return;

      // Save cursor position relative to the total text content
      const sel = window.getSelection();
      let cursorPos = 0;

      if (sel && sel.rangeCount > 0) {
        const range = sel.getRangeAt(0);
        const preCaretRange = range.cloneRange();
        preCaretRange.selectNodeContents(inputRef.current);
        preCaretRange.setEnd(range.endContainer, range.endOffset);
        cursorPos = preCaretRange.toString().length;
      }

      // Split text by spaces to get words, preserving spaces
      const parts = text.split(/(\s+)/);

      // Build HTML with styled spans
      const styledHTML = parts
        .map(part => {
          if (/^\s+$/.test(part)) {
            // It's whitespace, preserve it
            return part;
          }
          const isColumn = part && isMatchingColumn(part);
          const isOp = part && isOperator(part);
          const isSep = part && isSeparator(part);
          const style = isColumn
            ? 'color: rgb(59, 130, 246);'
            : isOp
              ? 'color: rgb(168, 85, 247);'
              : isSep
                ? 'color: rgb(34, 197, 94);'
                : '';
          return part ? `<span style="${style}">${part}</span>` : '';
        })
        .join('');

      inputRef.current.innerHTML = styledHTML || '<br>';

      // Restore cursor position
      // TODO: do we need this?
      try {
        const range = document.createRange();
        const selection = window.getSelection();

        const walker = document.createTreeWalker(
          inputRef.current,
          NodeFilter.SHOW_TEXT,
          null
        );

        let currentOffset = 0;
        let targetNode: Node | null = null;
        let targetOffset = 0;

        while (walker.nextNode()) {
          const node = walker.currentNode;
          const nodeLength = node.textContent?.length || 0;

          if (currentOffset + nodeLength >= cursorPos) {
            targetNode = node;
            targetOffset = cursorPos - currentOffset;
            break;
          }
          currentOffset += nodeLength;
        }

        if (targetNode) {
          range.setStart(
            targetNode,
            Math.min(targetOffset, targetNode.textContent?.length || 0)
          );
          range.collapse(true);
          selection?.removeAllRanges();
          selection?.addRange(range);
        } else {
          // Cursor is at the end
          range.selectNodeContents(inputRef.current);
          range.collapse(false);
          selection?.removeAllRanges();
          selection?.addRange(range);
        }
      } catch (e) {
        // Fallback: move cursor to end
        const range = document.createRange();
        const selection = window.getSelection();
        range.selectNodeContents(inputRef.current);
        range.collapse(false);
        selection?.removeAllRanges();
        selection?.addRange(range);
        console.error(e);
      }
    },
    [isMatchingColumn, isOperator, isSeparator, inputRef]
  );

  useEffect(() => {
    renderStyledText(inputValue);
  }, [
    columns,
    inputValue,
    savedTokens,
    tokenPositions,
    onTokensSaved,
    renderStyledText,
  ]);

  const handleBlur = useCallback(() => {
    saveCurrentValue();
  }, [saveCurrentValue]);

  return (
    <div className="relative">
      <div
        ref={inputRef}
        contentEditable
        role="textbox"
        aria-multiline="true"
        onInput={handleInput}
        onKeyDown={handleKeyDown}
        onBlur={handleBlur}
        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 min-h-[40px]"
        style={{
          whiteSpace: 'pre-wrap',
          wordBreak: 'break-word',
        }}
      />
    </div>
  );
};
