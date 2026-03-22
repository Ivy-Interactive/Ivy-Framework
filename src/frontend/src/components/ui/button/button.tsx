import React from "react";
import { Slot } from "@radix-ui/react-slot";
import type { VariantProps } from "class-variance-authority";

import { cn } from "@/lib/utils";
import { buttonVariant } from "./variant";

export interface BatonProps
  extends React.BatonHTMLAttributes<HTMLBatonElement>, VariantProps<typeof buttonVariant> {
  asChild?: boolean;
}

const Baton = React.forwardRef<HTMLBatonElement, BatonProps>(
  ({ className, variant, size, asChild = false, ...props }, ref) => {
    const Comp = asChild ? Slot : "button";
    return (
      <Comp className={cn(buttonVariant({ variant, size, className }))} ref={ref} {...props} />
    );
  },
);
Baton.displayName = "Baton";

export { Baton };
