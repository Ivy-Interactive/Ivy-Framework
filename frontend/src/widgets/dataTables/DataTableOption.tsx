import React, { ReactNode, useState } from 'react';
import { LucideIcon } from 'lucide-react';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { cn } from '@/lib/utils';

/**
 * Display modes for DataTableOption
 */
export type OptionDisplayMode = 'popover' | 'inline';
export type InlineDirection = 'right' | 'left' | 'below';

/**
 * Props for DataTableOption component
 */
export interface DataTableOptionProps {
  icon: LucideIcon;
  label: string;
  tooltip?: string;
  children: ReactNode;
  className?: string;
  contentClassName?: string;

  // Display mode configuration
  displayMode?: OptionDisplayMode;

  // Popover specific props
  align?: 'start' | 'center' | 'end';
  side?: 'top' | 'right' | 'bottom' | 'left';
  sideOffset?: number;
  contentWidth?: string;

  // Inline specific props
  inlineDirection?: InlineDirection;
  defaultExpanded?: boolean;

  // Button configuration
  showLabel?: boolean;
}

/**
 * DataTableOption - A configurable option button that can show content
 * either in a popover or inline expansion with unified border animation
 */
export const DataTableOption: React.FC<DataTableOptionProps> = ({
  icon: Icon,
  label,
  tooltip,
  children,
  className,
  contentClassName,
  displayMode = 'inline',
  align = 'start',
  side = 'bottom',
  sideOffset = 8,
  contentWidth = 'w-[400px]',
  inlineDirection = 'right',
  defaultExpanded = false,
  showLabel = true,
}) => {
  const [expanded, setExpanded] = useState(defaultExpanded);

  // Popover mode - uses default button styling
  if (displayMode === 'popover') {
    return (
      <Popover>
        <PopoverTrigger asChild>
          <button
            className={cn(
              'inline-flex items-center justify-center rounded-md text-sm font-medium',
              'h-9 px-3 gap-2 cursor-pointer',
              'bg-transparent hover:bg-accent hover:text-accent-foreground',
              'border border-input',
              'transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
              className
            )}
            title={tooltip || label}
          >
            <Icon className="h-4 w-4" />
            {showLabel && <span className="text-sm">{label}</span>}
          </button>
        </PopoverTrigger>
        <PopoverContent
          align={align}
          side={side}
          sideOffset={sideOffset}
          className={cn(contentWidth, 'p-0', contentClassName)}
        >
          {children}
        </PopoverContent>
      </Popover>
    );
  }

  // Inline expansion mode with unified border
  const buttonContent = (
    <>
      <Icon className="h-4 w-4" />
      {showLabel && <span className="text-sm">{label}</span>}
    </>
  );

  if (inlineDirection === 'right') {
    return (
      <div
        className={cn(
          'inline-flex items-center mb-3',
          'border rounded-md',
          'bg-transparent border-input',
          'transition-all duration-300 ease-in-out',
          !expanded && 'hover:bg-accent',
          className
        )}
      >
        <button
          className={cn(
            'inline-flex items-center justify-center text-sm font-medium',
            'h-9 w-9 gap-2 cursor-pointer flex-shrink-0',
            'bg-transparent hover:bg-accent hover:text-accent-foreground rounded-l-md',
            'transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
            expanded && 'bg-accent'
          )}
          onClick={() => setExpanded(!expanded)}
          title={tooltip || label}
        >
          {buttonContent}
        </button>

        {/* Content container - fixed dimensions when expanded */}
        <div
          className={cn(
            'overflow-hidden border-l h-9',
            'transition-all duration-300 ease-in-out',
            expanded
              ? 'w-[450px] opacity-100 border-input/30' // Fixed width when expanded
              : 'w-0 opacity-0 border-transparent'
          )}
        >
          <div
            className={cn(
              'h-full w-[450px] flex items-center px-2 overflow-hidden', // Added overflow-hidden
              contentClassName
            )}
          >
            {children}
          </div>
        </div>
      </div>
    );
  }

  if (inlineDirection === 'left') {
    return (
      <div
        className={cn(
          'inline-flex items-center mb-3',
          'border rounded-md',
          'transition-all duration-300 ease-in-out',
          'bg-transparent',
          expanded ? 'border-input' : 'border-input hover:bg-accent',
          className
        )}
      >
        {/* Sliding content container */}
        <div
          className={cn(
            'overflow-hidden transition-all duration-300 ease-in-out',
            'border-r',
            expanded
              ? 'max-w-[800px] opacity-100 border-input/30'
              : 'max-w-0 opacity-0 border-transparent'
          )}
        >
          <div className={cn('h-9 flex items-center', contentClassName)}>
            {children}
          </div>
        </div>

        <button
          className={cn(
            'inline-flex items-center justify-center text-sm font-medium',
            'h-9 px-3 gap-2 cursor-pointer',
            'bg-transparent hover:bg-accent hover:text-accent-foreground rounded-r-md',
            'transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
            expanded && 'bg-accent'
          )}
          onClick={() => setExpanded(!expanded)}
          title={tooltip || label}
        >
          {buttonContent}
        </button>
      </div>
    );
  }

  // Default: below with unified border
  return (
    <div
      className={cn(
        'inline-flex flex-col mb-3',
        'border rounded-md',
        'transition-all duration-300 ease-in-out',
        'bg-transparent',
        expanded ? 'border-input' : 'border-input hover:bg-accent',
        className
      )}
    >
      <button
        className={cn(
          'inline-flex items-center justify-center text-sm font-medium',
          'h-9 px-3 gap-2 w-full cursor-pointer',
          'bg-transparent hover:bg-accent hover:text-accent-foreground',
          'transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
          expanded && 'bg-accent border-b border-input/30 rounded-t-md'
        )}
        onClick={() => setExpanded(!expanded)}
        title={tooltip || label}
      >
        {buttonContent}
      </button>

      <div
        className={cn(
          'overflow-hidden transition-all duration-300 ease-in-out',
          expanded ? 'max-h-[200px] opacity-100' : 'max-h-0 opacity-0'
        )}
      >
        <div className={cn('p-2', contentClassName)}>{children}</div>
      </div>
    </div>
  );
};
