import React from 'react';
import { useEventHandler, EventHandler } from '@/components/event-handler';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { InvalidIcon } from '@/components/InvalidIcon';
import { cn } from '@/lib/utils';
import { getWidth, inputStyles } from '@/lib/styles';
import { Input } from '@/components/ui/input';
import {
  Tooltip,
  TooltipProvider,
  TooltipTrigger,
  TooltipContent,
} from '@/components/ui/tooltip';
import Icon from '@/components/Icon';
import { X, Search, Loader2 } from 'lucide-react';
import { useCallback, useMemo, useState } from 'react';
import { useOptimisticValue } from './shared/useOptimisticValue';
import { logger } from '@/lib/logger';
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle';
import { Slider } from '@/components/ui/slider';
import { Densities } from '@/types/density';
import { cva } from 'class-variance-authority';
import { xIconVariant } from '@/components/ui/input/text-input-variant';

import {
  NullableSelectValue,
  Option,
  SelectInputWidgetProps,
} from './select-types';
import {
  convertValuesToOriginalType,
  useSelectValueHandler,
} from './select-utils';
import { SelectMultiVariant } from './SelectMultiVariant';
import { SelectSingleVariant } from './SelectSingleVariant';

import { EMPTY_ARRAY } from '@/lib/constants';
// variants for SelectInputWidget container
const selectContainerVariant = cva(
  'relative border border-input bg-transparent rounded-box shadow-sm focus-within:ring-1 focus-within:ring-ring dark:border-white/10',
  {
    variants: {
      density: {
        Small: 'px-2 py-1',
        Medium: 'px-3 py-2',
        Large: 'px-4 py-3',
      },
    },
    defaultVariants: {
      density: 'Medium',
    },
  }
);

const selectTextVariant = {
  Small: 'text-xs',
  Medium: 'text-sm',
  Large: 'text-base',
};

const circleSizeVariant = {
  Small: 'h-3 w-3',
  Medium: 'h-4 w-4',
  Large: 'h-5 w-5',
};

// Helper component for ToggleGroupItem with validation
const ToggleOptionItem: React.FC<{
  option: Option;
  isSelected: boolean;
  invalid?: string;
  density?: Densities;
  disabled?: boolean;
}> = ({
  option,
  isSelected,
  invalid,
  density = Densities.Medium,
  disabled,
}) => {
  const isInvalid = !!invalid && isSelected;

  const sizeClasses = {
    Small: 'px-1 py-1 text-xs',
    Medium: 'px-3 py-2 text-sm',
    Large: 'px-5 py-3 text-base',
  };

  const iconClasses = {
    Small: 'h-3 w-3',
    Medium: 'h-4 w-4',
    Large: 'h-5 w-5',
  };

  const toggleItem = (
    <ToggleGroupItem
      key={option.value}
      value={option.value.toString()}
      aria-label={option.label || option.value.toString()}
      title={option.label}
      className={cn(
        'hover:text-foreground gap-2',
        sizeClasses[density],
        isInvalid
          ? cn(
              inputStyles.invalidInput,
              'bg-destructive/10 border-destructive text-destructive'
            )
          : isSelected
            ? 'data-[state=on]:bg-primary data-[state=on]:border-primary data-[state=on]:text-primary-foreground'
            : undefined
      )}
      disabled={disabled}
    >
      {option.icon && (
        <Icon
          name={option.icon}
          className={cn(iconClasses[density], !option.label && 'mx-auto')}
        />
      )}
      {option.description ? (
        <div className="flex flex-col items-center">
          <span>{option.label}</span>
          <span className="text-xs text-muted-foreground mt-0.5 font-normal">
            {option.description}
          </span>
        </div>
      ) : (
        option.label
      )}
    </ToggleGroupItem>
  );

  if (isInvalid) {
    return (
      <TooltipProvider key={option.value}>
        <Tooltip>
          <TooltipTrigger asChild>{toggleItem}</TooltipTrigger>
          <TooltipContent className="bg-popover text-popover-foreground shadow-md">
            <div className="max-w-xs sm:max-w-sm">{invalid}</div>
          </TooltipContent>
        </Tooltip>
      </TooltipProvider>
    );
  }

  return toggleItem;
};

const ToggleVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = EMPTY_ARRAY,
  eventHandler,
  selectMany = false,
  separator = ',',
  nullable = false,
  maxSelections,
  minSelections,
  searchable = false,
  searchMode = 'CaseInsensitive',
  emptyMessage,
  loading = false,
  ghost = false,
  density = Densities.Medium,
  'data-testid': dataTestId,
  width,
}) => {
  const validOptions = useMemo(
    () =>
      options.filter(
        option => option.value != null && option.value.toString().trim() !== ''
      ),
    [options]
  );

  const selectedValues = useMemo(() => {
    let values: (string | number)[] = [];
    if (selectMany) {
      if (Array.isArray(value)) {
        values = value;
      } else if (value != null && value.toString().trim() !== '') {
        values = value
          .toString()
          .split(separator)
          .map(v => v.trim());
      }
    } else {
      const stringValue =
        value != null && value.toString().trim() !== ''
          ? value.toString()
          : undefined;
      if (stringValue !== undefined) {
        values = [stringValue];
      }
    }
    return values;
  }, [value, selectMany, separator]);

  const hasValue = selectedValues.length > 0;
  const isAtMax =
    maxSelections != null && selectedValues.length >= maxSelections;

  const [searchTerm, setSearchTerm] = useState('');

  const filteredOptions = useMemo(() => {
    if (!searchable || !searchTerm) return validOptions;

    return validOptions.filter(option => {
      if (searchMode === 'Fuzzy') {
        let i = 0;
        let j = 0;
        const searchLower = searchTerm.toLowerCase();
        const labelLower = (option.label || '').toLowerCase();
        while (i < searchLower.length && j < labelLower.length) {
          if (searchLower[i] === labelLower[j]) i++;
          j++;
        }
        return i === searchLower.length;
      }
      const term =
        searchMode === 'CaseInsensitive'
          ? searchTerm.toLowerCase()
          : searchTerm;
      const label =
        searchMode === 'CaseInsensitive'
          ? (option.label || '').toLowerCase()
          : option.label || '';
      return label.includes(term);
    });
  }, [validOptions, searchable, searchTerm, searchMode]);

  const handleValueChange = useSelectValueHandler(
    id,
    value,
    validOptions,
    eventHandler,
    selectMany
  );
  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  const container = (
    <div
      className={cn(
        selectContainerVariant({ density }),
        invalid && 'border-destructive focus-within:ring-destructive',
        ghost &&
          'border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent'
      )}
      style={styles}
    >
      <div className="flex items-center gap-2">
        <div className="flex-1">
          {searchable && (
            <div className="relative mb-3">
              <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                type="text"
                placeholder="Search..."
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
                className="pl-9 h-9"
                disabled={disabled || loading}
              />
            </div>
          )}
          {loading ? (
            <div className="flex justify-center p-2">
              <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
            </div>
          ) : filteredOptions.length === 0 ? (
            <div className="p-2 text-center text-sm text-muted-foreground">
              {emptyMessage || 'No options available'}
            </div>
          ) : selectMany ? (
            <ToggleGroup
              type="multiple"
              value={selectedValues.map(v => v.toString())}
              onValueChange={handleValueChange}
              disabled={disabled}
              className="flex flex-wrap gap-2"
              data-testid={dataTestId}
            >
              {filteredOptions.map(option => {
                const isSelected = selectedValues.includes(option.value);
                const isDisabled =
                  disabled ||
                  loading ||
                  option.disabled ||
                  (!isSelected && isAtMax) ||
                  (isSelected &&
                    minSelections != null &&
                    selectedValues.length <= minSelections);

                return (
                  <ToggleOptionItem
                    key={option.value}
                    option={option}
                    isSelected={isSelected}
                    invalid={invalid}
                    density={density}
                    disabled={isDisabled}
                  />
                );
              })}
            </ToggleGroup>
          ) : (
            <ToggleGroup
              type="single"
              value={selectedValues[0]?.toString() ?? ''}
              onValueChange={handleValueChange}
              disabled={disabled}
              className="flex flex-wrap gap-2"
            >
              {filteredOptions.map(option => {
                const isSelected =
                  selectedValues[0] === option.value.toString();
                const isDisabled =
                  disabled ||
                  loading ||
                  option.disabled ||
                  (!isSelected && isAtMax) ||
                  (isSelected &&
                    minSelections != null &&
                    selectedValues.length <= minSelections);

                return (
                  <ToggleOptionItem
                    key={option.value}
                    option={option}
                    isSelected={isSelected}
                    invalid={invalid}
                    density={density}
                    disabled={isDisabled}
                  />
                );
              })}
            </ToggleGroup>
          )}
        </div>
        {((nullable && hasValue && !disabled) || invalid) && (
          <div className="flex items-center gap-1">
            {nullable && hasValue && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label={selectMany ? 'Clear All' : 'Clear'}
                onClick={() => {
                  // For nullable inputs, send null; for non-nullable, send empty array for multi-select or null for single
                  const clearedValue = nullable ? null : selectMany ? [] : null;
                  logger.debug(
                    'Select input clear button clicked (ToggleVariant)',
                    {
                      id,
                      selectMany,
                      nullable,
                      clearValue: clearedValue,
                    }
                  );
                  eventHandler('OnChange', id, [clearedValue]);
                }}
                className="flex-shrink-0 p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
              >
                <X className={xIconVariant({ density })} />
              </button>
            )}
            {/* Invalid icon - rightmost */}
            {invalid && (
              <InvalidIcon message={invalid} className="pointer-events-auto" />
            )}
          </div>
        )}
      </div>
    </div>
  );

  return container;
};

const RadioVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = EMPTY_ARRAY,
  eventHandler,
  nullable = false,
  ghost = false,
  density = Densities.Medium,
  'data-testid': dataTestId,
  width,
}) => {
  const validOptions = options.filter(
    option => option.value != null && option.value.toString().trim() !== ''
  );
  const stringValue =
    value != null && value.toString().trim() !== '' ? value.toString() : '';

  const hasValue = stringValue !== '';

  const handleValueChange = useSelectValueHandler(
    id,
    value,
    validOptions,
    eventHandler,
    false // Always single select for RadioVariant
  );
  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  const container = (
    <div
      className={cn(
        selectContainerVariant({ density }),
        invalid && 'border-destructive focus-within:ring-destructive',
        ghost &&
          'border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent'
      )}
      style={styles}
    >
      <div className="flex items-center gap-4">
        <div className="flex-1">
          <RadioGroup
            value={stringValue}
            onValueChange={handleValueChange}
            disabled={disabled}
            className="flex flex-col gap-4"
            data-testid={dataTestId}
          >
            {validOptions.map(option => {
              const isOptionDisabled = disabled || option.disabled;
              return (
                <div key={option.value} className="flex items-center space-x-2">
                  <RadioGroupItem
                    value={option.value.toString()}
                    id={`${id}-${option.value}`}
                    disabled={isOptionDisabled}
                    className={cn(
                      'border-input text-input',
                      circleSizeVariant[density],
                      stringValue === option.value.toString() && !invalid
                        ? 'border-primary text-primary'
                        : undefined,
                      stringValue === option.value.toString() && invalid
                        ? inputStyles.invalidInput
                        : undefined,
                      isOptionDisabled && 'opacity-50 cursor-not-allowed'
                    )}
                  />
                  <Label
                    htmlFor={`${id}-${option.value}`}
                    className={cn(
                      'cursor-pointer leading-none flex items-center gap-2',
                      selectTextVariant[density],
                      stringValue === option.value.toString() && invalid
                        ? inputStyles.invalidInput
                        : undefined,
                      isOptionDisabled && 'opacity-50 cursor-not-allowed'
                    )}
                  >
                    {option.icon && (
                      <Icon
                        name={option.icon}
                        className="h-4 w-4 flex-shrink-0"
                      />
                    )}
                    {option.description ? (
                      <div className="flex flex-col">
                        <span>{option.label}</span>
                        <span className="text-xs text-muted-foreground mt-0.5 font-normal">
                          {option.description}
                        </span>
                      </div>
                    ) : (
                      option.label
                    )}
                  </Label>
                </div>
              );
            })}
          </RadioGroup>
        </div>
        {((nullable && hasValue && !disabled) || invalid) && (
          <div className="flex items-center gap-1">
            {nullable && hasValue && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear"
                onClick={() => {
                  logger.debug('Select input clear button clicked', { id });
                  eventHandler('OnChange', id, [null]);
                }}
                className="flex-shrink-0 p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
              >
                <X className={xIconVariant({ density })} />
              </button>
            )}
            {/* Invalid icon - rightmost */}
            {invalid && (
              <InvalidIcon message={invalid} className="pointer-events-auto" />
            )}
          </div>
        )}
      </div>
    </div>
  );

  return container;
};

const CheckboxVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = EMPTY_ARRAY,
  eventHandler,
  separator = ',',
  nullable = false,
  maxSelections,
  minSelections,
  searchable = false,
  searchMode = 'CaseInsensitive',
  emptyMessage,
  loading = false,
  ghost = false,
  density = Densities.Medium,
  'data-testid': dataTestId,
  width,
}) => {
  const validOptions = useMemo(
    () =>
      options.filter(
        option => option.value != null && option.value.toString().trim() !== ''
      ),
    [options]
  );

  const selectedValues = useMemo(() => {
    let values: (string | number)[] = [];
    if (Array.isArray(value)) {
      values = value;
    } else if (value != null && value.toString().trim() !== '') {
      values = value
        .toString()
        .split(separator)
        .map(v => v.trim());
    }
    return values;
  }, [value, separator]);

  const handleCheckboxChange = useCallback(
    (optionValue: string | number, checked: boolean) => {
      logger.debug('Select input checkbox change', {
        id,
        optionValue,
        checked,
        currentValue: value,
      });

      // Calculate new values based on current value, not selectedValues state
      let currentValues: (string | number)[] = [];
      if (Array.isArray(value)) {
        currentValues = value;
      } else if (value != null && value.toString().trim() !== '') {
        currentValues = value
          .toString()
          .split(separator)
          .map(v => v.trim());
      }

      let newValues: (string | number)[];
      if (checked) {
        newValues = [...currentValues, optionValue];
      } else {
        newValues = currentValues.filter(v => v !== optionValue);
      }

      const convertedValue = convertValuesToOriginalType(
        newValues.map(v => v.toString()),
        value,
        validOptions,
        true // Always multi-select for CheckboxVariant
      );

      logger.debug('Select input checkbox converted value', {
        id,
        newValues,
        convertedValue,
      });

      eventHandler('OnChange', id, [convertedValue]);
    },
    [value, validOptions, eventHandler, id, separator]
  );

  const hasValues = selectedValues.length > 0;
  const isAtMax =
    maxSelections != null && selectedValues.length >= maxSelections;

  const [searchTerm, setSearchTerm] = useState('');

  const filteredOptions = useMemo(() => {
    if (!searchable || !searchTerm) return validOptions;

    return validOptions.filter(option => {
      if (searchMode === 'Fuzzy') {
        let i = 0;
        let j = 0;
        const searchLower = searchTerm.toLowerCase();
        const labelLower = (option.label || '').toLowerCase();
        while (i < searchLower.length && j < labelLower.length) {
          if (searchLower[i] === labelLower[j]) i++;
          j++;
        }
        return i === searchLower.length;
      }
      const term =
        searchMode === 'CaseInsensitive'
          ? searchTerm.toLowerCase()
          : searchTerm;
      const label =
        searchMode === 'CaseInsensitive'
          ? (option.label || '').toLowerCase()
          : option.label || '';
      return label.includes(term);
    });
  }, [validOptions, searchable, searchTerm, searchMode]);

  const styles: React.CSSProperties = {
    ...getWidth(width),
  };
  const container = (
    <div
      className={cn(
        'relative w-full border border-input bg-transparent rounded-box shadow-sm px-3 py-2 focus-within:ring-1 focus-within:ring-ring dark:border-white/10',
        invalid && 'border-destructive focus-within:ring-destructive',
        ghost &&
          'border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent'
      )}
      style={styles}
    >
      <div className="flex items-start gap-2">
        <div className="flex-1 min-w-0">
          {searchable && (
            <div className="relative mb-3">
              <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                type="text"
                placeholder="Search..."
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
                className="pl-9 h-9"
                disabled={disabled || loading}
              />
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
            <div
              className={cn(
                'flex flex-col gap-4',
                filteredOptions.length > 6
                  ? 'max-h-48 overflow-y-auto pr-2 -mr-2'
                  : ''
              )}
              data-testid={dataTestId}
            >
              {filteredOptions.map(option => {
                const isSelected = selectedValues.includes(option.value);
                const isInvalid = !!invalid && isSelected;
                const isDisabled =
                  disabled ||
                  loading ||
                  option.disabled ||
                  (!isSelected && isAtMax) ||
                  (isSelected &&
                    minSelections != null &&
                    selectedValues.length <= minSelections);

                return (
                  <div
                    key={option.value}
                    className="flex items-center space-x-2"
                  >
                    {isInvalid ? (
                      <TooltipProvider>
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <Checkbox
                              id={`${id}-${option.value}`}
                              checked={isSelected}
                              onCheckedChange={checked =>
                                handleCheckboxChange(
                                  option.value,
                                  checked === true
                                )
                              }
                              disabled={isDisabled}
                              className={cn(
                                inputStyles.invalidInput,
                                'bg-destructive/10 border-destructive text-destructive',
                                selectTextVariant[density]
                              )}
                            />
                          </TooltipTrigger>
                          <TooltipContent className="bg-popover text-popover-foreground shadow-md">
                            <div className="max-w-xs sm:max-w-sm">
                              {invalid}
                            </div>
                          </TooltipContent>
                        </Tooltip>
                      </TooltipProvider>
                    ) : (
                      <Checkbox
                        id={`${id}-${option.value}`}
                        checked={isSelected}
                        onCheckedChange={checked =>
                          handleCheckboxChange(option.value, checked === true)
                        }
                        disabled={isDisabled}
                        className={cn(
                          'data-[state=unchecked]:bg-transparent data-[state=unchecked]:border-border',
                          selectTextVariant[density],
                          isSelected
                            ? 'data-[state=checked]:bg-primary data-[state=checked]:border-primary data-[state=checked]:text-primary-foreground'
                            : undefined
                        )}
                      />
                    )}
                    <Label
                      htmlFor={`${id}-${option.value}`}
                      className={cn(
                        'flex-1 cursor-pointer flex items-center gap-2',
                        selectTextVariant[density],
                        isInvalid ? inputStyles.invalidInput : undefined,
                        isDisabled && !isSelected ? 'opacity-50' : undefined
                      )}
                    >
                      {option.icon && (
                        <Icon
                          name={option.icon}
                          className="h-4 w-4 flex-shrink-0"
                        />
                      )}
                      {option.description ? (
                        <div className="flex flex-col">
                          <span>{option.label}</span>
                          <span className="text-xs text-muted-foreground mt-0.5 font-normal">
                            {option.description}
                          </span>
                        </div>
                      ) : (
                        option.label
                      )}
                    </Label>
                  </div>
                );
              })}
            </div>
          )}
        </div>
        {((nullable && hasValues && !disabled) || invalid) && (
          <div className="flex flex-col items-center gap-1">
            {nullable && hasValues && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear All"
                onClick={() => {
                  // For nullable inputs, send null; for non-nullable, send empty array
                  const clearedValue = nullable ? null : [];
                  logger.debug(
                    'Select input clear button clicked (CheckboxVariant)',
                    { id, nullable, clearValue: clearedValue }
                  );
                  eventHandler('OnChange', id, [clearedValue]);
                }}
                className="flex-shrink-0 p-1 rounded hover:bg-accent focus:outline-none"
              >
                <X className={xIconVariant({ density })} />
              </button>
            )}
            {/* Invalid icon - rightmost */}
            {invalid && (
              <InvalidIcon message={invalid} className="pointer-events-auto" />
            )}
          </div>
        )}
      </div>
    </div>
  );
  return container;
};

