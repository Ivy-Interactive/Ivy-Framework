import React from 'react';
import { ivyTagClassMap } from '@/lib/utils';
import CopyToClipboardButton from './CopyToClipboardButton';

interface HtmlRendererProps {
  content: string;
  className?: string;
  allowedTags?: string[];
  onLinkClick?: (
    event: React.MouseEvent<HTMLAnchorElement>,
    href: string
  ) => void;
}

const sanitizeHtml = (
  html: string,
  allowedTags: string[] = [
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
    'img',
    'table',
    'thead',
    'tbody',
    'tr',
    'th',
    'td',
    'blockquote',
    'pre',
    'code',
  ]
): string => {
  const temp = document.createElement('div');
  temp.innerHTML = html;

  // Remove any script tags and inline event handlers
  const scripts = temp.getElementsByTagName('script');
  const elements = temp.getElementsByTagName('*');

  // Remove script tags
  while (scripts[0]) {
    scripts[0].parentNode?.removeChild(scripts[0]);
  }

  // Remove inline event handlers and potentially harmful attributes
  for (let i = 0; i < elements.length; i++) {
    const element = elements[i];
    const attrs = element.attributes;

    for (let j = attrs.length - 1; j >= 0; j--) {
      const attr = attrs[j];
      if (
        attr.name.startsWith('on') ||
        (attr.name === 'href' && attr.value.startsWith('javascript:'))
      ) {
        element.removeAttribute(attr.name);
      }
    }

    // Remove elements with tags not in allowedTags
    if (!allowedTags.includes(element.tagName.toLowerCase())) {
      element.parentNode?.removeChild(element);
    }
  }

  return temp.innerHTML;
};

function domToReact(node: ChildNode): React.ReactNode {
  if (node.nodeType === Node.TEXT_NODE) {
    return node.textContent;
  }
  if (node.nodeType !== Node.ELEMENT_NODE) {
    return null;
  }
  const el = node as HTMLElement;
  const tag = el.tagName.toLowerCase();
  const className = ivyTagClassMap[tag] || undefined;
  const children = Array.from(el.childNodes).map(domToReact);
  const props: Record<string, unknown> = { className };

  // Special handling for different tags
  if (tag === 'a') {
    props.href = el.getAttribute('href');
    props.target = el.getAttribute('target') || undefined;
    props.rel = el.getAttribute('rel') || undefined;
    props.onClick = (e: React.MouseEvent<HTMLAnchorElement>) => {
      if (
        props.href &&
        typeof props.href === 'string' &&
        props.href.startsWith('javascript:')
      ) {
        e.preventDefault();
        return;
      }
    };
  } else if (tag === 'img') {
    props.src = el.getAttribute('src');
    props.alt = el.getAttribute('alt') || '';
    props.loading = 'lazy';
    props.className =
      'w-32 h-20 bg-gray-300 flex items-center justify-center text-gray-600 text-sm rounded';
  } else if (tag === 'code') {
    // Handle code blocks with copy button like in MarkdownRenderer
    const content = el.textContent || '';
    return (
      <div className="relative">
        <div className="absolute top-2 right-2 z-10">
          <CopyToClipboardButton textToCopy={content} />
        </div>
        <pre className="bg-gray-900 text-gray-100 p-4 rounded-lg overflow-x-auto">
          <code className="text-sm font-mono">{content}</code>
        </pre>
      </div>
    );
  } else if (tag === 'pre') {
    // Handle pre tags by just passing through children
    return <>{children}</>;
  } else if (tag === 'blockquote') {
    props.className = 'border-l-4 border-gray-300 pl-4 italic text-gray-600';
  } else if (tag === 'table') {
    props.className = 'w-full border-collapse border border-gray-300 my-4';
  } else if (tag === 'thead') {
    props.className = 'bg-gray-100';
  } else if (tag === 'tr') {
    props.className = 'border border-gray-300';
  } else if (tag === 'th') {
    props.className = 'border border-gray-300 p-3 text-left font-semibold';
  } else if (tag === 'td') {
    props.className = 'border border-gray-300 p-3';
  }

  return React.createElement(tag, props, ...children);
}

export const HtmlRenderer: React.FC<Omit<HtmlRendererProps, 'onLinkClick'>> = ({
  content,
  allowedTags,
}) => {
  // Sanitize the HTML content
  const sanitizedContent = sanitizeHtml(content, allowedTags);

  // Parse the sanitized HTML into a DOM tree
  const temp = document.createElement('div');
  temp.innerHTML = sanitizedContent;
  const nodes = Array.from(temp.childNodes);

  // Render as React elements with Ivy classes
  return (
    <>
      {nodes.map((node, i) => (
        <React.Fragment key={i}>{domToReact(node)}</React.Fragment>
      ))}
    </>
  );
};
