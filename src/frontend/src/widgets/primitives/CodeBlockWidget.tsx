import CopyToClipboardButton from "@/components/CopyToClipboardButton";
import { getHeight, getWidth } from "@/lib/styles";
import React, { CSSProperties, useMemo, memo, lazy, Suspense } from "react";
const SyntaxHighlighter = lazy(() =>
  import("react-syntax-highlighter").then((mod) => ({ default: mod.Prism })),
);
import { createPrismTheme } from "@/lib/prismTheme";
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";
import { codeCopyButtonVariant } from "@/components/ui/code-variant";

interface CodeWidgetProps {
  id: string;
  content: string;
  language: string;
  showCopyButton?: boolean;
  showLineNumbers?: boolean;
  startingLineNumber?: number;
  showBorder?: boolean;
  wrapLines?: boolean;
  width?: string;
  height?: string;
  density?: Densities;
}

const languageMap: Record<string, string> = {
  Csharp: "csharp",
  Javascript: "javascript",
  Typescript: "typescript",
  Python: "python",
  Sql: "sql",
  Html: "markup",
  Css: "css",
  Json: "json",
  Dbml: "sql",
  Text: "text",
  Xml: "xml",
  Yaml: "yaml",
  Csv: "text",
  Powershell: "powershell",
  Bash: "bash",
};

const mapLanguageToPrism = (language: string): string | undefined => {
  if (!languageMap[language])
    console.warn(
      `Language ${language} is not specified in the code widget, attempting to use the language name as a fallback.`,
    );

  const result = languageMap[language] || language.toLowerCase();
  return result === "text" ? undefined : result;
};

const MemoizedCopyButton = memo(
  ({ textToCopy, density }: { textToCopy: string; density: Densities }) => (
    <div className={codeCopyButtonVariant({ density })}>
      <CopyToClipboardButton textToCopy={textToCopy} density={density} />
    </div>
  ),
);

const CodeWidget: React.FC<CodeWidgetProps> = memo(
  ({
    id,
    content = "",
    language = "Csharp",
    showCopyButton = true,
    showLineNumbers = false,
    startingLineNumber = 1,
    showBorder = true,
    wrapLines = false,
    width = "Full",
    height = "MaxContent,,Px:800",
    density = Densities.Medium,
  }) => {
    const scaleStyles: Record<
      Densities,
      {
        fontSize: string;
        padding: string;
        lineHeight: string;
        lineNumberMinWidth: string;
        lineNumberPaddingRight: string;
      }
    > = useMemo(
      () => ({
        [Densities.Small]: {
          fontSize: "0.75rem",
          padding: "0.5rem",
          lineHeight: "1.4",
          lineNumberMinWidth: "1.5rem",
          lineNumberPaddingRight: "0.5rem",
        },
        [Densities.Medium]: {
          fontSize: "0.875rem",
          padding: "0.75rem",
          lineHeight: "1.5",
          lineNumberMinWidth: "2.25rem",
          lineNumberPaddingRight: "0.75rem",
        },
        [Densities.Large]: {
          fontSize: "1rem",
          padding: "1rem",
          lineHeight: "1.6",
          lineNumberMinWidth: "2.5rem",
          lineNumberPaddingRight: "1rem",
        },
      }),
      [],
    );

    const currentScale = scaleStyles[density];

    /** Pre (container): padding, dimensions, typography. Padding here applies to all lines. */
    const preStyle = useMemo<CSSProperties>(() => {
      const style: CSSProperties = {
        ...getWidth(width),
        ...getHeight(height),
        margin: 0,
        wordBreak: "normal",
        overflowWrap: "break-word",
        fontSize: currentScale.fontSize,
        padding: currentScale.padding,
        lineHeight: currentScale.lineHeight,
        // Ensure empty code blocks have at least one line of height
        minHeight: `calc(${currentScale.fontSize} * ${currentScale.lineHeight} + ${currentScale.padding} * 2)`,
      };
      if (!showBorder) {
        style.border = "none";
        style.padding = "0";
        style.borderRadius = "0";
      }
      return style;
    }, [width, height, showBorder, currentScale]);

    const codeTagStyle = useMemo<CSSProperties>(
      () => ({
        margin: 0,
        wordBreak: wrapLines ? "break-word" : "normal",
        whiteSpace: wrapLines ? "pre-wrap" : "pre",
        fontSize: currentScale.fontSize,
        lineHeight: currentScale.lineHeight,
        ...(showLineNumbers && { display: "table" }),
      }),
      [currentScale, wrapLines, showLineNumbers],
    );

    const highlighterKey = useMemo(
      () =>
        `${id}-${mapLanguageToPrism(language)}-${showLineNumbers}-${showBorder}-${startingLineNumber}-${wrapLines}`,
      [id, language, showLineNumbers, showBorder, startingLineNumber, wrapLines],
    );

    const dynamicTheme = useMemo(() => createPrismTheme(), []);

    const isFull = height?.toLowerCase().startsWith("full");
    const containerStyles: React.CSSProperties = isFull
      ? {
          display: "flex",
          flexDirection: "column",
          height: "100%",
          minHeight: 0,
        }
      : { ...getWidth(width) };

    return (
      <div className="relative" style={containerStyles}>
        {showCopyButton && <MemoizedCopyButton textToCopy={content} density={density} />}
        <ScrollArea
          className={cn(
            "w-full",
            isFull ? "flex-1 min-h-0" : "h-full",
            showBorder && "border border-border rounded-md",
          )}
        >
          <Suspense
            fallback={
              <pre
                className={cn("p-4 bg-muted rounded-md font-mono text-sm")}
                style={isFull ? { ...preStyle, height: "auto" } : preStyle}
              >
                {content}
              </pre>
            }
          >
            <SyntaxHighlighter
              language={mapLanguageToPrism(language)}
              customStyle={isFull ? { ...preStyle, height: "auto" } : preStyle}
              style={dynamicTheme}
              showLineNumbers={showLineNumbers}
              startingLineNumber={startingLineNumber}
              wrapLines={true}
              wrapLongLines={wrapLines}
              key={highlighterKey}
              codeTagProps={{ style: codeTagStyle }}
              lineProps={showLineNumbers ? { style: { display: "table-row" } } : undefined}
              lineNumberStyle={
                showLineNumbers
                  ? {
                      display: "table-cell",
                      paddingRight: currentScale.lineNumberPaddingRight,
                      minWidth: currentScale.lineNumberMinWidth,
                      textAlign: "right",
                      userSelect: "none",
                      color: "var(--muted-foreground)",
                      opacity: 0.5,
                    }
                  : undefined
              }
            >
              {content}
            </SyntaxHighlighter>
          </Suspense>
          <ScrollBar orientation="horizontal" />
        </ScrollArea>
      </div>
    );
  },
);

CodeWidget.displayName = "CodeWidget";

export default CodeWidget;