const SelectVariant: React.FC<
  SelectInputWidgetProps & { eventHandler: EventHandler }
> = props => {
  return props.selectMany ? (
    <SelectMultiVariant {...props} />
  ) : (
    <SelectSingleVariant key={props.value?.toString() ?? 'null'} {...props} />
  );
};

const sliderLabelVariant: Record<string, string> = {
  Small: 'text-xs',
  Medium: 'text-sm',
  Large: 'text-base',
};

const SliderVariant: React.FC<
  SelectInputWidgetProps & { eventHandler: EventHandler }
> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = EMPTY_ARRAY,
  eventHandler,
  selectMany = false,
  ghost = false,
  density = Densities.Medium,
  'data-testid': dataTestId,
  width,
}) => {
  if (selectMany) {
    logger.warn(
      'SelectInput Slider variant does not support selectMany. Falling back to single-select.'
    );
  }

  const validOptions = useMemo(
    () => options.filter(o => !o.disabled),
    [options]
  );

  const currentIndex = useMemo(() => {
    if (value == null) return -1;
    const strValue = String(value);
    return validOptions.findIndex(o => String(o.value) === strValue);
  }, [value, validOptions]);

  const [localIndex, setLocalIndex] = useState(currentIndex);

  const handleSliderChange = useCallback((values: number[]) => {
    const newIndex = values[0];
    if (typeof newIndex === 'number') {
      setLocalIndex(newIndex);
    }
  }, []);

  const handleSliderCommit = useCallback(
    (values: number[]) => {
      const newIndex = values[0];
      if (typeof newIndex === 'number' && validOptions[newIndex]) {
        eventHandler('OnChange', id, [validOptions[newIndex].value]);
      }
    },
    [eventHandler, id, validOptions]
  );

  if (validOptions.length === 0) {
    return (
      <div
        className={cn(
          'flex items-center justify-center text-muted-foreground',
          sliderLabelVariant[String(density)]
        )}
        style={width ? getWidth(width) : undefined}
        data-testid={dataTestId}
      >
        No options available
      </div>
    );
  }

  const sliderValue = localIndex >= 0 ? localIndex : 0;
  const currentLabel = validOptions[sliderValue]?.label ?? '';
  const firstLabel = validOptions[0]?.label ?? '';
  const lastLabel = validOptions[validOptions.length - 1]?.label ?? '';
  const textSize = sliderLabelVariant[String(density)];

  return (
    <div
      className={cn(
        'relative w-full flex-1 flex flex-col gap-1 pt-6 pb-2 my-auto justify-center',
        ghost && 'border-transparent shadow-none'
      )}
      style={width ? getWidth(width) : undefined}
      data-testid={dataTestId}
    >
      <div className="relative">
        <Slider
          min={0}
          max={validOptions.length - 1}
          step={1}
          value={[sliderValue]}
          disabled={disabled}
          density={density}
          tooltipValue={currentLabel}
          onValueChange={handleSliderChange}
          onValueCommit={handleSliderCommit}
          className={cn(invalid && inputStyles.invalidInput)}
        />
        {validOptions.length > 1 && (
          <div
            className="absolute w-full flex justify-between px-[2px]"
            style={{
              top: '50%',
              transform: 'translateY(-50%)',
              pointerEvents: 'none',
            }}
          >
            {validOptions.map((option, i) => (
              <div
                key={option.value}
                className={cn(
                  'rounded-full',
                  i === sliderValue
                    ? 'bg-transparent'
                    : 'bg-muted-foreground/40',
                  density === Densities.Small
                    ? 'w-1 h-1'
                    : density === Densities.Large
                      ? 'w-1.5 h-1.5'
                      : 'w-1 h-1'
                )}
              />
            ))}
          </div>
        )}
      </div>
      <div
        className={cn(
          'flex w-full items-center justify-between gap-1',
          textSize
        )}
        aria-hidden="true"
      >
        <span className="text-muted-foreground">{firstLabel}</span>
        <span className="text-muted-foreground">{lastLabel}</span>
      </div>
      {invalid && (
        <div className="absolute right-2.5 translate-y-1/2 -top-1.5">
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
};

const selectValueEqual = (
  a: NullableSelectValue,
  b: NullableSelectValue
): boolean => {
  if (a === b) return true;
  if (a == null || b == null) return a == b;
  if (Array.isArray(a) && Array.isArray(b)) {
    if (a.length !== b.length) return false;
    return a.every((v, i) => v === b[i]);
  }
  return String(a) === String(b);
};

export const SelectInputWidget: React.FC<SelectInputWidgetProps> = props => {
  const eventHandler = useEventHandler();

  // Normalize undefined to null when nullable
  const serverValue =
    props.nullable && props.value === undefined ? null : props.value;

  const [localValue, setLocalValue] = useOptimisticValue(
    serverValue,
    false,
    selectValueEqual
  );

  // Wrap eventHandler to intercept OnChange and apply optimistic update
  const optimisticEventHandler: EventHandler = useCallback(
    (event: string, id: string, args: unknown[]) => {
      if (event === 'OnChange') {
        const newValue = args[0] as NullableSelectValue;
        setLocalValue(newValue as NullableSelectValue & undefined);
      }
      eventHandler(event, id, args);
    },
    [eventHandler, setLocalValue]
  );

  const normalizedProps = {
    ...props,
    value: localValue,
    density: props.density ?? Densities.Medium,
    variant: props.variant ?? 'Select',
    separator: props.separator ?? ';',
    selectMany: props.selectMany ?? false,
    maxSelections: props.maxSelections,
    minSelections: props.minSelections,
    searchable: props.searchable ?? false,
    searchMode: props.searchMode ?? 'CaseInsensitive',
    emptyMessage: props.emptyMessage,
    loading: props.loading ?? false,
    ghost: props.ghost ?? false,
  };

  switch (normalizedProps.variant) {
    case 'List':
      return normalizedProps.selectMany ? (
        <CheckboxVariant
          {...normalizedProps}
          eventHandler={optimisticEventHandler}
        />
      ) : (
        <RadioVariant
          {...normalizedProps}
          eventHandler={optimisticEventHandler}
        />
      );
    case 'Radio':
      return (
        <RadioVariant
          {...normalizedProps}
          eventHandler={optimisticEventHandler}
        />
      );
    case 'Toggle':
      return (
        <ToggleVariant
          {...normalizedProps}
          eventHandler={optimisticEventHandler}
        />
      );
    case 'Slider':
      return (
        <SliderVariant
          key={normalizedProps.value?.toString() ?? 'null'}
          {...normalizedProps}
          eventHandler={optimisticEventHandler}
        />
      );
    default:
      return (
        <SelectVariant
          {...normalizedProps}
          eventHandler={optimisticEventHandler}
        />
      );
  }
};

export default SelectInputWidget;
