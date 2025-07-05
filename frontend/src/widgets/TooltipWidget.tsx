import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import React from "react";

interface TooltipWidgetProps {
  id: string;
  slots?: {
    Trigger?: React.ReactNode[];
    Content?: React.ReactNode[];
  };
}

export const TooltipWidget: React.FC<TooltipWidgetProps> = ({ slots }) => {
  if (!slots?.Trigger || !slots?.Content) {
    return (
      <div className="text-red-500">
        Error: Tooltip requires both Trigger and Content slots.
      </div>
    );
  }
  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger>{slots.Trigger}</TooltipTrigger>
        <TooltipContent className="bg-popover text-popover-foreground shadow-md">
          {slots.Content}
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
};
