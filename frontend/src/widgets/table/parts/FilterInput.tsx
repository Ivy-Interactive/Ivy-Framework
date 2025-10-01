import React, {
  useRef,
  useEffect,
  useState,
  useMemo,
  useCallback,
} from 'react';

interface FilterInputProps {
  columns: string[];
  onTokensSaved: (tokens: string[]) => void;
}

export const FilterInput: React.FC<FilterInputProps> = ({
  columns,
  onTokensSaved,
}) => {
  const [inputValue, setInputValue] = useState('');
  const [savedTokens, setSavedTokens] = useState<string[]>([]);
  const [tokenPositions, setTokenPositions] = useState<Map<number, string>>(
    new Map()
  );
  const inputRef = useRef<HTMLDivElement>(null);

  const OPERATORS = useMemo(() => ['=', '!=', '<', '>', '<=', '>='], []);

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

  const handleInput = (e: React.FormEvent<HTMLDivElement>) => {
    const text = e.currentTarget.textContent || '';
    setInputValue(text);

    // Check all words and collect matching columns that were previously saved
    const words = text
      .trim()
      .split(/\s+/)
      .filter(w => w);
    const newTokenPositions = new Map<number, string>();
    const newSavedTokens: string[] = [];

    // Build a set of currently saved tokens for quick lookup
    const savedTokenSet = new Set(Array.from(tokenPositions.values()));

    // Check if the current word (last word being typed) matches
    const currentWord = words[words.length - 1];
    if (
      currentWord &&
      (isMatchingColumn(currentWord) || isOperator(currentWord)) &&
      !savedTokenSet.has(currentWord)
    ) {
      // Auto-save and add space
      e.preventDefault();
      const newText = text + ' ';
      if (inputRef.current) {
        inputRef.current.textContent = newText;
      }
      setInputValue(newText);

      const newTokens = [...savedTokens, currentWord];
      setSavedTokens(newTokens);

      const newPositions = new Map(tokenPositions);
      newPositions.set(words.length - 1, currentWord);
      setTokenPositions(newPositions);

      onTokensSaved(newTokens);

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

    words.forEach((word, index) => {
      if (
        word &&
        (isMatchingColumn(word) || isOperator(word)) &&
        savedTokenSet.has(word)
      ) {
        // This matching word was previously saved, keep it
        newTokenPositions.set(index, word);
        newSavedTokens.push(word);
      }
    });

    // Update saved tokens if they changed
    const tokensChanged =
      newSavedTokens.length !== savedTokens.length ||
      newSavedTokens.some((t, i) => t !== savedTokens[i]);

    if (tokensChanged) {
      setSavedTokens(newSavedTokens);
      setTokenPositions(newTokenPositions);
      onTokensSaved(newSavedTokens);
    }

    renderStyledText(text);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>) => {
    // Prevent manual spaces
    if (e.key === ' ') {
      e.preventDefault();
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
          const style = isColumn
            ? 'color: rgb(59, 130, 246);'
            : isOp
              ? 'color: rgb(168, 85, 247);'
              : '';
          return part ? `<span style="${style}">${part}</span>` : '';
        })
        .join('');

      inputRef.current.innerHTML = styledHTML || '<br>';

      // Restore cursor position
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
    [isMatchingColumn, isOperator, inputRef]
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

  return (
    <div className="relative">
      <div
        ref={inputRef}
        contentEditable
        role="textbox"
        aria-multiline="true"
        onInput={handleInput}
        onKeyDown={handleKeyDown}
        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 min-h-[40px]"
        style={{
          whiteSpace: 'pre-wrap',
          wordBreak: 'break-word',
        }}
      />
    </div>
  );
};
