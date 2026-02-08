import * as React from 'react';
import { Menu } from 'lucide-react';

import { cn } from '@/lib/utils';

const resizerStyle =
  'textarea.ivy-textarea::-webkit-resizer{-webkit-appearance:none!important;appearance:none!important;background:transparent!important}.ivy-textarea-wrapper{overflow:hidden;border-radius:0.375rem}.ivy-textarea-wrapper .ivy-textarea-grip{position:absolute;bottom:0;right:0;width:14px;height:14px;pointer-events:none;z-index:10;color:var(--muted-foreground);display:flex;align-items:flex-end;justify-content:flex-end}.ivy-textarea-wrapper:has(.resize-none) .ivy-textarea-grip{display:none}';

function injectResizerStyle() {
  if (typeof document === 'undefined') return;
  let el = document.getElementById('ivy-textarea-resizer-style');
  if (!el) {
    el = document.createElement('style');
    el.id = 'ivy-textarea-resizer-style';
    document.head.appendChild(el);
  }
  el.textContent = resizerStyle;
}

const Textarea = React.forwardRef<
  HTMLTextAreaElement,
  React.ComponentProps<'textarea'>
>(({ className, ...props }, ref) => {
  React.useLayoutEffect(() => injectResizerStyle(), []);
  return (
    <div className="ivy-textarea-wrapper relative w-full h-full overflow-hidden rounded-md">
      <textarea
        className={cn(
          'ivy-textarea flex min-h-[60px] w-full rounded-md border border-input px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 dark:border-white/10',
          className
        )}
        ref={ref}
        {...props}
      />
      <div
        className="ivy-textarea-grip"
        aria-hidden
        style={{ position: 'absolute', bottom: -4, right: -4, zIndex: 10 }}
      >
        <Menu className="h-3.5 w-3.5 shrink-0 rotate-315" />
      </div>
    </div>
  );
});
Textarea.displayName = 'Textarea';

export { Textarea };
