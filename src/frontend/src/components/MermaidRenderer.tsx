import { memo, useRef, useState, useEffect, useCallback } from 'react';
import CopyToClipboardButton from './CopyToClipboardButton';
import { logger } from '@/lib/logger';

interface MermaidRendererProps {
  content: string;
}

/**
 * Sanitize SVG content to prevent XSS attacks
 * Removes dangerous elements and attributes that could execute scripts
 */
const sanitizeSvg = (svg: string): string => {
  // Create a temporary DOM element to parse the SVG
  const parser = new DOMParser();
  const doc = parser.parseFromString(svg, 'image/svg+xml');

  // Check for parser errors
  const parserError = doc.querySelector('parsererror');
  if (parserError) {
    logger.warn('SVG parsing error, falling back to empty content');
    return '<svg></svg>';
  }

  const svgElement = doc.documentElement;

  // Remove potentially dangerous elements
  const dangerousElements = [
    'script',
    'object',
    'embed',
    'link',
    'meta',
    'iframe',
    'frame',
    'frameset',
    'form',
    'input',
    'button',
    'textarea',
    'select',
  ];

  dangerousElements.forEach(tagName => {
    const elements = svgElement.querySelectorAll(tagName);
    elements.forEach(el => el.remove());
  });

  // Remove event handlers and dangerous attributes
  const walker = document.createTreeWalker(
    svgElement,
    NodeFilter.SHOW_ELEMENT,
    null
  );

  const attributesToRemove = [
    'onload',
    'onerror',
    'onclick',
    'onmouseover',
    'onfocus',
    'onblur',
    'onchange',
    'onsubmit',
    'onreset',
    'onselect',
    'onunload',
    'href',
    'xlink:href', // Remove links that could navigate away
  ];

  let node;
  while ((node = walker.nextNode())) {
    const element = node as Element;
    attributesToRemove.forEach(attr => {
      if (element.hasAttribute(attr)) {
        element.removeAttribute(attr);
      }
    });

    // Remove any attribute that starts with 'on' (event handlers)
    const attributes = Array.from(element.attributes);
    attributes.forEach(attr => {
      if (attr.name.toLowerCase().startsWith('on')) {
        element.removeAttribute(attr.name);
      }
    });
  }

  return new XMLSerializer().serializeToString(svgElement);
};

const MermaidRenderer = memo(({ content }: MermaidRendererProps) => {
  const elementRef = useRef<HTMLDivElement>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentTheme, setCurrentTheme] = useState<'light' | 'dark'>('light');
  const themeRef = useRef<'light' | 'dark'>('light');
  const renderTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  // Function to detect current theme
  const detectTheme = useCallback(() => {
    return document.documentElement.classList.contains('dark')
      ? 'dark'
      : 'light';
  }, []);

  // Debounced render function
  const debouncedRender = useCallback((theme: 'light' | 'dark') => {
    if (renderTimeoutRef.current) {
      clearTimeout(renderTimeoutRef.current);
    }

    renderTimeoutRef.current = setTimeout(() => {
      if (theme !== themeRef.current) {
        setCurrentTheme(theme);
        themeRef.current = theme;
      }
    }, 100); // Small delay to avoid excessive re-renders
  }, []);

  useEffect(() => {
    // Set initial theme
    const initialTheme = detectTheme();
    setCurrentTheme(initialTheme);
    themeRef.current = initialTheme;

    // Create a mutation observer to watch for theme changes
    const observer = new MutationObserver(mutations => {
      mutations.forEach(mutation => {
        if (
          mutation.type === 'attributes' &&
          mutation.attributeName === 'class'
        ) {
          const newTheme = detectTheme();
          if (newTheme !== themeRef.current) {
            debouncedRender(newTheme);
          }
        }
      });
    });

    // Start observing the document element for class changes
    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['class'],
    });

    // Track cleanup to prevent double cleanup during rapid theme changes
    let cleanedUp = false;
    const currentTimeout = renderTimeoutRef.current;

    return () => {
      if (cleanedUp) return;
      cleanedUp = true;

      observer.disconnect();
      if (currentTimeout) {
        clearTimeout(currentTimeout);
      }
      // Clear the ref if it still points to our timeout
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

        // Dynamically import mermaid
        const mermaid = (await import('mermaid')).default;

        // Get computed CSS variables to use as actual color values
        const computedStyle = getComputedStyle(document.documentElement);
        const getCSSVariable = (variable: string) =>
          computedStyle.getPropertyValue(variable).trim();

        // Initialize mermaid with theme-aware configuration
        mermaid.initialize({
          startOnLoad: false,
          theme: 'base',
          themeVariables: {
            // Use hex colors from index.css CSS variables
            primaryColor: getCSSVariable('--primary'),
            primaryTextColor: getCSSVariable('--foreground'),
            primaryBorderColor: getCSSVariable('--foreground'),
            lineColor: getCSSVariable('--foreground'),
            sectionBkgColor: getCSSVariable('--muted'),
            altSectionBkgColor: getCSSVariable('--background'),
            gridColor: getCSSVariable('--border'),
            secondaryColor: getCSSVariable('--secondary'),
            tertiaryColor: getCSSVariable('--muted'),
            background: getCSSVariable('--background'),
            mainBkg: getCSSVariable('--background'),
            secondBkg: getCSSVariable('--muted'),
            tertiaryBkg: getCSSVariable('--accent'),
          },
          fontFamily: 'inherit',
          securityLevel: 'strict',
          htmlLabels: false, // Prevent HTML injection in diagram labels
          suppressErrorRendering: true, // Prevent Mermaid from adding error divs to the page
        });

        // Generate unique ID for this diagram
        const id = `mermaid-${crypto.randomUUID()}`;

        // Clear the element first
        elementRef.current.innerHTML = '';

        // Render the diagram
        const { svg } = await mermaid.render(id, content.trim());
        logger.debug('Mermaid rendered', { svg });

        if (mounted && elementRef.current) {
          // Sanitize SVG content before setting innerHTML
          const sanitizedSvg = sanitizeSvg(svg);
          elementRef.current.innerHTML = sanitizedSvg;
          setIsLoading(false);
        }
      } catch (err) {
        logger.error('Mermaid rendering error:', err);
        if (mounted) {
          setError(
            err instanceof Error ? err.message : 'Failed to render diagram'
          );
          setIsLoading(false);
        }
      }
    };

    renderDiagram();

    return () => {
      mounted = false;
    };
  }, [content, currentTheme]); // Re-render when content or theme changes

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
          <span>Invalid Mermaid diagram syntax</span>
        </div>
      </div>
    );
  }

  return (
    <div className="relative">
      <div className="absolute top-2 right-2 z-10">
        <CopyToClipboardButton textToCopy={content} />
      </div>
      <div className="mermaid-container rounded-md border bg-background p-4 overflow-x-auto">
        {isLoading && (
          <div className="flex items-center justify-center p-8 text-muted-foreground">
            <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
            <span className="ml-2 text-sm">Loading diagram...</span>
          </div>
        )}
        <div
          ref={elementRef}
          className="mermaid-diagram"
          style={{ minHeight: isLoading ? '100px' : 'auto' }}
        />
      </div>
    </div>
  );
});

MermaidRenderer.displayName = 'MermaidRenderer';

export default MermaidRenderer;
