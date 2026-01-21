import CopyToClipboardButton from '@/components/CopyToClipboardButton';
import { getHeight, getWidth } from '@/lib/styles';
import React, { CSSProperties, useMemo, memo, lazy, Suspense } from 'react';
const SyntaxHighlighter = lazy(() =>
  import('react-syntax-highlighter').then(mod => ({ default: mod.Prism }))
);
import { createPrismTheme } from '@/lib/prismTheme';
import { ScrollArea, ScrollBar } from '@/components/ui/scroll-area';
import { cn } from '@/lib/utils';
import { Scales } from '@/types/scale';
import { codeCopyButtonVariants } from '@/components/ui/code-variants';

interface CodeWidgetProps {
  id: string;
  content: string;
  language: string;
  showCopyButton?: boolean;
  showLineNumbers?: boolean;
  showBorder?: boolean;
  width?: string;
  height?: string;
  scale?: Scales;
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
  Xml: 'xml',
  Yaml: 'yaml',
};

const mapLanguageToPrism = (language: string): string | undefined => {
  if (!languageMap[language])
    console.warn(
      `Language ${language} is not specified in the code widget, attempting to use the language name as a fallback.`
    );

  const result = languageMap[language] || language.toLowerCase();
  return result === 'text' ? undefined : result;
};

const MemoizedCopyButton = memo(
  ({ textToCopy, scale }: { textToCopy: string; scale: Scales }) => (
    <div className={codeCopyButtonVariants({ scale })}>
      <CopyToClipboardButton textToCopy={textToCopy} scale={scale} />
    </div>
  )
);

const CodeWidget: React.FC<CodeWidgetProps> = memo(
  ({
    id,
    content = '',
    language = 'Csharp',
    showCopyButton = true,
    showLineNumbers = false,
    showBorder = true,
    width = 'Full',
    height = 'MaxContent,,Px:800',
    scale = Scales.Medium,
  }) => {
    const styles = useMemo<CSSProperties>(() => {
      const scaleStyles: Record<
        Scales,
        { fontSize: string; padding: string; lineHeight: string }
      > = {
        [Scales.Small]: {
          fontSize: '0.75rem',
          padding: '0.5rem',
          lineHeight: '1.4',
        },
        [Scales.Medium]: {
          fontSize: '0.875rem',
          padding: '0.75rem',
          lineHeight: '1.5',
        },
        [Scales.Large]: {
          fontSize: '1rem',
          padding: '1rem',
          lineHeight: '1.6',
        },
      };

      const currentScale = scaleStyles[scale];

      const baseStyles: CSSProperties = {
        ...getWidth(width),
        ...getHeight(height),
        margin: 0,
        fontSize: currentScale.fontSize,
        padding: currentScale.padding,
        lineHeight: currentScale.lineHeight,
      };

      if (!showBorder) {
        baseStyles.border = 'none';
        baseStyles.padding = '0';
        baseStyles.borderRadius = '0';
      }

      return baseStyles;
    }, [width, height, showBorder, scale]);

    const highlighterKey = useMemo(
      () =>
        `${id}-${mapLanguageToPrism(language)}-${showLineNumbers}-${showBorder}`,
      [id, language, showLineNumbers, showBorder]
    );

    const dynamicTheme = useMemo(() => createPrismTheme(), []);

    const isFull = height?.toLowerCase().startsWith('full');
    const containerStyles: React.CSSProperties = isFull
      ? {
          display: 'flex',
          flexDirection: 'column',
          height: '100%',
          minHeight: 0,
        }
      : { ...getWidth(width) };

    return (
      <div className="relative" style={containerStyles}>
        {showCopyButton && (
          <MemoizedCopyButton textToCopy={content} scale={scale} />
        )}
        <ScrollArea
          className={cn(
            'w-full',
            isFull ? 'flex-1 min-h-0' : 'h-full',
            showBorder && 'border border-border rounded-md'
          )}
        >
          <Suspense
            fallback={
              <pre
                className={cn('p-4 bg-muted rounded-md font-mono text-sm')}
                style={isFull ? { ...styles, height: 'auto' } : styles}
              >
                {content}
              </pre>
            }
          >
            <SyntaxHighlighter
              language={mapLanguageToPrism(language)}
              customStyle={isFull ? { ...styles, height: 'auto' } : styles}
              style={dynamicTheme}
              showLineNumbers={showLineNumbers}
              wrapLines={true}
              key={highlighterKey}
              codeTagProps={{
                style: {
                  fontSize: styles.fontSize,
                  lineHeight: styles.lineHeight,
                },
              }}
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
