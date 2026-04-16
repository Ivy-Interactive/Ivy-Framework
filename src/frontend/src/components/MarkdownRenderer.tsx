import React, { memo, useMemo, useCallback } from "react";
import ReactMarkdown, { defaultUrlTransform } from "react-markdown";
import remarkGfm from "remark-gfm";
import remarkGemoji from "remark-gemoji";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
import rehypeRaw from "rehype-raw";
import rehypeSlug from "rehype-slug";
import "katex/dist/katex.min.css";
import { cn, getIvyHost, convertAppUrlToPath, isLocalFilesEnabled } from "@/lib/utils";
import {
  validateLinkUrl,
  validateMediaUrl,
  isExternalUrl,
  isAnchorLink,
  isAppProtocol,
  extractAnchorId,
} from "@/lib/url";
import { useTypography } from "@/contexts/TypographyContext";
import { CustomEmoji } from "./custom-emojis/CustomEmoji";
import { remarkCustomEmojiPlugin } from "./custom-emojis/remarkCustomEmojiPlugin";

import { ImageOverlay } from "./markdown/ImageOverlay";
import { CodeBlock } from "./markdown/CodeBlock";
import { PopoverLink } from "./markdown/PopoverLink";
import Icon from "@/components/Icon";
import { Components } from "react-markdown";
import { parseGitHubAlert, githubAlertStyles } from "@/lib/markdown-utils";

interface MarkdownRendererProps {
  content: string;
  onLinkClick?: (url: string) => void;
  dangerouslyAllowLocalFiles?: boolean;
}

interface FenceBlock {
  openLine: number;
  closeLine: number;
  indent: string;
  infoString: string;
  children: FenceBlock[];
}

/**
 * Normalizes nested fenced code blocks so that outer fences use more backticks
 * than inner fences. This fixes CommonMark rendering where nested fences of
 * the same length cause the inner fence to prematurely close the outer one.
 *
 * For example, a markdown block containing a ```csharp block would have its
 * outer fence increased to ```` so the inner ``` doesn't close it.
 */
export function normalizeNestedFences(content: string): string {
  const lines = content.split("\n");
  const fenceRegex = /^(\s{0,3})(`{3,})\s*(.*)/;

  const stack: { line: number; indent: string; infoString: string; children: FenceBlock[] }[] = [];
  const topLevel: FenceBlock[] = [];

  for (let i = 0; i < lines.length; i++) {
    const match = lines[i].match(fenceRegex);
    if (!match) continue;

    const indent = match[1];
    const infoString = match[3].trim();

    if (stack.length === 0 || infoString) {
      // Opening fence: either top-level or nested (has info string)
      stack.push({ line: i, indent, infoString, children: [] });
    } else {
      // Closing fence (no info string, inside an open block)
      const open = stack.pop()!;
      const block: FenceBlock = {
        openLine: open.line,
        closeLine: i,
        indent: open.indent,
        infoString: open.infoString,
        children: open.children,
      };
      if (stack.length > 0) {
        stack[stack.length - 1].children.push(block);
      } else {
        topLevel.push(block);
      }
    }
  }

  // No nesting found — return content unchanged
  if (topLevel.every((b) => b.children.length === 0)) {
    return content;
  }

  function getRequiredBackticks(block: FenceBlock): number {
    if (block.children.length === 0) return 3;
    const maxChild = Math.max(...block.children.map(getRequiredBackticks));
    return maxChild + 1;
  }

  function rewriteBlock(block: FenceBlock) {
    const count = getRequiredBackticks(block);
    const backticks = "`".repeat(count);

    lines[block.openLine] = block.infoString
      ? `${block.indent}${backticks}${block.infoString}`
      : `${block.indent}${backticks}`;
    lines[block.closeLine] = `${block.indent}${backticks}`;

    for (const child of block.children) {
      rewriteBlock(child);
    }
  }

  for (const block of topLevel) {
    rewriteBlock(block);
  }

  return lines.join("\n");
}

const hasContentFeature = (content: string, feature: RegExp): boolean => {
  return feature.test(content);
};

