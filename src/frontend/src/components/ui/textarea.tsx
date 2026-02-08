import * as React from 'react';

import { cn } from '@/lib/utils';

// Hide native resizer; grip is inside the component. Hidden when textarea has resize-none.
const resizerStyle =
  '.ivy-textarea::-webkit-resizer{-webkit-appearance:none;appearance:none;background:transparent}.ivy-textarea-wrapper .ivy-textarea-grip{position:absolute;bottom:-2px;right:-4px;width:14px;height:14px;pointer-events:none;z-index:10;color:var(--muted-foreground);display:flex;align-items:flex-end;justify-content:flex-end}.ivy-textarea-wrapper:has(.resize-none) .ivy-textarea-grip{display:none}';

function injectResizerStyle() {
  if (
    typeof document !== 'undefined' &&
    !document.getElementById('ivy-textarea-resizer-style')
  ) {
    const style = document.createElement('style');
    style.id = 'ivy-textarea-resizer-style';
    style.textContent = resizerStyle;
    document.head.appendChild(style);
  }
}

const Textarea = React.forwardRef<
  HTMLTextAreaElement,
  React.ComponentProps<'textarea'>
>(({ className, ...props }, ref) => {
  React.useLayoutEffect(() => {
    injectResizerStyle();
  }, []);
  return (
    <div className="ivy-textarea-wrapper relative w-full h-full">
      <textarea
        className={cn(
          'ivy-textarea flex min-h-[60px] w-full rounded-md border border-input px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 dark:border-white/10',
          className
        )}
        ref={ref}
        {...props}
      />
      <div className="ivy-textarea-grip" aria-hidden>
        <svg
          viewBox="0 0 20 20"
          fill="none"
          stroke="currentColor"
          strokeWidth={1}
          strokeLinecap="round"
          className={cn('h-3.5 w-3.5 shrink-0')}
        >
          <path d="M 4 20 L 12 12 M 8 20 L 16 12 M 12 20 L 20 12" />
        </svg>
      </div>
    </div>
  );
});
Textarea.displayName = 'Textarea';

export { Textarea };
