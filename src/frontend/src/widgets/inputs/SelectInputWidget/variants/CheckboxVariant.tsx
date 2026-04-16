import React, { useCallback, useMemo, useState } from "react";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Tooltip, TooltipProvider, TooltipTrigger, TooltipContent } from "@/components/ui/tooltip";
import Icon from "@/components/Icon";
import { Loader2, Search, X } from "lucide-react";
import { InvalidIcon } from "@/components/InvalidIcon";
import { cn } from "@/lib/utils";
import { getWidth, inputStyles } from "@/lib/styles";
import { Densities } from "@/types/density";
import { xIconVariant } from "@/components/ui/input/text-input-variant";
import { SelectInputWidgetProps } from "../../select-types";
import {
  computeClearAllValues,
  computeSelectAllValues,
  convertValuesToOriginalType,
} from "../../select-utils";
import { EMPTY_ARRAY } from "@/lib/constants";
import { selectTextVariant } from "../styles";
import { SelectBulkActionsFooter } from "./SelectBulkActionsFooter";

export const CheckboxVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = EMPTY_ARRAY,
  eventHandler,
  separator = ",",
  nullable = false,
  maxSelections,
  minSelections,
  searchable = false,
  searchMode = "CaseInsensitive",
  emptyMessage,
  loading = false,
  ghost = false,
  showActions = false,
  density = Densities.Medium,
  events = EMPTY_ARRAY,
  "data-testid": dataTestId,
  width,
}) => {
  const validOptions = useMemo(
    () => options.filter((option) => option.value != null && option.value.toString().trim() !== ""),
    [options],
  );

  const selectedValues = useMemo(() => {
    let values: (string | number)[] = [];
    if (Array.isArray(value)) {
      values = value;
    } else if (value != null && value.toString().trim() !== "") {
      values = value
        .toString()
        .split(separator)
        .map((v) => v.trim());
    }
    return values;
  }, [value, separator]);

  const handleCheckboxChange = useCallback(
    (optionValue: string | number, checked: boolean) => {
      let currentValues: (string | number)[] = [];
      if (Array.isArray(value)) {
        currentValues = value;
      } else if (value != null && value.toString().trim() !== "") {
        currentValues = value
          .toString()
          .split(separator)
          .map((v) => v.trim());
      }

      let newValues: (string | number)[];
      if (checked) {
        newValues = [...currentValues, optionValue];
      } else {
        newValues = currentValues.filter((v) => v !== optionValue);
      }

      const convertedValue = convertValuesToOriginalType(
        newValues.map((v) => v.toString()),
        value,
        validOptions,
        true,
      );

      if (events.includes("OnChange")) eventHandler("OnChange", id, [convertedValue]);
    },
    [value, validOptions, eventHandler, id, separator, events],
  );

  const hasValues = selectedValues.length > 0;
  const isAtMax = maxSelections != null && selectedValues.length >= maxSelections;

  const [searchTerm, setSearchTerm] = useState("");

  const filteredOptions = useMemo(() => {
    if (!searchable || !searchTerm) return validOptions;

    return validOptions.filter((option) => {
      if (searchMode === "Fuzzy") {
        let i = 0;
        let j = 0;
        const searchLower = searchTerm.toLowerCase();
        const labelLower = (option.label || "").toLowerCase();
        while (i < searchLower.length && j < labelLower.length) {
          if (searchLower[i] === labelLower[j]) i++;
          j++;
        }
        return i === searchLower.length;
      }
      const term = searchMode === "CaseInsensitive" ? searchTerm.toLowerCase() : searchTerm;
      const label =
        searchMode === "CaseInsensitive" ? (option.label || "").toLowerCase() : option.label || "";
      return label.includes(term);
    });
  }, [validOptions, searchable, searchTerm, searchMode]);

  const visibleEnabledForBulkList = useMemo(
    () => filteredOptions.filter((o) => !disabled && !loading && !o.disabled),
    [filteredOptions, disabled, loading],
  );

  const bulkSelectAllDisabledList = visibleEnabledForBulkList.every((o) =>
    selectedValues.includes(o.value),
  );

  const bulkClearAllDisabledList = selectedValues.length <= (minSelections ?? 0);

  const handleBulkSelectAllCheckbox = useCallback(() => {
    const merged = computeSelectAllValues(
      selectedValues,
      filteredOptions.map((o) => ({
        value: o.value,
        disabled: !!(disabled || loading || o.disabled),
      })),
      maxSelections,
    );
    const converted = convertValuesToOriginalType(
      merged.map((v) => v.toString()),
      value,
      validOptions,
      true,
    );
    if (events.includes("OnChange")) eventHandler("OnChange", id, [converted]);
  }, [
    selectedValues,
    filteredOptions,
    disabled,
    loading,
    maxSelections,
    value,
    validOptions,
    eventHandler,
    id,
    events,
  ]);

  const handleBulkClearAllCheckbox = useCallback(() => {
    if (!events.includes("OnChange")) return;
    const cleared = computeClearAllValues(selectedValues, minSelections);
    if (cleared.length === 0 && nullable) {
      eventHandler("OnChange", id, [null]);
      return;
    }
    const converted = convertValuesToOriginalType(
      cleared.map((v) => v.toString()),
      value,
      validOptions,
      true,
    );
    eventHandler("OnChange", id, [converted]);
  }, [selectedValues, minSelections, nullable, value, validOptions, eventHandler, id, events]);

  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  return (
    <div
      className={cn(
        "relative w-full border border-input bg-transparent rounded-box shadow-sm px-3 py-2 focus-within:ring-1 focus-within:ring-ring dark:border-white/10",
        invalid && "border-destructive focus-within:ring-destructive",
        ghost &&
          "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
      )}
      style={styles}
      onBlur={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
        }
      }}
      onFocus={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
        }
      }}
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
                onChange={(e) => setSearchTerm(e.target.value)}
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
              {emptyMessage || "No options available"}
            </div>
          ) : (
            <React.Fragment>
              <div
                className={cn(
                  "flex flex-col gap-4",
                  filteredOptions.length > 6 ? "max-h-48 overflow-y-auto slim-scrollbar" : "",
                )}
                data-testid={dataTestId}
              >
                {filteredOptions.map((option) => {
                  const isSelected = selectedValues.includes(option.value);
                  const isInvalid = !!invalid && isSelected;
                  const isDisabled =
                    disabled ||
                    loading ||
                    option.disabled ||
                    (!isSelected && isAtMax) ||
                    (isSelected && minSelections != null && selectedValues.length <= minSelections);

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
                                disabled={isDisabled}
                                className={cn(
                                  inputStyles.invalidInput,
                                  "bg-destructive/10 border-destructive text-destructive",
                                  selectTextVariant[density],
                                )}
                              />
                            </TooltipTrigger>
                            <TooltipContent className="bg-popover text-popover-foreground shadow-md">
                              <div className="max-w-xs sm:max-w-sm">{invalid}</div>
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
                          disabled={isDisabled}
                          className={cn(
                            "data-[state=unchecked]:bg-transparent data-[state=unchecked]:border-border",
                            selectTextVariant[density],
                            isSelected
                              ? "data-[state=checked]:bg-primary data-[state=checked]:border-primary data-[state=checked]:text-primary-foreground"
                              : undefined,
                          )}
                        />
                      )}
                      {option.tooltip ? (
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Label
                                htmlFor={`${id}-${option.value}`}
                                className={cn(
                                  "flex-1 cursor-pointer flex items-center gap-2",
                                  selectTextVariant[density],
                                  isInvalid ? inputStyles.invalidInput : undefined,
                                  isDisabled && !isSelected ? "opacity-50" : undefined,
                                )}
                              >
                                {option.icon && (
                                  <Icon name={option.icon} className="h-4 w-4 flex-shrink-0" />
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
                            </TooltipTrigger>
                            <TooltipContent>{option.tooltip}</TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
                      ) : (
                        <Label
                          htmlFor={`${id}-${option.value}`}
                          className={cn(
                            "flex-1 cursor-pointer flex items-center gap-2",
                            selectTextVariant[density],
                            isInvalid ? inputStyles.invalidInput : undefined,
                            isDisabled && !isSelected ? "opacity-50" : undefined,
                          )}
                        >
                          {option.icon && (
                            <Icon name={option.icon} className="h-4 w-4 flex-shrink-0" />
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
                      )}
                    </div>
                  );
                })}
              </div>
              {showActions && !loading && filteredOptions.length > 0 && (
                <SelectBulkActionsFooter
                  density={density}
                  onSelectAll={handleBulkSelectAllCheckbox}
                  onClearAll={handleBulkClearAllCheckbox}
                  selectAllDisabled={bulkSelectAllDisabledList}
                  clearAllDisabled={bulkClearAllDisabledList}
                />
              )}
            </React.Fragment>
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
                  const clearedValue = nullable ? null : [];
                  if (events.includes("OnChange")) eventHandler("OnChange", id, [clearedValue]);
                }}
                className="flex-shrink-0 p-1 rounded hover:bg-accent focus:outline-none"
              >
                <X className={xIconVariant({ density })} />
              </button>
            )}
            {invalid && <InvalidIcon message={invalid} className="pointer-events-auto" />}
          </div>
        )}
      </div>
    </div>
  );
};
