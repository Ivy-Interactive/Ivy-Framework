import { getHeight, getWidth } from '@/lib/styles';
import React, { useEffect, useRef, useState } from 'react';

interface IframeWidgetProps {
  id: string;
  src: string;
  width?: string;
  height?: string;
  refreshToken?: number;
}

export const IframeWidget: React.FC<IframeWidgetProps> = ({
  id,
  src = '',
  width = 'Full',
  height = 'Full',
  refreshToken,
}) => {
  const [iframeKey, setIframeKey] = useState(id);
  const iframeRef = useRef<HTMLIFrameElement>(null);

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    maxWidth: '100%',
  };

  useEffect(() => {
    setIframeKey(`${id}-${refreshToken}`);
  }, [refreshToken, id]);

  const iframeLoadedRef = useRef(false);

  // Reset loaded state when iframe key changes
  useEffect(() => {
    iframeLoadedRef.current = false;
  }, [iframeKey]);

  const handleIframeLoad = () => {
    iframeLoadedRef.current = true;
  };

  return (
    <iframe
      ref={iframeRef}
      src={src}
      key={iframeKey}
      style={styles}
      onLoad={handleIframeLoad}
    />
  );
};
