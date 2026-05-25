import type { Activity } from "./types";

export function computeMaxCount(data: Pick<Activity, "count">[]): number {
  return Math.max(0, ...data.map((d) => d.count ?? 0));
}

export function getLevel(count: Activity["count"] | null, maxCount: number): number {
  const normalizedCount = count ?? 0;
  if (normalizedCount === 0 || maxCount === 0) return 0;
  if (normalizedCount <= maxCount * 0.25) return 1;
  if (normalizedCount <= maxCount * 0.5) return 2;
  if (normalizedCount <= maxCount * 0.75) return 3;
  return 4;
}
