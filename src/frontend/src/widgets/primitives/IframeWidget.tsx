import { getHeight, getWidth } from '@/lib/styles';
import React, { useEffect, useState } from 'react';

interface IframeWidgetProps {
  id: string;
  src: string;
  width?: string;
  height?: string;
  refreshToken?: number;
  allowJavaScript?: boolean;
}

export const IframeWidget: React.FC<IframeWidgetProps> = ({
  id,
  src = '',
  width = 'Full',
  height = 'Full',
  refreshToken,
  allowJavaScript = false,
}) => {
  const [iframeKey, setIframeKey] = useState(id);

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    maxWidth: '100%',
  };

  useEffect(() => {
    setIframeKey(`${id}-${refreshToken}`);
  }, [refreshToken, id]);

  const sandbox = [
    'allow-forms',
    'allow-modals',
    'allow-popups',
    'allow-same-origin',
    allowJavaScript ? 'allow-scripts' : '',
  ]
    .filter(Boolean)
    .join(' ');

  return <iframe src={src} key={iframeKey} style={styles} sandbox={sandbox} />;
};
