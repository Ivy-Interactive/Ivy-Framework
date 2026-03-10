import React, {
  lazy,
  Suspense,
  memo,
  useMemo,
  useCallback,
  useState,
} from 'react';
import ErrorBoundary from './ErrorBoundary';
import ReactMarkdown, { defaultUrlTransform, Components } from 'react-markdown';
import remarkGfm from 'remark-gfm';
import remarkGemoji from 'remark-gemoji';
import remarkMath from 'remark-math';
import rehypeKatex from 'rehype-katex';
import rehypeRaw from 'rehype-raw';
import rehypeSlug from 'rehype-slug';
import 'katex/dist/katex.min.css';
import { cn, getIvyHost, convertAppUrlToPath } from '@/lib/utils';
import {
  validateLinkUrl,
  validateImageUrl,
  isExternalUrl,
  isAnchorLink,
  isAppProtocol,
  isRelativePath,
  isStandardUrl,
  extractAnchorId,
} from '@/lib/url';
import CopyToClipboardButton from './CopyToClipboardButton';
import { createPrismTheme } from '@/lib/prismTheme';
import { useTypography } from '@/contexts/TypographyContext';
import { ScrollArea, ScrollBar } from '@/components/ui/scroll-area';
import { CustomEmoji } from './custom-emojis/CustomEmoji';
import { remarkCustomEmojiPlugin } from './custom-emojis/remarkCustomEmojiPlugin';

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

  // Validate and sanitize image URL to prevent open redirect vulnerabilities
  const validatedSrc = src ? validateImageUrl(src) : null;
  if (!validatedSrc) {
    // Invalid URL, don't render image
    return null;
  }

  return (
    <div
      className="fixed inset-0 bg-black/70 flex items-center justify-center z-50 cursor-zoom-out"
      onClick={handleBackdropClick}
    >
      <div className="relative max-w-[90vw] max-h-[90vh]">
        <img
          src={validatedSrc}
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
    inline,
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

const MarkdownRenderer: React.FC<MarkdownRendererProps> = ({
  content,
  onLinkClick,
}) => {
  const typography = useTypography();
  const contentFeatures = useMemo(
    () => ({
      hasMath: hasContentFeature(content, /(\$|\\\(|\\\[|\\begin\{)/),
      hasCodeBlocks: hasContentFeature(content, /```/),
      hasMermaid: hasContentFeature(content, /```mermaid/),
    }),
    [content]
  );

  const plugins = useMemo(() => {
    const remarkPlugins = [remarkGfm, remarkGemoji, remarkCustomEmojiPlugin];
    if (contentFeatures.hasMath)
      remarkPlugins.push(remarkMath as typeof remarkGfm);

    const rehypePlugins = [rehypeRaw, rehypeSlug];
    if (contentFeatures.hasMath)
      rehypePlugins.push(rehypeKatex as unknown as typeof rehypeRaw);

    return { remarkPlugins, rehypePlugins };
  }, [contentFeatures.hasMath]);

  const handleLinkClick = useCallback(
    (href: string, event: React.MouseEvent<HTMLAnchorElement>) => {
      // Validate URL to prevent open redirect vulnerabilities
      // validateLinkUrl always returns a string ('#' for invalid URLs)
      const validatedHref = validateLinkUrl(href);
      if (validatedHref === '#') {
        event.preventDefault();
        return;
      }

      // Only call backend handler for custom link handling scenarios
      // validateLinkUrl already handles external links, anchor links, app:// URLs, and relative paths safely
      // If the URL is one of these standard types, the browser will handle it naturally
      // Only call onLinkClick for non-standard URLs that need custom handling
      if (!isStandardUrl(validatedHref) && onLinkClick) {
        event.preventDefault();
        onLinkClick(validatedHref);
      }
    },
    [onLinkClick]
  );

  // Memoize static components separately (they don't need handleLinkClick)
  const staticComponents = useMemo(
    () => ({
      h1: memo(
        ({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
          <h1 className={typography.h1} {...props}>
            {children}
          </h1>
        )
      ),
      h2: memo(
        ({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
          <h2 className={typography.h2} {...props}>
            {children}
          </h2>
        )
      ),
      h3: memo(
        ({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
          <h3 className={typography.h3} {...props}>
            {children}
          </h3>
        )
      ),
      h4: memo(
        ({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
          <h4 className={typography.h4} {...props}>
            {children}
          </h4>
        )
      ),
      h5: memo(
        ({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
          <h5 className={typography.h5} {...props}>
            {children}
          </h5>
        )
      ),
      h6: memo(
        ({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
          <h6 className={typography.h6} {...props}>
            {children}
          </h6>
        )
      ),
      p: memo(({ children }: { children: React.ReactNode }) => (
        <p className={typography.p}>{children}</p>
      )),
      ul: memo(({ children }: { children: React.ReactNode }) => (
        <ul className={typography.ul}>{children}</ul>
      )),
      ol: memo(({ children }: { children: React.ReactNode }) => (
        <ol className={typography.ol}>{children}</ol>
      )),
      li: memo(({ children }: { children: React.ReactNode }) => (
        <li className={typography.li}>{children}</li>
      )),
      strong: memo(({ children }: { children: React.ReactNode }) => (
        <strong className={typography.strong}>{children}</strong>
      )),
      em: memo(({ children }: { children: React.ReactNode }) => (
        <em className={typography.em}>{children}</em>
      )),
      pre: memo(({ children }: { children: React.ReactNode }) => (
        <>{children}</>
      )),
      blockquote: memo(
        ({ children }: React.BlockquoteHTMLAttributes<HTMLQuoteElement>) => (
          <blockquote className={typography.blockquote}>{children}</blockquote>
        )
      ),
      table: memo(({ children }: { children: React.ReactNode }) => (
        <table className={typography.table}>{children}</table>
      )),
      thead: memo(({ children }: { children: React.ReactNode }) => (
        <thead className="bg-muted">{children}</thead>
      )),
      tr: memo(({ children }: { children: React.ReactNode }) => (
        <tr className="border border-border">{children}</tr>
      )),
      th: memo(({ children }: { children: React.ReactNode }) => (
        <th className={typography.th}>{children}</th>
      )),
      td: memo(({ children }: { children: React.ReactNode }) => (
        <td className={typography.td}>{children}</td>
      )),
      img: memo(
        (props: React.ImgHTMLAttributes<HTMLImageElement>) => {
          const [showOverlay, setShowOverlay] = useState(false);
          const src = props.src;

          // Early validation: if src is missing or invalid, don't render anything
          if (!src || typeof src !== 'string') {
            return null;
          }

          // Validate and sanitize image URL to prevent open redirect vulnerabilities
          const validatedSrc = validateImageUrl(src);
          if (!validatedSrc) {
            // Invalid URL, don't render image (return null to prevent any rendering)
            return null;
          }

          // Construct the final image source URL
          const imageSrc = validatedSrc.match(
            /^(https?:\/\/|data:|blob:|app:)/i
          )
            ? validatedSrc
            : (() => {
                const normalizedSrc = validatedSrc.startsWith('/')
                  ? validatedSrc
                  : `/${validatedSrc}`;
                const prefixedSrc = normalizedSrc.startsWith('/ivy/')
                  ? normalizedSrc
                  : `/ivy${normalizedSrc}`;
                return `${getIvyHost()}${prefixedSrc}`;
              })();

          return (
            <>
              <img
                {...props}
                src={imageSrc}
                className={cn(typography.img, 'cursor-zoom-in')}
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
      hr: memo((props: React.HTMLAttributes<HTMLHRElement>) => (
        <hr className={typography.hr} {...props} />
      )),
    }),
    [
      typography.h1,
      typography.h2,
      typography.h3,
      typography.h4,
      typography.h5,
      typography.h6,
      typography.p,
      typography.ul,
      typography.ol,
      typography.li,
      typography.strong,
      typography.em,
      typography.blockquote,
      typography.table,
      typography.td,
      typography.th,
      typography.img,
      typography.hr,
    ]
  );

  // Memoize code component separately (depends on contentFeatures.hasCodeBlocks and hasMermaid)
  const codeComponent = useMemo(
    () => ({
      code: memo(
        (props: React.ComponentProps<'code'> & { inline?: boolean }) => (
          <CodeBlock
            className={props.className}
            children={props.children || ''}
            inline={props.inline}
            hasCodeBlocks={contentFeatures.hasCodeBlocks}
            hasMermaid={contentFeatures.hasMermaid}
          />
        )
      ),
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
          // Validate URL to prevent open redirect vulnerabilities
          // validateLinkUrl always returns a string ('#' for invalid URLs)
          const safeHref = validateLinkUrl(href);
          if (safeHref === '#') {
            return <span {...props}>{children}</span>;
          }

          // Use helper functions for URL type detection
          const isExternalLink = isExternalUrl(safeHref);
          const isAnchor = isAnchorLink(safeHref);
          const isApp = isAppProtocol(safeHref);
          const isRelative = isRelativePath(safeHref);

          // Convert app:// URLs to regular paths for href attribute
          let hrefForNavigation = safeHref;
          if (isApp) {
            // Use the utility function to convert app:// URLs, preserving chrome=false
            hrefForNavigation = convertAppUrlToPath(safeHref);
          }

          return (
            <a
              {...props}
              className="text-primary underline underline-offset-[3px] brightness-90 hover:brightness-100"
              href={hrefForNavigation}
              target={isExternalLink ? '_blank' : undefined}
              rel={isExternalLink ? 'noopener noreferrer' : undefined}
              onClick={
                isAnchor
                  ? e => {
                      e.preventDefault();
                      // Extract anchor ID by removing the '#' prefix
                      const targetId = extractAnchorId(safeHref);
                      if (targetId) {
                        // Small delay to ensure content is rendered
                        requestAnimationFrame(() => {
                          const targetElement =
                            document.getElementById(targetId);
                          if (targetElement) {
                            targetElement.scrollIntoView({
                              behavior: 'smooth',
                              block: 'start',
                            });
                            // Update URL hash
                            window.history.replaceState(
                              null,
                              '',
                              `#${targetId}`
                            );
                          }
                        });
                      }
                    }
                  : isApp || isRelative
                    ? undefined // Let browser handle navigation naturally
                    : e => handleLinkClick(safeHref, e)
              }
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
  // This is useful to declare emoji as a new type of valid markdown component
  type MarkdownComponents = Components & {
    emoji?: React.FC<{ name: string }>;
  };

  // add the components that use memo and the ones that don't in a single variable of the extended type we just created
  const componentsParams: MarkdownComponents = {
    ...(components as React.ComponentProps<typeof ReactMarkdown>['components']),

    // ReactMarkdown will execute this when he finds an image node with hName emoji
    emoji: ({ name }: { name: string }) => <CustomEmoji name={name} />,
  };

  const urlTransform = useCallback((url: string) => {
    if (url.startsWith('app://')) {
      return url;
    }
    // Validate URL before transforming to prevent open redirect vulnerabilities
    // validateLinkUrl always returns a string ('#' for invalid URLs)
    const validatedUrl = validateLinkUrl(url);
    // defaultUrlTransform handles all valid URLs, and '#' for invalid URLs
    return defaultUrlTransform(validatedUrl);
  }, []);

  return (
    <>
      <ReactMarkdown
        components={{
          ...componentsParams,
        }}
        remarkPlugins={plugins.remarkPlugins}
        rehypePlugins={plugins.rehypePlugins}
        urlTransform={urlTransform}
      >
        {content}
      </ReactMarkdown>
    </>
  );
};

export default memo(MarkdownRenderer);
