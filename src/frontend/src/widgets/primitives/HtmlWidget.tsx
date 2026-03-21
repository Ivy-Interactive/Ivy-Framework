import { HtmlRenderer } from '@/components/HtmlRenderer';
import React, { useEffect, useRef } from 'react';
import { Densities } from '@/types/density';

interface HtmlWidgetProps {
  id: string;
  content: string;
  density?: Densities;
  dangerouslyAllowScripts?: boolean;
}

export const HtmlWidget: React.FC<HtmlWidgetProps> = ({
  id,
  content,
  density = Densities.Medium,
  dangerouslyAllowScripts = false,
}) => {
  const getScaleStyle = (s: Densities): React.CSSProperties => {
    switch (s) {
      case Densities.Small:
        return {
          transform: 'scale(0.85)',
          width: '117.65%',
          transformOrigin: 'top left',
        };
      case Densities.Large:
        return {
          transform: 'scale(1.15)',
          width: '86.96%',
          transformOrigin: 'top left',
        };
      default:
        return {};
    }
  };

  const styles: React.CSSProperties = {
    display: 'flex',
    flexDirection: 'column',
    gap: '1rem',
    ...getScaleStyle(density),
  };

  const containerRef = useRef<HTMLDivElement>(null);

  // Execute scripts when dangerouslyAllowScripts is enabled
  useEffect(() => {
    if (!dangerouslyAllowScripts || !containerRef.current) return;

    // Find all script tags and execute them
    const scripts = containerRef.current.querySelectorAll('script');
    scripts.forEach(oldScript => {
      const newScript = document.createElement('script');
      // Copy all attributes
      Array.from(oldScript.attributes).forEach(attr => {
        newScript.setAttribute(attr.name, attr.value);
      });
      // Copy inline script content
      newScript.textContent = oldScript.textContent;
      // Replace old script with new one to trigger execution
      oldScript.parentNode?.replaceChild(newScript, oldScript);
    });
  }, [dangerouslyAllowScripts, content]);

  if (dangerouslyAllowScripts) {
    return (
      <div
        ref={containerRef}
        style={styles}
        className="w-full"
        dangerouslySetInnerHTML={{ __html: content }}
      />
    );
  }

  return (
    <div style={styles} className="w-full">
      <HtmlRenderer
        content={content}
        key={id}
        allowedTags={[
          'p',
          'div',
          'span',
          'h1',
          'h2',
          'h3',
          'h4',
          'h5',
          'h6',
          'ul',
          'ol',
          'li',
          'a',
          'strong',
          'em',
          'b',
          'i',
          'br',
          'pre',
          'code',
          'blockquote',
          'hr',
          'table',
          'thead',
          'tbody',
          'tr',
          'th',
          'td',
          'img',
        ]}
      />
    </div>
  );
};
