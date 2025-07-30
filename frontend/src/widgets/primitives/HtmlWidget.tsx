import { HtmlRenderer } from '@/components/HtmlRenderer';
import React from 'react';

interface HtmlWidgetProps {
  id: string;
  content: string;
}

export const HtmlWidget: React.FC<HtmlWidgetProps> = ({ id, content }) => (
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
);
