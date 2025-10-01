import { cn } from '@/lib/utils';
import React from 'react';

interface DetailWidgetProps {
  id: string;
  label: string;
  multiLine?: boolean;
  children?: React.ReactNode[];
}

export const DetailWidget: React.FC<DetailWidgetProps> = ({
  id,
  label,
  children,
  multiLine,
}) => {
  return (
    <div
      className={cn(
        'border-b flex',
        multiLine && 'flex-col',
        !multiLine && 'items-center'
      )}
      key={id}
    >
      <div
        className={cn(
          'p-3 pl-0 align-middle text-body font-bold whitespace-nowrap'
        )}
      >
        {label}
      </div>
      <div
        className={cn(
          'align-middle text-body min-w-0',
          multiLine
            ? 'whitespace-normal break-words text-left pb-3'
            : 'truncate text-right ml-auto p-3 pl-2 pr-0'
        )}
      >
        {children}
      </div>
    </div>
  );
};
