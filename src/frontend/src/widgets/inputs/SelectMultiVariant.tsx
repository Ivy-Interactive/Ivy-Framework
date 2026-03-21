import React, { useMemo, useCallback } from "react";
import { cn } from "@/lib/utils";
import { MultipleSelector, Option as MultiSelectOption } from "@/components/ui/multiselect";
import { Loader2, X } from "lucide-react";
import { InvalidIcon } from "@/components/InvalidIcon";
import { logger } from "@/lib/logger";
import { selectIconContainerVariant } from "@/components/ui/select/variant";
import { xIconVariant } from "@/components/ui/input/text-input-variant";
import { SelectInputWidgetProps, Option } from "./select-types";
import { convertValuesToOriginalType } from "./select-utils";
import { getWidth } from "@/lib/styles";

export const SelectMultiVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  placeholder = "",
  value,
  disabled = false,
  invalid,
  options = [],
  eventHandler,
  selectMany = true,
  maxSelections,
  minSelections,
  loading = false,
  ghost = false,
  density,
  "data-testid": dataTestId,
  width,
}) => {
  const validOptions = options.filter(
    (option) => option.value != null && option.value.toString().trim() !== "",
  );

  const selectedValues = useMemo(() => {
    let values: string[] = [];
    if (value != null) {
      if (Array.isArray(value)) {
        values = (value as (string | number)[]).map((v) => v.toString());
      } else {
        values = value
          .toString()
          .split(",")
          .map((v) => v.trim());
      }
    }
    return values;
  }, [value]);

  const multiSelectOptions: MultiSelectOption[] = useMemo(() => {
    const isAtMax = maxSelections != null && selectedValues.length >= maxSelections;
    return validOptions.map((option) => ({
      label: option.label || option.value.toString(),
      value: option.value.toString(),
      disable:
        disabled ||
        loading ||
        option.disabled ||
        (isAtMax && !selectedValues.includes(option.value.toString())),
    }));
  }, [validOptions, selectedValues, maxSelections, disabled, loading]);

  const optionsLookup = useMemo(() => {
    const map = new Map<string, Option>();
    validOptions.forEach((option) => {
      map.set(option.value.toString(), option);
    });
    return map;
  }, [validOptions]);

  const selectedMultiSelectOptions: MultiSelectOption[] = useMemo(
    () =>
      selectedValues.map((val) => {
        const option = optionsLookup.get(val.toString());
        return {
          label: option?.label || val.toString(),
          value: val.toString(),
          disable: false,
        };
      }),
    [selectedValues, optionsLookup],
  );

  const handleMultiSelectChange = useCallback(
    (newSelectedOptions: MultiSelectOption[]) => {
      if (
        minSelections != null &&
        newSelectedOptions.length < minSelections &&
        newSelectedOptions.length < selectedValues.length
      ) {
        return;
      }

      const newValues = newSelectedOptions.map((opt) => opt.value);
      const convertedValue = convertValuesToOriginalType(
        newValues,
        value,
        validOptions,
        selectMany,
      );
      eventHandler("OnChange", id, [convertedValue]);
    },
    [minSelections, selectedValues.length, value, validOptions, selectMany, eventHandler, id],
  );

  const styles = getWidth(width);

  return (
    <div className="flex items-center gap-2 w-full" style={styles}>
      <div className="flex-1 relative w-full">
        <MultipleSelector
          value={selectedMultiSelectOptions}
          defaultOptions={multiSelectOptions}
          onValueChange={handleMultiSelectChange}
          placeholder={placeholder}
          disabled={disabled || loading}
          className={cn("w-full", ghost && "ghost")}
          invalid={!!invalid}
          hidePlaceholderWhenSelected
          density={density}
          ghost={ghost}
          data-testid={dataTestId}
        />
        {(selectedMultiSelectOptions.length > 0 && !disabled) || invalid || loading ? (
          <div className={selectIconContainerVariant({ density })} style={{ zIndex: 2 }}>
            {loading && (
              <div className="pointer-events-auto flex items-center h-6 p-1">
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground text-opacity-50" />
              </div>
            )}
            {selectedMultiSelectOptions.length > 0 && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear All"
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  logger.debug("Select input clear button clicked (MultiSelect)", { id });
                  eventHandler("OnChange", id, [null]);
                }}
                onKeyDown={(e) => {
                  if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    e.stopPropagation();
                    eventHandler("OnChange", id, [null]);
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
