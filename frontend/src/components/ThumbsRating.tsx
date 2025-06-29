import { ThumbsUp, ThumbsDown } from 'lucide-react';
import { cn } from "@/lib/utils";
import { inputStyles } from '@/lib/styles';
import { InvalidIcon } from "@/components/InvalidIcon";

export enum ThumbsEnum {
  Down = 1,
  Up = 2,
  None = 0
}

interface ThumbsRatingProps {
  value?: number;
  onRate?: (rating: number) => void;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
  disabled?: boolean;
  invalid?: string;
}

const ThumbsRating = ({
  value = ThumbsEnum.None,
  onRate,
  size = 'sm',
  className,
  disabled = false,
  invalid,
}: ThumbsRatingProps) => {
  const iconSizes = {
    sm: 16,
    md: 24,
    lg: 32
  };

  const handleClick = (rating: number) => {
    if (disabled) return;
    onRate?.(value === rating ? ThumbsEnum.None : rating);
  };

  return (
    <div className="relative">
      <div className={cn("flex gap-1 items-center", disabled && "opacity-50", className)}>
        <button
          onClick={() => handleClick(ThumbsEnum.Down)}
          disabled={disabled}
          className={cn(
            "p-2 rounded-full transition-all",
            "hover:bg-red-100 dark:hover:bg-red-900/30",
            "focus-visible:outline-none focus-visible:ring-2",
            "focus-visible:ring-ring focus-visible:ring-offset-2",
            value === ThumbsEnum.Down && "bg-red-100 dark:bg-red-900/30",
            disabled && "cursor-not-allowed hover:bg-transparent dark:hover:bg-transparent",
            invalid && inputStyles.invalid
          )}
        >
          <ThumbsDown
            size={iconSizes[size]}
            className={cn(
              "transition-colors",
              value === ThumbsEnum.Down ? "text-red-600 dark:text-red-400" : "text-muted-foreground"
            )}
          />
        </button>

        <button
          onClick={() => handleClick(ThumbsEnum.Up)}
          disabled={disabled}
          className={cn(
            "p-2 rounded-full transition-all",
            "hover:bg-green-100 dark:hover:bg-green-900/30",
            "focus-visible:outline-none focus-visible:ring-2",
            "focus-visible:ring-ring focus-visible:ring-offset-2",
            value === ThumbsEnum.Up && "bg-green-100 dark:bg-green-900/30",
            disabled && "cursor-not-allowed hover:bg-transparent dark:hover:bg-transparent",
            invalid && inputStyles.invalid
          )}
        >
          <ThumbsUp
            size={iconSizes[size]}
            className={cn(
              "transition-colors",
              value === ThumbsEnum.Up ? "text-green-600 dark:text-green-400" : "text-muted-foreground"
            )}
          />
        </button>
      </div>
      {invalid && (
        <div className="absolute right-0 top-1/2 -translate-y-1/2 h-4 w-4">
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
};

export default ThumbsRating;