import React, { memo, useMemo, useCallback } from "react";
import ReactMarkdown, { defaultUrlTransform } from "react-markdown";
import remarkGfm from "remark-gfm";
import remarkGemoji from "remark-gemoji";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
import rehypeRaw from "rehype-raw";
import rehypeSlug from "rehype-slug";
import "katex/dist/katex.min.css";
import { cn, getIvyHost, convertAppUrlToPath } from "@/lib/utils";
import {
  validateLinkUrl,
  validateImageUrl,
  isExternalUrl,
  isAnchorLink,
  isAppProtocol,
  isRelativePath,
  isStandardUrl,
  extractAnchorId,
} from "@/lib/url";
import { useTypography } from "@/contexts/TypographyContext";
import { CustomEmoji } from "./custom-emojis/CustomEmoji";
import { remarkCustomEmojiPlugin } from "./custom-emojis/remarkCustomEmojiPlugin";

import { ImageOverlay } from "./markdown/ImageOverlay";
import { CodeBlock } from "./markdown/CodeBlock";
import { Components } from "react-markdown";

interface MarkdownRendererProps {
  content: string;
  onLinkClick?: (url: string) => void;
}

const hasContentFeature = (content: string, feature: RegExp): boolean => {
  return feature.test(content);
};

const MarkdownRenderer: React.FC<MarkdownRendererProps> = ({ content, onLinkClick }) => {
  const typography = useTypography();
  const contentFeatures = useMemo(
    () => ({
      hasMath: hasContentFeature(content, /(\$|\\\(|\\\[|\\begin\{)/),
      hasCodeBlocks: hasContentFeature(content, /```/),
      hasMermaid: hasContentFeature(content, /```mermaid/),
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
      // Validate URL to prevent open redirect vulnerabilities
      // validateLinkUrl always returns a string ('#' for invalid URLs)
      const validatedHref = validateLinkUrl(href);
      if (validatedHref === "#") {
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
    [onLinkClick],
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
      li: memo(({ children }: { children: React.ReactNode }) => (
        <li className={typography.li}>{children}</li>
      )),
      strong: memo(({ children }: { children: React.ReactNode }) => (
        <strong className={typography.strong}>{children}</strong>
      )),
      em: memo(({ children }: { children: React.ReactNode }) => (
        <em className={typography.em}>{children}</em>
      )),
      pre: memo(({ children }: { children: React.ReactNode }) => <>{children}</>),
      blockquote: memo(({ children }: React.BlockquoteHTMLAttributes<HTMLQuoteElement>) => (
        <blockquote className={typography.blockquote}>{children}</blockquote>
      )),
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

          // Validate and sanitize image URL to prevent open redirect vulnerabilities
          const validatedSrc = validateImageUrl(src);
          if (!validatedSrc) {
            // Invalid URL, don't render image (return null to prevent any rendering)
            return null;
          }

          // Construct the final image source URL
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
    ],
  );

  // Memoize code component separately (depends on contentFeatures.hasCodeBlocks and hasMermaid)
  const codeComponent = useMemo(
    () => ({
      code: memo((props: React.ComponentProps<"code"> & { inline?: boolean }) => {
        const { children, className, inline } = props;
        return (
          <CodeBlock
            className={className}
            inline={inline}
            hasCodeBlocks={contentFeatures.hasCodeBlocks}
            hasMermaid={contentFeatures.hasMermaid}
          >
            {children}
          </CodeBlock>
        );
      }),
    }),
    [contentFeatures.hasCodeBlocks, contentFeatures.hasMermaid],
  );

  // Memoize link component separately (depends on handleLinkClick)
  const linkComponent = useMemo(
    () => ({
      a: memo(({ children, href, ...props }: React.AnchorHTMLAttributes<HTMLAnchorElement>) => {
        // Validate URL to prevent open redirect vulnerabilities
        // validateLinkUrl always returns a string ('#' for invalid URLs)
        const safeHref = validateLinkUrl(href);
        if (safeHref === "#") {
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
            target={isExternalLink ? "_blank" : undefined}
            rel={isExternalLink ? "noopener noreferrer" : undefined}
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
                : isApp || isRelative
                  ? undefined // Let browser handle navigation naturally
                  : (e) => handleLinkClick(safeHref, e)
            }
          >
            {children}
          </a>
        );
      }),
    }),
    [handleLinkClick],
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

  const urlTransform = useCallback((url: string) => {
    if (url.startsWith("app://")) {
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
