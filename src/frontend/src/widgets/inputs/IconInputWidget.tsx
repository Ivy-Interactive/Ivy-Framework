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
const POPOVER_HEIGHT = 280;

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
            size={
              scale === Scales.Small
                ? 'sm'
                : scale === Scales.Large
                  ? 'lg'
                  : 'default'
            }
            className={cn(
              'min-w-[120px] justify-start font-normal',
              !hasValue && 'text-muted-foreground',
              invalid && inputStyles.invalidInput
            )}
          >
            {hasValue ? (
              <span className="flex items-center gap-2">
                <Icon name={value} size={18} className="shrink-0" />
                <span className="truncate">{value}</span>
              </span>
            ) : (
              placeholder
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent
          className="w-[320px] p-0"
          align="start"
          onOpenAutoFocus={e => e.preventDefault()}
        >
          <div className="p-2 border-b">
            <div className="relative">
              <Search
                className="absolute left-2.5 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground"
                strokeWidth={2}
              />
              <Input
                placeholder="Search icons..."
                value={search}
                onChange={e => setSearch(e.target.value)}
                className="pl-8 h-9"
              />
            </div>
          </div>
          <div className="overflow-auto" style={{ height: POPOVER_HEIGHT }}>
            {filteredIcons.length === 0 ? (
              <div className="flex items-center justify-center h-24 text-muted-foreground text-sm">
                No icons found
              </div>
            ) : (
              <div
                className="grid gap-1 p-2"
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
                      <Icon name={iconName} size={20} className="shrink-0" />
                    </button>
                  );
                })}
              </div>
            )}
          </div>
          {nullable && (
            <div className="p-2 border-t">
              <Button
                type="button"
                variant="ghost"
                size="sm"
                className="w-full justify-center text-muted-foreground"
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
