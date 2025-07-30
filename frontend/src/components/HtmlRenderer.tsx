import React from 'react';
import { textBlockClassMap, textContainerClass } from '@/lib/textBlockClassMap';

interface HtmlRendererProps {
  content: string;
  allowedTags?: string[];
}

export const HtmlRenderer: React.FC<HtmlRendererProps> = ({
  content,
  allowedTags = [
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
  ],
}) => {
  // Simple HTML sanitization - only allow specified tags
  const sanitizeHtml = (html: string): string => {
    const parser = new DOMParser();
    const doc = parser.parseFromString(html, 'text/html');

    // Remove all tags that are not in allowedTags
    const walker = document.createTreeWalker(
      doc.body,
      NodeFilter.SHOW_ELEMENT,
      null
    );

    const nodesToRemove: Element[] = [];
    let node;
    while ((node = walker.nextNode())) {
      const element = node as Element;
      if (!allowedTags.includes(element.tagName.toLowerCase())) {
        nodesToRemove.push(element);
      }
    }

    // Remove disallowed tags
    nodesToRemove.forEach(element => {
      const parent = element.parentNode;
      if (parent) {
        while (element.firstChild) {
          parent.insertBefore(element.firstChild, element);
        }
        parent.removeChild(element);
      }
    });

    return doc.body.innerHTML;
  };

  const sanitizedContent = sanitizeHtml(content);

  // Parse HTML and render with proper React components and classes
  const renderHtml = (html: string): React.ReactNode => {
    const parser = new DOMParser();
    const doc = parser.parseFromString(html, 'text/html');

    const renderNode = (node: Node): React.ReactNode => {
      if (node.nodeType === Node.TEXT_NODE) {
        return node.textContent;
      }

      if (node.nodeType === Node.ELEMENT_NODE) {
        const element = node as Element;
        const tagName = element.tagName.toLowerCase();
        const children = Array.from(element.childNodes).map(renderNode);

        switch (tagName) {
          case 'h1':
            return <h1 className={textBlockClassMap.h1}>{children}</h1>;
          case 'h2':
            return <h2 className={textBlockClassMap.h2}>{children}</h2>;
          case 'h3':
            return <h3 className={textBlockClassMap.h3}>{children}</h3>;
          case 'h4':
            return <h4 className={textBlockClassMap.h4}>{children}</h4>;
          case 'p':
            return <p className={textBlockClassMap.p}>{children}</p>;
          case 'ul':
            return <ul className={textBlockClassMap.ul}>{children}</ul>;
          case 'ol':
            return <ol className={textBlockClassMap.ol}>{children}</ol>;
          case 'li':
            return <li className={textBlockClassMap.li}>{children}</li>;
          case 'strong':
            return (
              <strong className={textBlockClassMap.strong}>{children}</strong>
            );
          case 'em':
            return <em className={textBlockClassMap.em}>{children}</em>;
          case 'a':
            return (
              <a
                className={textBlockClassMap.a}
                href={element.getAttribute('href') || '#'}
                target="_blank"
                rel="noopener noreferrer"
              >
                {children}
              </a>
            );
          case 'blockquote':
            return (
              <blockquote className={textBlockClassMap.blockquote}>
                {children}
              </blockquote>
            );
          case 'table':
            return (
              <table className={textBlockClassMap.table}>{children}</table>
            );
          case 'thead':
            return <thead className="bg-muted">{children}</thead>;
          case 'tr':
            return <tr className="border border-border">{children}</tr>;
          case 'th':
            return <th className={textBlockClassMap.th}>{children}</th>;
          case 'td':
            return <td className={textBlockClassMap.td}>{children}</td>;
          case 'code':
            return <code className={textBlockClassMap.code}>{children}</code>;
          case 'img':
            return (
              <img
                src={element.getAttribute('src') || ''}
                alt={element.getAttribute('alt') || ''}
                className={textBlockClassMap.img}
              />
            );
          case 'hr':
            return <hr className="my-6" />;
          case 'br':
            return <br />;
          default:
            return <div>{children}</div>;
        }
      }

      return null;
    };

    return Array.from(doc.body.childNodes).map((node, index) => (
      <React.Fragment key={index}>{renderNode(node)}</React.Fragment>
    ));
  };

  return (
    <div className={textContainerClass}>{renderHtml(sanitizedContent)}</div>
  );
};
