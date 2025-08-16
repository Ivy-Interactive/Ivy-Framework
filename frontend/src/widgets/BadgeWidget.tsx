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
  variant = 'primary',
  size = 'Default',
}) => {
  let badgeClasses = 'badge-text-primary';
  let iconClasses = 'h-3 w-3';

  switch (size) {
    case 'Small':
      badgeClasses = 'badge-text-small';
      iconClasses = 'h-2.5 w-2.5';
      break;
    case 'Large':
      badgeClasses = 'badge-text-large';
      iconClasses = 'h-4 w-4';
      break;
    default:
      break;
  }

  // Map backend variant names to frontend badge variants
  const getBadgeVariant = (variant: string) => {
    switch (variant) {
      case 'Primary':
        return 'primary';
      case 'Destructive':
        return 'destructive';
      case 'Outline':
        return 'outline';
      case 'Secondary':
        return 'secondary';
      default:
        return camelCase(variant) as
          | 'primary'
          | 'destructive'
          | 'outline'
          | 'secondary';
    }
  };

  return (
    <Badge
      variant={getBadgeVariant(variant)}
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
