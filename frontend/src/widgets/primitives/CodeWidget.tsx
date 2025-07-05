import CopyToClipboardButton from "@/components/CopyToClipboardButton";
import ivyPrismTheme from "@/lib/ivy-prism-theme";
import { getHeight, getWidth } from "@/lib/styles";
import React, {
  CSSProperties,
  useState,
  useMemo,
  memo,
  useCallback,
} from "react";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";

interface CodeWidgetProps {
  id: string;
  content: string;
  language: string;
  showCopyButton?: boolean;
  showLineNumbers?: boolean;
  showBorder?: boolean;
  width?: string;
  height?: string;
}

const languageMap: Record<string, string> = {
  Csharp: "csharp",
  Javascript: "javascript",
  Typescript: "typescript",
  Python: "python",
  Sql: "sql",
  Html: "html",
  Css: "css",
  Json: "json",
  Dbml: "dbml",
};

const mapLanguageToPrism = (language: string): string => {
  if (!languageMap[language])
    console.warn(
      `Language ${language} is not specified in the code widget, attempting to use the language name as a fallback.`
    );

  return languageMap[language] || language.toLowerCase();
};

const MemoizedCopyButton = memo(({ textToCopy }: { textToCopy: string }) => (
  <div className="absolute top-2 right-2 z-10">
    <CopyToClipboardButton textToCopy={textToCopy} />
  </div>
));

const CodeWidget: React.FC<CodeWidgetProps> = memo(
  ({
    id,
    content,
    language,
    showCopyButton = false,
    showLineNumbers = false,
    showBorder = true,
    width,
    height,
  }) => {
    const [isHovered, setIsHovered] = useState(false);

    const handleMouseEnter = useCallback(() => setIsHovered(true), []);
    const handleMouseLeave = useCallback(() => setIsHovered(false), []);

    const styles = useMemo<CSSProperties>(() => {
      const baseStyles: CSSProperties = {
        ...getWidth(width),
        ...getHeight(height),
        margin: 0,
        overflow: isHovered ? "auto" : "hidden",
      };

      if (!showBorder) {
        baseStyles.border = "none";
        baseStyles.padding = "0";
        baseStyles.borderRadius = "0";
      }

      return baseStyles;
    }, [width, height, isHovered, showBorder]);

    const highlighterKey = useMemo(
      () =>
        `${id}-${mapLanguageToPrism(
          language
        )}-${showLineNumbers}-${showBorder}`,
      [id, language, showLineNumbers, showBorder]
    );

    return (
      <div
        className="relative"
        onMouseEnter={handleMouseEnter}
        onMouseLeave={handleMouseLeave}
      >
        {showCopyButton && <MemoizedCopyButton textToCopy={content} />}
        <SyntaxHighlighter
          language={mapLanguageToPrism(language)}
          customStyle={styles}
          style={ivyPrismTheme}
          showLineNumbers={showLineNumbers}
          wrapLines={true}
          key={highlighterKey}
        >
          {content}
        </SyntaxHighlighter>
      </div>
    );
  }
);

CodeWidget.displayName = "CodeWidget";

export default CodeWidget;
