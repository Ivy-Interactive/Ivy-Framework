import React from 'react';
import Icon from '@/components/Icon';
import { camelCase } from '@/lib/utils';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { Scales } from '@/types/scale';

interface BadgeWidgetProps {
  title: string;
  icon?: string;
  iconPosition?: 'Left' | 'Right';
  variant?: string;
  scale?: Scales;
}

export const BadgeWidget: React.FC<BadgeWidgetProps> = ({
  title,
  icon = undefined,
  iconPosition = 'Left',
  variant = 'primary',
  scale = Scales.Medium,
}) => {
  let badgeClasses = 'badge-text-primary';
  let iconSize: number = 4;

  switch (scale) {
    case Scales.Small:
      badgeClasses = 'badge-text-small';
      iconSize = 3;
      break;
    case Scales.Large:
      badgeClasses = 'badge-text-large';
      iconSize = 5;
      break;
    default:
      break;
  }

  const iconStyles: React.CSSProperties = {
    width: `${iconSize * 0.25}rem`,
    height: `${iconSize * 0.25}rem`,
  };

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
      case 'Success':
        return 'success';
      case 'Warning':
        return 'warning';
      case 'Info':
        return 'info';
      default:
        return camelCase(variant) as
          | 'primary'
          | 'destructive'
          | 'outline'
          | 'secondary'
          | 'success'
          | 'warning'
          | 'info';
    }
  };

  return (
    <Badge
      variant={getBadgeVariant(variant)}
      className={cn('w-min whitespace-nowrap gap-1', badgeClasses)}
    >
      {iconPosition === 'Left' && icon && icon !== 'None' && (
        <Icon style={iconStyles} name={icon} />
      )}
      {title}
      {iconPosition === 'Right' && icon && icon !== 'None' && (
        <Icon style={iconStyles} name={icon} />
      )}
    </Badge>
  );
};
