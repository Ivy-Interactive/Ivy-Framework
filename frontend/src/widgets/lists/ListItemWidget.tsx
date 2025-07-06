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
      className="pl-4 pr-4 w-full h-full flex-left flex items-center rounded-none hover:bg-gray-100 focus:bg-gray-100 focus:outline-none"
    >
      <div className="flex flex-col items-start text-sm text-nowrap overflow-hidden">
        <span>{title}</span>
        {subtitle && <span className="text-gray-500">{subtitle}</span>}
      </div>
      {icon && icon != 'None' && (
        <Icon className="h-6 w-6 text-gray-500 ml-auto" name={icon} />
      )}
      {badge && (
        <Badge variant="default" className="ml-auto">
          {badge}
        </Badge>
      )}
    </button>
  );
};
