import React, { useCallback } from 'react';
import { useEventHandler } from '@/components/event-handler';
import Icon from '@/components/Icon';
import { camelCase } from '@/lib/utils';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { Densities } from '@/types/density';

const EMPTY_ARRAY: never[] = [];

interface BadgeWidgetProps {
  title: string;
  icon?: string;
  iconPosition?: 'Left' | 'Right';
  variant?:
    | 'Primary'
    | 'Destructive'
    | 'Outline'
    | 'Secondary'
    | 'Success'
    | 'Warning'
    | 'Info';
  density?: Densities;
  id: string;
  events?: string[];
}

export const BadgeWidget: React.FC<BadgeWidgetProps> = ({
  title,
  icon = undefined,
  iconPosition = 'Left',
  variant = 'Primary',
  density = Densities.Medium,
  id,
  events = EMPTY_ARRAY,
}) => {
  const eventHandler = useEventHandler();
  const isClickable = events.includes('OnClick');

  const handleClick = useCallback(() => {
    if (isClickable) {
      eventHandler('OnClick', id, []);
    }
  }, [id, isClickable, eventHandler]);
  let iconSize: number = 4;

  switch (density) {
    case Densities.Small:
      iconSize = 3;
      break;
    case Densities.Large:
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

  const hasIcon = icon && icon !== 'None';

  return (
    <Badge
      variant={getBadgeVariant(variant)}
      density={density.toLowerCase() as 'small' | 'medium' | 'large'}
      className={cn(
        'w-min whitespace-nowrap gap-1',
        hasIcon &&
          title &&
          iconPosition === 'Left' &&
          (density === Densities.Small
            ? 'pl-1'
            : density === Densities.Large
              ? 'pl-2'
              : 'pl-1.5'),
        hasIcon &&
          title &&
          iconPosition === 'Right' &&
          (density === Densities.Small
            ? 'pr-1'
            : density === Densities.Large
              ? 'pr-2'
              : 'pr-1.5'),
        isClickable && 'cursor-pointer hover:opacity-80 transition-opacity'
      )}
      onClick={isClickable ? handleClick : undefined}
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
