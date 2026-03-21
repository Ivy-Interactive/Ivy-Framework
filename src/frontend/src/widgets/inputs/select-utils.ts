import { useCallback } from 'react';
import { EventHandler } from '@/components/event-handler';
import { NullableSelectValue, Option } from './select-types';

// Helper function to convert string values back to their original types
export const convertValuesToOriginalType = (
  stringValues: string[],
  originalValue: NullableSelectValue,
  options: Option[],
  selectMany: boolean = false
): NullableSelectValue => {
  if (stringValues.length === 0) {
    if (originalValue === null || originalValue === undefined) {
      if (selectMany) {
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

  if (originalValue instanceof Array) {
    if (originalValue.length > 0 && typeof originalValue[0] === 'number') {
      return stringValues.map(v => {
        const option = optionsMap.get(v);
        return option ? Number(option.value) : Number(v);
      });
    } else if (
      originalValue.length > 0 &&
      typeof originalValue[0] === 'string'
    ) {
      return stringValues.map(v => {
        const option = optionsMap.get(v);
        return option ? String(option.value) : v;
      });
    }
    return stringValues;
  }

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
  selectMany: boolean = false
) => {
  return useCallback(
    (newValue: string | string[]) => {
      const stringValues = Array.isArray(newValue) ? newValue : [newValue];
      const convertedValue = convertValuesToOriginalType(
        stringValues,
        value,
        options,
        selectMany
      );
      eventHandler('OnChange', id, [convertedValue]);
    },
    [id, value, options, eventHandler, selectMany]
  );
};
