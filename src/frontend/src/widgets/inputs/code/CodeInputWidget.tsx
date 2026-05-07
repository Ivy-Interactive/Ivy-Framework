import React, { useState, useCallback, useMemo, Suspense, lazy, useEffect } from "react";
import { useOptimisticValue } from "../shared/useOptimisticValue";
import { useEventHandler } from "@/components/event-handler";
import { cn } from "@/lib/utils";
import { copyToClipboard } from "@/lib/clipboard";
import { getHeight, getWidth, inputStyles } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import { X, Copy, Loader2 } from "lucide-react";
import { xIconVariant } from "@/components/ui/input/text-input-variant";
import {
  keymap,
  EditorView,
  lineNumbers,
  highlightActiveLine,
  drawSelection,
} from "@codemirror/view";
import { history } from "@codemirror/commands";

import { useDebouncedCallback } from "use-debounce";
import { EMPTY_ARRAY } from "@/lib/constants";

// Lazy load CodeMirror and language extensions
const CodeMirror = lazy(() => import("@uiw/react-codemirror"));
const javascript = (options?: any) =>
  import("@codemirror/lang-javascript").then((m) => m.javascript(options));
const python = () => import("@codemirror/lang-python").then((m) => m.python());
const sql = () => import("@codemirror/lang-sql").then((m) => m.sql());
const html = () => import("@codemirror/lang-html").then((m) => m.html());
const css = () => import("@codemirror/lang-css").then((m) => m.css());
const json = () => import("@codemirror/lang-json").then((m) => m.json());
const markdown = () => import("@codemirror/lang-markdown").then((m) => m.markdown());
const yaml = () => import("@codemirror/lang-yaml").then((m) => m.yaml());
const cpp = () => import("@codemirror/lang-cpp").then((m) => m.cpp());
import { dbml } from "./dbml-language";
import { createIvyCodeTheme } from "./theme";
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
  autoFocus?: boolean;
  slots?: { Prefix?: React.ReactNode[]; Suffix?: React.ReactNode[] };
}

const languageExtensions = {
  Csharp: cpp,
  Javascript: () => javascript(),
  Typescript: () => javascript({ typescript: true }),
  Tsx: () => javascript({ typescript: true, jsx: true }),
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
  autoFocus,
  slots,
}) => {
  const eventHandler = useEventHandler();
  const [isFocused, setIsFocused] = useState(false);

  const serverValue = value || "";
  const [localValue, setLocalValue] = useOptimisticValue(serverValue, isFocused);

  const debouncedOnChange = useDebouncedCallback((value: string) => {
    if (events.includes("OnChange")) {
      eventHandler("OnChange", id, [value]);
    }
  }, 300);

  const handleChange = useCallback(
    (value: string) => {
      setLocalValue(value);
      debouncedOnChange(value);
    },
    [debouncedOnChange, setLocalValue],
  );

  const handleBlur = useCallback(() => {
    setIsFocused(false);
    if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
  }, [eventHandler, id, events]);

  const handleFocus = useCallback(() => {
    setIsFocused(true);
    if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
  }, [eventHandler, id, events]);

  const handleClear = useCallback(
    (e: React.MouseEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!events.includes("OnChange")) return;
      if (disabled) return;
      // For nullable inputs, set to null; otherwise set to empty string
      const clearedValue = nullable ? null : "";
      setLocalValue(clearedValue ?? "");
      eventHandler("OnChange", id, [clearedValue]);
    },
    [eventHandler, id, events, disabled, nullable, setLocalValue],
  );

  const hasValue = localValue && localValue.toString().trim() !== "";
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
      drawSelection(),
      history(),
      keymap.of([
        { key: "Ctrl-d", run: () => false },
        { key: "Ctrl-Shift-l", run: () => false },
      ]),
      EditorView.theme({}),
    ];
  }, []);

  const [langExtensions, setLangExtensions] = useState<any[]>([]);

  useEffect(() => {
    const loadLang = async () => {
      const lang = language
        ? languageExtensions[language as keyof typeof languageExtensions]
        : undefined;
      if (lang) {
        const ext = typeof lang === "function" ? await (lang as any)() : lang;
        setLangExtensions([ext]);
      } else {
        setLangExtensions([]);
      }
    };
    loadLang();
  }, [language]);

  const extensions = useMemo(() => {
    return [...langExtensions, minimalSetup, themeExtension];
  }, [langExtensions, minimalSetup, themeExtension]);

  const prefixContent = slots?.Prefix;
  const suffixContent = slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;

  const codeEditor = (
    <div style={styles} className="relative w-full h-full overflow-hidden">
      {(showCopy || showClear || invalid) && (
        <div className="absolute top-2 right-2 z-50 flex items-center">
          {showCopy && (
            <button
              type="button"
              onClick={() => copyToClipboard(localValue)}
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
          {invalid && <InvalidIcon message={invalid} className="pointer-events-auto p-1" />}
        </div>
      )}
      <Suspense
        fallback={
          <div className="h-full flex items-center justify-center bg-muted/20 animate-pulse rounded-field border border-input">
            <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
          </div>
        }
      >
        <CodeMirror
          value={localValue}
          extensions={extensions}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          placeholder={placeholder}
          editable={!disabled}
          autoFocus={autoFocus}
          data-gramm="false"
          className={cn(
            "h-full overflow-hidden",
            "[&_.cm-editor]:bg-transparent",
            "border border-input shadow-sm rounded-field",
            "dark:bg-white/5 dark:border-white/10",
            invalid && inputStyles.invalid,
            disabled && "opacity-50 cursor-not-allowed",
          )}
          height="100%"
          basicSetup={false}
        />
      </Suspense>
    </div>
  );

  if (!hasAffixes) return codeEditor;

  return (
    <div
      className={cn(
        "relative flex items-stretch rounded-field border border-input bg-transparent shadow-sm transition-colors dark:bg-white/5 dark:border-white/10",
        invalid && "border-destructive",
        disabled && "cursor-not-allowed opacity-50",
      )}
    >
      {hasPrefix && (
        <div className="flex items-center px-3 bg-muted text-muted-foreground border-r border-input rounded-tl-[var(--radius-fields)] rounded-bl-[var(--radius-fields)]">
          {prefixContent}
        </div>
      )}
      <div className="flex-1 min-w-0">{codeEditor}</div>
      {hasSuffix && (
        <div className="flex items-center px-3 bg-muted text-muted-foreground border-l border-input rounded-tr-[var(--radius-fields)] rounded-br-[var(--radius-fields)]">
          {suffixContent}
        </div>
      )}
    </div>
  );
};

export default CodeInputWidget;
