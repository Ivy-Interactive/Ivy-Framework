import { useMemo } from "react";
import { Loader2 } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";

interface LoadingProps {
  type?: "Spinner" | "Skeleton";
}

interface SkeletonLine {
  widthPercent: number;
  heightPx: number;
}

const SKELETON_LINE_COUNT_MIN = 3;
const SKELETON_LINE_COUNT_MAX = 5;
const SKELETON_LINE_MIN_WIDTH = 40;
const SKELETON_LINE_MAX_WIDTH = 100;
const SKELETON_LINE_MIN_HEIGHT = 10;
const SKELETON_LINE_MAX_HEIGHT = 14;

function randomBetween(min: number, max: number): number {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function generateSkeletonLayout(): SkeletonLine[] {
  const count = randomBetween(SKELETON_LINE_COUNT_MIN, SKELETON_LINE_COUNT_MAX);
  const lines: SkeletonLine[] = [];
  for (let i = 0; i < count; i++) {
    lines.push({
      widthPercent: randomBetween(SKELETON_LINE_MIN_WIDTH, SKELETON_LINE_MAX_WIDTH),
      heightPx: randomBetween(SKELETON_LINE_MIN_HEIGHT, SKELETON_LINE_MAX_HEIGHT),
    });
  }
  return lines;
}

export function Loading({ type = "Spinner" }: LoadingProps) {
  const skeletonLines = useMemo<SkeletonLine[]>(generateSkeletonLayout, []);
  const titleBarWidth = useMemo(() => randomBetween(30, 55), []);

  if (type === "Skeleton") {
    return (
      <div className="flex flex-col gap-2 w-full max-w-sm" role="status" aria-label="Loading">
        <Skeleton className="h-5 rounded-md bg-muted" style={{ width: `${titleBarWidth}%` }} />
        {skeletonLines.map((line, index) => (
          <Skeleton
            key={`loading-skeleton-line-${index}`}
            className="rounded bg-muted"
            style={{
              width: `${line.widthPercent}%`,
              height: `${line.heightPx}px`,
            }}
          />
        ))}
      </div>
    );
  }

  return (
    <div
      className="flex items-center gap-2 text-muted-foreground"
      role="status"
      aria-label="Loading"
    >
      <Loader2 className="animate-spin size-4" />
      <p className="text-sm">Loading...</p>
    </div>
  );
}
