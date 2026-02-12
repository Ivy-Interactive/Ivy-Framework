import * as React from 'react';

import { cn } from '@/lib/utils';

import './textarea.css';

const Textarea = React.forwardRef<
  HTMLTextAreaElement,
  React.ComponentProps<'textarea'>
>(({ className, ...props }, ref) => {
  return (
    <div className="ivy-textarea-wrapper relative w-full h-full">
      <textarea
        className={cn(
          'ivy-textarea flex min-h-[60px] w-full rounded-field border border-input px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 dark:border-white/10',
          className
        )}
        ref={ref}
        {...props}
      />
    </div>
  );
});
Textarea.displayName = 'Textarea';

export { Textarea };
