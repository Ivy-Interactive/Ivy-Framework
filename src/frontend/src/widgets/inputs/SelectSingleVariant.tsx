import React, { useMemo, useRef, useState, useEffect } from 'react';
import { cn } from '@/lib/utils';
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Tooltip,
  TooltipProvider,
  TooltipTrigger,
  TooltipContent,
} from '@/components/ui/tooltip';
import { Input } from '@/components/ui/input';
import { Search, Loader2, X } from 'lucide-react';
import Icon from '@/components/Icon';
import { InvalidIcon } from '@/components/InvalidIcon';
import { selectIconContainerVariant } from '@/components/ui/select/variant';
import { xIconVariant } from '@/components/ui/input/text-input-variant';
import { getWidth, inputStyles } from '@/lib/styles';
import { SelectInputWidgetProps } from './select-types';
import { useSelectValueHandler } from './select-utils';

export const SelectSingleVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  placeholder = '',
  value,
  disabled = false,
  invalid,
  options = [],
  eventHandler,
  nullable = false,
  searchable = false,
  searchMode = 'CaseInsensitive',
  emptyMessage,
  loading = false,
  ghost = false,
  density,
  'data-testid': dataTestId,
  width,
}) => {
  const validOptions = options.filter(
    option => option.value != null && option.value.toString().trim() !== ''
  );

  const handleValueChange = useSelectValueHandler(
    id,
    value,
    validOptions,
    eventHandler,
    false
  );

  const stringValue =
    value != null && value.toString().trim() !== ''
      ? value.toString()
      : undefined;

  const selectedOption = useMemo(() => {
    if (!stringValue) return undefined;
    return validOptions.find(opt => opt.value.toString() === stringValue);
  }, [stringValue, validOptions]);

  const selectedLabel = selectedOption?.label;
  const triggerRef = useRef<HTMLButtonElement>(null);
  const [isEllipsed, setIsEllipsed] = useState(false);

  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    if (!selectedLabel) return;

    const checkEllipsis = () => {
      const firstSpan = triggerRef.current?.querySelector(
        'span:first-child'
      ) as HTMLSpanElement;
      if (firstSpan) {
        setIsEllipsed(firstSpan.scrollWidth > firstSpan.clientWidth);
      }
    };

    requestAnimationFrame(checkEllipsis);
    const handleResize = () => setTimeout(checkEllipsis, 150);
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [selectedLabel]);

  const filteredOptions = useMemo(() => {
    if (!searchable || !searchTerm) return validOptions;
    return validOptions.filter(option => {
      const term =
        searchMode === 'CaseInsensitive'
          ? searchTerm.toLowerCase()
          : searchTerm;
      const label = (option.label || '').toLowerCase();
      if (searchMode === 'Fuzzy') {
        let i = 0,
          j = 0;
        while (i < term.length && j < label.length) {
          if (term[i] === label[j]) i++;
          j++;
        }
        return i === term.length;
      }
      return label.includes(term);
    });
  }, [validOptions, searchable, searchTerm, searchMode]);

  const groupedOptions = filteredOptions.reduce<
    Record<string, typeof validOptions>
  >((acc, option) => {
    const key = option.group || 'default';
    if (!acc[key]) acc[key] = [];
    acc[key].push(option);
    return acc;
  }, {});

  const hasValue = stringValue !== undefined;
  const styles = getWidth(width);

  const selectTriggerElement = (
    <SelectTrigger
      ref={triggerRef}
      className={cn(
        'relative',
        invalid && inputStyles.invalidInput,
        !hasValue && 'text-muted-foreground',
        ghost &&
          'border-transparent shadow-none bg-transparent hover:bg-accent hover:text-accent-foreground'
      )}
      density={density}
    >
      <SelectValue placeholder={placeholder} />
    </SelectTrigger>
  );

  return (
    <div className="flex items-center gap-2 w-full" style={styles}>
      <div className="flex-1 relative w-full">
        <Select
          key={`${id}-${stringValue ?? 'null'}`}
          disabled={disabled}
          value={stringValue}
          onValueChange={handleValueChange}
          open={isOpen}
          onOpenChange={setIsOpen}
          data-testid={dataTestId}
        >
          {isEllipsed && selectedLabel ? (
            <TooltipProvider>
              <Tooltip delayDuration={300} open={isOpen ? false : undefined}>
                <TooltipTrigger asChild>{selectTriggerElement}</TooltipTrigger>
                <TooltipContent className="bg-popover text-popover-foreground shadow-md max-w-sm">
                  <div className="whitespace-pre-wrap break-words">
                    {selectedLabel}
                  </div>
                </TooltipContent>
              </Tooltip>
            </TooltipProvider>
          ) : (
            selectTriggerElement
          )}
          <SelectContent density={density}>
            {searchable && (
              <div className="p-2 border-b">
                <div className="relative">
                  <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                  <Input
                    type="text"
                    placeholder="Search..."
                    value={searchTerm}
                    onChange={e => setSearchTerm(e.target.value)}
                    onKeyDown={e => e.stopPropagation()}
                    onClick={e => e.stopPropagation()}
                    className="pl-9 h-9"
                    disabled={disabled || loading}
                  />
                </div>
              </div>
            )}
            {loading ? (
              <div className="flex justify-center p-4">
                <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
              </div>
            ) : filteredOptions.length === 0 ? (
              <div className="p-4 text-center text-sm text-muted-foreground">
                {emptyMessage || 'No options available'}
              </div>
            ) : (
              Object.entries(groupedOptions).map(([group, options]) => (
                <SelectGroup key={group}>
                  {group !== 'default' && <SelectLabel>{group}</SelectLabel>}
                  {options.map(option => (
                    <SelectItem
                      key={option.value}
                      value={option.value.toString()}
                      textValue={option.label}
                      density={density}
                      disabled={disabled || loading || option.disabled}
                    >
                      <div className="flex items-center gap-2">
                        {option.icon && (
                          <Icon
                            name={option.icon}
                            className="h-4 w-4 flex-shrink-0"
                          />
                        )}
                        {option.label}
                      </div>
                    </SelectItem>
                  ))}
                </SelectGroup>
              ))
            )}
          </SelectContent>
        </Select>
        {(nullable && hasValue && !disabled) || invalid || loading ? (
          <div
            className={selectIconContainerVariant({ density })}
            style={{ zIndex: 2 }}
          >
            {loading && (
              <div className="pointer-events-auto flex items-center h-6 p-1">
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground text-opacity-50" />
              </div>
            )}
            {hasValue && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear"
                onClick={e => {
                  e.preventDefault();
                  e.stopPropagation();
                  eventHandler('OnChange', id, [null]);
                }}
                onKeyDown={e => {
                  if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    e.stopPropagation();
                    eventHandler('OnChange', id, [null]);
                  }
                }}
                className="pointer-events-auto p-1 rounded hover:bg-accent focus:outline-none cursor-pointer flex items-center h-6"
              >
                <X className={xIconVariant({ density })} />
              </button>
            )}
            {invalid && (
              <div className="pointer-events-auto flex items-center h-6 p-1">
                <InvalidIcon message={invalid} />
              </div>
            )}
          </div>
        ) : null}
      </div>
    </div>
  );
};
