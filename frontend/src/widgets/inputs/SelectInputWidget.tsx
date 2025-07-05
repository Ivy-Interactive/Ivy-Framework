import {
  useEventHandler,
  EventHandler,
} from "@/components/EventHandlerContext";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import {
  Tooltip,
  TooltipProvider,
  TooltipTrigger,
  TooltipContent,
} from "@/components/ui/tooltip";
import { inputStyles } from "@/lib/styles";
import { cn } from "@/lib/utils";
import { X } from "lucide-react";
import React from "react";

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
  variant?: "Select" | "List" | "Toggle";
  nullable?: boolean;
  disabled?: boolean;
  invalid?: string;
  options: Option[];
  eventHandler: EventHandler;
  selectMany: boolean;
  separator: string;
}

const ToggleVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = [],
  eventHandler,
  selectMany = false,
  separator = ",",
  nullable = false,
}) => {
  const validOptions = options.filter(
    (option) => option.value != null && option.value.toString().trim() !== ""
  );

  // Handle both single and multiple selection
  let selectedValues: (string | number)[] = [];
  if (selectMany) {
    if (Array.isArray(value)) {
      selectedValues = value;
    } else if (value != null && value.toString().trim() !== "") {
      selectedValues = value
        .toString()
        .split(separator)
        .map((v) => v.trim());
    }
  } else {
    const stringValue =
      value != null && value.toString().trim() !== ""
        ? value.toString()
        : undefined;
    if (stringValue !== undefined) {
      selectedValues = [stringValue];
    }
  }

  const hasValue = selectedValues.length > 0;

  // Outer container
  const container = (
    <div className="flex items-center gap-2">
      <div className="flex-1">
        {selectMany ? (
          <ToggleGroup
            type="multiple"
            value={selectedValues.map((v) => v.toString())}
            onValueChange={(newValue: string[]) => {
              eventHandler("OnChange", id, [newValue]);
            }}
            disabled={disabled}
            className="flex flex-wrap gap-2"
          >
            {validOptions.map((option) => {
              const isSelected = selectedValues.includes(option.value);
              const isInvalid = !!invalid && isSelected;
              const toggleItem = (
                <ToggleGroupItem
                  key={option.value}
                  value={option.value.toString()}
                  aria-label={option.label}
                  className={cn(
                    "px-3 py-2",
                    isInvalid
                      ? `${inputStyles.invalid} !bg-red-50 !border-red-500 !text-red-900`
                      : isSelected
                      ? "data-[state=on]:bg-emerald-100 data-[state=on]:border-emerald-500 data-[state=on]:text-emerald-900"
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
            onValueChange={(newValue: string) => {
              eventHandler("OnChange", id, [newValue]);
            }}
            disabled={disabled}
            className="flex flex-wrap gap-2"
          >
            {validOptions.map((option) => {
              const isSelected = selectedValues[0] === option.value.toString();
              const isInvalid = !!invalid && isSelected;
              const toggleItem = (
                <ToggleGroupItem
                  key={option.value}
                  value={option.value.toString()}
                  aria-label={option.label}
                  className={cn(
                    "px-3 py-2",
                    isInvalid
                      ? `${inputStyles.invalid} !bg-red-50 !border-red-500 !text-red-900`
                      : isSelected
                      ? "data-[state=on]:bg-emerald-100 data-[state=on]:border-emerald-500 data-[state=on]:text-emerald-900"
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
          aria-label={selectMany ? "Clear All" : "Clear"}
          onClick={() => eventHandler("OnChange", id, [selectMany ? [] : null])}
          className="flex-shrink-0 p-1 rounded hover:bg-gray-100 focus:outline-none"
        >
          <X className="h-4 w-4 text-gray-400 hover:text-gray-600" />
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
}) => {
  const validOptions = options.filter(
    (option) => option.value != null && option.value.toString().trim() !== ""
  );
  const stringValue =
    value != null && value.toString().trim() !== ""
      ? value.toString()
      : undefined;

  const hasValue = stringValue !== undefined;

  const container = (
    <div className="flex items-center gap-2">
      <div className="flex-1">
        <RadioGroup
          value={stringValue}
          onValueChange={(newValue) => eventHandler("OnChange", id, [newValue])}
          disabled={disabled}
          className="flex flex-col space-y-2"
        >
          {validOptions.map((option) => (
            <div key={option.value} className="flex items-center space-x-2">
              <RadioGroupItem
                value={option.value.toString()}
                id={`${id}-${option.value}`}
                className={cn(
                  stringValue === option.value.toString() && invalid
                    ? inputStyles.invalid
                    : undefined
                )}
              />
              <Label
                htmlFor={`${id}-${option.value}`}
                className={cn(
                  stringValue === option.value.toString() && invalid
                    ? inputStyles.invalid
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
          onClick={() => eventHandler("OnChange", id, [null])}
          className="flex-shrink-0 p-1 rounded hover:bg-gray-100 focus:outline-none"
        >
          <X className="h-4 w-4 text-gray-400 hover:text-gray-600" />
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
  separator = ",",
  nullable = false,
}) => {
  const validOptions = options.filter(
    (option) => option.value != null && option.value.toString().trim() !== ""
  );
  let selectedValues: (string | number)[] = [];
  if (Array.isArray(value)) {
    selectedValues = value;
  } else if (value != null && value.toString().trim() !== "") {
    selectedValues = value
      .toString()
      .split(separator)
      .map((v) => v.trim());
  }
  const handleCheckboxChange = (
    optionValue: string | number,
    checked: boolean
  ) => {
    let newValues: (string | number)[];
    if (checked) {
      newValues = [...selectedValues, optionValue];
    } else {
      newValues = selectedValues.filter((v) => v !== optionValue);
    }
    eventHandler("OnChange", id, [newValues]);
  };

  const hasValues = selectedValues.length > 0;

  const container = (
    <div className="flex items-center gap-2">
      <div className="flex-1">
        <div className="flex flex-col space-y-2 gap-2">
          {validOptions.map((option) => {
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
                          onCheckedChange={(checked) =>
                            handleCheckboxChange(option.value, checked === true)
                          }
                          disabled={disabled}
                          className={cn(
                            inputStyles.invalid +
                              " !bg-red-50 !border-red-500 !text-red-900"
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
                    onCheckedChange={(checked) =>
                      handleCheckboxChange(option.value, checked === true)
                    }
                    disabled={disabled}
                    className={cn(
                      isSelected
                        ? "data-[state=checked]:bg-emerald-100 data-[state=checked]:border-emerald-500 data-[state=checked]:text-emerald-900"
                        : undefined
                    )}
                  />
                )}
                <Label
                  htmlFor={`${id}-${option.value}`}
                  className={cn(isInvalid ? inputStyles.invalid : undefined)}
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
          onClick={() => eventHandler("OnChange", id, [[]])}
          className="flex-shrink-0 p-1 rounded hover:bg-gray-100 focus:outline-none"
        >
          <X className="h-4 w-4 text-gray-400 hover:text-gray-600" />
        </button>
      )}
    </div>
  );
  return container;
};

const SelectVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  placeholder = "",
  value,
  disabled = false,
  invalid,
  options = [],
  eventHandler,
  nullable = false,
}) => {
  const validOptions = options.filter(
    (option) => option.value != null && option.value.toString().trim() !== ""
  );

  const groupedOptions = validOptions.reduce<Record<string, Option[]>>(
    (acc, option) => {
      const key = option.group || "default";
      if (!acc[key]) {
        acc[key] = [];
      }
      acc[key].push(option);
      return acc;
    },
    {}
  );

  const stringValue =
    value != null && value.toString().trim() !== ""
      ? value.toString()
      : undefined;

  const hasValue = stringValue !== undefined;

  return (
    <div className="flex items-center gap-2">
      <div className="flex-1 relative">
        <Select
          key={`${id}-${stringValue ?? "null"}`}
          disabled={disabled}
          value={stringValue}
          onValueChange={(newValue) => eventHandler("OnChange", id, [newValue])}
        >
          <SelectTrigger className={cn(invalid && inputStyles.invalid)}>
            <SelectValue placeholder={placeholder} />
          </SelectTrigger>
          <SelectContent>
            {Object.entries(groupedOptions).map(([group, options]) => (
              <SelectGroup key={group}>
                {group !== "default" && <SelectLabel>{group}</SelectLabel>}
                {options.map((option) => (
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
        {nullable && hasValue && !disabled && (
          <button
            type="button"
            tabIndex={-1}
            aria-label="Clear"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              eventHandler("OnChange", id, [null]);
            }}
            className="absolute top-1/2 -translate-y-1/2 right-8 z-10 p-1 rounded hover:bg-gray-100 focus:outline-none"
            style={{ pointerEvents: "auto" }}
          >
            <X className="h-4 w-4 text-gray-400 hover:text-gray-600" />
          </button>
        )}
      </div>
      {invalid && <InvalidIcon message={invalid} className="flex-shrink-0" />}
    </div>
  );
};

export const SelectInputWidget: React.FC<SelectInputWidgetProps> = (props) => {
  const eventHandler = useEventHandler();

  // Normalize undefined to null when nullable
  const normalizedProps = {
    ...props,
    value: props.nullable && props.value === undefined ? null : props.value,
  };

  switch (normalizedProps.variant) {
    case "List":
      return normalizedProps.selectMany ? (
        <CheckboxVariant {...normalizedProps} eventHandler={eventHandler} />
      ) : (
        <RadioVariant {...normalizedProps} eventHandler={eventHandler} />
      );
    case "Toggle":
      return <ToggleVariant {...normalizedProps} eventHandler={eventHandler} />;
    default:
      return <SelectVariant {...normalizedProps} eventHandler={eventHandler} />;
  }
};

export default SelectInputWidget;
