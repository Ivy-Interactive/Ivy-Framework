import Icon from '@/components/Icon';
import { getHeight, getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React from 'react';

interface CalloutWidgetProps {
  title?: string;
  children?: React.ReactNode;
  variant?: 'Info' | 'Success' | 'Warning' | 'Error';
  width?: string;
  height?: string;
  icon?: string;
}

const backgroundColors = {
  Info: 'bg-cyan/10 border-cyan/20',
  Success: 'bg-primary/10 border-primary/20',
  Warning: 'bg-yellow/10 border-yellow/20',
  Error: 'border-destructive',
};

const defaultIcons = {
  Info: 'Info',
  Success: 'CircleCheck',
  Warning: 'CircleAlert',
  Error: 'CircleAlert',
};

export const CalloutWidget: React.FC<CalloutWidgetProps> = ({
  title,
  children,
  variant,
  icon,
  width,
  height,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  if (!icon) {
    icon = defaultIcons[variant || 'Info'];
  }

  const variantKey = variant || 'Info';
  const bgClasses = backgroundColors[variantKey];

  return (
    <div
      style={styles}
      className={cn(
        'flex items-center p-4 text-large-body rounded-lg border',
        bgClasses,
        'text-foreground'
      )}
      role="alert"
    >
      {icon && <Icon size="30" name={icon} className="mr-4 opacity-70" />}
      <span className="sr-only">{variant}</span>
      <div className="flex flex-col">
        {title && <div className="font-medium">{title}</div>}
        {children && <div className="opacity-80">{children}</div>}
      </div>
    </div>
  );
};
