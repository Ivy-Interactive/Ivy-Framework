import React from 'react';
import { useEventHandler } from '@/components/event-handler';
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
      className="pl-4 pr-4 w-full h-full flex-left flex items-center rounded-none hover:bg-accent focus:bg-accent focus:outline-none cursor-pointer min-w-0"
    >
      <div className="flex flex-col items-start text-body w-full flex-1 min-w-0 text-left">
        <span className="block w-full truncate text-left">{title}</span>
        {subtitle && (
          <span className="block text-sm text-muted-foreground w-full truncate text-left">
            {subtitle}
          </span>
        )}
      </div>
      {icon && icon != 'None' && (
        <Icon
          className="h-6 w-6 text-muted-foreground ml-auto flex-none"
          name={icon}
        />
      )}
      {badge && (
        <Badge variant="primary" className="ml-auto flex-none">
          {badge}
        </Badge>
      )}
    </button>
  );
};
