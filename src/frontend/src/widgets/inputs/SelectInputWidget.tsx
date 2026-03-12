import React from 'react';
import { useEventHandler, EventHandler } from '@/components/event-handler';
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { selectIconContainerVariant } from '@/components/ui/select/variant';
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
import { useCallback, useMemo, useRef, useEffect, useState } from 'react';
import { logger } from '@/lib/logger';
import {
  MultipleSelector,
  Option as MultiSelectOption,
} from '@/components/ui/multiselect';
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle';
import { Densities } from '@/types/density';
import { cva } from 'class-variance-authority';
import { xIconVariant } from '@/components/ui/input/text-input-variant';

const EMPTY_ARRAY: never[] = [];
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

export type NullableSelectValue =
  | string
  | number
  | string[]
  | number[]
  | null
  | undefined;

interface Option {
  value: string | number;
  label?: string;
  group?: string;
  icon?: string;
  disabled?: boolean;
}

interface SelectInputWidgetProps {
  id: string;
  placeholder?: string;
  value?: NullableSelectValue;
  variant?: 'Select' | 'List' | 'Toggle';
  nullable?: boolean;
  disabled?: boolean;
  invalid?: string;
  options: Option[];
  eventHandler: EventHandler;
  selectMany: boolean;
  separator: string;
  maxSelections?: number;
  minSelections?: number;
  searchable?: boolean;
  searchMode?: 'CaseInsensitive' | 'CaseSensitive' | 'Fuzzy';
  emptyMessage?: string;
  loading?: boolean;
  ghost?: boolean;
  'data-testid'?: string;
  density?: Densities;
  width?: string;
}

// Helper function to convert string values back to their original types
const convertValuesToOriginalType = (
  stringValues: string[],
  originalValue: NullableSelectValue,
  options: Option[],
  selectMany: boolean = false
): NullableSelectValue => {
  if (stringValues.length === 0) {
    // For nullable types, we need to determine the expected array type from options
    if (originalValue === null || originalValue === undefined) {
      if (selectMany) {
        // For nullable collection types, determine the expected array type from options
        if (options.length > 0) {
          const firstOption = options[0];
          if (typeof firstOption.value === 'number') {
            return [];
          } else if (typeof firstOption.value === 'string') {
            return [];
          }
        }
        return [];
      }
      return null;
    }
    return originalValue instanceof Array ? [] : null;
  }

  const optionsMap = new Map<string, Option>();
  for (const option of options) {
    optionsMap.set(option.value.toString(), option);
  }

  // If original value is an array, preserve the array type
  if (originalValue instanceof Array) {
    // Check if original array contains numbers
    if (originalValue.length > 0 && typeof originalValue[0] === 'number') {
      return stringValues.map(v => {
        const option = optionsMap.get(v);
        return option ? Number(option.value) : Number(v);
      });
    }
    // Check if original array contains strings
    else if (originalValue.length > 0 && typeof originalValue[0] === 'string') {
      return stringValues.map(v => {
        const option = optionsMap.get(v);
        return option ? String(option.value) : v;
      });
    }
    // Default to string array
    return stringValues;
  }

  // For nullable collection types where originalValue is null, determine type from options
  if ((originalValue === null || originalValue === undefined) && selectMany) {
    if (options.length > 0) {
      const firstOption = options[0];
      if (typeof firstOption.value === 'number') {
        return stringValues.map(v => {
          const option = optionsMap.get(v);
          return option ? Number(option.value) : Number(v);
        });
      } else if (typeof firstOption.value === 'string') {
        return stringValues.map(v => {
          const option = optionsMap.get(v);
          return option ? String(option.value) : v;
        });
      }
    }
    // Default to string array if we can't determine the type
    return stringValues;
  }

  // For single values, return the first value with proper type
  const firstValue = stringValues[0];
  const option = optionsMap.get(firstValue);
  if (option) {
    return option.value;
  }
  return firstValue;
};

