import React, {
  lazy,
  Suspense,
  memo,
  useMemo,
  useCallback,
  useState,
} from 'react';
import ErrorBoundary from './ErrorBoundary';
import ReactMarkdown, { defaultUrlTransform } from 'react-markdown';
import remarkGfm from 'remark-gfm';
import remarkGemoji from 'remark-gemoji';
import remarkMath from 'remark-math';
import rehypeKatex from 'rehype-katex';
import rehypeRaw from 'rehype-raw';
import 'katex/dist/katex.min.css';
import { cn, getIvyHost } from '@/lib/utils';
import CopyToClipboardButton from './CopyToClipboardButton';
import { createPrismTheme } from '@/lib/ivy-prism-theme';
import { textBlockClassMap, textContainerClass } from '@/lib/textBlockClassMap';
import { ScrollArea, ScrollBar } from '@/components/ui/scroll-area';

const SyntaxHighlighter = lazy(() =>
  import('react-syntax-highlighter').then(mod => ({ default: mod.Prism }))
);

// Import MermaidRenderer component
const MermaidRenderer = lazy(() => import('./MermaidRenderer'));

interface MarkdownRendererProps {
  content: string;
  onLinkClick?: (url: string) => void;
}

const ImageOverlay = ({
  src,
  alt,
  onClose,
}: {
  src: string | undefined;
  alt: string | undefined;
  onClose: () => void;
}) => {
  // Handle click on the overlay background to close it
  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <div
      className="fixed inset-0 bg-black/70 flex items-center justify-center z-50 cursor-zoom-out"
      onClick={handleBackdropClick}
    >
      <div className="relative max-w-[90vw] max-h-[90vh]">
        <img
          src={src}
          alt={alt}
          className="max-w-full max-h-[90vh] object-contain"
        />
        <button
          className="absolute top-4 right-4 bg-black/50 text-white rounded-full w-8 h-8 flex items-center justify-center"
          onClick={onClose}
        >
          ✕
        </button>
      </div>
    </div>
  );
};

const hasContentFeature = (content: string, feature: RegExp): boolean => {
  return feature.test(content);
};

const CodeBlock = memo(
  ({
    className,
    children,
    hasCodeBlocks,
    hasMermaid,
  }: {
    className?: string;
    children: React.ReactNode;
    inline?: boolean;
    hasCodeBlocks: boolean;
    hasMermaid: boolean;
  }) => {
    const match = /language-(\w+)/.exec(className || '');
    const content = String(children).replace(/\n$/, '');
    const isTerminal = match && match[1] === 'terminal';
    const isMermaid = match && match[1] === 'mermaid';

    // Create dynamic theme that adapts to current CSS variables
    const dynamicTheme = useMemo(() => createPrismTheme(), []);

    if (match && hasCodeBlocks) {
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
              <pre className="p-4 bg-muted rounded-md font-mono text-sm">
                {lines.map((line, index) => (
                  <div key={index} className="flex">
                    <span className="text-muted-foreground select-none pointer-events-none mr-2">
                      {'> '}
                    </span>
                    <span className="flex-1">{line}</span>
                  </div>
                ))}
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
              <pre className="p-4 bg-muted rounded-md font-mono text-sm">
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
            <ScrollArea className="w-full">
              <SyntaxHighlighter
                language={match[1]}
                style={dynamicTheme}
                customStyle={{ margin: 0 }}
              >
                {content}
              </SyntaxHighlighter>
              <ScrollBar orientation="horizontal" />
            </ScrollArea>
          </div>
        </Suspense>
      );
    }

    return (
      <code className={cn(textBlockClassMap.code, className)}>{children}</code>
    );
  }
);

