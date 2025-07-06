import React, {
  lazy,
  Suspense,
  memo,
  useMemo,
  useCallback,
  useState,
} from 'react';
import ReactMarkdown, { defaultUrlTransform } from 'react-markdown';
import remarkGfm from 'remark-gfm';
import remarkGemoji from 'remark-gemoji';
import remarkMath from 'remark-math';
import rehypeKatex from 'rehype-katex';
import rehypeRaw from 'rehype-raw';
import 'katex/dist/katex.min.css';
import { cn, getIvyHost } from '@/lib/utils';
import CopyToClipboardButton from './CopyToClipboardButton';
import ivyPrismTheme from '@/lib/ivy-prism-theme';

const SyntaxHighlighter = lazy(() =>
  import('react-syntax-highlighter').then(mod => ({ default: mod.Prism }))
);

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

const MemoizedImage = memo(
  ({ src, alt, ...props }: React.ImgHTMLAttributes<HTMLImageElement>) => {
    const [showOverlay, setShowOverlay] = useState(false);
    const imageSrc =
      src && !src.match(/^(https?:\/\/|data:|blob:|app:)/i)
        ? `${getIvyHost()}${src?.startsWith('/') ? '' : '/'}${src}`
        : src;

    return (
      <>
        <img
          src={imageSrc}
          alt={alt}
          className="max-w-full h-auto cursor-zoom-in"
          loading="lazy"
          onClick={() => setShowOverlay(true)}
          {...props}
        />
        {showOverlay && (
          <ImageOverlay
            src={imageSrc}
            alt={alt}
            onClose={() => setShowOverlay(false)}
          />
        )}
      </>
    );
  },
  (prevProps, nextProps) =>
    prevProps.src === nextProps.src && prevProps.alt === nextProps.alt
);

const MemoizedH1 = memo(({ children }: { children: React.ReactNode }) => (
  <h1 className="scroll-m-20 text-4xl font-extrabold tracking-tight lg:text-5xl">
    {children}
  </h1>
));

const MemoizedH2 = memo(({ children }: { children: React.ReactNode }) => (
  <h2 className="scroll-m-20 text-3xl font-semibold tracking-tight first:mt-0">
    {children}
  </h2>
));

const MemoizedH3 = memo(({ children }: { children: React.ReactNode }) => (
  <h3 className="scroll-m-20 text-2xl font-semibold tracking-tight">
    {children}
  </h3>
));

const MemoizedH4 = memo(({ children }: { children: React.ReactNode }) => (
  <h4 className="scroll-m-20 text-xl font-semibold tracking-tight">
    {children}
  </h4>
));

const hasContentFeature = (content: string, feature: RegExp): boolean => {
  return feature.test(content);
};

const CodeBlock = memo(
  ({
    className,
    children,
    hasCodeBlocks,
  }: {
    className?: string;
    children: React.ReactNode;
    inline?: boolean;
    hasCodeBlocks: boolean;
  }) => {
    const match = /language-(\w+)/.exec(className || '');
    const content = String(children).replace(/\n$/, '');

    if (match && hasCodeBlocks) {
      return (
        <Suspense
          fallback={
            <pre className="p-4 bg-muted rounded-md overflow-x-auto">
              {content}
            </pre>
          }
        >
          <div className="relative">
            <div className="absolute top-2 right-2 z-10">
              <CopyToClipboardButton textToCopy={content} />
            </div>
            <SyntaxHighlighter
              language={match[1]}
              style={ivyPrismTheme}
              customStyle={{ margin: 0 }}
            >
              {content}
            </SyntaxHighlighter>
          </div>
        </Suspense>
      );
    }

    return (
      <code
        className={cn(
          'relative rounded bg-muted px-[0.3rem] py-[0.3rem] font-mono text-sm',
          className
        )}
      >
        {children}
      </code>
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

  const components = useMemo(
    () => ({
      // Headings
      h1: MemoizedH1,
      h2: MemoizedH2,
      h3: MemoizedH3,
      h4: MemoizedH4,

      // Paragraphs and text
      p: memo(({ children }: { children: React.ReactNode }) => (
        <p className="scroll-m-20 text-md leading-8">{children}</p>
      )),
      strong: memo(({ children }: { children: React.ReactNode }) => (
        <strong className="font-semibold">{children}</strong>
      )),
      em: memo(({ children }: { children: React.ReactNode }) => (
        <em className="italic">{children}</em>
      )),

      // Lists
      ul: memo(({ children }: { children: React.ReactNode }) => (
        <ul className="ml-6 list-disc [&>li:first-child]:mt-0">{children}</ul>
      )),
      ol: memo(({ children }: { children: React.ReactNode }) => (
        <ol className="ml-6 list-decimal [&>li:first-child]:mt-0">
          {children}
        </ol>
      )),
      li: memo(({ children }: { children: React.ReactNode }) => (
        <li className="mt-3">{children}</li>
      )),

      // Code blocks - with memoization and optimized rendering
      code: (props: React.ComponentProps<'code'>) => (
        <CodeBlock
          className={props.className}
          children={props.children || ''}
          hasCodeBlocks={contentFeatures.hasCodeBlocks}
        />
      ),

      // Pre tag (for code blocks)
      pre: memo(({ children }: { children: React.ReactNode }) => (
        <>{children}</>
      )),

      // Links with memoized click handler
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

      // Blockquotes
      blockquote: memo(
        ({ children }: React.BlockquoteHTMLAttributes<HTMLQuoteElement>) => (
          <blockquote className="border-l-2 pl-6 italic">{children}</blockquote>
        )
      ),

      // Tables
      table: memo(({ children }: { children: React.ReactNode }) => (
        <table className="w-full border-collapse border border-border">
          {children}
        </table>
      )),
      thead: memo(({ children }: { children: React.ReactNode }) => (
        <thead className="bg-muted">{children}</thead>
      )),
      tr: memo(({ children }: { children: React.ReactNode }) => (
        <tr className="border border-border">{children}</tr>
      )),
      th: memo(({ children }: { children: React.ReactNode }) => (
        <th className="border border-border px-4 py-2 text-left font-bold">
          {children}
        </th>
      )),
      td: memo(({ children }: { children: React.ReactNode }) => (
        <td className="border border-border px-4 py-2">{children}</td>
      )),

      img: MemoizedImage,
    }),
    [contentFeatures.hasCodeBlocks, handleLinkClick]
  );

  const urlTransform = useCallback((url: string) => {
    if (url.startsWith('app://')) {
      return url;
    }
    return defaultUrlTransform(url);
  }, []);

  return (
    <div className="flex flex-col gap-8">
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
