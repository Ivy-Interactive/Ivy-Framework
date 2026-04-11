import React, { useCallback, useMemo, useState } from "react";
import { ToggleGroup } from "@/components/ui/toggle";
import { Input } from "@/components/ui/input";
import { Loader2, Search, X } from "lucide-react";
import { InvalidIcon } from "@/components/InvalidIcon";
import { cn } from "@/lib/utils";
import { getWidth } from "@/lib/styles";
import { Densities } from "@/types/density";
import { xIconVariant } from "@/components/ui/input/text-input-variant";
import { SelectInputWidgetProps } from "../../select-types";
import {
  computeClearAllValues,
  computeSelectAllValues,
  convertValuesToOriginalType,
  useSelectValueHandler,
} from "../../select-utils";
import { EMPTY_ARRAY } from "@/lib/constants";
import { selectContainerVariant } from "../styles";
import { ToggleOptionItem } from "./ToggleOptionItem";
import { SelectBulkActionsFooter } from "./SelectBulkActionsFooter";

export const ToggleVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = EMPTY_ARRAY,
  eventHandler,
  selectMany = false,
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
    if (selectMany) {
      if (Array.isArray(value)) {
        values = value;
      } else if (value != null && value.toString().trim() !== "") {
        values = value
          .toString()
          .split(separator)
          .map((v) => v.trim());
      }
    } else {
      const stringValue =
        value != null && value.toString().trim() !== "" ? value.toString() : undefined;
      if (stringValue !== undefined) {
        values = [stringValue];
      }
    }
    return values;
  }, [value, selectMany, separator]);

  const hasValue = selectedValues.length > 0;
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

  const handleValueChange = useSelectValueHandler(
    id,
    value,
    validOptions,
    eventHandler,
    selectMany,
    nullable,
  );

  const visibleEnabledForBulk = useMemo(
    () => filteredOptions.filter((o) => !disabled && !loading && !o.disabled),
    [filteredOptions, disabled, loading],
  );

  const bulkSelectAllDisabled =
    !selectMany || visibleEnabledForBulk.every((o) => selectedValues.includes(o.value));

  const bulkClearAllDisabled = !selectMany || selectedValues.length <= (minSelections ?? 0);

  const handleBulkSelectAllToggle = useCallback(() => {
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
      selectMany,
    );
    eventHandler("OnChange", id, [converted]);
  }, [
    selectedValues,
    filteredOptions,
    disabled,
    loading,
    maxSelections,
    value,
    validOptions,
    selectMany,
    eventHandler,
    id,
  ]);

  const handleBulkClearAllToggle = useCallback(() => {
    const cleared = computeClearAllValues(selectedValues, minSelections);
    if (cleared.length === 0 && nullable) {
      eventHandler("OnChange", id, [null]);
      return;
    }
    const converted = convertValuesToOriginalType(
      cleared.map((v) => v.toString()),
      value,
      validOptions,
      selectMany,
    );
    eventHandler("OnChange", id, [converted]);
  }, [selectedValues, minSelections, nullable, value, validOptions, selectMany, eventHandler, id]);

  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  return (
    <div
      className={cn(
        selectContainerVariant({ density }),
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
      <div className="flex items-center gap-2">
        <div className="flex-1">
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
            <div className="flex justify-center p-2">
              <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
            </div>
          ) : filteredOptions.length === 0 ? (
            <div className="p-2 text-center text-sm text-muted-foreground">
              {emptyMessage || "No options available"}
            </div>
          ) : selectMany ? (
            <ToggleGroup
              type="multiple"
              value={selectedValues.map((v) => v.toString())}
              onValueChange={handleValueChange}
              disabled={disabled}
              className="flex flex-wrap gap-2"
              data-testid={dataTestId}
            >
              {filteredOptions.map((option) => {
                const isSelected = selectedValues.includes(option.value);
                const isDisabled =
                  disabled ||
                  loading ||
                  option.disabled ||
                  (!isSelected && isAtMax) ||
                  (isSelected && minSelections != null && selectedValues.length <= minSelections);

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
              value={selectedValues[0]?.toString() ?? ""}
              onValueChange={handleValueChange}
              disabled={disabled}
              className="flex flex-wrap gap-2"
            >
              {filteredOptions.map((option) => {
                const isSelected = selectedValues[0] === option.value.toString();
                const isDisabled =
                  disabled ||
                  loading ||
                  option.disabled ||
                  (!isSelected && isAtMax) ||
                  (isSelected && minSelections != null && selectedValues.length <= minSelections);

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
          {showActions && selectMany && !loading && filteredOptions.length > 0 && (
            <SelectBulkActionsFooter
              density={density}
              onSelectAll={handleBulkSelectAllToggle}
              onClearAll={handleBulkClearAllToggle}
              selectAllDisabled={bulkSelectAllDisabled}
              clearAllDisabled={bulkClearAllDisabled}
            />
          )}
        </div>
        {((selectMany && nullable && hasValue && !disabled) || invalid) && (
          <div className="flex items-center gap-1">
            {selectMany && nullable && hasValue && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear All"
                onClick={() => {
                  eventHandler("OnChange", id, [null]);
                }}
                className="flex-shrink-0 p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
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
