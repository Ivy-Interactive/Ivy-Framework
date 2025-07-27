import React from 'react';
import { useEventHandler } from '@/components/EventHandlerContext';
import Icon from '@/components/Icon';
import { Badge } from '@/components/ui/badge';

interface ListItemWidgetProps {
  id: string;
  title?: string;
  subtitle?: string;
  icon?: string;
  badge?: string;
}

export const ListItemWidget: React.FC<ListItemWidgetProps> = ({
  id,
  title,
  subtitle,
  icon,
  badge,
}) => {
  const eventHandler = useEventHandler();

  return (
    <button
      onClick={() => eventHandler('OnClick', id, [])}
      className="pl-4 pr-4 w-full h-full flex-left flex items-center rounded-none hover:bg-accent focus:bg-accent focus:outline-none cursor-pointer"
    >
      <div className="flex flex-col items-start text-sm text-nowrap overflow-hidden">
        <span>{title}</span>
        {subtitle && <span className="text-muted-foreground">{subtitle}</span>}
      </div>
      {icon && icon != 'None' && (
        <Icon className="h-6 w-6 text-muted-foreground ml-auto" name={icon} />
      )}
      {badge && (
        <Badge variant="default" className="ml-auto">
          {badge}
        </Badge>
      )}
    </button>
  );
};
