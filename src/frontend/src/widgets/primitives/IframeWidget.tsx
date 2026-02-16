import { getHeight, getWidth } from '@/lib/styles';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useEventHandler } from '@/components/event-handler';

interface IframeWidgetProps {
  id: string;
  src: string;
  width?: string;
  height?: string;
  refreshToken?: number;
  events?: string[];
  outboundMessageType?: string;
  outboundMessageToken?: string;
}

export const IframeWidget: React.FC<IframeWidgetProps> = ({
  id,
  src = '',
  width = 'Full',
  height = 'Full',
  refreshToken,
  events = [],
  outboundMessageType,
  outboundMessageToken,
}) => {
  const [iframeKey, setIframeKey] = useState(id);
  const iframeRef = useRef<HTMLIFrameElement>(null);
  const eventHandler = useEventHandler();

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    maxWidth: '100%',
  };

  useEffect(() => {
    setIframeKey(`${id}-${refreshToken}`);
  }, [refreshToken, id]);

  const handleMessage = useCallback(
    (event: MessageEvent) => {
      if (!events.includes('OnMessageReceived')) return;
      if (iframeRef.current?.contentWindow !== event.source) return;

      const { type, payload } = event.data ?? {};
      if (typeof type === 'string') {
        eventHandler('OnMessageReceived', id, [type, payload]);
      }
    },
    [id, events, eventHandler]
  );

  useEffect(() => {
    window.addEventListener('message', handleMessage);
    return () => window.removeEventListener('message', handleMessage);
  }, [handleMessage]);

  useEffect(() => {
    if (!outboundMessageType || outboundMessageToken == null) return;
    iframeRef.current?.contentWindow?.postMessage(
      { type: outboundMessageType, token: outboundMessageToken },
      '*'
    );
  }, [outboundMessageType, outboundMessageToken]);

  return <iframe ref={iframeRef} src={src} key={iframeKey} style={styles} />;
};
