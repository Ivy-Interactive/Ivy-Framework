import React, { memo, useMemo, Suspense } from "react";
import { cn } from "@/lib/utils";
import { createPrismTheme } from "@/lib/prismTheme";
import { useTypography } from "@/contexts/TypographyContext";
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";
import {
  codeCopyViewportInsetStyle,
  markdownCodeCopyScrollPaddingClass,
  markdownCodeCopyGutterLength,
} from "@/components/ui/code-variant";
import CopyToClipboardButton from "@/components/CopyToClipboardButton";
import ErrorBoundary from "@/components/ErrorBoundary";
import { lazyWithRetry } from "@/lib/lazyWithRetry";

const SyntaxHighlighter = lazyWithRetry(() =>
  import("react-syntax-highlighter").then((mod) => ({ default: mod.Prism })),
);

/** Copy stays transparent/floating; scroll content gets right inset so lines never run under the control. */
function CodeBlockChromeWithCopy({
  textToCopy,
  outerClassName,
  scrollAreaClassName,
  children,
}: {
  textToCopy: string;
  outerClassName: string;
  scrollAreaClassName?: string;
  children: React.ReactNode;
}) {
  return (
    <div className={cn("relative w-full", outerClassName)}>
      <div className="absolute top-2 right-2 z-30">
        <CopyToClipboardButton textToCopy={textToCopy} />
      </div>
      <ScrollArea
        className={cn("w-full", scrollAreaClassName)}
        viewportClassName="min-w-0"
        viewportStyle={codeCopyViewportInsetStyle(markdownCodeCopyGutterLength)}
      >
        <div className={markdownCodeCopyScrollPaddingClass}>{children}</div>
        <ScrollBar orientation="horizontal" />
      </ScrollArea>
    </div>
  );
}

const MermaidRenderer = lazyWithRetry(() => import("../MermaidRenderer"));
const GraphvizRenderer = lazyWithRetry(() => import("../GraphvizRenderer"));

interface CodeBlockProps {
  className?: string;
  children: React.ReactNode;
  inline?: boolean;
  hasCodeBlocks: boolean;
  hasMermaid: boolean;
  hasGraphviz: boolean;
}

export const CodeBlock = memo(
  ({ className, children, inline, hasCodeBlocks, hasMermaid, hasGraphviz }: CodeBlockProps) => {
    const match = /language-(\w+)/.exec(className || "");
    const content = String(children).replace(/\n$/, "");
    const isTerminal = match && match[1] === "terminal";
    const isMermaid = match && match[1] === "mermaid";
    const isGraphviz = match && (match[1] === "graphviz" || match[1] === "dot");

    // Create dynamic theme that uses CSS variables for dynamic theming
    const dynamicTheme = useMemo(() => createPrismTheme(), []);
    const typography = useTypography();

    if (!inline && hasCodeBlocks) {
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

      // Handle Graphviz diagrams
      if (isGraphviz && hasGraphviz) {
        return (
          <ErrorBoundary>
            <Suspense
              fallback={
                <div className="rounded-md border bg-background p-4">
                  <div className="flex items-center justify-center p-8 text-muted-foreground">
                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
                    <span className="ml-2 text-sm">Loading Graphviz...</span>
                  </div>
                </div>
              }
            >
              <GraphvizRenderer content={content} />
            </Suspense>
          </ErrorBoundary>
        );
      }

      if (isTerminal) {
        // Handle terminal blocks with prompt styling
        const lines = content.split("\n").filter((line) => line.trim());
        const cleanContent = lines.join("\n"); // Remove any empty lines

        return (
          <CodeBlockChromeWithCopy
            textToCopy={cleanContent}
            outerClassName="rounded-md bg-muted"
            scrollAreaClassName="w-full"
          >
            <pre className="p-4 font-mono text-sm" style={{ overflowX: "auto" }}>
              {lines.map((line, i) => {
                const lineKey = `md-term-line-${i}`;
                return (
                  <div key={lineKey} className="flex">
                    <span className="text-muted-foreground select-none pointer-events-none mr-2">
                      {"> "}
                    </span>
                    <span className="flex-1">{line}</span>
                  </div>
                );
              })}
            </pre>
          </CodeBlockChromeWithCopy>
        );
      }

      const language = match ? match[1] : "text";
      const useHighlighter = language !== "markdown" && language !== "md";

      if (!useHighlighter) {
        return (
          <CodeBlockChromeWithCopy
            textToCopy={content}
            outerClassName="rounded-md border border-border"
            scrollAreaClassName="w-full bg-muted"
          >
            <pre
              className="p-4 font-mono text-sm"
              style={{ margin: 0, whiteSpace: "pre", overflowX: "auto" }}
            >
              {content}
            </pre>
          </CodeBlockChromeWithCopy>
        );
      }

      return (
        <Suspense
          fallback={
            <CodeBlockChromeWithCopy
              textToCopy={content}
              outerClassName="rounded-md border border-border"
              scrollAreaClassName="w-full bg-muted"
            >
              <pre className="p-4 font-mono text-sm" style={{ overflowX: "auto" }}>
                {content}
              </pre>
            </CodeBlockChromeWithCopy>
          }
        >
          <CodeBlockChromeWithCopy
            textToCopy={content}
            outerClassName="rounded-md border border-border"
            scrollAreaClassName="w-full bg-muted"
          >
            <SyntaxHighlighter
              language={language}
              style={dynamicTheme}
              customStyle={{
                margin: 0,
                wordBreak: "normal",
                overflowWrap: "break-word",
              }}
              wrapLongLines={false}
            >
              {content}
            </SyntaxHighlighter>
          </CodeBlockChromeWithCopy>
        </Suspense>
      );
    }

    return <code className={cn(typography.code, className)}>{children}</code>;
  },
);

CodeBlock.displayName = "CodeBlock";
