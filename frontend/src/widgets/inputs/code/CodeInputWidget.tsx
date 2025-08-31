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
import { useEventHandler } from '@/components/event-handler';
import { cn } from '@/lib/utils';
import { getHeight, getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import CopyToClipboardButton from '@/components/CopyToClipboardButton';
import { cpp } from '@codemirror/lang-cpp';
import { dbml } from './dbml-language';
import { createIvyCodeTheme } from './theme';
import { Extension } from '@codemirror/state';
import { EditorView, Decoration, DecorationSet } from '@codemirror/view';
import { StateField, StateEffect, RangeSetBuilder } from '@codemirror/state';

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
  Typescript: javascript({ typescript: true }),
  Tsx: javascript({ typescript: true, jsx: true }),
  Python: python,
  Sql: sql,
  Html: html,
  Css: css,
  Json: json,
  Dbml: dbml,
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
  // Create selection extension logic
  const createSelectionExtension = useCallback((): Extension => {
    // Effect to add selection decorations
    const addSelection = StateEffect.define<{ from: number; to: number }>();
    const clearSelections = StateEffect.define();

    // Decoration for selected text
    const selectionMark = Decoration.mark({
      class: 'ivy-selection-highlight',
      attributes: { 'data-selection': 'true' },
    });

    // State field to track selections
    const selectionField = StateField.define<DecorationSet>({
      create() {
        return Decoration.none;
      },
      update(selections, tr) {
        const builder = new RangeSetBuilder<Decoration>();

        // Clear existing selections
        selections.between(0, tr.newDoc.length, () => {});

        // Add new selections from the transaction
        for (const effect of tr.effects) {
          if (effect.is(addSelection)) {
            builder.add(effect.value.from, effect.value.to, selectionMark);
          } else if (effect.is(clearSelections)) {
            return Decoration.none;
          }
        }

        return builder.finish();
      },
      provide: f => EditorView.decorations.from(f),
    });

    // Extension to handle selection changes
    const selectionHandler = EditorView.updateListener.of(update => {
      if (update.selectionSet) {
        const { from, to } = update.state.selection.main;

        // Clear previous selections
        update.view.dispatch({
          effects: clearSelections.of(null),
        });

        // Add new selection if there is one
        if (from !== to) {
          update.view.dispatch({
            effects: addSelection.of({ from, to }),
          });
        }
      }
    });

    return [selectionField, selectionHandler];
  }, []);

  // Create theme and selection extension once and reuse them
  const themeExtension = useMemo(() => createIvyCodeTheme(), []);
  const selectionExtension = useMemo(
    () => createSelectionExtension(),
    [createSelectionExtension]
  );

  const extensions = useMemo(() => {
    const lang = language
      ? languageExtensions[language as keyof typeof languageExtensions]
      : undefined;
    const langExtension = lang
      ? [typeof lang === 'function' ? lang() : lang]
      : [];
    return [...langExtension, themeExtension, selectionExtension];
  }, [language, themeExtension, selectionExtension]);

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
