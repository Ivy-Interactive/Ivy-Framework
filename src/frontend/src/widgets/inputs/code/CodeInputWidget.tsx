import React, {
  useState,
  useCallback,
  useMemo,
  useEffect,
  useRef,
} from 'react';
import CodeMirror from '@uiw/react-codemirror';
import { javascript } from '@codemirror/lang-javascript';
import { python } from '@codemirror/lang-python';
import { sql } from '@codemirror/lang-sql';
import { html } from '@codemirror/lang-html';
import { css } from '@codemirror/lang-css';
import { json } from '@codemirror/lang-json';
import { markdown } from '@codemirror/lang-markdown';
import { yaml } from '@codemirror/lang-yaml';
import { useEventHandler } from '@/components/event-handler';
import { cn } from '@/lib/utils';
import { getHeight, getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { cpp } from '@codemirror/lang-cpp';
import { dbml } from './dbml-language';
import { createIvyCodeTheme } from './theme';
import { Densities } from '@/types/density';
import { X, Copy } from 'lucide-react';
import { xIconVariant } from '@/components/ui/input/text-input-variant';
import {
  keymap,
  EditorView,
  lineNumbers,
  highlightActiveLine,
} from '@codemirror/view';
import { history } from '@codemirror/commands';

import { useDebouncedCallback } from 'use-debounce';

const EMPTY_ARRAY: never[] = [];

interface CodeInputWidgetProps {
  id: string;
  placeholder?: string;
  value?: string;
  language?: string;
  disabled: boolean;
  invalid?: string;
  nullable?: boolean;
  events: string[];
  width?: string;
  height?: string;
  density?: Densities;
}

const languageExtensions = {
  Csharp: cpp,
  Javascript: javascript,
  Typescript: javascript({ typescript: true }),
  Tsx: javascript({ typescript: true, jsx: true }),
  Python: python,
  Sql: sql,
  Html: html,
  Css: css,
  Json: json,
  Dbml: dbml,
  Markdown: markdown,
  Text: undefined,
  Yaml: yaml,
  Csv: undefined,
};

export const CodeInputWidget: React.FC<CodeInputWidgetProps> = ({
  id,
  placeholder,
  value,
  language,
  disabled = false,
  invalid,
  nullable = false,
  width,
  height,
  density = Densities.Medium,
  events = EMPTY_ARRAY,
}) => {
  const eventHandler = useEventHandler();
  const [localValue, setLocalValue] = useState(value || '');
  const [isFocused, setIsFocused] = useState(false);
  const localValueRef = useRef(localValue);

  // Update local value when server value changes and control is not focused
  useEffect(() => {
    if (!isFocused && value !== localValueRef.current) {
      queueMicrotask(() => setLocalValue(value || ''));
    }
  }, [value, isFocused]);

  // Keep ref in sync with state
  useEffect(() => {
    localValueRef.current = localValue;
  }, [localValue]);

  const debouncedOnChange = useDebouncedCallback((value: string) => {
    if (events.includes('OnChange')) {
      eventHandler('OnChange', id, [value]);
    }
  }, 300);

  const handleChange = useCallback(
    (value: string) => {
      setLocalValue(value);
      debouncedOnChange(value);
    },
    [debouncedOnChange]
  );

  const handleBlur = useCallback(() => {
    setIsFocused(false);
    if (events.includes('OnBlur')) eventHandler('OnBlur', id, []);
  }, [eventHandler, id, events]);

  const handleFocus = useCallback(() => {
    setIsFocused(true);
  }, []);

  const handleClear = useCallback(
    (e: React.MouseEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!events.includes('OnChange')) return;
      if (disabled) return;
      // For nullable inputs, set to null; otherwise set to empty string
      const clearedValue = nullable ? null : '';
      setLocalValue(clearedValue ?? '');
      eventHandler('OnChange', id, [clearedValue]);
    },
    [eventHandler, id, events, disabled, nullable]
  );

  const hasValue = localValue && localValue.toString().trim() !== '';
  const showClear = nullable && !disabled && hasValue;
  // Copy button always shows when there's a value (doesn't depend on showCopyButton prop)
  const showCopy = hasValue;

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  // Create theme extension once and reuse it
  const themeExtension = useMemo(() => createIvyCodeTheme(density), [density]);

  // Minimal setup without search features
  const minimalSetup = useMemo(() => {
    return [
      lineNumbers(),
      highlightActiveLine(),
      history(),
      keymap.of([
        { key: 'Ctrl-d', run: () => false },
        { key: 'Ctrl-Shift-l', run: () => false },
      ]),
      EditorView.theme({}),
    ];
  }, []);

  const extensions = useMemo(() => {
    const lang = language
      ? languageExtensions[language as keyof typeof languageExtensions]
      : undefined;
    const langExtension = lang
      ? [typeof lang === 'function' ? lang() : lang]
      : [];
    return [...langExtension, minimalSetup, themeExtension];
  }, [language, minimalSetup, themeExtension]);

  return (
    <div style={styles} className="relative w-full h-full overflow-hidden">
      {(showCopy || showClear || invalid) && (
        <div className="absolute top-2 right-2 z-50 flex items-center">
          {showCopy && (
            <button
              type="button"
              onClick={() => navigator.clipboard.writeText(localValue)}
              aria-label="Copy to clipboard"
              className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
            >
              <Copy className={xIconVariant({ density })} />
            </button>
          )}
          {showClear && (
            <button
              type="button"
              tabIndex={-1}
              aria-label="Clear"
              onClick={handleClear}
              className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
            >
              <X className={xIconVariant({ density })} />
            </button>
          )}
          {/* Invalid icon - rightmost */}
          {invalid && (
            <InvalidIcon
              message={invalid}
              className="pointer-events-auto p-1"
            />
          )}
        </div>
      )}
      <CodeMirror
        value={localValue}
        extensions={extensions}
        onChange={handleChange}
        onBlur={handleBlur}
        onFocus={handleFocus}
        placeholder={placeholder}
        editable={!disabled}
        data-gramm="false"
        className={cn(
          'h-full',
          'border',
          invalid && inputStyles.invalid,
          disabled && 'opacity-50 cursor-not-allowed'
        )}
        height="100%"
        basicSetup={false}
      />
    </div>
  );
};

export default CodeInputWidget;
