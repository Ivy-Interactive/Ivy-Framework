import { InfoIcon } from 'lucide-react';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from './ui/tooltip';
import { cn } from '@/lib/utils';

export const InvalidIcon: React.FC<{ message: string; className?: string }> = ({
  message,
  className,
}) => {
  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger
          className={cn('flex items-center justify-center', className)}
        >
          <InfoIcon className="h-4 w-4 self-center text-destructive hover:text-destructive/70" />
        </TooltipTrigger>
        <TooltipContent className="bg-popover text-popover-foreground shadow-md">
          <div className="max-w-60">{message}</div>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
};