const MarkdownRenderer: React.FC<MarkdownRendererProps> = ({
  content,
  onLinkClick,
  dangerouslyAllowLocalFiles = false,
}) => {
  const typography = useTypography();
  const contentFeatures = useMemo(
    () => ({
      hasMath: hasContentFeature(content, /(\$|\\\(|\\\[|\\begin\{)/),
      hasCodeBlocks: hasContentFeature(content, /```/),
      hasMermaid: hasContentFeature(content, /```mermaid/),
      hasGraphviz: hasContentFeature(content, /```(graphviz|dot)/),
    }),
    [content],
  );

  const plugins = useMemo(() => {
    const remarkPlugins = [remarkGfm, remarkGemoji, remarkCustomEmojiPlugin];
    if (contentFeatures.hasMath) remarkPlugins.push(remarkMath as typeof remarkGfm);

    const rehypePlugins = [rehypeRaw, rehypeSlug];
    if (contentFeatures.hasMath) rehypePlugins.push(rehypeKatex as unknown as typeof rehypeRaw);

    return { remarkPlugins, rehypePlugins };
  }, [contentFeatures.hasMath]);

  const handleLinkClick = useCallback(
    (href: string, event: React.MouseEvent<HTMLAnchorElement>) => {
      // When local files are enabled, pass file:// URLs directly to onLinkClick
      if (dangerouslyAllowLocalFiles && href.startsWith("file:///")) {
        if (onLinkClick) {
          event.preventDefault();
          onLinkClick(href);
        }
        return;
      }

      // Validate URL to prevent open redirect vulnerabilities
      // validateLinkUrl always returns a string ('#' for invalid URLs)
      // When onLinkClick is registered, allow custom protocols (e.g. plan://)
      // since the handler intercepts navigation rather than the browser
      const validatedHref = validateLinkUrl(href, {
        allowCustomProtocols: !!onLinkClick,
      });
      if (validatedHref === "#") {
        event.preventDefault();
        return;
      }

      // When onLinkClick is registered, intercept ALL link clicks
      // This allows the backend handler to decide how to handle the URL
      if (onLinkClick) {
        event.preventDefault();
        onLinkClick(validatedHref);
      }
    },
    [onLinkClick, dangerouslyAllowLocalFiles],
  );

  // Memoize static components separately (they don't need handleLinkClick)
  const staticComponents = useMemo(
    () => ({
      h1: memo(({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
        <h1 className={typography.h1} {...props}>
          {children}
        </h1>
      )),
      h2: memo(({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
        <h2 className={typography.h2} {...props}>
          {children}
        </h2>
      )),
      h3: memo(({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
        <h3 className={typography.h3} {...props}>
          {children}
        </h3>
      )),
      h4: memo(({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
        <h4 className={typography.h4} {...props}>
          {children}
        </h4>
      )),
      h5: memo(({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
        <h5 className={typography.h5} {...props}>
          {children}
        </h5>
      )),
      h6: memo(({ children, ...props }: React.HTMLAttributes<HTMLHeadingElement>) => (
        <h6 className={typography.h6} {...props}>
          {children}
        </h6>
      )),
      p: memo(({ children }: { children: React.ReactNode }) => (
        <p className={typography.p}>{children}</p>
      )),
      ul: memo(({ children }: { children: React.ReactNode }) => (
        <ul className={typography.ul}>{children}</ul>
      )),
      ol: memo(({ children }: { children: React.ReactNode }) => (
        <ol className={typography.ol}>{children}</ol>
      )),
      li: memo(({ children, className }: { children: React.ReactNode; className?: string }) => {
        const isTaskItem = className?.includes("task-list-item");
        return <li className={cn(typography.li, isTaskItem && "list-none")}>{children}</li>;
      }),
      strong: memo(({ children }: { children: React.ReactNode }) => (
        <strong className={typography.strong}>{children}</strong>
      )),
      em: memo(({ children }: { children: React.ReactNode }) => (
        <em className={typography.em}>{children}</em>
      )),
      pre: memo(({ children }: { children: React.ReactNode }) => <>{children}</>),
      blockquote: memo(({ children }: React.BlockquoteHTMLAttributes<HTMLQuoteElement>) => {
        const alert = parseGitHubAlert(children);
        if (alert) {
          const style = githubAlertStyles[alert.type];
          return (
            <div
              className={cn(
                "flex items-start text-large-body rounded-box border transition-colors relative",
                "py-4 px-4 my-4",
                style.className,
              )}
              role="alert"
            >
              <Icon
                name={style.icon}
                size="24"
                className={cn("mr-3.5 shrink-0 opacity-90", style.iconColor)}
              />
              <div className="flex flex-col min-w-0 flex-1">
                <div className="font-medium mb-1 leading-6">{style.title}</div>
                <div className="text-sm opacity-90 leading-relaxed [&_p]:text-sm [&_p]:mb-0">
                  {alert.content}
                </div>
              </div>
            </div>
          );
        }
        return <blockquote className={typography.blockquote}>{children}</blockquote>;
      }),
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
          const [showOverlay, setShowOverlay] = React.useState(false);
          const src = props.src;

          // Early validation: if src is missing or invalid, don't render anything
          if (!src || typeof src !== "string") {
            return null;
          }

          // Validate and sanitize image URL with optional local file support
          const validatedSrc = validateMediaUrl(src, {
            mediaType: "image",
            dangerouslyAllowLocalFiles,
          });
          if (!validatedSrc) {
            // Invalid URL, don't render image
            return null;
          }

          // Construct the final image source URL
          // For file:// URLs, use them directly (no Ivy host prefix)
          const imageSrc = validatedSrc.match(/^(https?:\/\/|data:|blob:|app:)/i)
            ? validatedSrc
            : (() => {
                const normalizedSrc = validatedSrc.startsWith("/")
                  ? validatedSrc
                  : `/${validatedSrc}`;
                const prefixedSrc = normalizedSrc.startsWith("/ivy/")
                  ? normalizedSrc
                  : `/ivy${normalizedSrc}`;
                return `${getIvyHost()}${prefixedSrc}`;
              })();

          return (
            <>
              <img
                {...props}
                src={imageSrc}
                alt={props.alt || ""}
                className={cn(typography.img, "cursor-zoom-in")}
                loading="lazy"
                onClick={() => setShowOverlay(true)}
                onKeyDown={(e) => e.key === "Enter" && setShowOverlay(true)}
                role="button"
                tabIndex={0}
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
          prevProps.src === nextProps.src && prevProps.alt === nextProps.alt,
      ),
      hr: memo((props: React.HTMLAttributes<HTMLHRElement>) => (
        <hr className={typography.hr} {...props} />
      )),
      details: memo(({ children, ...props }: React.DetailsHTMLAttributes<HTMLDetailsElement>) => (
        <details className={cn(typography.details, "group")} {...props}>
          {children}
        </details>
      )),
      summary: memo(({ children, ...props }: React.HTMLAttributes<HTMLElement>) => (
        <summary className={typography.summary} {...props}>
          <div className="flex items-center gap-2">
            <svg
              className="h-4 w-4 shrink-0 transition-transform group-open:rotate-90"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
            {children}
          </div>
        </summary>
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
      typography.details,
      typography.summary,
      dangerouslyAllowLocalFiles,
    ],
  );

  // Memoize code component separately (depends on contentFeatures.hasCodeBlocks and hasMermaid)
  const codeComponent = useMemo(
    () => ({
      code: memo((props: React.ComponentProps<"code">) => {
        const { children, className } = props;
        const node = (props as any).node;
        const isInPre = node?.parent?.tagName === "pre";
        const inline = isInPre ? false : !className && !String(children).includes("\n");

        // Detect Icons.X pattern in inline code
        if (inline) {
          const text = String(children);
          const iconMatch = text.match(/^Icons\.([A-Z][a-zA-Z0-9]*)$/);
          if (iconMatch) {
            return (
              <span style={{ display: "inline-flex", alignItems: "center", gap: "0.25em" }}>
                <code className={typography.code}>{children}</code>
                <Icon name={iconMatch[1]} size="1em" />
              </span>
            );
          }
        }

        return (
          <CodeBlock
            className={className}
            inline={inline}
            hasCodeBlocks={contentFeatures.hasCodeBlocks}
            hasMermaid={contentFeatures.hasMermaid}
            hasGraphviz={contentFeatures.hasGraphviz}
          >
            {children}
          </CodeBlock>
        );
      }),
    }),
    [
      contentFeatures.hasCodeBlocks,
      contentFeatures.hasMermaid,
      contentFeatures.hasGraphviz,
      typography.code,
    ],
  );

  // Memoize link component separately (depends on handleLinkClick)
  const linkComponent = useMemo(
    () => ({
      a: memo(
        ({ children, href, title, ...props }: React.AnchorHTMLAttributes<HTMLAnchorElement>) => {
          // Popover links: [text](## "popover content")
          if (href === "##" && title) {
            return <PopoverLink content={title}>{children}</PopoverLink>;
          }

          // When local files are enabled, allow file:// URLs to render as clickable links
          const isLocalFileUrl = dangerouslyAllowLocalFiles && href?.startsWith("file:///");

          const safeHref = isLocalFileUrl
            ? href!
            : validateLinkUrl(href, { allowCustomProtocols: !!onLinkClick });
          if (safeHref === "#") {
            return <span {...props}>{children}</span>;
          }

          // Use helper functions for URL type detection
          const isExternalLink = isExternalUrl(safeHref);
          const isAnchor = isAnchorLink(safeHref);
          const isApp = isAppProtocol(safeHref);

          // Convert app:// URLs to regular paths for href attribute
          let hrefForNavigation = safeHref;
          if (isApp) {
            // Use the utility function to convert app:// URLs, preserving shell=false
            hrefForNavigation = convertAppUrlToPath(safeHref);
          }

          return (
            <a
              {...props}
              className="text-primary underline underline-offset-[3px] brightness-90 hover:brightness-100"
              href={hrefForNavigation}
              target={isExternalLink && !onLinkClick ? "_blank" : undefined}
              rel={isExternalLink && !onLinkClick ? "noopener noreferrer" : undefined}
              onClick={
                isAnchor
                  ? (e) => {
                      e.preventDefault();
                      // Extract anchor ID by removing the '#' prefix
                      const targetId = extractAnchorId(safeHref);
                      if (targetId) {
                        // Small delay to ensure content is rendered
                        requestAnimationFrame(() => {
                          const targetElement = document.getElementById(targetId);
                          if (targetElement) {
                            targetElement.scrollIntoView({
                              behavior: "smooth",
                              block: "start",
                            });
                            // Update URL hash
                            window.history.replaceState(null, "", `#${targetId}`);
                          }
                        });
                      }
                    }
                  : onLinkClick
                    ? (e) => handleLinkClick(safeHref, e)
                    : undefined
              }
            >
              {children}
            </a>
          );
        },
      ),
    }),
    [handleLinkClick, dangerouslyAllowLocalFiles],
  );

  const components = useMemo(
    () => ({
      ...staticComponents,
      ...codeComponent,
      ...linkComponent,
    }),
    [staticComponents, codeComponent, linkComponent],
  );
  // This is useful to declare emoji as a new type of valid markdown component
  type MarkdownComponents = Components & {
    emoji?: React.FC<{ name: string }>;
  };

  // add the components that use memo and the ones that don't in a single variable of the extended type we just created
  const componentsParams: MarkdownComponents = {
    ...(components as React.ComponentProps<typeof ReactMarkdown>["components"]),

    // ReactMarkdown will execute this when he finds an image node with hName emoji
    emoji: ({ name }: { name: string }) => <CustomEmoji name={name} />,
  };

  const normalizedContent = useMemo(() => normalizeNestedFences(content), [content]);

  const urlTransform = useCallback(
    (url: string, key: string) => {
      // Preserve popover link marker
      if (url === "##") {
        return url;
      }
      if (url.startsWith("app://")) {
        return url;
      }
      // Allow file:// URLs and Windows paths when local files are enabled
      if (
        dangerouslyAllowLocalFiles &&
        (url.startsWith("file://") || /^[a-zA-Z]:[\\/]/.test(url))
      ) {
        // For links (href), preserve file:// URL for onLinkClick to handle
        if (key === "href") {
          if (/^[a-zA-Z]:[\\/]/.test(url)) {
            const normalized = url.replace(/\\/g, "/");
            return `file:///${normalized}`;
          }
          return url;
        }
        // For images (src), use local-file proxy
        if (isLocalFilesEnabled()) {
          // Server supports local file proxy - use /ivy/local-file endpoint
          let filePath: string;
          if (url.startsWith("file:///")) {
            filePath = decodeURIComponent(url.slice(8));
          } else if (url.startsWith("file://")) {
            filePath = decodeURIComponent(url.slice(7));
          } else {
            filePath = url.replace(/\\/g, "/");
          }
          return `/ivy/local-file?path=${encodeURIComponent(filePath)}`;
        }
        // Fallback: pass file:// URL through (browser will likely block it)
        if (/^[a-zA-Z]:[\\/]/.test(url)) {
          const normalized = url.replace(/\\/g, "/");
          return `file:///${normalized}`;
        }
        return url;
      }
      // Validate URL before transforming to prevent open redirect vulnerabilities
      // validateLinkUrl always returns a string ('#' for invalid URLs)
      const validatedUrl = validateLinkUrl(url);
      // defaultUrlTransform handles all valid URLs, and '#' for invalid URLs
      return defaultUrlTransform(validatedUrl);
    },
    [dangerouslyAllowLocalFiles],
  );

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
        {normalizedContent}
      </ReactMarkdown>
    </>
  );
};

export default memo(MarkdownRenderer);
