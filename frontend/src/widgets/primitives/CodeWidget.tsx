import CopyToClipboardButton from '@/components/CopyToClipboardButton';
import { getHeight, getWidth } from '@/lib/styles';
import React, { CSSProperties, useMemo, memo, lazy, Suspense } from 'react';
const SyntaxHighlighter = lazy(() =>
  import('react-syntax-highlighter').then(mod => ({ default: mod.Prism }))
);
import { createPrismTheme } from '@/lib/ivy-prism-theme';
import { ScrollArea, ScrollBar } from '@/components/ui/scroll-area';
import { cn } from '@/lib/utils';

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
  Csharp: 'csharp',
  Javascript: 'javascript',
  Typescript: 'typescript',
  Python: 'python',
  Sql: 'sql',
  Html: 'markup',
  Css: 'css',
  Json: 'json',
  Dbml: 'sql',
  Text: 'text',
};

const mapLanguageToPrism = (language: string): string | undefined => {
  if (!languageMap[language])
    console.warn(
      `Language ${language} is not specified in the code widget, attempting to use the language name as a fallback.`
    );

  const result = languageMap[language] || language.toLowerCase();
  return result === 'text' ? undefined : result;
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
    const styles = useMemo<CSSProperties>(() => {
      const baseStyles: CSSProperties = {
        ...getWidth(width),
        ...getHeight(height),
        margin: 0,
      };

      if (!showBorder) {
        baseStyles.border = 'none';
        baseStyles.padding = '0';
        baseStyles.borderRadius = '0';
      }

      return baseStyles;
    }, [width, height, showBorder]);

    const highlighterKey = useMemo(
      () =>
        `${id}-${mapLanguageToPrism(language)}-${showLineNumbers}-${showBorder}`,
      [id, language, showLineNumbers, showBorder]
    );

    const dynamicTheme = useMemo(() => createPrismTheme(), []);

    return (
      <div className="relative">
        {showCopyButton && <MemoizedCopyButton textToCopy={content} />}
        <ScrollArea
          className={cn(
            'w-full h-full',
            showBorder && 'border border-border rounded-md'
          )}
        >
          <Suspense
            fallback={
              <pre
                className="p-4 bg-muted rounded-md font-mono text-sm"
                style={styles}
              >
                {content}
              </pre>
            }
          >
            <SyntaxHighlighter
              language={mapLanguageToPrism(language)}
              customStyle={styles}
              style={dynamicTheme}
              showLineNumbers={showLineNumbers}
              wrapLines={true}
              key={highlighterKey}
            >
              {content}
            </SyntaxHighlighter>
          </Suspense>
          <ScrollBar orientation="horizontal" />
        </ScrollArea>
      </div>
    );
  }
);

CodeWidget.displayName = 'CodeWidget';

export default CodeWidget;
