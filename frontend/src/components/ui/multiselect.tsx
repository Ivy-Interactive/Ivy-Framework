import * as React from 'react';
import { X } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Command, CommandGroup, CommandItem } from '@/components/ui/command';
import { Command as CommandPrimitive } from 'cmdk';
import { cn } from '@/lib/utils';
import { cva } from 'class-variance-authority';
import { Sizes } from '@/types/sizes';

// Variants for MultipleSelector
const multipleSelectorVariants = cva(
  'group border border-input bg-transparent shadow-sm ring-offset-background rounded-md focus-within:ring-1 focus-within:ring-ring',
  {
    variants: {
      size: {
        Small: 'px-2 py-1 text-xs',
        Medium: 'px-3 py-2 text-sm',
        Large: 'px-4 py-3 text-base',
      },
    },
    defaultVariants: {
      size: 'Medium',
    },
  }
);

// Variants for menu items
const menuItemVariants = cva('cursor-pointer', {
  variants: {
    size: {
      Small: 'px-2 py-1 text-xs',
      Medium: 'px-3 py-2 text-sm',
      Large: 'px-4 py-3 text-base',
    },
  },
  defaultVariants: {
    size: 'Medium',
  },
});

// Variants for Badge components
const badgeVariants = cva('hover:bg-secondary', {
  variants: {
    size: {
      Small: 'text-xs',
      Medium: 'text-sm',
      Large: 'text-base',
    },
  },
  defaultVariants: {
    size: 'Medium',
  },
});

// Variants for close icon
const closeIconVariants = cva('text-muted-foreground hover:text-foreground', {
  variants: {
    size: {
      Small: 'h-3 w-3',
      Medium: 'h-4 w-4',
      Large: 'h-5 w-5',
    },
  },
  defaultVariants: {
    size: 'Medium',
  },
});

export interface Option {
  label: string;
  value: string;
  disable?: boolean;
}

interface MultipleSelectorProps {
  value?: Option[];
  defaultOptions?: Option[];
  onValueChange?: (value: Option[]) => void;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
  commandProps?: {
    label?: string;
  };
  hideClearAllButton?: boolean;
  hidePlaceholderWhenSelected?: boolean;
  emptyIndicator?: React.ReactNode;
  invalid?: boolean;
  size?: Sizes;
}

const MultipleSelector = React.forwardRef<
  React.ElementRef<typeof CommandPrimitive>,
  MultipleSelectorProps
>(
  (
    {
      value = [],
      defaultOptions = [],
      onValueChange,
      placeholder = 'Select options...',
      disabled = false,
      className,
      commandProps,
      hideClearAllButton = false,
      hidePlaceholderWhenSelected = false,
      emptyIndicator,
      invalid = false,
      size = Sizes.Medium,
    },
    ref
  ) => {
    const inputRef = React.useRef<HTMLInputElement>(null);
    const [open, setOpen] = React.useState(false);
    const [inputValue, setInputValue] = React.useState('');

    const handleUnselect = React.useCallback(
      (option: Option) => {
        onValueChange?.(value.filter(item => item.value !== option.value));
      },
      [onValueChange, value]
    );

    const handleKeyDown = React.useCallback(
      (e: React.KeyboardEvent<HTMLDivElement>) => {
        const input = inputRef.current;
        if (input) {
          if (e.key === 'Delete' || e.key === 'Backspace') {
            if (input.value === '' && value.length > 0) {
              const lastValue = value[value.length - 1];
              handleUnselect(lastValue);
            }
          }
          if (e.key === 'Escape') {
            input.blur();
          }
        }
      },
      [value, handleUnselect]
    );

    const selectables = defaultOptions.filter(
      option => !value.find(item => item.value === option.value)
    );

    return (
      <Command
        ref={ref}
        onKeyDown={handleKeyDown}
        className={cn('overflow-visible bg-transparent', className)}
        {...commandProps}
      >
        <div
          className={cn(
            multipleSelectorVariants({ size }),
            invalid
              ? 'border-destructive text-destructive-foreground focus-within:ring-destructive focus-within:border-destructive'
              : undefined
          )}
        >
          <div className="flex gap-1 flex-wrap">
            {value.map(option => (
              <Badge
                key={option.value}
                variant="secondary"
                className={cn(
                  badgeVariants({ size }),
                  invalid &&
                    'bg-destructive/10 border-destructive text-destructive'
                )}
              >
                {option.label}
                <button
                  className="ml-1 ring-offset-background rounded-full outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 cursor-pointer hover:bg-accent hover:text-accent-foreground"
                  onKeyDown={e => {
                    if (e.key === 'Enter') {
                      handleUnselect(option);
                    }
                  }}
                  onMouseDown={e => {
                    e.preventDefault();
                    e.stopPropagation();
                  }}
                  onClick={() => handleUnselect(option)}
                >
                  <X className={closeIconVariants({ size })} />
                </button>
              </Badge>
            ))}
            <CommandPrimitive.Input
              ref={inputRef}
              value={inputValue}
              onValueChange={setInputValue}
              onBlur={() => setOpen(false)}
              onFocus={() => setOpen(true)}
              placeholder={
                hidePlaceholderWhenSelected && value.length > 0
                  ? undefined
                  : placeholder
              }
              disabled={disabled}
              className="ml-2 bg-transparent outline-none placeholder:text-muted-foreground flex-1"
            />
          </div>
        </div>
        <div className="relative mt-2">
          {open && selectables.length > 0 && (
            <div className="absolute w-full z-10 top-0 rounded-md border bg-popover text-popover-foreground shadow-md outline-none animate-in">
              <CommandGroup className="h-full overflow-auto">
                {selectables.map(option => {
                  return (
                    <CommandItem
                      key={option.value}
                      onMouseDown={e => {
                        e.preventDefault();
                        e.stopPropagation();
                      }}
                      onSelect={() => {
                        setInputValue('');
                        onValueChange?.([...value, option]);
                      }}
                      className={menuItemVariants({ size })}
                      disabled={option.disable}
                    >
                      {option.label}
                    </CommandItem>
                  );
                })}
              </CommandGroup>
            </div>
          )}
        </div>
        {!hideClearAllButton && value.length > 0 && (
          <button
            onClick={() => onValueChange?.([])}
            className="mt-2 text-xs text-muted-foreground hover:text-foreground"
          >
            Clear all
          </button>
        )}
        {selectables.length === 0 && emptyIndicator && (
          <div className="mt-2">{emptyIndicator}</div>
        )}
      </Command>
    );
  }
);

MultipleSelector.displayName = 'MultipleSelector';

export { MultipleSelector };
