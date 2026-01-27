import { useEventHandler } from '@/components/event-handler';
import MarkdownRenderer from '@/components/MarkdownRenderer';
import React, { useCallback } from 'react';

import { WordBreak } from '@/lib/styles';
import { Scales } from '@/types/scale';

interface MarkdownWidgetProps {
  id: string;
  content: string;
  scale?: Scales;
  wordBreak?: WordBreak;
}

const MarkdownWidget: React.FC<MarkdownWidgetProps> = ({
  id,
  content = '',
  scale = Scales.Medium,
  wordBreak,
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
    ...getScaleStyle(scale),
  };

  return (
    <div className="markdown-widget w-full" style={styles}>
      <MarkdownRenderer
        key={id}
        content={content}
        onLinkClick={handleLinkClick}
        wordBreak={wordBreak}
      />
    </div>
  );
};

export default React.memo(MarkdownWidget);
