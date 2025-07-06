import { getHeight, getWidth } from '@/lib/styles';
import React, { useEffect, useState } from 'react';

interface IframeWidgetProps {
  id: string;
  src: string;
  width?: string;
  height?: string;
  refreshToken?: number;
}

export const IframeWidget: React.FC<IframeWidgetProps> = ({
  id,
  src,
  width,
  height,
  refreshToken,
}) => {
  const [iframeKey, setIframeKey] = useState(id);

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  useEffect(() => {
    setIframeKey(`${id}-${refreshToken}`);
  }, [refreshToken, id]);

  return <iframe src={src} key={iframeKey} style={styles} />;
};
