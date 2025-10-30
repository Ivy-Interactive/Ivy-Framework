import React from 'react';
import { getColor } from '@/lib/styles';

interface SparklesWidgetProps {
  id?: string;
  text?: string | null;
  color?: string | null;
  size?: 'Small' | 'Medium' | 'Large';
}

export function SparklesWidget({
  text,
  color,
  size = 'Medium',
}: SparklesWidgetProps) {
  const dim =
    size === 'Small' ? 'h-4 w-4' : size === 'Large' ? 'h-7 w-7' : 'h-5 w-5';
  const colorToken = color ?? undefined;
  const styles: React.CSSProperties = {
    ...getColor(colorToken, 'color', 'foreground'),
  };
  return (
    <span className="inline-flex items-center gap-2" style={styles}>
      <svg
        className={dim}
        viewBox="0 0 20 20"
        aria-hidden="true"
        fill="currentColor"
      >
        <path d="M10 1l2 5 5 2-5 2-2 5-2-5-5-2 5-2 2-5z" />
      </svg>
      {text ? <span>{text}</span> : null}
    </span>
  );
}
