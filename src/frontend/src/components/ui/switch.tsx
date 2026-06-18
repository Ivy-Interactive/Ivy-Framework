import Icon from "@/components/Icon";
import * as React from "react";
import * as SwitchPrimitives from "@radix-ui/react-switch";
import type { VariantProps } from "class-variance-authority";

import { cn } from "@/lib/utils";
import { switchVariant, switchThumbVariant } from "./input/switch-variant";

const Switch = React.forwardRef<
  React.ElementRef<typeof SwitchPrimitives.Root>,
  React.ComponentPropsWithoutRef<typeof SwitchPrimitives.Root> &
    VariantProps<typeof switchVariant> & {
      icon?: string;
    }
>(({ className, density, icon, ...props }, ref) => {
  const isInvalid = className?.includes("border-destructive") || className?.includes("bg-red-50");
  const baseClass = switchVariant({ density });
  const finalClass = isInvalid
    ? baseClass
        .replace("data-[state=checked]:bg-primary", "data-[state=checked]:bg-destructive")
        .replace(
          "hover:data-[state=checked]:bg-primary/90",
          "hover:data-[state=checked]:bg-destructive/90",
        )
        .replace("focus-visible:ring-ring", "focus-visible:ring-destructive")
    : baseClass;
  return (
    <SwitchPrimitives.Root className={cn(finalClass, className)} {...props} ref={ref}>
      <SwitchPrimitives.Thumb className={cn(switchThumbVariant({ density }))}>
        {icon && (
          <div className="flex items-center justify-center w-full h-full">
            <Icon name={icon} className="w-[12px] h-[12px]" />
          </div>
        )}
      </SwitchPrimitives.Thumb>
    </SwitchPrimitives.Root>
  );
});
Switch.displayName = SwitchPrimitives.Root.displayName;

export { Switch };
