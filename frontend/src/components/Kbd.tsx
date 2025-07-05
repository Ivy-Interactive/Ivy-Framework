import React from "react";

export function Kbd({ children }: { children: React.ReactNode }) {
  return (
    <kbd className="rounded w-min bg-neutral-100 px-1 py-0.5 text-xs font-medium text-neutral-700 dark:bg-neutral-800 dark:text-neutral-200 border border-neutral-300 dark:border-neutral-700 text-nowrap">
      {children}
    </kbd>
  );
}
