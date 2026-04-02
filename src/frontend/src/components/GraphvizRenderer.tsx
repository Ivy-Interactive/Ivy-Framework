import { memo, useRef, useState, useEffect, useCallback } from "react";
import CopyToClipboardButton from "./CopyToClipboardButton";
import { logger } from "@/lib/logger";

interface GraphvizRendererProps {
  content: string;
}

/**
 * Sanitize SVG content to prevent XSS attacks
 * Removes dangerous elements and attributes that could execute scripts
 */
const sanitizeSvg = (svg: string): string => {
  const parser = new DOMParser();
  const doc = parser.parseFromString(svg, "image/svg+xml");

  const parserError = doc.querySelector("parsererror");
  if (parserError) {
    logger.warn("SVG parsing error, falling back to empty content");
    return "<svg></svg>";
  }

  const svgElement = doc.documentElement;

  const dangerousElements = [
    "script",
    "object",
    "embed",
    "link",
    "meta",
    "iframe",
    "frame",
    "frameset",
    "form",
    "input",
    "button",
    "textarea",
    "select",
  ];

  dangerousElements.forEach((tagName) => {
    const elements = svgElement.querySelectorAll(tagName);
    elements.forEach((el) => el.remove());
  });

  const walker = document.createTreeWalker(svgElement, NodeFilter.SHOW_ELEMENT, null);

  const attributesToRemove = [
    "onload",
    "onerror",
    "onclick",
    "onmouseover",
    "onfocus",
    "onblur",
    "onchange",
    "onsubmit",
    "onreset",
    "onselect",
    "onunload",
    "href",
    "xlink:href",
  ];

  let node;
  while ((node = walker.nextNode())) {
    const element = node as Element;
    attributesToRemove.forEach((attr) => {
      if (element.hasAttribute(attr)) {
        element.removeAttribute(attr);
      }
    });

    const attributes = Array.from(element.attributes);
    attributes.forEach((attr) => {
      if (attr.name.toLowerCase().startsWith("on")) {
        element.removeAttribute(attr.name);
      }
    });
  }

  return new XMLSerializer().serializeToString(svgElement);
};

/**
 * Apply Ivy system font to all text elements in SVG.
 * Graphviz WASM renders text with "Times New Roman" by default.
 * We resolve --font-sans and set it on all <text> elements.
 */
const applyFontToSvg = (svgString: string): string => {
  let fontSans = "Geist, sans-serif";
  if (typeof document !== "undefined") {
    try {
      const resolved = getComputedStyle(document.documentElement)
        .getPropertyValue("--font-sans")
        .trim();
      if (resolved) fontSans = resolved;
    } catch {
      // Use fallback
    }
  }

  const parser = new DOMParser();
  const doc = parser.parseFromString(svgString, "image/svg+xml");
  const svgElement = doc.documentElement;

  const textElements = svgElement.querySelectorAll("text");
  textElements.forEach((el) => {
    el.setAttribute("font-family", fontSans);
  });

  return new XMLSerializer().serializeToString(svgElement);
};

const GraphvizRenderer = memo(({ content }: GraphvizRendererProps) => {
  const elementRef = useRef<HTMLDivElement>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentTheme, setCurrentTheme] = useState<"light" | "dark">("light");
  const themeRef = useRef<"light" | "dark">("light");
  const renderTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  const detectTheme = useCallback(() => {
    return document.documentElement.classList.contains("dark") ? "dark" : "light";
  }, []);

  const debouncedRender = useCallback((theme: "light" | "dark") => {
    if (renderTimeoutRef.current) {
      clearTimeout(renderTimeoutRef.current);
    }

    renderTimeoutRef.current = setTimeout(() => {
      if (theme !== themeRef.current) {
        setCurrentTheme(theme);
        themeRef.current = theme;
      }
    }, 100);
  }, []);

  useEffect(() => {
    const initialTheme = detectTheme();
    setCurrentTheme(initialTheme);
    themeRef.current = initialTheme;

    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.type === "attributes" && mutation.attributeName === "class") {
          const newTheme = detectTheme();
          if (newTheme !== themeRef.current) {
            debouncedRender(newTheme);
          }
        }
      });
    });

    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ["class"],
    });

    let cleanedUp = false;
    const currentTimeout = renderTimeoutRef.current;

    return () => {
      if (cleanedUp) return;
      cleanedUp = true;

      observer.disconnect();
      if (currentTimeout) {
        clearTimeout(currentTimeout);
      }
      if (renderTimeoutRef.current === currentTimeout) {
        renderTimeoutRef.current = null;
      }
    };
  }, [detectTheme, debouncedRender]);

  useEffect(() => {
    let mounted = true;

    const renderDiagram = async () => {
      if (!elementRef.current) return;

      try {
        setIsLoading(true);
        setError(null);

        const { Graphviz } = await import("@hpcc-js/wasm-graphviz");
        const graphviz = await Graphviz.load();

        const svg = graphviz.dot(content.trim());

        if (mounted && elementRef.current) {
          const fontApplied = applyFontToSvg(svg);
          const sanitized = sanitizeSvg(fontApplied);
          elementRef.current.innerHTML = sanitized;
          setIsLoading(false);
        }
      } catch (err) {
        logger.error("Graphviz rendering error:", err);
        if (mounted) {
          setError(err instanceof Error ? err.message : "Failed to render diagram");
          setIsLoading(false);
        }
      }
    };

    renderDiagram();

    return () => {
      mounted = false;
    };
  }, [content, currentTheme]);

  if (error) {
    return (
      <div className="rounded-md border border-destructive bg-destructive/10 p-3">
        <div className="flex items-center gap-2 text-destructive text-sm font-medium">
          <svg
            className="h-4 w-4 flex-shrink-0"
            fill="none"
            viewBox="0 0 24 24"
            strokeWidth={1.5}
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z"
            />
          </svg>
          <span>Invalid Graphviz DOT syntax</span>
        </div>
      </div>
    );
  }

  return (
    <div className="relative">
      <div className="absolute top-2 right-2 z-10">
        <CopyToClipboardButton textToCopy={content} />
      </div>
      <div className="graphviz-container rounded-md border bg-background p-4 overflow-x-auto">
        {isLoading && (
          <div className="flex items-center justify-center p-8 text-muted-foreground">
            <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
            <span className="ml-2 text-sm">Loading diagram...</span>
          </div>
        )}
        <div
          ref={elementRef}
          className="graphviz-diagram"
          style={{ minHeight: isLoading ? "100px" : "auto" }}
        />
      </div>
    </div>
  );
});

GraphvizRenderer.displayName = "GraphvizRenderer";

export default GraphvizRenderer;
