import { useEventHandler } from '@/components/event-handler';
import MarkdownRenderer from '@/components/MarkdownRenderer';
import React, { useCallback } from 'react';

import { Scales } from '@/types/scale';
import { TextAlignment } from '@/types/textAlignment';

interface MarkdownWidgetProps {
  id: string;
  content: string;
  scale?: Scales;
  textAlignment?: TextAlignment;
}

const MarkdownWidget: React.FC<MarkdownWidgetProps> = ({
  id,
  content = '',
  scale = Scales.Medium,
  textAlignment,
}) => {
  const eventHandler = useEventHandler();

  const handleLinkClick = useCallback(
    (href: string) => eventHandler('OnLinkClick', id, [href]),
    [eventHandler, id]
  );

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
    wordBreak: 'normal',
    overflowWrap: 'break-word',
    ...(textAlignment && {
      textAlign:
        textAlignment.toLowerCase() as React.CSSProperties['textAlign'],
    }),
    ...getScaleStyle(scale),
  };

  return (
    <div className="markdown-widget w-full" style={styles}>
      <MarkdownRenderer
        key={id}
        content={content}
        onLinkClick={handleLinkClick}
      />
    </div>
  );
};

export default React.memo(MarkdownWidget);
