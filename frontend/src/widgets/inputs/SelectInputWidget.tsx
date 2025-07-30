import React from 'react';
import {
  useEventHandler,
  EventHandler,
} from '@/components/EventHandlerContext';
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { InvalidIcon } from '@/components/InvalidIcon';
import { cn } from '@/lib/utils';
import { inputStyles } from '@/lib/styles';
import {
  Tooltip,
  TooltipProvider,
  TooltipTrigger,
  TooltipContent,
} from '@/components/ui/tooltip';
import { X } from 'lucide-react';
import { useCallback } from 'react';
import { logger } from '@/lib/logger';
import {
  MultipleSelector,
  Option as MultiSelectOption,
} from '@/components/ui/multiselect';

export type NullableSelectValue =
  | string
  | number
  | string[]
  | number[]
  | null
  | undefined;

interface Option {
  value: string | number;
  label: string;
  group?: string;
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
  'data-testid'?: string;
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

const ToggleVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = [],
  eventHandler,
  selectMany = false,
  separator = ',',
  nullable = false,
  'data-testid': dataTestId,
}) => {
  const validOptions = options.filter(
    option => option.value != null && option.value.toString().trim() !== ''
  );

  // Handle both single and multiple selection
  let selectedValues: (string | number)[] = [];
  if (selectMany) {
    if (Array.isArray(value)) {
      selectedValues = value;
    } else if (value != null && value.toString().trim() !== '') {
      selectedValues = value
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
      selectedValues = [stringValue];
    }
  }

  const hasValue = selectedValues.length > 0;

  const handleValueChange = useSelectValueHandler(
    id,
    value,
    validOptions,
    eventHandler,
    selectMany
  );

  // Outer container
  const container = (
    <div className="flex items-center gap-2">
      <div className="flex-1">
        {selectMany ? (
          <ToggleGroup
            type="multiple"
            value={selectedValues.map(v => v.toString())}
            onValueChange={handleValueChange}
            disabled={disabled}
            className="flex flex-wrap gap-2"
            data-testid={dataTestId}
          >
            {validOptions.map(option => {
              const isSelected = selectedValues.includes(option.value);
              const isInvalid = !!invalid && isSelected;
              const toggleItem = (
                <ToggleGroupItem
                  key={option.value}
                  value={option.value.toString()}
                  aria-label={option.label}
                  className={cn(
                    'px-3 py-2 hover:text-foreground',
                    isInvalid
                      ? `${inputStyles.invalidInput} !bg-destructive/10 !border-destructive !text-destructive`
                      : isSelected
                        ? 'data-[state=on]:bg-primary data-[state=on]:border-primary data-[state=on]:text-primary-foreground'
                        : undefined
                  )}
                >
                  {option.label}
                </ToggleGroupItem>
              );
              if (isInvalid) {
                return (
                  <TooltipProvider key={option.value}>
                    <Tooltip>
                      <TooltipTrigger asChild>{toggleItem}</TooltipTrigger>
                      <TooltipContent className="bg-popover text-popover-foreground shadow-md">
                        <div className="max-w-60">{invalid}</div>
                      </TooltipContent>
                    </Tooltip>
                  </TooltipProvider>
                );
              }
              return toggleItem;
            })}
          </ToggleGroup>
        ) : (
          <ToggleGroup
            type="single"
            value={selectedValues[0]?.toString()}
            onValueChange={handleValueChange}
            disabled={disabled}
            className="flex flex-wrap gap-2"
            data-testid={dataTestId}
          >
            {validOptions.map(option => {
              const isSelected = selectedValues[0] === option.value.toString();
              const isInvalid = !!invalid && isSelected;
              const toggleItem = (
                <ToggleGroupItem
                  key={option.value}
                  value={option.value.toString()}
                  aria-label={option.label}
                  className={cn(
                    'px-3 py-2 hover:text-foreground',
                    isInvalid
                      ? `${inputStyles.invalidInput} !bg-destructive/10 !border-destructive !text-destructive`
                      : isSelected
                        ? 'data-[state=on]:bg-primary data-[state=on]:border-primary data-[state=on]:text-primary-foreground'
                        : undefined
                  )}
                >
                  {option.label}
                </ToggleGroupItem>
              );
              if (isInvalid) {
                return (
                  <TooltipProvider key={option.value}>
                    <Tooltip>
                      <TooltipTrigger asChild>{toggleItem}</TooltipTrigger>
                      <TooltipContent className="bg-popover text-popover-foreground shadow-md">
                        <div className="max-w-60">{invalid}</div>
                      </TooltipContent>
                    </Tooltip>
                  </TooltipProvider>
                );
              }
              return toggleItem;
            })}
          </ToggleGroup>
        )}
      </div>
      {nullable && hasValue && !disabled && (
        <button
          type="button"
          tabIndex={-1}
          aria-label={selectMany ? 'Clear All' : 'Clear'}
          onClick={() => {
            logger.debug('Select input clear button clicked (ToggleVariant)', {
              id,
              selectMany,
              clearValue: selectMany ? [] : null,
            });
            eventHandler('OnChange', id, [selectMany ? [] : null]);
          }}
          className="flex-shrink-0 p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
        >
          <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
        </button>
      )}
    </div>
  );

  return container;
};

const RadioVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = [],
  eventHandler,
  nullable = false,
  'data-testid': dataTestId,
}) => {
  const validOptions = options.filter(
    option => option.value != null && option.value.toString().trim() !== ''
  );
  const stringValue =
    value != null && value.toString().trim() !== ''
      ? value.toString()
      : undefined;

  const hasValue = stringValue !== undefined;

  const handleValueChange = useSelectValueHandler(
    id,
    value,
    validOptions,
    eventHandler,
    false // Always single select for RadioVariant
  );

  const container = (
    <div className="flex items-center gap-2">
      <div className="flex-1">
        <RadioGroup
          value={stringValue}
          onValueChange={handleValueChange}
          disabled={disabled}
          className="flex flex-col space-y-2"
          data-testid={dataTestId}
        >
          {validOptions.map(option => (
            <div key={option.value} className="flex items-center space-x-2">
              <RadioGroupItem
                value={option.value.toString()}
                id={`${id}-${option.value}`}
                className={cn(
                  'border-input text-input',
                  stringValue === option.value.toString() && !invalid
                    ? 'border-primary text-primary'
                    : undefined,
                  stringValue === option.value.toString() && invalid
                    ? inputStyles.invalidInput
                    : undefined
                )}
              />
              <Label
                htmlFor={`${id}-${option.value}`}
                className={cn(
                  stringValue === option.value.toString() && invalid
                    ? inputStyles.invalidInput
                    : undefined
                )}
              >
                {option.label}
              </Label>
            </div>
          ))}
        </RadioGroup>
      </div>
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
          <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
        </button>
      )}
    </div>
  );

  return invalid ? (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger asChild>{container}</TooltipTrigger>
        <TooltipContent className="bg-popover text-popover-foreground shadow-md">
          <div className="max-w-60">{invalid}</div>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  ) : (
    container
  );
};

const CheckboxVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = [],
  eventHandler,
  separator = ',',
  nullable = false,
  'data-testid': dataTestId,
}) => {
  const validOptions = options.filter(
    option => option.value != null && option.value.toString().trim() !== ''
  );
  let selectedValues: (string | number)[] = [];
  if (Array.isArray(value)) {
    selectedValues = value;
  } else if (value != null && value.toString().trim() !== '') {
    selectedValues = value
      .toString()
      .split(separator)
      .map(v => v.trim());
  }
  const handleCheckboxChange = useCallback(
    (optionValue: string | number, checked: boolean) => {
      logger.debug('Select input checkbox change', {
        id,
        optionValue,
        checked,
        currentSelectedValues: selectedValues,
      });

      let newValues: (string | number)[];
      if (checked) {
        newValues = [...selectedValues, optionValue];
      } else {
        newValues = selectedValues.filter(v => v !== optionValue);
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
    [selectedValues, value, validOptions, eventHandler, id]
  );

  const hasValues = selectedValues.length > 0;

  const container = (
    <div className="flex items-center gap-2">
      <div className="flex-1">
        <div className="flex flex-col space-y-2 gap-2" data-testid={dataTestId}>
          {validOptions.map(option => {
            const isSelected = selectedValues.includes(option.value);
            const isInvalid = !!invalid && isSelected;
            return (
              <div key={option.value} className="flex items-center space-x-2">
                {isInvalid ? (
                  <TooltipProvider>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Checkbox
                          id={`${id}-${option.value}`}
                          checked={isSelected}
                          onCheckedChange={checked =>
                            handleCheckboxChange(option.value, checked === true)
                          }
                          disabled={disabled}
                          className={cn(
                            inputStyles.invalidInput +
                              ' !bg-red-50 !border-red-500 !text-red-900'
                          )}
                        />
                      </TooltipTrigger>
                      <TooltipContent className="bg-popover text-popover-foreground shadow-md">
                        <div className="max-w-60">{invalid}</div>
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
                    disabled={disabled}
                    className={cn(
                      'data-[state=unchecked]:bg-transparent data-[state=unchecked]:border-border',
                      isSelected
                        ? 'data-[state=checked]:bg-primary data-[state=checked]:border-primary data-[state=checked]:text-primary-foreground'
                        : undefined
                    )}
                  />
                )}
                <Label
                  htmlFor={`${id}-${option.value}`}
                  className={cn(
                    isInvalid ? inputStyles.invalidInput : undefined
                  )}
                >
                  {option.label}
                </Label>
              </div>
            );
          })}
        </div>
      </div>
      {nullable && hasValues && !disabled && (
        <button
          type="button"
          tabIndex={-1}
          aria-label="Clear All"
          onClick={() => {
            logger.debug(
              'Select input clear button clicked (CheckboxVariant)',
              { id }
            );
            eventHandler('OnChange', id, [[]]);
          }}
          className="flex-shrink-0 p-1 rounded hover:bg-accent focus:outline-none"
        >
          <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
        </button>
      )}
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
  options = [],
  eventHandler,
  nullable = false,
  selectMany = false,
  'data-testid': dataTestId,
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

  // Handle multiselect case
  if (selectMany) {
    // Convert current value to array format for multiselect
    let selectedValues: (string | number)[] = [];
    if (Array.isArray(value)) {
      selectedValues = value;
    } else if (value != null && value.toString().trim() !== '') {
      selectedValues = value
        .toString()
        .split(',')
        .map(v => v.trim());
    }

    // Convert options to MultiSelectOption format
    const multiSelectOptions: MultiSelectOption[] = validOptions.map(
      option => ({
        label: option.label,
        value: option.value.toString(),
        disable: false,
      })
    );

    // Convert selected values to MultiSelectOption format
    const selectedMultiSelectOptions: MultiSelectOption[] = selectedValues.map(
      val => {
        const option = validOptions.find(
          opt => opt.value.toString() === val.toString()
        );
        return {
          label: option?.label || val.toString(),
          value: val.toString(),
          disable: false,
        };
      }
    );

    const handleMultiSelectChange = (
      newSelectedOptions: MultiSelectOption[]
    ) => {
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
      <div className="flex items-center gap-2 w-full">
        <div className="flex-1 relative w-full">
          <MultipleSelector
            value={selectedMultiSelectOptions}
            defaultOptions={multiSelectOptions}
            onValueChange={handleMultiSelectChange}
            placeholder={placeholder}
            disabled={disabled}
            className="w-full"
            invalid={!!invalid}
            hideClearAllButton={!nullable}
            hidePlaceholderWhenSelected
            emptyIndicator={
              <p className="text-center text-large-body">No results found</p>
            }
          />
          {invalid && (
            <div className="absolute right-2 top-1/2 -translate-y-1/2">
              <InvalidIcon message={invalid} />
            </div>
          )}
        </div>
      </div>
    );
  }

  // Original single select logic
  const groupedOptions = validOptions.reduce<Record<string, Option[]>>(
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

  const stringValue =
    value != null && value.toString().trim() !== ''
      ? value.toString()
      : undefined;

  const hasValue = stringValue !== undefined;

  return (
    <div className="flex items-center gap-2 w-full">
      <div className="flex-1 relative w-full">
        <Select
          key={`${id}-${stringValue ?? 'null'}`}
          disabled={disabled}
          value={stringValue}
          onValueChange={handleValueChange}
          data-testid={dataTestId}
        >
          <SelectTrigger
            className={cn('relative', invalid && inputStyles.invalidInput)}
          >
            <SelectValue placeholder={placeholder} />
          </SelectTrigger>
          {/* Right-side icon container */}
          {(nullable && hasValue && !disabled) || invalid ? (
            <div
              className="absolute top-1/2 -translate-y-1/2 flex items-center gap-1 right-8"
              style={{ zIndex: 2 }}
            >
              {/* Clear (X) button */}
              {nullable && hasValue && !disabled && (
                <span
                  role="button"
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
                  className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
                >
                  <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
                </span>
              )}
              {/* Invalid icon */}
              {invalid && (
                <span className="flex items-center">
                  <InvalidIcon message={invalid} />
                </span>
              )}
            </div>
          ) : null}
          <SelectContent>
            {Object.entries(groupedOptions).map(([group, options]) => (
              <SelectGroup key={group}>
                {group !== 'default' && <SelectLabel>{group}</SelectLabel>}
                {options.map(option => (
                  <SelectItem
                    key={option.value}
                    value={option.value.toString()}
                  >
                    {option.label}
                  </SelectItem>
                ))}
              </SelectGroup>
            ))}
          </SelectContent>
        </Select>
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
