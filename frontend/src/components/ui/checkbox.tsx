import { cn } from "@/lib/utils";
import * as CheckboxPrimitive from "@radix-ui/react-checkbox";
import { Check, Minus } from "lucide-react";
import * as React from "react";

export type NullableBoolean = boolean | null | undefined;

type AppCheckboxProps = {
  id: string;
  checked: NullableBoolean;
  onCheckedChange: (checked: boolean | null) => void;
  disabled?: boolean;
  nullable?: boolean;
  className?: string;
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
      ...props
    },
    ref
  ) => {
    // Map undefined to null when nullable, then null to 'indeterminate' for Radix
    const normalizedChecked = nullable && checked === undefined ? null : checked;
    const uiChecked =
      nullable && normalizedChecked === null ? "indeterminate" : !!normalizedChecked;

    // Cycle: null -> true -> false -> null (if nullable)
    const handleCheckedChange = (next: boolean) => {
      console.log("Checkbox clicked, next:", next, "current checked:", normalizedChecked);
      if (nullable) {
        if (normalizedChecked === null) onCheckedChange(true);
        else if (normalizedChecked === true) onCheckedChange(false);
        else onCheckedChange(null);
      } else {
        onCheckedChange(next);
      }
    };

    const baseClass =
      "peer h-4 w-4 shrink-0 rounded-sm border border-primary shadow focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 data-[state=checked]:bg-primary data-[state=checked]:text-primary-foreground";
    const finalClass = className?.includes("bg-red-50")
      ? baseClass.replace("data-[state=checked]:bg-primary", "")
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
            <Minus className="h-4 w-4" />
          ) : (
            <Check className="h-4 w-4" />
          )}
        </CheckboxPrimitive.Indicator>
      </CheckboxPrimitive.Root>
    );
  }
);
Checkbox.displayName = "AppCheckbox";

export { Checkbox };
