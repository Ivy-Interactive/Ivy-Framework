import { InfoIcon } from 'lucide-react';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from './ui/tooltip';

export const InvalidIcon: React.FC<{ message: string; className?: string }> = ({
  message,
  className,
}) => {
  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger className={className}>
          <InfoIcon className="h-4 w-4 text-red-900 hover:text-red-400" />
        </TooltipTrigger>
        <TooltipContent className="bg-popover text-popover-foreground shadow-md">
          <div className="max-w-60">{message}</div>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
};