const useSelectValueHandler = (
  id: string,
  value: NullableSelectValue,
  options: Option[],
  eventHandler: EventHandler,
  selectMany: boolean = false
) => {
  return useCallback(
    (newValue: string | string[]) => {
      logger.debug('Select input value change', {
        id,
        currentValue: value,
        newValue,
        optionsCount: options.length,
      });

      const stringArray = Array.isArray(newValue) ? newValue : [newValue];
      const convertedValue = convertValuesToOriginalType(
        stringArray,
        value,
        options,
        selectMany
      );

      logger.debug('Select input converted value', {
        id,
        originalValue: value,
        stringArray,
        convertedValue,
      });

      eventHandler('OnChange', id, [convertedValue]);
    },
    [id, value, options, eventHandler, selectMany]
  );
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
      {option.label}
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
                    {option.label}
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
                      {option.label}
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

const SelectVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  placeholder = '',
  value,
  disabled = false,
  invalid,
  options = EMPTY_ARRAY,
  eventHandler,
  nullable = false,
  selectMany = false,
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
  const validOptions = options.filter(
    option => option.value != null && option.value.toString().trim() !== ''
  );

  const handleValueChange = useSelectValueHandler(
    id,
    value,
    validOptions,
    eventHandler,
    selectMany
  );

  // Convert current value to array format for multiselect
  const selectedValues = useMemo(() => {
    let values: (string | number)[] = [];
    if (selectMany) {
      if (Array.isArray(value)) {
        values = value;
      } else if (value != null && value.toString().trim() !== '') {
        values = value
          .toString()
          .split(',')
          .map(v => v.trim());
      }
    }
    return values;
  }, [selectMany, value]);

  // Convert options to MultiSelectOption format
  const multiSelectOptions: MultiSelectOption[] = useMemo(() => {
    const isAtMax =
      maxSelections != null && selectedValues.length >= maxSelections;
    return validOptions.map(option => ({
      label: option.label || option.value.toString(),
      value: option.value.toString(),
      disable:
        disabled ||
        loading ||
        option.disabled ||
        (isAtMax && !selectedValues.includes(option.value.toString())),
    }));
  }, [validOptions, selectedValues, maxSelections, disabled, loading]);

  // Create lookup map for efficient option finding
  const optionsLookup = useMemo(() => {
    const map = new Map<string, Option>();
    validOptions.forEach(option => {
      map.set(option.value.toString(), option);
    });
    return map;
  }, [validOptions]);

  // Convert selected values to MultiSelectOption format
  const selectedMultiSelectOptions: MultiSelectOption[] = useMemo(
    () =>
      selectedValues.map(val => {
        const option = optionsLookup.get(val.toString());
        return {
          label: option?.label || val.toString(),
          value: val.toString(),
          disable: false,
        };
      }),
    [selectedValues, optionsLookup]
  );

  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  // Get string value for both single and multi-select
  const stringValue =
    value != null && value.toString().trim() !== ''
      ? value.toString()
      : undefined;

  // Get the selected option's label for tooltip (only for single select)
  const selectedOption = useMemo(() => {
    if (selectMany || !stringValue) return undefined;
    return validOptions.find(opt => opt.value.toString() === stringValue);
  }, [stringValue, validOptions, selectMany]);

  const selectedLabel = selectedOption?.label;

  // Create ref for SelectTrigger (needs to be before early returns)
  const triggerRef = useRef<HTMLButtonElement>(null);

  // Detect ellipsis on the SelectValue span (needs to be before early returns)
  const [isEllipsed, setIsEllipsed] = useState(false);
  // Track if select dropdown is open to disable tooltip (needs to be before early returns)
  const [isOpen, setIsOpen] = useState(false);

  useEffect(() => {
    // Skip ellipsis check for multiselect or when no label
    if (selectMany || !selectedLabel) {
      requestAnimationFrame(() => setIsEllipsed(false));
      return;
    }

    const checkEllipsis = () => {
      if (!triggerRef?.current) {
        return;
      }
      // SelectValue renders as the first span child of SelectTrigger
      const firstSpan = triggerRef.current.querySelector(
        'span:first-child'
      ) as HTMLSpanElement;
      if (firstSpan) {
        setIsEllipsed(firstSpan.scrollWidth > firstSpan.clientWidth);
      } else {
        setIsEllipsed(false);
      }
    };

    // Check after render
    requestAnimationFrame(checkEllipsis);

    // Debounced resize handler
    let resizeTimeout: NodeJS.Timeout;
    const handleResize = () => {
      clearTimeout(resizeTimeout);
      resizeTimeout = setTimeout(checkEllipsis, 150);
    };
    window.addEventListener('resize', handleResize);

    return () => {
      clearTimeout(resizeTimeout);
      window.removeEventListener('resize', handleResize);
    };
  }, [selectedLabel, stringValue, selectMany]);

  // Shared Search State
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

  // Handle multiselect case
  if (selectMany) {
    const handleMultiSelectChange = (
      newSelectedOptions: MultiSelectOption[]
    ) => {
      // Prevent deselection if at or below min
      if (
        minSelections != null &&
        newSelectedOptions.length < minSelections &&
        newSelectedOptions.length < selectedValues.length
      ) {
        return; // ignore the change
      }

      const newValues = newSelectedOptions.map(opt => opt.value);
      const convertedValue = convertValuesToOriginalType(
        newValues,
        value,
        validOptions,
        selectMany
      );
      eventHandler('OnChange', id, [convertedValue]);
    };

    return (
      <div className="flex items-center gap-2 w-full" style={styles}>
        <div className="flex-1 relative w-full">
          <MultipleSelector
            value={selectedMultiSelectOptions}
            defaultOptions={multiSelectOptions}
            onValueChange={handleMultiSelectChange}
            placeholder={placeholder}
            disabled={disabled || loading}
            className={cn('w-full', ghost && 'ghost')}
            invalid={!!invalid}
            hidePlaceholderWhenSelected
            density={density}
            ghost={ghost}
            data-testid={dataTestId}
          />
          {(nullable && selectedMultiSelectOptions.length > 0 && !disabled) ||
          invalid ||
          loading ? (
            <div
              className={selectIconContainerVariant({ density })}
              style={{ zIndex: 2 }}
            >
              {/* Loading spinner */}
              {loading && (
                <div className="pointer-events-auto flex items-center h-6 p-1">
                  <Loader2 className="h-4 w-4 animate-spin text-muted-foreground text-opacity-50" />
                </div>
              )}
              {/* Clear (X) button */}
              {nullable &&
                selectedMultiSelectOptions.length > 0 &&
                !disabled && (
                  <button
                    type="button"
                    tabIndex={-1}
                    aria-label="Clear All"
                    onClick={e => {
                      e.preventDefault();
                      e.stopPropagation();
                      logger.debug(
                        'Select input clear button clicked (MultiSelect)',
                        { id }
                      );
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
              {/* Invalid icon - rightmost */}
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
  }

  const groupedOptions = filteredOptions.reduce<Record<string, Option[]>>(
    (acc, option) => {
      const key = option.group || 'default';
      if (!acc[key]) {
        acc[key] = [];
      }
      acc[key].push(option);
      return acc;
    },
    {}
  );

  const hasValue = stringValue !== undefined;

  const selectTriggerElement = (
    <SelectTrigger
      ref={triggerRef}
      className={cn(
        'relative',
        invalid && inputStyles.invalidInput,
        !hasValue && 'text-muted-foreground',
        ghost &&
          'border-transparent shadow-none bg-transparent hover:bg-accent hover:text-accent-foreground dark:border-transparent dark:bg-transparent dark:hover:bg-accent dark:hover:text-accent-foreground'
      )}
      density={density}
    >
      <SelectValue placeholder={placeholder} />
    </SelectTrigger>
  );

  // Wrap trigger with tooltip if ellipsed (tooltip hidden when dropdown is open)
  const shouldShowTooltip = isEllipsed && selectedLabel;
  const selectTrigger = shouldShowTooltip ? (
    <TooltipProvider>
      <Tooltip delayDuration={300} open={!isOpen ? undefined : false}>
        <TooltipTrigger asChild>{selectTriggerElement}</TooltipTrigger>
        <TooltipContent className="bg-popover text-popover-foreground shadow-md max-w-sm">
          <div className="whitespace-pre-wrap break-words">{selectedLabel}</div>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  ) : (
    selectTriggerElement
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
          {selectTrigger}
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
        {/* Right-side icon container */}
        {(nullable && hasValue && !disabled) || invalid || loading ? (
          <div
            className={selectIconContainerVariant({ density })}
            style={{ zIndex: 2 }}
          >
            {/* Loading spinner */}
            {loading && (
              <div className="pointer-events-auto flex items-center h-6 p-1">
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground text-opacity-50" />
              </div>
            )}
            {/* Clear (X) button */}
            {nullable && hasValue && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear"
                onClick={e => {
                  e.preventDefault();
                  e.stopPropagation();
                  logger.debug(
                    'Select input clear button clicked (SelectVariant)',
                    { id }
                  );
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
            {/* Invalid icon - rightmost */}
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

export const SelectInputWidget: React.FC<SelectInputWidgetProps> = props => {
  const eventHandler = useEventHandler();

  // Normalize undefined to null when nullable
  const normalizedProps = {
    ...props,
    value: props.nullable && props.value === undefined ? null : props.value,
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
        <CheckboxVariant {...normalizedProps} eventHandler={eventHandler} />
      ) : (
        <RadioVariant {...normalizedProps} eventHandler={eventHandler} />
      );
    case 'Toggle':
      return <ToggleVariant {...normalizedProps} eventHandler={eventHandler} />;
    default:
      return <SelectVariant {...normalizedProps} eventHandler={eventHandler} />;
  }
};

export default SelectInputWidget;
