import React, { useState, useCallback, useMemo, useEffect } from 'react';
import CodeMirror from '@uiw/react-codemirror';
import { javascript } from '@codemirror/lang-javascript';
import { python } from '@codemirror/lang-python';
import { sql } from '@codemirror/lang-sql';
import { html } from '@codemirror/lang-html';
import { css } from '@codemirror/lang-css';
import { json } from '@codemirror/lang-json';
import { useEventHandler } from '@/components/EventHandlerContext';
import { cn } from '@/lib/utils';
import { getHeight, getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import CopyToClipboardButton from '@/components/CopyToClipboardButton';
import './CodeInputWidget.css';
import { StreamLanguage } from '@codemirror/language';
import { cpp } from '@codemirror/lang-cpp';

const dbmlMode = {
  startState: () => ({}),
  // eslint-disable-next-line @typescript-eslint/no-explicit-any,
  token: (stream: any) => {
    if (stream.match(/\/\//)) {
      stream.skipToEnd();
      return 'comment';
    }

    if (stream.match(/"(.*?)"/) || stream.match(/'(.*?)'/)) {
      return 'string';
    }

    if (stream.match(/\b(Table|Ref|Enum|Project|TableGroup|Note)\b/i)) {
      return 'keyword';
    }

    if (stream.match(/\b(int|uuid|varchar|boolean|text|datetime)\b/i)) {
      return 'typeName';
    }

    if (stream.match(/\b(primary key|not null|unique|increment)\b/i)) {
      return 'attribute';
    }

    if (stream.match(/[{}[\](),;]/)) {
      return 'bracket';
    }

    if (stream.match(/[a-zA-Z_][\w-]*/)) {
      return 'variableName';
    }

    stream.next();
    return null;
  },
};

export const dbml = () => StreamLanguage.define(dbmlMode);

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
}

const languageExtensions = {
  Csharp: cpp,
  Javascript: javascript,
  Python: python,
  Sql: sql,
  Html: html,
  Css: css,
  Json: json,
  Dbml: dbml,
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
  events,
}) => {
  const eventHandler = useEventHandler();
  const [localValue, setLocalValue] = useState(value || '');
  const [isFocused, setIsFocused] = useState(false);

  // Update local value when server value changes and control is not focused
  useEffect(() => {
    if (!isFocused && value !== localValue) {
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
  const extensions = useMemo(() => {
    const lang = language
      ? languageExtensions[language as keyof typeof languageExtensions]
      : undefined;
    return lang ? [lang()] : [];
  }, [language]);

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
        basicSetup={{
          lineNumbers: true,
          highlightActiveLine: true,
          foldGutter: false,
        }}
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
