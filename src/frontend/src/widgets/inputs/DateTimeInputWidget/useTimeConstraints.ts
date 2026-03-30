import { useMemo, useCallback } from "react";
import { format } from "date-fns";
import {
  parseTimeSpanStepToSeconds,
  parseLocalTimeToSeconds,
  snapLocalTimeSeconds,
  formatSecondsToHms,
} from "./timeStepSnap";

export function useTimeConstraints(min?: string, max?: string, step?: string) {
  const timeStepSeconds = useMemo(() => parseTimeSpanStepToSeconds(step), [step]);

  const timeMin = useMemo(() => {
    if (!min) return undefined;
    try {
      const d = new Date(min);
      if (!isNaN(d.getTime())) return format(d, "HH:mm:ss");
    } catch {
      /* ignore */
    }
    return undefined;
  }, [min]);

  const timeMax = useMemo(() => {
    if (!max) return undefined;
    try {
      const d = new Date(max);
      if (!isNaN(d.getTime())) return format(d, "HH:mm:ss");
    } catch {
      /* ignore */
    }
    return undefined;
  }, [max]);

  const getSnappedTime = useCallback(
    (timeValue: string) => {
      const trimmed = timeValue.trim();
      const parsed = parseLocalTimeToSeconds(trimmed);
      if (parsed === null) return trimmed;

      const minSec = timeMin ? (parseLocalTimeToSeconds(timeMin) ?? undefined) : undefined;
      const maxSec = timeMax ? (parseLocalTimeToSeconds(timeMax) ?? undefined) : undefined;
      const snapped = snapLocalTimeSeconds(parsed, timeStepSeconds, minSec, maxSec);
      return formatSecondsToHms(snapped);
    },
    [timeStepSeconds, timeMin, timeMax],
  );

  return {
    timeStepSeconds,
    timeMin,
    timeMax,
    getSnappedTime,
  };
}
