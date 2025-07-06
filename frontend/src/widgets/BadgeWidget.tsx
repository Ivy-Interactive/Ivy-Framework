import React from 'react';
import Icon from '@/components/Icon';
import { camelCase } from '@/lib/utils';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';

interface BadgeWidgetProps {
  title: string;
  icon?: string;
  iconPosition?: 'Left' | 'Right';
  variant?: string;
  size?: 'Default' | 'Small' | 'Large';
}

export const BadgeWidget: React.FC<BadgeWidgetProps> = ({
  title,
  icon = undefined,
  iconPosition = 'Left',
  variant = 'default',
  size = 'Default',
}) => {
  let badgeClasses = 'text-sm px-2.5 py-0.5';
  let iconClasses = 'h-3 w-3';

  switch (size) {
    case 'Small':
      badgeClasses = 'text-xs px-2 py-0.5';
      iconClasses = 'h-2.5 w-2.5';
      break;
    case 'Large':
      badgeClasses = 'text-base px-3 py-1';
      iconClasses = 'h-4 w-4';
      break;
    default:
      break;
  }

  return (
    <Badge
      variant={
        camelCase(variant) as
          | 'default'
          | 'destructive'
          | 'outline'
          | 'secondary'
      }
      className={cn('w-min whitespace-nowrap', badgeClasses)}
    >
      {iconPosition === 'Left' && icon && icon !== 'None' && (
        <Icon className={cn('mr-1', iconClasses)} name={icon} />
      )}
      {title}
      {iconPosition === 'Right' && icon && icon !== 'None' && (
        <Icon className={cn('ml-1', iconClasses)} name={icon} />
      )}
    </Badge>
  );
};
