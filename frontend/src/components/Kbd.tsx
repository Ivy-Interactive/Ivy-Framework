import React from 'react';

export function Kbd({ children }: { children: React.ReactNode }) {
  return (
    <kbd className="rounded text-foreground border border-border px-1 py-0.5">
      {children}
    </kbd>
  );
}
