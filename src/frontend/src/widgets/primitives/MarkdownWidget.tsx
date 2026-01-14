import { useEventHandler } from '@/components/event-handler';
import MarkdownRenderer from '@/components/MarkdownRenderer';
import React, { useCallback, useState, useEffect } from 'react';
import {
  widgetContentOverrides,
  subscribeToContentOverride,
} from '@/widgets/widgetRenderer';

interface MarkdownWidgetProps {
  id: string;
  content: string;
}

const MarkdownWidget: React.FC<MarkdownWidgetProps> = ({
  id,
  content = '',
}) => {
  const eventHandler = useEventHandler();
  const [, forceUpdate] = useState(0);

  // Subscribe to content override changes
  useEffect(() => {
    return subscribeToContentOverride(id, () => forceUpdate(n => n + 1));
  }, [id]);

  const handleLinkClick = useCallback(
    (href: string) => eventHandler('OnLinkClick', id, [href]),
    [eventHandler, id]
  );

  // Use override content if available, otherwise use prop
  const displayContent = widgetContentOverrides.get(id) ?? content;

  return (
    <div className="markdown-widget w-full">
      <MarkdownRenderer
        key={id}
        content={displayContent}
        onLinkClick={handleLinkClick}
      />
    </div>
  );
};

export default React.memo(MarkdownWidget);
