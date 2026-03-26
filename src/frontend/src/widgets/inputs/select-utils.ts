import { useCallback } from "react";
import { defaultFilter } from "cmdk";
import { EventHandler } from "@/components/event-handler";
import { NullableSelectValue, Option } from "./select-types";

/** Options that can be included in bulk select; matches cmdk's default filter when `search` is non-empty. */
export function filterOptionsLikeCmdk(
  options: { label: string; value: string }[],
  search: string,
): { label: string; value: string }[] {
  const term = search.trim();
  if (!term) return options;
  return options.filter((o) => defaultFilter(o.label, term, []) > 0);
}

/** Merge current selection with all enabled options in `visibleOptions`, preserving selections outside that set; cap with `maxSelections`. */
export function computeSelectAllValues(
  selectedValues: (string | number)[],
  visibleOptions: { value: string | number; disabled?: boolean }[],
  maxSelections?: number | null,
): (string | number)[] {
  const visibleEnabled = visibleOptions.filter((o) => !o.disabled);
  const visibleKeys = new Set(visibleEnabled.map((o) => o.value.toString()));
  const outside = selectedValues.filter((v) => !visibleKeys.has(v.toString()));
  const visibleValues = visibleEnabled.map((o) => o.value);

  const combinedKeys = new Set<string>();
  const combined: (string | number)[] = [];
  const pushUnique = (v: string | number) => {
    const k = v.toString();
    if (!combinedKeys.has(k)) {
      combinedKeys.add(k);
      combined.push(v);
    }
  };
  for (const v of outside) pushUnique(v);
  for (const v of visibleValues) pushUnique(v);

  if (maxSelections != null && combined.length > maxSelections) {
    const truncated: (string | number)[] = [];
    for (const v of outside) {
      if (truncated.length >= maxSelections) break;
      truncated.push(v);
    }
    for (const v of visibleValues) {
      if (truncated.length >= maxSelections) break;
      if (!truncated.some((t) => t.toString() === v.toString())) {
        truncated.push(v);
      }
    }
    return truncated;
  }
  return combined;
}

/** Clear down to `minSelections` items (keeps the first entries in current selection order). */
export function computeClearAllValues(
  selectedValues: (string | number)[],
  minSelections?: number | null,
): (string | number)[] {
  const min = minSelections != null && minSelections > 0 ? minSelections : 0;
  if (min === 0) return [];
  return selectedValues.slice(0, min);
}

// Helper function to convert string values back to their original types
export const convertValuesToOriginalType = (
  stringValues: string[],
  originalValue: NullableSelectValue,
  options: Option[],
  selectMany: boolean = false,
): NullableSelectValue => {
  if (stringValues.length === 0) {
    if (originalValue === null || originalValue === undefined) {
      if (selectMany) {
        if (options.length > 0) {
          const firstOption = options[0];
          if (typeof firstOption.value === "number") {
            return [];
          } else if (typeof firstOption.value === "string") {
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

  if (originalValue instanceof Array) {
    if (originalValue.length > 0 && typeof originalValue[0] === "number") {
      return stringValues.map((v) => {
        const option = optionsMap.get(v);
        return option ? Number(option.value) : Number(v);
      });
    } else if (originalValue.length > 0 && typeof originalValue[0] === "string") {
      return stringValues.map((v) => {
        const option = optionsMap.get(v);
        return option ? String(option.value) : v;
      });
    }
    return stringValues;
  }

  if ((originalValue === null || originalValue === undefined) && selectMany) {
    if (options.length > 0) {
      const firstOption = options[0];
      if (typeof firstOption.value === "number") {
        return stringValues.map((v) => {
          const option = optionsMap.get(v);
          return option ? Number(option.value) : Number(v);
        });
      } else if (typeof firstOption.value === "string") {
        return stringValues.map((v) => {
          const option = optionsMap.get(v);
          return option ? String(option.value) : v;
        });
      }
    }
    return stringValues;
  }

  const firstValue = stringValues[0];
  const option = optionsMap.get(firstValue);
  if (option) {
    return option.value;
  }
  return firstValue;
};

export const useSelectValueHandler = (
  id: string,
  value: NullableSelectValue,
  options: Option[],
  eventHandler: EventHandler,
  selectMany: boolean = false,
) => {
  return useCallback(
    (newValue: string | string[]) => {
      const stringValues = Array.isArray(newValue) ? newValue : [newValue];
      const convertedValue = convertValuesToOriginalType(stringValues, value, options, selectMany);
      eventHandler("OnChange", id, [convertedValue]);
    },
    [id, value, options, eventHandler, selectMany],
  );
};
