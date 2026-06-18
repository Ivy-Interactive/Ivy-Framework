import { InfoIcon } from "lucide-react";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "./ui/tooltip";
import { cn } from "@/lib/utils";

export const InvalidIcon: React.FC<{
  message: string;
  className?: string;
  iconClassName?: string;
}> = ({ message, className, iconClassName }) => {
  return (
    <TooltipProvider>
      <Tooltip className="contents">
        <TooltipTrigger asChild>
          <button
            type="button"
            tabIndex={-1}
            data-invalid-icon="true"
            className={cn(
              "inline-flex shrink-0 cursor-pointer items-center justify-center border-0 bg-transparent p-0 shadow-none outline-none leading-none",
              "pointer-events-auto focus-visible:ring-1 focus-visible:ring-ring",
              className,
            )}
          >
            <InfoIcon
              className={cn(
                "block shrink-0 text-red-900 transition-colors duration-200 hover:text-red-400",
                iconClassName ?? "size-4",
              )}
            />
          </button>
        </TooltipTrigger>
        <TooltipContent className="bg-popover text-popover-foreground shadow-md">
          <div className="max-w-60">{message}</div>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
};