const MarkdownRenderer: React.FC<MarkdownRendererProps> = ({
  content,
  onLinkClick,
}) => {
  const contentFeatures = useMemo(
    () => ({
      hasMath: hasContentFeature(content, /(\$|\\\(|\\\[|\\begin\{)/),
      hasCodeBlocks: hasContentFeature(content, /```/),
      hasMermaid: hasContentFeature(content, /```mermaid/),
    }),
    [content]
  );

  const plugins = useMemo(() => {
    const remarkPlugins = [remarkGfm, remarkGemoji];
    if (contentFeatures.hasMath)
      remarkPlugins.push(remarkMath as typeof remarkGfm);

    const rehypePlugins = [rehypeRaw];
    if (contentFeatures.hasMath)
      rehypePlugins.push(rehypeKatex as unknown as typeof rehypeRaw);

    return { remarkPlugins, rehypePlugins };
  }, [contentFeatures.hasMath]);

  const handleLinkClick = useCallback(
    (href: string, event: React.MouseEvent<HTMLAnchorElement>) => {
      const isExternalLink = href?.match(/^(https?:\/\/|mailto:|tel:)/i);
      if (!isExternalLink && onLinkClick && href) {
        event.preventDefault();
        onLinkClick(href);
      }
    },
    [onLinkClick]
  );

  // Memoize static components separately (they don't need handleLinkClick)
  const staticComponents = useMemo(
    () => ({
      h1: memo(({ children }: { children: React.ReactNode }) => (
        <h1 className={textBlockClassMap.h1}>{children}</h1>
      )),
      h2: memo(({ children }: { children: React.ReactNode }) => (
        <h2 className={textBlockClassMap.h2}>{children}</h2>
      )),
      h3: memo(({ children }: { children: React.ReactNode }) => (
        <h3 className={textBlockClassMap.h3}>{children}</h3>
      )),
      h4: memo(({ children }: { children: React.ReactNode }) => (
        <h4 className={textBlockClassMap.h4}>{children}</h4>
      )),
      p: memo(({ children }: { children: React.ReactNode }) => (
        <p className={textBlockClassMap.p}>{children}</p>
      )),
      ul: memo(({ children }: { children: React.ReactNode }) => (
        <ul className={textBlockClassMap.ul}>{children}</ul>
      )),
      ol: memo(({ children }: { children: React.ReactNode }) => (
        <ol className={textBlockClassMap.ol}>{children}</ol>
      )),
      li: memo(({ children }: { children: React.ReactNode }) => (
        <li className={textBlockClassMap.li}>{children}</li>
      )),
      strong: memo(({ children }: { children: React.ReactNode }) => (
        <strong className={textBlockClassMap.strong}>{children}</strong>
      )),
      em: memo(({ children }: { children: React.ReactNode }) => (
        <em className={textBlockClassMap.em}>{children}</em>
      )),

      // Pre tag (for code blocks)
      pre: memo(({ children }: { children: React.ReactNode }) => (
        <>{children}</>
      )),

      // Blockquotes
      blockquote: memo(
        ({ children }: React.BlockquoteHTMLAttributes<HTMLQuoteElement>) => (
          <blockquote className={textBlockClassMap.blockquote}>
            {children}
          </blockquote>
        )
      ),

      // Tables
      table: memo(({ children }: { children: React.ReactNode }) => (
        <table className={textBlockClassMap.table}>{children}</table>
      )),
      thead: memo(({ children }: { children: React.ReactNode }) => (
        <thead className="bg-muted">{children}</thead>
      )),
      tr: memo(({ children }: { children: React.ReactNode }) => (
        <tr className="border border-border">{children}</tr>
      )),
      th: memo(({ children }: { children: React.ReactNode }) => (
        <th className={textBlockClassMap.th}>{children}</th>
      )),
      td: memo(({ children }: { children: React.ReactNode }) => (
        <td className={textBlockClassMap.td}>{children}</td>
      )),

      img: memo(
        (props: React.ImgHTMLAttributes<HTMLImageElement>) => {
          const [showOverlay, setShowOverlay] = useState(false);
          const src = props.src;
          const imageSrc =
            src && !src?.match(/^(https?:\/\/|data:|blob:|app:)/i)
              ? `${getIvyHost()}${src?.startsWith('/') ? '' : '/'}${src}`
              : src;

          return (
            <>
              <img
                {...props}
                src={imageSrc}
                className={cn(textBlockClassMap.img, 'cursor-zoom-in')}
                loading="lazy"
                onClick={() => setShowOverlay(true)}
              />
              {showOverlay && (
                <ImageOverlay
                  src={imageSrc}
                  alt={props.alt}
                  onClose={() => setShowOverlay(false)}
                />
              )}
            </>
          );
        },
        (prevProps, nextProps) =>
          prevProps.src === nextProps.src && prevProps.alt === nextProps.alt
      ),
    }),
    []
  );

  // Memoize code component separately (depends on contentFeatures.hasCodeBlocks and hasMermaid)
  const codeComponent = useMemo(
    () => ({
      code: memo((props: React.ComponentProps<'code'>) => (
        <CodeBlock
          className={props.className}
          children={props.children || ''}
          hasCodeBlocks={contentFeatures.hasCodeBlocks}
          hasMermaid={contentFeatures.hasMermaid}
        />
      )),
    }),
    [contentFeatures.hasCodeBlocks, contentFeatures.hasMermaid]
  );

  // Memoize link component separately (depends on handleLinkClick)
  const linkComponent = useMemo(
    () => ({
      a: memo(
        ({
          children,
          href,
          ...props
        }: React.AnchorHTMLAttributes<HTMLAnchorElement>) => {
          const isExternalLink = href?.match(/^(https?:\/\/|mailto:|tel:)/i);

          return (
            <a
              {...props}
              className="text-primary underline brightness-90 hover:brightness-100"
              href={href || '#'}
              target={isExternalLink ? '_blank' : undefined}
              rel={isExternalLink ? 'noopener noreferrer' : undefined}
              onClick={e => href && handleLinkClick(href, e)}
            >
              {children}
            </a>
          );
        }
      ),
    }),
    [handleLinkClick]
  );

  const components = useMemo(
    () => ({
      ...staticComponents,
      ...codeComponent,
      ...linkComponent,
    }),
    [staticComponents, codeComponent, linkComponent]
  );

  const urlTransform = useCallback((url: string) => {
    if (url.startsWith('app://')) {
      return url;
    }
    return defaultUrlTransform(url);
  }, []);

  return (
    <div className={textContainerClass}>
      <ReactMarkdown
        components={
          components as React.ComponentProps<typeof ReactMarkdown>['components']
        }
        remarkPlugins={plugins.remarkPlugins}
        rehypePlugins={plugins.rehypePlugins}
        urlTransform={urlTransform}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
};

export default memo(MarkdownRenderer);
