import React, { useMemo, useState, useCallback } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { InvalidIcon } from '@/components/InvalidIcon';
import { inputStyles } from '@/lib/styles';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import Icon from '@/components/Icon';
import { icons } from 'lucide-react';
import { X, Search } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Scales } from '@/types/scale';
import { xIconVariants } from '@/components/ui/input/text-input-variants';
import {
  iconInputTriggerVariants,
  iconInputIconVariants,
  iconInputTextVariants,
  iconInputPopoverVariants,
  iconInputPopoverScrollVariants,
  iconInputPopoverHeaderVariants,
  iconInputPopoverFooterVariants,
  iconInputGridVariants,
  iconInputSearchIconVariants,
  iconInputSearchInputVariants,
  iconInputEmptyStateVariants,
} from '@/components/ui/input/icon-input-variants';

// Lucide icon names (PascalCase) - React components are typeof 'object', not 'function'
const LUCIDE_ICON_NAMES = (Object.keys(icons) as string[]).filter(
  name =>
    typeof name === 'string' &&
    name.length > 0 &&
    /^[A-Z]/.test(name) &&
    (icons as Record<string, unknown>)[name] != null
);

interface IconInputWidgetProps {
  id: string;
  value: string | null;
  disabled?: boolean;
  invalid?: string;
  placeholder?: string;
  nullable?: boolean;
  events?: string[];
  scale?: Scales;
}

const ICONS_PER_ROW = 8;

export const IconInputWidget: React.FC<IconInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  placeholder = 'Select an icon',
  nullable = false,
  events = [],
  scale = Scales.Medium,
}) => {
  const eventHandler = useEventHandler();
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');

  const filteredIcons = useMemo(() => {
    if (!search.trim()) return LUCIDE_ICON_NAMES;
    const q = search.toLowerCase().trim();
    return LUCIDE_ICON_NAMES.filter(name => name.toLowerCase().includes(q));
  }, [search]);

  const handleSelect = useCallback(
    (iconName: string) => {
      eventHandler('OnChange', id, [iconName]);
      setOpen(false);
      setSearch('');
    },
    [eventHandler, id]
  );

  const handleClear = useCallback(() => {
    eventHandler('OnChange', id, [null]);
    if (events.includes('OnBlur')) eventHandler('OnBlur', id, [null]);
  }, [eventHandler, id, events]);

  const handleOpenChange = useCallback((newOpen: boolean) => {
    setOpen(newOpen);
    if (!newOpen) setSearch('');
  }, []);

  const hasValue = value != null && value !== '' && value !== 'None';

  return (
    <div className="flex items-center gap-2 min-w-0">
      <Popover open={open} onOpenChange={handleOpenChange}>
        <PopoverTrigger asChild>
          <Button
            type="button"
            variant="outline"
            disabled={disabled}
            className={cn(
              iconInputTriggerVariants({ scale }),
              !hasValue && 'text-muted-foreground',
              invalid && inputStyles.invalidInput
            )}
          >
            {hasValue ? (
              <span className="flex items-center gap-2">
                <Icon
                  name={value}
                  className={cn('shrink-0', iconInputIconVariants({ scale }))}
                />
                <span
                  className={cn('truncate', iconInputTextVariants({ scale }))}
                >
                  {value}
                </span>
              </span>
            ) : (
              <span className={cn(iconInputTextVariants({ scale }))}>
                {placeholder}
              </span>
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent
          className={cn(iconInputPopoverVariants({ scale }))}
          align="start"
          onOpenAutoFocus={e => e.preventDefault()}
        >
          <div className={cn(iconInputPopoverHeaderVariants({ scale }))}>
            <div className="relative">
              <Search
                className={cn(iconInputSearchIconVariants({ scale }))}
                strokeWidth={2}
              />
              <Input
                placeholder="Search icons..."
                value={search}
                onChange={e => setSearch(e.target.value)}
                scale={scale}
                className={iconInputSearchInputVariants({ scale })}
              />
            </div>
          </div>
          <div className={iconInputPopoverScrollVariants({ scale })}>
            {filteredIcons.length === 0 ? (
              <div className={iconInputEmptyStateVariants({ scale })}>
                No icons found
              </div>
            ) : (
              <div
                className={cn(iconInputGridVariants({ scale }))}
                style={{
                  gridTemplateColumns: `repeat(${ICONS_PER_ROW}, minmax(0, 1fr))`,
                }}
              >
                {filteredIcons.map(iconName => {
                  const isSelected = value === iconName;
                  return (
                    <button
                      key={iconName}
                      type="button"
                      onClick={() => handleSelect(iconName)}
                      className={cn(
                        'flex items-center justify-center aspect-square min-w-0 rounded-md',
                        'hover:bg-accent transition-colors',
                        isSelected && 'bg-primary text-primary-foreground'
                      )}
                      title={iconName}
                    >
                      <Icon
                        name={iconName}
                        className={cn(
                          'shrink-0',
                          iconInputIconVariants({ scale })
                        )}
                      />
                    </button>
                  );
                })}
              </div>
            )}
          </div>
          {nullable && (
            <div className={cn(iconInputPopoverFooterVariants({ scale }))}>
              <Button
                type="button"
                variant="ghost"
                size={scale === Scales.Large ? 'default' : 'sm'}
                className={cn(
                  'w-full justify-center text-muted-foreground',
                  iconInputTextVariants({ scale })
                )}
                onClick={() => {
                  eventHandler('OnChange', id, [null]);
                  setOpen(false);
                }}
              >
                No icon
              </Button>
            </div>
          )}
        </PopoverContent>
      </Popover>
      {(invalid || (nullable && hasValue && !disabled)) && (
        <div className="flex items-center gap-1 shrink-0">
          {invalid && (
            <span className="flex items-center">
              <InvalidIcon message={invalid} />
            </span>
          )}
          {nullable && hasValue && !disabled && (
            <button
              type="button"
              tabIndex={-1}
              aria-label="Clear"
              onClick={handleClear}
              className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
            >
              <X className={xIconVariants({ scale })} />
            </button>
          )}
        </div>
      )}
    </div>
  );
};
