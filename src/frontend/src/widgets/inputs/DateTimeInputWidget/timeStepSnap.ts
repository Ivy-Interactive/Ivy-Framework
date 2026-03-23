/**
 * .NET System.Text.Json serializes TimeSpan as "HH:mm:ss" or "d.HH:mm:ss".
 */
export function parseTimeSpanStepToSeconds(step: string | undefined): number {
  if (!step) return 1;
  const trimmed = step.trim();
  let rest = trimmed;
  let daySeconds = 0;
  const dotMatch = /^(-?\d+)\.(.*)$/.exec(trimmed);
  if (dotMatch) {
    daySeconds = (parseInt(dotMatch[1], 10) || 0) * 86400;
    rest = dotMatch[2];
  }
  const parts = rest.split(":");
  if (parts.length >= 3) {
    const h = parseInt(parts[0], 10) || 0;
    const m = parseInt(parts[1], 10) || 0;
    const s = parseFloat(parts[2]) || 0;
    return daySeconds + h * 3600 + m * 60 + s;
  }
  return 1;
}

const HMS = /^(\d{1,2}):(\d{2})(?::(\d{2}))?$/;

/** Seconds since local midnight; null if empty/invalid. */
export function parseLocalTimeToSeconds(time: string): number | null {
  const t = time.trim();
  if (!t) return null;
  const m = HMS.exec(t);
  if (!m) return null;
  const h = parseInt(m[1], 10);
  const min = parseInt(m[2], 10);
  const sec = m[3] != null ? parseInt(m[3], 10) : 0;
  if (h > 23 || min > 59 || sec > 59) return null;
  return h * 3600 + min * 60 + sec;
}

export function formatSecondsToHms(total: number): string {
  const s = Math.round(total);
  const h = Math.floor(s / 3600);
  const m = Math.floor((s % 3600) / 60);
  const sec = s % 60;
  const pad = (n: number) => n.toString().padStart(2, "0");
  return `${pad(h)}:${pad(m)}:${pad(sec)}`;
}

const END_OF_DAY = 24 * 3600 - 1;

/**
 * Align to HTML time input stepping: grid is base + n*step (seconds), base = minSec ?? 0.
 */
export function snapLocalTimeSeconds(
  valueSec: number,
  stepSec: number,
  minSec?: number | null,
  maxSec?: number | null,
): number {
  if (stepSec <= 0 || !Number.isFinite(stepSec)) return valueSec;

  const base = minSec ?? 0;
  const effectiveMax = maxSec ?? END_OF_DAY;

  let v = valueSec;
  if (minSec != null) v = Math.max(v, minSec);
  if (maxSec != null) v = Math.min(v, maxSec);

  const offset = v - base;
  const n = Math.round(offset / stepSec);
  let snapped = base + n * stepSec;

  const lastOnGrid = base + Math.floor((effectiveMax - base) / stepSec) * stepSec;
  const firstOnGrid = base;

  snapped = Math.min(snapped, lastOnGrid);
  snapped = Math.max(snapped, firstOnGrid);

  if (minSec != null) snapped = Math.max(snapped, minSec);
  if (maxSec != null) snapped = Math.min(snapped, maxSec);

  return snapped;
}
