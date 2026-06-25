import React, { useState, useCallback, useMemo, Suspense, lazy, useEffect } from "react";
import { useOptimisticValue } from "../shared/useOptimisticValue";
import { useEventHandler } from "@/components/event-handler";
import { cn } from "@/lib/utils";
import { copyToClipboard } from "@/lib/clipboard";
import { getHeight, getWidth } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import { boolInputRowMinHeightVariant } from "@/components/ui/input/bool-input-variant";
import { X, Copy, Loader2 } from "lucide-react";
import {
  normalizeInputDensity,
  textInputAffixCellClasses,
  textInputAffixGrowContentColumnClasses,
  textInputAffixInvalidIconClasses,
  textInputAffixPrefixCellClasses,
  textInputAffixSuffixCellClasses,
  textInputSuffixWithTrailingClusterClasses,
  textInputFieldShellClasses,
  textInputSuffixGlyphSlotClasses,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
  textareaTrailingOverlayClasses,
} from "@/components/ui/input/text-input-variant";
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

/** Affix strip for tall editor — top-aligned, density-scaled seam gaps. */
function codeInputAffixCellClasses(
  side: "prefix" | "suffix",
  density: Densities,
  content?: React.ReactNode[],
  options: { showTrailing?: boolean } = {},
): string {
  const densityKey = normalizeInputDensity(density);
  const chrome = cn(
    "relative z-10 shrink-0 self-start overflow-visible",
    boolInputRowMinHeightVariant({ density: densityKey }),
    options.showTrailing && "[&_button]:overflow-visible [&_[data-invalid-icon]]:overflow-visible",
  );
  if (side === "prefix") {
    return cn(textInputAffixPrefixCellClasses(density, content), chrome);
  }
  return cn(
    textInputAffixSuffixCellClasses(density, content, { showTrailing: options.showTrailing }),
    chrome,
  );
}

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
  const densityKey = normalizeInputDensity(density);

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
      const clearedValue = nullable ? null : "";
      setLocalValue(clearedValue ?? "");
      eventHandler("OnChange", id, [clearedValue]);
    },
    [eventHandler, id, events, disabled, nullable, setLocalValue],
  );

  const prefixContent = slots?.Prefix;
  const suffixContent = slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  const trailingBesideSuffix = hasSuffix;

  const hasValue = localValue && localValue.toString().trim() !== "";
  const showClear = nullable && !disabled && hasValue;
  const showCopy = hasValue;
  const showTrailing = showCopy || showClear || Boolean(invalid);
  const trailingControlCount = [showCopy, showClear, Boolean(invalid)].filter(Boolean).length;
  /** Trailing in its own affix column (aligned with prefix) — not overlaid on tall editor. */
  const trailingInAffixCell = hasAffixes && !trailingBesideSuffix && showTrailing;
  const trailingInOverlay = !hasAffixes && showTrailing;

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const themeExtension = useMemo(() => createIvyCodeTheme(density), [density]);

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

  const trailingCluster = (overlay: boolean) => (
    <>
      {showCopy && (
        <button
          type="button"
          onClick={() => copyToClipboard(localValue)}
          aria-label="Copy to clipboard"
          className={cn(
            textInputTrailingIconButtonClasses(overlay, density),
            "opacity-0 group-hover:opacity-100 focus-within:opacity-100 transition-opacity duration-200",
          )}
        >
          <Copy className={textInputTrailingIconSizeVariant({ density: densityKey })} />
        </button>
      )}
      {showClear && (
        <button
          type="button"
          tabIndex={-1}
          aria-label="Clear"
          onClick={handleClear}
          className={textInputTrailingIconButtonClasses(overlay, density)}
        >
          <X className={textInputTrailingIconSizeVariant({ density: densityKey })} />
        </button>
      )}
      {invalid && (
        <InvalidIcon
          message={invalid}
          className={
            overlay
              ? textInputTrailingInvalidSlotClasses(true, density)
              : textInputAffixInvalidIconClasses()
          }
          iconClassName={textInputTrailingIconSizeVariant({ density: densityKey })}
        />
      )}
    </>
  );

  const codeEditor = () => (
    <div className="relative h-full min-h-0 w-full overflow-hidden">
      {trailingInOverlay && (
        <div className={textareaTrailingOverlayClasses(density)}>{trailingCluster(true)}</div>
      )}
      <Suspense
        fallback={
          <div
            className={cn(
              "flex h-full items-center justify-center bg-muted/20 animate-pulse",
              !hasAffixes && "rounded-field border border-input dark:border-white/10",
            )}
          >
            <Loader2 className="size-6 animate-spin text-muted-foreground" />
          </div>
        }
      >
        <CodeMirror
          value={localValue}
          extensions={extensions}
          theme="none"
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          placeholder={placeholder}
          editable={!disabled}
          autoFocus={autoFocus}
          data-gramm="false"
          className={cn(
            "h-full overflow-hidden",
            "[&_.cm-editor]:border-0 [&_.cm-editor]:bg-transparent! [&_.cm-editor]:shadow-none",
            "[&_.cm-scroller]:bg-transparent! [&_.cm-content]:bg-transparent! [&_.cm-gutters]:bg-transparent!",
            trailingInOverlay && "[&_.cm-content]:pr-8 [&_.cm-placeholder]:pr-8",
            trailingInOverlay &&
              (showClear || invalid) &&
              "[&_.cm-content]:pr-16 [&_.cm-placeholder]:pr-16",
            hasAffixes && "[&_.cm-editor]:rounded-none",
            hasAffixes && hasPrefix && "[&_.cm-editor]:rounded-l-none",
            hasAffixes && (hasSuffix || trailingInAffixCell) && "[&_.cm-editor]:rounded-r-none",
            disabled && "opacity-50 cursor-not-allowed",
          )}
          height="100%"
          basicSetup={false}
        />
      </Suspense>
    </div>
  );

  if (!hasAffixes) {
    return (
      <div
        style={styles}
        className={cn(
          "relative flex w-full flex-col overflow-hidden group",
          textInputFieldShellClasses({ focused: isFocused, invalid, disabled }),
        )}
      >
        <div className="min-h-0 min-w-0 flex-1 overflow-hidden">{codeEditor()}</div>
      </div>
    );
  }

  return (
    <div
      style={styles}
      className={cn(
        textInputFieldShellClasses({
          focused: isFocused,
          invalid,
          disabled,
        }),
        "group",
      )}
    >
      {hasPrefix && (
        <div className={codeInputAffixCellClasses("prefix", density, prefixContent)}>
          {prefixContent}
        </div>
      )}
      <div className={textInputAffixGrowContentColumnClasses(density)}>{codeEditor()}</div>
      {hasSuffix && (
        <div
          className={codeInputAffixCellClasses("suffix", density, suffixContent, {
            showTrailing: trailingBesideSuffix && showTrailing,
          })}
        >
          {trailingBesideSuffix && showTrailing && trailingCluster(false)}
          {trailingBesideSuffix && showTrailing ? (
            <span className={cn(textInputSuffixGlyphSlotClasses(density), "overflow-visible")}>
              {suffixContent}
            </span>
          ) : (
            suffixContent
          )}
        </div>
      )}
      {trailingInAffixCell && (
        <div
          className={cn(
            textInputAffixCellClasses("suffix", density),
            "relative z-10 shrink-0 self-start overflow-visible",
            boolInputRowMinHeightVariant({ density: densityKey }),
            trailingControlCount > 1 && textInputSuffixWithTrailingClusterClasses(density),
            "[&_button]:overflow-visible [&_[data-invalid-icon]]:overflow-visible",
          )}
        >
          {trailingCluster(false)}
        </div>
      )}
    </div>
  );
};

export default CodeInputWidget;
