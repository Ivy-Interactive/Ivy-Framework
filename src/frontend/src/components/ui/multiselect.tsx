import * as React from 'react';
import { X, ChevronDown } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Command, CommandGroup, CommandItem } from '@/components/ui/command';
import { Command as CommandPrimitive } from 'cmdk';
import { cn } from '@/lib/utils';
import { cva } from 'class-variance-authority';
import { Scales } from '@/types/scale';
import { xIconVariants } from '@/components/ui/input/text-input-variants';
import { selectTriggerVariants } from '@/components/ui/select/variants';

// Variants for MultipleSelector - matches selectTriggerVariants exactly
const multipleSelectorVariants = selectTriggerVariants;

// Variants for menu items
const menuItemVariants = cva('cursor-pointer', {
  variants: {
    scale: {
      Small: 'px-2 py-1 text-xs',
      Medium: 'px-3 py-2 text-sm',
      Large: 'px-4 py-3 text-base',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

// Variants for Badge components
const badgeVariants = cva('hover:bg-secondary', {
  variants: {
    scale: {
      Small: 'text-xs',
      Medium: 'text-sm',
      Large: 'text-base',
    },
  },
  defaultVariants: {
    scale: 'Medium',
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
  hidePlaceholderWhenSelected?: boolean;
  emptyIndicator?: React.ReactNode;
  invalid?: boolean;
  scale?: Scales;
  maxVisibleBadges?: number;
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
      hidePlaceholderWhenSelected = false,
      emptyIndicator,
      invalid = false,
      scale = Scales.Medium,
      maxVisibleBadges = 2,
    },
    ref
  ) => {
    const inputRef = React.useRef<HTMLInputElement>(null);
    const containerRef = React.useRef<HTMLSpanElement>(null);
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

    const isSelected = React.useCallback(
      (option: Option) => value.some(item => item.value === option.value),
      [value]
    );

    const toggleOption = React.useCallback(
      (option: Option) => {
        if (isSelected(option)) {
          handleUnselect(option);
        } else {
          onValueChange?.([...value, option]);
        }
      },
      [isSelected, handleUnselect, onValueChange, value]
    );

    return (
      <Command
        ref={ref}
        onKeyDown={handleKeyDown}
        className={cn(
          'overflow-visible bg-transparent h-auto flex-row rounded-none border-0 shadow-none p-0',
          className
        )}
        {...commandProps}
      >
        <div className="relative w-full">
          <div
            className={cn(
              multipleSelectorVariants({ scale }),
              disabled && 'cursor-not-allowed opacity-50',
              (!value || value.length === 0) && 'text-muted-foreground',
              invalid
                ? 'border-destructive text-destructive-foreground focus-within:ring-destructive focus-within:border-destructive'
                : undefined
            )}
          >
            <span
              ref={containerRef}
              className="flex gap-1 items-center flex-1 min-w-0 overflow-hidden"
            >
              {value.slice(0, maxVisibleBadges).map(option => (
                <Badge
                  key={option.value}
                  variant="secondary"
                  className={cn(
                    badgeVariants({ scale }),
                    invalid &&
                      'bg-destructive/10 border-destructive text-destructive'
                  )}
                >
                  {option.label}
                  <button
                    type="button"
                    tabIndex={-1}
                    aria-label="Remove"
                    className="ml-1 p-1 rounded hover:bg-accent focus:outline-none cursor-pointer flex items-center justify-center h-3 self-center leading-none"
                    onKeyDown={e => {
                      if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        handleUnselect(option);
                      }
                    }}
                    onMouseDown={e => {
                      e.preventDefault();
                      e.stopPropagation();
                    }}
                    onClick={() => handleUnselect(option)}
                  >
                    <X className={xIconVariants({ scale })} />
                  </button>
                </Badge>
              ))}
              {value.length > maxVisibleBadges && (
                <Badge
                  variant="outline"
                  className={cn(
                    badgeVariants({ scale }),
                    'bg-muted text-muted-foreground'
                  )}
                >
                  +{value.length - maxVisibleBadges}
                </Badge>
              )}
              <CommandPrimitive.Input
                ref={inputRef}
                value={inputValue}
                onValueChange={setInputValue}
                onBlur={() => {
                  setOpen(false);
                  if (containerRef.current) {
                    containerRef.current.scrollLeft = 0;
                  }
                }}
                onFocus={() => setOpen(true)}
                placeholder={
                  hidePlaceholderWhenSelected && value.length > 0
                    ? undefined
                    : placeholder
                }
                disabled={disabled}
                readOnly={!open}
                className="ml-2 bg-transparent outline-none placeholder:text-muted-foreground flex-1 min-w-[120px] cursor-pointer"
              />
            </span>
            <ChevronDown
              className={cn(
                'h-4 w-4 ml-2 opacity-50 shrink-0 cursor-pointer',
                disabled && 'cursor-not-allowed opacity-50'
              )}
              onClick={e => {
                if (disabled) return;
                e.preventDefault();
                e.stopPropagation();
                if (inputRef.current) {
                  if (open) {
                    inputRef.current.blur();
                  } else {
                    inputRef.current.focus();
                  }
                }
              }}
              onKeyDown={e => {
                if (disabled) return;
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault();
                  e.stopPropagation();
                  if (inputRef.current) {
                    if (open) {
                      inputRef.current.blur();
                    } else {
                      inputRef.current.focus();
                    }
                  }
                }
              }}
              role="button"
              tabIndex={disabled ? -1 : 0}
              aria-label="Toggle dropdown"
            />
          </div>
          {open && defaultOptions.length > 0 && (
            <div className="absolute w-full z-50 top-full mt-1 rounded-md border bg-popover text-popover-foreground shadow-md outline-none animate-in">
              <CommandGroup className="h-full overflow-auto max-h-[300px]">
                {defaultOptions.map(option => {
                  const selected = isSelected(option);
                  return (
                    <CommandItem
                      key={option.value}
                      onMouseDown={e => {
                        e.preventDefault();
                        e.stopPropagation();
                      }}
                      onSelect={() => {
                        setInputValue('');
                        toggleOption(option);
                      }}
                      className={cn(
                        menuItemVariants({ scale }),
                        'flex items-center justify-between'
                      )}
                      disabled={option.disable}
                    >
                      <span>{option.label}</span>
                      {selected && (
                        <X
                          className={cn(
                            xIconVariants({ scale }),
                            'text-muted-foreground hover:text-foreground'
                          )}
                        />
                      )}
                    </CommandItem>
                  );
                })}
              </CommandGroup>
            </div>
          )}
          {open && defaultOptions.length === 0 && emptyIndicator && (
            <div className="absolute w-full z-50 top-full mt-1 rounded-md border bg-popover text-popover-foreground shadow-md outline-none p-2">
              {emptyIndicator}
            </div>
          )}
        </div>
      </Command>
    );
  }
);

MultipleSelector.displayName = 'MultipleSelector';

export { MultipleSelector };
