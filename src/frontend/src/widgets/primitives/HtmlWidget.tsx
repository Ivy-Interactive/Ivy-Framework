import { HtmlRenderer } from '@/components/HtmlRenderer';
import React from 'react';
import { Scales } from '@/types/scale';

interface HtmlWidgetProps {
  id: string;
  content: string;
  scale?: Scales;
}

export const HtmlWidget: React.FC<HtmlWidgetProps> = ({
  id,
  content,
  scale = Scales.Medium,
}) => {
  const getScaleStyle = (s: Scales): React.CSSProperties => {
    switch (s) {
      case Scales.Small:
        return {
          transform: 'scale(0.85)',
          width: '117.65%',
          transformOrigin: 'top left',
        };
      case Scales.Large:
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
    ...getScaleStyle(scale),
  };

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
