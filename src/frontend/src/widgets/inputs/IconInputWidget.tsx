import React, {
  useMemo,
  useState,
  useCallback,
  useRef,
  useEffect,
} from 'react';
import { useEventHandler } from '@/components/event-handler';
import { InvalidIcon } from '@/components/InvalidIcon';
import { inputStyles } from '@/lib/styles';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import {
  Tooltip,
  TooltipProvider,
  TooltipTrigger,
  TooltipContent,
} from '@/components/ui/tooltip';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import Icon from '@/components/Icon';
import { icons } from 'lucide-react';
import { X, Search } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Densities } from '@/types/density';
import { xIconVariant } from '@/components/ui/input/text-input-variant';
import {
  iconInputTriggerVariant,
  iconInputIconVariant,
  iconInputTextVariant,
  iconInputPopoverVariant,
  iconInputPopoverScrollVariant,
  iconInputPopoverHeaderVariant,
  iconInputPopoverFooterVariant,
  iconInputGridVariant,
  iconInputSearchIconVariant,
  iconInputSearchInputVariant,
  iconInputEmptyStateVariant,
} from '@/components/ui/input/icon-input-variant';

const EMPTY_ARRAY: never[] = [];

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
  density?: Densities;
}

const ICONS_PER_ROW = 8;

export const IconInputWidget: React.FC<IconInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  placeholder = 'Select an icon',
  nullable = false,
  events = EMPTY_ARRAY,
  density = Densities.Medium,
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

  const valueTextRef = useRef<HTMLSpanElement>(null);
  const [isEllipsed, setIsEllipsed] = useState(false);

  useEffect(() => {
    if (!hasValue || !value) {
      requestAnimationFrame(() => setIsEllipsed(false));
      return;
    }

    const checkEllipsis = () => {
      if (!valueTextRef.current) return;
      setIsEllipsed(
        valueTextRef.current.scrollWidth > valueTextRef.current.clientWidth
      );
    };

    requestAnimationFrame(checkEllipsis);

    let resizeTimeout: ReturnType<typeof setTimeout>;
    const handleResize = () => {
      clearTimeout(resizeTimeout);
      resizeTimeout = setTimeout(checkEllipsis, 150);
    };
    window.addEventListener('resize', handleResize);

    return () => {
      clearTimeout(resizeTimeout);
      window.removeEventListener('resize', handleResize);
    };
  }, [hasValue, value]);

  const valueTextSpan = hasValue ? (
    <span
      ref={valueTextRef}
      className={cn(
        'min-w-0 overflow-hidden text-ellipsis whitespace-nowrap',
        iconInputTextVariant({ density })
      )}
    >
      {value}
    </span>
  ) : null;

  const wrappedValueText =
    isEllipsed && value ? (
      <TooltipProvider>
        <Tooltip delayDuration={300}>
          <TooltipTrigger asChild>{valueTextSpan}</TooltipTrigger>
          <TooltipContent className="bg-popover text-popover-foreground shadow-md max-w-sm">
            <div className="whitespace-pre-wrap wrap-break-word">{value}</div>
          </TooltipContent>
        </Tooltip>
      </TooltipProvider>
    ) : (
      valueTextSpan
    );

  return (
    <div className="flex items-center gap-2 min-w-0">
      <Popover open={open} onOpenChange={handleOpenChange}>
        <PopoverTrigger asChild>
          <Button
            type="button"
            variant="outline"
            disabled={disabled}
            className={cn(
              iconInputTriggerVariant({ density }),
              !hasValue && 'text-muted-foreground',
              invalid && inputStyles.invalidInput
            )}
          >
            {hasValue ? (
              <span className="flex items-center gap-2 min-w-0">
                <Icon
                  name={value}
                  className={cn('shrink-0', iconInputIconVariant({ density }))}
                />
                {wrappedValueText}
              </span>
            ) : (
              <span className={cn(iconInputTextVariant({ density }))}>
                {placeholder}
              </span>
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent
          className={cn(iconInputPopoverVariant({ density }))}
          align="start"
          onOpenAutoFocus={e => e.preventDefault()}
        >
          <div className={cn(iconInputPopoverHeaderVariant({ density }))}>
            <div className="relative">
              <Search
                className={cn(iconInputSearchIconVariant({ density }))}
                strokeWidth={2}
              />
              <Input
                placeholder="Search icons..."
                value={search}
                onChange={e => setSearch(e.target.value)}
                density={density}
                className={iconInputSearchInputVariant({ density })}
              />
            </div>
          </div>
          <div className={iconInputPopoverScrollVariant({ density })}>
            {filteredIcons.length === 0 ? (
              <div className={iconInputEmptyStateVariant({ density })}>
                No icons found
              </div>
            ) : (
              <div
                className={cn(iconInputGridVariant({ density }))}
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
                          iconInputIconVariant({ density })
                        )}
                      />
                    </button>
                  );
                })}
              </div>
            )}
          </div>
          {nullable && (
            <div className={cn(iconInputPopoverFooterVariant({ density }))}>
              <Button
                type="button"
                variant="ghost"
                size={density === Densities.Large ? 'default' : 'sm'}
                className={cn(
                  'w-full justify-center text-muted-foreground',
                  iconInputTextVariant({ density })
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
              <X className={xIconVariant({ density })} />
            </button>
          )}
        </div>
      )}
    </div>
  );
};
