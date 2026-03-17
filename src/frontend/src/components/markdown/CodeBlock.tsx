import React, { memo, useMemo, lazy, Suspense } from 'react';
import { cn } from '@/lib/utils';
import { createPrismTheme } from '@/lib/prismTheme';
import { useTypography } from '@/contexts/TypographyContext';
import { ScrollArea, ScrollBar } from '@/components/ui/scroll-area';
import CopyToClipboardButton from '@/components/CopyToClipboardButton';
import ErrorBoundary from '@/components/ErrorBoundary';

const SyntaxHighlighter = lazy(() =>
  import('react-syntax-highlighter').then(mod => ({ default: mod.Prism }))
);

const MermaidRenderer = lazy(() => import('../MermaidRenderer'));

interface CodeBlockProps {
  className?: string;
  children: React.ReactNode;
  inline?: boolean;
  hasCodeBlocks: boolean;
  hasMermaid: boolean;
}

export const CodeBlock = memo(
  ({
    className,
    children,
    inline,
    hasCodeBlocks,
    hasMermaid,
  }: CodeBlockProps) => {
    const match = /language-(\w+)/.exec(className || '');
    const content = String(children).replace(/\n$/, '');
    const isTerminal = match && match[1] === 'terminal';
    const isMermaid = match && match[1] === 'mermaid';

    // Create dynamic theme that adapts to current CSS variables
    const dynamicTheme = useMemo(() => createPrismTheme(), []);
    const typography = useTypography();

    const shouldWrap = true;
    const whiteSpaceStyle = shouldWrap ? { whiteSpace: 'pre-wrap' } : {};

    if (!inline && match && hasCodeBlocks) {
      // Handle Mermaid diagrams
      if (isMermaid && hasMermaid) {
        return (
          <ErrorBoundary>
            <Suspense
              fallback={
                <div className="rounded-md border bg-background p-4">
                  <div className="flex items-center justify-center p-8 text-muted-foreground">
                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
                    <span className="ml-2 text-sm">Loading Mermaid...</span>
                  </div>
                </div>
              }
            >
              <MermaidRenderer content={content} />
            </Suspense>
          </ErrorBoundary>
        );
      }

      if (isTerminal) {
        // Handle terminal blocks with prompt styling
        const lines = content.split('\n').filter(line => line.trim());
        const cleanContent = lines.join('\n'); // Remove any empty lines

        return (
          <div className="relative">
            <div className="absolute top-2 right-2 z-10">
              <CopyToClipboardButton textToCopy={cleanContent} />
            </div>
            <ScrollArea className="w-full">
              <pre
                className={cn(
                  'p-4 bg-muted rounded-md font-mono text-sm',
                  shouldWrap && 'whitespace-pre-wrap break-all'
                )}
                style={shouldWrap ? {} : { overflowX: 'auto' }}
              >
                {lines.map((line, i) => {
                  const lineKey = `md-term-line-${i}`;
                  return (
                    <div key={lineKey} className="flex">
                      <span className="text-muted-foreground select-none pointer-events-none mr-2">
                        {'> '}
                      </span>
                      <span className="flex-1">{line}</span>
                    </div>
                  );
                })}
              </pre>
              <ScrollBar orientation="horizontal" />
            </ScrollArea>
          </div>
        );
      }

      return (
        <Suspense
          fallback={
            <ScrollArea className="w-full border border-border rounded-md">
              <pre
                className={cn(
                  'p-4 bg-muted rounded-md font-mono text-sm',
                  shouldWrap && 'whitespace-pre-wrap break-all'
                )}
                style={shouldWrap ? {} : { overflowX: 'auto' }}
              >
                {content}
              </pre>
              <ScrollBar orientation="horizontal" />
            </ScrollArea>
          }
        >
          <div className="relative">
            <div className="absolute top-2 right-2 z-10">
              <CopyToClipboardButton textToCopy={content} />
            </div>
            <ScrollArea className="w-full border border-border rounded-md">
              <SyntaxHighlighter
                language={match[1]}
                style={dynamicTheme}
                customStyle={{
                  margin: 0,
                  ...whiteSpaceStyle,
                  wordBreak: 'normal',
                  overflowWrap: 'break-word',
                }}
                wrapLongLines={shouldWrap}
              >
                {content}
              </SyntaxHighlighter>
              <ScrollBar orientation="horizontal" />
            </ScrollArea>
          </div>
        </Suspense>
      );
    }

    // Apply styles to fallback blocks (no language) if it's a block (!inline)
    const fallbackStyles =
      !inline && shouldWrap
        ? {
            ...whiteSpaceStyle,
            wordBreak: 'normal' as const,
            overflowWrap: 'break-word' as const,
          }
        : {};

    return (
      <code className={cn(typography.code, className)} style={fallbackStyles}>
        {children}
      </code>
    );
  }
);

CodeBlock.displayName = 'CodeBlock';
