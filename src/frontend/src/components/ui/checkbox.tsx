import { logger } from "@/lib/logger";
import { cn } from "@/lib/utils";
import * as CheckboxPrimitive from "@radix-ui/react-checkbox";
import { Check, Minus } from "lucide-react";
import * as React from "react";
import { Densities } from "@/types/density";
export type NullableBoolean = boolean | null | undefined;

const getSizeClasses = (density?: Densities): string => {
  switch (density) {
    case Densities.Small:
      return "size-3";
    case Densities.Large:
      return "size-5";
    default:
      return "size-4";
  }
};

type AppCheckboxProps = {
  id: string;
  checked: NullableBoolean;
  onCheckedChange: (checked: boolean | null) => void;
  disabled?: boolean;
  nullable?: boolean;
  autoFocus?: boolean;
  className?: string;
  density?: Densities;
};

const Checkbox = React.forwardRef<
  React.ElementRef<typeof CheckboxPrimitive.Root>,
  AppCheckboxProps
>(
  (
    {
      id,
      checked,
      onCheckedChange,
      disabled,
      nullable = false,
      className = "",
      density = Densities.Medium,
      ...props
    },
    ref,
  ) => {
    // Map checked value helper for safe boolean/null states
    const normalizedChecked = React.useMemo(() => {
      if (checked === undefined || checked === null) return nullable ? null : false;
      if (checked === true || (checked as any) === 1) return true;
      if (checked === false || (checked as any) === 0) return false;
      return !!checked;
    }, [checked, nullable]);

    const uiChecked =
      nullable && normalizedChecked === null ? "indeterminate" : !!normalizedChecked;

    // Cycle: null -> true -> false -> null (if nullable)
    const handleCheckedChange = (next: boolean) => {
      logger.debug("Checkbox clicked, next:", next, "current checked:", normalizedChecked);
      if (nullable) {
        if (normalizedChecked === null) onCheckedChange(true);
        else if (normalizedChecked === true) onCheckedChange(false);
        else onCheckedChange(null);
      } else {
        onCheckedChange(next);
      }
    };

    const isInvalid = className?.includes("border-destructive") || className?.includes("bg-red-50");
    const baseClass = `peer ${getSizeClasses(density)} shrink-0 rounded-checkbox border border-border shadow transition-colors hover:bg-accent/50 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer disabled:cursor-not-allowed disabled:opacity-50 data-[state=checked]:bg-primary data-[state=checked]:text-primary-foreground data-[state=checked]:border-primary data-[state=checked]:hover:bg-primary/90 dark:border-white/10`;
    const finalClass = isInvalid
      ? baseClass
          .replace("data-[state=checked]:bg-primary", "data-[state=checked]:bg-destructive")
          .replace("data-[state=checked]:border-primary", "data-[state=checked]:border-destructive")
          .replace(
            "data-[state=checked]:text-primary-foreground",
            "data-[state=checked]:text-destructive-foreground",
          )
          .replace(
            "data-[state=checked]:hover:bg-primary/90",
            "data-[state=checked]:hover:bg-destructive/90",
          )
          .replace("focus-visible:ring-ring", "focus-visible:ring-destructive")
          .replace("border-border", "border-destructive")
          .replace("dark:border-white/10", "dark:border-destructive")
      : baseClass;

    return (
      <CheckboxPrimitive.Root
        ref={ref}
        id={id}
        checked={uiChecked}
        onCheckedChange={handleCheckedChange}
        disabled={disabled}
        className={cn(finalClass, className)}
        {...props}
      >
        <CheckboxPrimitive.Indicator
          className={cn("flex items-center justify-center text-current")}
        >
          {uiChecked === "indeterminate" ? (
            <Minus className={getSizeClasses(density)} />
          ) : (
            <Check className={getSizeClasses(density)} />
          )}
        </CheckboxPrimitive.Indicator>
      </CheckboxPrimitive.Root>
    );
  },
);
Checkbox.displayName = "AppCheckbox";

export { Checkbox };
