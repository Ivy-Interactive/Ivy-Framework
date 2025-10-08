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
import { useEventHandler } from '@/components/event-handler';
import { cn } from '@/lib/utils';
import { getHeight, getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import CopyToClipboardButton from '@/components/CopyToClipboardButton';
import { cpp } from '@codemirror/lang-cpp';
import { dbml } from './dbml-language';
import { createIvyCodeTheme } from './theme';
import { Sizes } from '@/types/sizes';
import { Extension } from '@codemirror/state';
import { EditorView, keymap } from '@codemirror/view';
import { lineNumbers, highlightActiveLine } from '@codemirror/view';
import { defaultKeymap, history, historyKeymap } from '@codemirror/commands';
import { bracketMatching, indentOnInput } from '@codemirror/language';

interface CodeInputWidgetProps {
  id: string;
  placeholder?: string;
  value?: string;
  language?: string;
  disabled: boolean;
  invalid?: string;
  showCopyButton?: boolean;
  events: string[];
  width?: string;
  height?: string;
  size?: Sizes;
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
};

export const CodeInputWidget: React.FC<CodeInputWidgetProps> = ({
  id,
  placeholder,
  value,
  language,
  disabled,
  invalid,
  showCopyButton = false,
  width,
  height,
  size = Sizes.Medium,
  events,
}) => {
  const eventHandler = useEventHandler();
  const [localValue, setLocalValue] = useState(value || '');
  const [isFocused, setIsFocused] = useState(false);
  const localValueRef = useRef(localValue);

  // Keep ref in sync with state
  localValueRef.current = localValue;

  // Update local value when server value changes and control is not focused
  useEffect(() => {
    if (!isFocused && value !== localValueRef.current) {
      setLocalValue(value || '');
    }
  }, [value, isFocused]);

  const handleChange = useCallback(
    (value: string) => {
      setLocalValue(value);
      if (events.includes('OnChange')) eventHandler('OnChange', id, [value]);
    },
    [eventHandler, id, events]
  );

  const handleBlur = useCallback(() => {
    setIsFocused(false);
    if (events.includes('OnBlur')) eventHandler('OnBlur', id, []);
  }, [eventHandler, id, events]);

  const handleFocus = useCallback(() => {
    setIsFocused(true);
  }, []);

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  // Create theme extension once and reuse it
  const themeExtension = useMemo(() => createIvyCodeTheme(size), [size]);

  // Custom basic setup without search/selection matching features
  const customBasicSetup = useMemo((): Extension => {
    return [
      lineNumbers(),
      highlightActiveLine(),
      // No foldGutter to avoid any potential issues
      // No searchKeymap to prevent selection matching
      history(),
      indentOnInput(),
      bracketMatching(),
      keymap.of([
        ...defaultKeymap,
        ...historyKeymap,
        // Explicitly exclude searchKeymap and lintKeymap
      ]),
    ];
  }, []);

  // Extension to disable multiple selection behavior and selection matching
  const disableMultipleSelectionExtension = useMemo((): Extension => {
    return [
      // Disable all search and selection matching keymaps
      keymap.of([]), // Empty keymap to override default search keymaps
      // Disable specific problematic commands
      keymap.of([
        {
          key: 'Ctrl-d',
          run: () => false, // Disable select next occurrence
        },
        {
          key: 'Ctrl-Shift-l',
          run: () => false, // Disable select all occurrences
        },
        {
          key: 'Ctrl-f',
          run: () => false, // Disable search
        },
        {
          key: 'Ctrl-g',
          run: () => false, // Disable find next
        },
        {
          key: 'Ctrl-Shift-g',
          run: () => false, // Disable find previous
        },
      ]),
      // Override mouse behavior to prevent multiple selections
      EditorView.domEventHandlers({
        mousedown: event => {
          // If Ctrl/Cmd is held, prevent the default behavior
          if (event.ctrlKey || event.metaKey) {
            event.preventDefault();
            return false;
          }
        },
      }),
      // Disable selection matching completely
      EditorView.theme({
        '.cm-selectionMatch': {
          backgroundColor: 'transparent !important',
          color: 'inherit !important',
        },
      }),
    ];
  }, []);

  const extensions = useMemo(() => {
    const lang = language
      ? languageExtensions[language as keyof typeof languageExtensions]
      : undefined;
    const langExtension = lang
      ? [typeof lang === 'function' ? lang() : lang]
      : [];
    return [
      ...langExtension,
      customBasicSetup,
      themeExtension,
      disableMultipleSelectionExtension,
    ];
  }, [
    language,
    customBasicSetup,
    themeExtension,
    disableMultipleSelectionExtension,
  ]);

  return (
    <div style={styles} className="relative w-full h-full overflow-hidden">
      {showCopyButton && (
        <div className="absolute top-2 right-2 z-10 rounded-md hover:bg-accent transition-colors duration-200">
          <CopyToClipboardButton
            textToCopy={localValue}
            aria-label="Copy to clipboard"
          />
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
      {invalid && (
        <div
          className={cn(
            'absolute top-2.5',
            showCopyButton ? 'right-14' : 'right-4'
          )}
        >
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
};

export default CodeInputWidget;
