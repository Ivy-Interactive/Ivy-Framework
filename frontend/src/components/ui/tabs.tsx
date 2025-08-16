import * as React from 'react';
import * as TabsPrimitive from '@radix-ui/react-tabs';

import { cn } from '@/lib/utils';

// Custom Tabs context for non-Radix implementation
interface TabsContextType {
  value?: string;
  onValueChange?: (value: string) => void;
}

const TabsContext = React.createContext<TabsContextType>({});

// Custom Tabs root component that can work with or without Radix
interface TabsProps {
  value?: string;
  onValueChange?: (value: string) => void;
  className?: string;
  children: React.ReactNode;
  useRadix?: boolean;
}

const Tabs = React.forwardRef<HTMLDivElement, TabsProps>(
  (
    { value, onValueChange, className, children, useRadix = false, ...props },
    ref
  ) => {
    if (useRadix) {
      return (
        <TabsPrimitive.Root
          value={value}
          onValueChange={onValueChange}
          className={className}
          {...props}
        >
          {children}
        </TabsPrimitive.Root>
      );
    }

    return (
      <TabsContext.Provider value={{ value, onValueChange }}>
        <div ref={ref} className={className} {...props}>
          {children}
        </div>
      </TabsContext.Provider>
    );
  }
);
Tabs.displayName = 'Tabs';

// Custom TabsList that works independently
const TabsList = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement> & { useRadix?: boolean }
>(({ className, useRadix = false, ...props }, ref) => {
  if (useRadix) {
    return (
      <TabsPrimitive.List
        ref={ref as React.Ref<React.ElementRef<typeof TabsPrimitive.List>>}
        className={cn(
          'inline-flex h-9 items-center justify-center rounded-lg bg-muted p-1 text-muted-foreground',
          className
        )}
        role="tablist"
        {...props}
      />
    );
  }

  return (
    <div
      ref={ref}
      className={cn(
        'inline-flex h-9 items-center justify-center rounded-lg bg-muted p-1 text-muted-foreground',
        className
      )}
      role="tablist"
      {...props}
    />
  );
});
TabsList.displayName = 'TabsList';

// Custom TabsTrigger that works independently
interface TabsTriggerProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  value: string;
  useRadix?: boolean;
}

const TabsTrigger = React.forwardRef<HTMLButtonElement, TabsTriggerProps>(
  ({ className, value, useRadix = false, onClick, ...props }, ref) => {
    const context = React.useContext(TabsContext);
    const isActive = context.value === value;

    if (useRadix) {
      return (
        <TabsPrimitive.Trigger
          ref={ref as React.Ref<React.ElementRef<typeof TabsPrimitive.Trigger>>}
          value={value}
          className={cn(
            'inline-flex items-center justify-center whitespace-nowrap rounded-md px-3 py-1 text-sm font-medium ring-offset-background transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow',
            className
          )}
          role="tab"
          {...props}
        />
      );
    }

    const handleClick = (e: React.MouseEvent<HTMLButtonElement>) => {
      context.onValueChange?.(value);
      onClick?.(e);
    };

    return (
      <button
        ref={ref}
        type="button"
        role="tab"
        aria-selected={isActive}
        data-state={isActive ? 'active' : 'inactive'}
        className={cn(
          'inline-flex items-center justify-center whitespace-nowrap rounded-md px-3 py-1 text-sm font-medium ring-offset-background transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
          isActive && 'bg-background text-foreground shadow',
          className
        )}
        onClick={handleClick}
        {...props}
      />
    );
  }
);
TabsTrigger.displayName = 'TabsTrigger';

// Custom TabsContent that works independently
interface TabsContentProps extends React.HTMLAttributes<HTMLDivElement> {
  value: string;
  useRadix?: boolean;
}

const TabsContent = React.forwardRef<HTMLDivElement, TabsContentProps>(
  ({ className, value, useRadix = false, children, ...props }, ref) => {
    const context = React.useContext(TabsContext);
    const isActive = context.value === value;

    if (useRadix) {
      return (
        <TabsPrimitive.Content
          ref={ref as React.Ref<React.ElementRef<typeof TabsPrimitive.Content>>}
          value={value}
          className={cn(
            'mt-2 ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
            className
          )}
          {...props}
        >
          {children}
        </TabsPrimitive.Content>
      );
    }

    if (!isActive) {
      return null;
    }

    return (
      <div
        ref={ref}
        role="tabpanel"
        aria-labelledby={`tab-${value}`}
        className={cn(
          'mt-2 ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
          className
        )}
        {...props}
      >
        {children}
      </div>
    );
  }
);
TabsContent.displayName = 'TabsContent';

export { Tabs, TabsList, TabsTrigger, TabsContent };
