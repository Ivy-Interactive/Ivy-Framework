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

const calloutVariants = {
  Info: {
    container:
      'border-cyan/20 bg-cyan/5 text-cyan-foreground dark:border-cyan/30 dark:bg-cyan/10',
    icon: '',
  },
  Success: {
    container:
      'border-emerald/20 bg-emerald/5 text-emerald-foreground dark:border-emerald/30 dark:bg-emerald/10',
    icon: 'text-emerald dark:text-emerald-light',
  },
  Warning: {
    container:
      'border-amber/20 bg-amber/5 text-amber-foreground dark:border-amber/30 dark:bg-amber/10',
    icon: 'text-amber dark:text-amber-light',
  },
  Error: {
    container:
      'border-destructive/20 bg-destructive/5 text-destructive-foreground dark:border-destructive/30 dark:bg-destructive/10',
    icon: 'text-destructive dark:text-destructive-light',
  },
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
  const variantStyles = calloutVariants[variantKey];

  return (
    <div
      style={styles}
      className={cn(
        'flex items-center p-4 text-large-body rounded-lg border transition-colors',
        variantStyles.container
      )}
      role="alert"
    >
      {icon && (
        <Icon
          size="30"
          name={icon}
          className={cn('mr-4 shrink-0', variantStyles.icon)}
        />
      )}
      <span className="sr-only">{variant}</span>
      <div className="flex flex-col min-w-0 flex-1">
        {title && <div className="font-medium leading-none mb-1">{title}</div>}
        {children && (
          <div className="text-sm opacity-90 leading-relaxed">{children}</div>
        )}
      </div>
    </div>
  );
};
