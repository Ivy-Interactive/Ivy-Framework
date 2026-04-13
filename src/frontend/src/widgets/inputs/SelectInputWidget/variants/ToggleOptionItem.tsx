import React from "react";
import { ToggleGroupItem } from "@/components/ui/toggle";
import { Tooltip, TooltipProvider, TooltipTrigger, TooltipContent } from "@/components/ui/tooltip";
import Icon from "@/components/Icon";
import { cn } from "@/lib/utils";
import { inputStyles } from "@/lib/styles";
import { Densities } from "@/types/density";
import { Option } from "../../select-types";

interface ToggleOptionItemProps {
  option: Option;
  isSelected: boolean;
  invalid?: string;
  density?: Densities;
  disabled?: boolean;
}

export const ToggleOptionItem: React.FC<ToggleOptionItemProps> = ({
  option,
  isSelected,
  invalid,
  density = Densities.Medium,
  disabled,
}) => {
  const isInvalid = !!invalid && isSelected;

  const sizeClasses = {
    Small: "px-1 py-1 text-xs",
    Medium: "px-3 py-2 text-sm",
    Large: "px-5 py-3 text-base",
  };

  const iconClasses = {
    Small: "h-3 w-3",
    Medium: "h-4 w-4",
    Large: "h-5 w-5",
  };

  const toggleItem = (
    <ToggleGroupItem
      key={option.value}
      value={option.value.toString()}
      aria-label={option.label || option.value.toString()}
      title={option.label}
      className={cn(
        "hover:text-foreground gap-2",
        sizeClasses[density],
        isInvalid
          ? cn(inputStyles.invalidInput, "bg-destructive/10 border-destructive text-destructive")
          : isSelected
            ? "data-[state=on]:bg-primary data-[state=on]:border-primary data-[state=on]:text-primary-foreground"
            : undefined,
      )}
      disabled={disabled}
    >
      {option.icon && (
        <Icon name={option.icon} className={cn(iconClasses[density], !option.label && "mx-auto")} />
      )}
      {option.description ? (
        <div className="flex flex-col items-center">
          <span>{option.label}</span>
          <span className="text-xs text-muted-foreground mt-0.5 font-normal">
            {option.description}
          </span>
        </div>
      ) : (
        option.label
      )}
    </ToggleGroupItem>
  );

  if (isInvalid) {
    return (
      <TooltipProvider key={option.value}>
        <Tooltip>
          <TooltipTrigger asChild>{toggleItem}</TooltipTrigger>
          <TooltipContent>
            <div className="max-w-xs sm:max-w-sm">{invalid}</div>
          </TooltipContent>
        </Tooltip>
      </TooltipProvider>
    );
  }

  if (option.tooltip) {
    return (
      <TooltipProvider key={option.value}>
        <Tooltip>
          <TooltipTrigger asChild>{toggleItem}</TooltipTrigger>
          <TooltipContent>{option.tooltip}</TooltipContent>
        </Tooltip>
      </TooltipProvider>
    );
  }

  return toggleItem;
};
