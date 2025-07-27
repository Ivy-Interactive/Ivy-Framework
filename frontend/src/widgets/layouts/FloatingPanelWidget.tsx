import { Align, getMargin } from '@/lib/styles';
import React from 'react';

interface FloatingPanelWidgetProps {
  id: string;
  align: Align;
  offset: string;
  children: React.ReactNode;
}

export const FloatingPanelWidget = ({
  id,
  align,
  offset,
  children,
}: FloatingPanelWidgetProps) => {
  const styles = {
    ...getMargin(offset),
  };

  const positionClasses: { [key in Align]: string } = {
    TopLeft: 'top-4 left-4',
    TopRight: 'top-4 right-4',
    BottomLeft: 'bottom-4 left-4',
    BottomRight: 'bottom-4 right-4',
    TopCenter: 'top-4 left-1/2 -translate-x-1/2',
    BottomCenter: 'bottom-4 left-1/2 -translate-x-1/2',
    Left: 'top-1/2 left-4 -translate-y-1/2',
    Right: 'top-1/2 right-4 -translate-y-1/2',
    Center: 'top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2',
    Stretch: '',
  };
  return (
    <div
      className={`fixed ${positionClasses[align]} z-50`}
      style={styles}
      key={id}
    >
      {children}
    </div>
  );
};
