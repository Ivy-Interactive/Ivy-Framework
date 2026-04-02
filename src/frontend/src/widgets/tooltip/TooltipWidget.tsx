import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import React from "react";

interface TooltipWidgetProps {
  id: string;
  slots?: {
    Trigger?: React.ReactNode[];
    Content?: React.ReactNode[];
  };
}

function extractText(nodes: React.ReactNode[]): string | undefined {
  const parts: string[] = [];
  for (const node of nodes) {
    if (typeof node === "string") parts.push(node);
    else if (typeof node === "number") parts.push(String(node));
  }
  return parts.length > 0 ? parts.join("") : undefined;
}

export const TooltipWidget: React.FC<TooltipWidgetProps> = ({ slots }) => {
  if (!slots?.Trigger || !slots?.Content) {
    return (
      <div className="text-red-500">Error: Tooltip requires both Trigger and Content slots.</div>
    );
  }

  const ariaLabel = extractText(slots.Content);

  // asChild + span: we need a single DOM node that receives ref/handlers (slot widgets like ButtonWidget don't forward ref).
  // A span wrapper avoids TooltipTrigger's default <button> so we don't get invalid button-in-button.
  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger asChild>
          <span style={{ display: "inline-block" }} aria-label={ariaLabel}>
            {slots.Trigger}
          </span>
        </TooltipTrigger>
        <TooltipContent className="bg-popover text-popover-foreground shadow-md">
          {slots.Content}
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
};
