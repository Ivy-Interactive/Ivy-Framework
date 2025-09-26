import { ThumbsUp, ThumbsDown } from 'lucide-react';
import { cn } from '@/lib/utils';
import { InvalidIcon } from '@/components/InvalidIcon';
import { ThumbsEnum } from './types';

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
    lg: 32,
  };

  const handleClick = (rating: number) => {
    if (disabled) return;
    onRate?.(value === rating ? ThumbsEnum.None : rating);
  };

  return (
    <div className="flex items-center gap-2">
      <div
        className={cn(
          'flex gap-1 items-center',
          disabled && 'opacity-50',
          className
        )}
      >
        <button
          onClick={() => handleClick(ThumbsEnum.Down)}
          disabled={disabled}
          className={cn(
            'p-2 rounded-full transition-all cursor-pointer',
            'hover:bg-destructive/20',
            'focus-visible:outline-none focus-visible:ring-2',
            'focus-visible:ring-ring focus-visible:ring-offset-2',
            value === ThumbsEnum.Down &&
              (invalid
                ? 'bg-destructive/20 border-destructive text-destructive'
                : 'bg-destructive text-destructive-foreground'),
            disabled && 'cursor-not-allowed hover:bg-transparent'
          )}
        >
          <ThumbsDown
            size={iconSizes[size]}
            className={cn(
              'transition-colors',
              value === ThumbsEnum.Down
                ? invalid
                  ? 'text-destructive'
                  : 'text-destructive-foreground'
                : 'text-muted-foreground'
            )}
          />
        </button>

        <button
          onClick={() => handleClick(ThumbsEnum.Up)}
          disabled={disabled}
          className={cn(
            'p-2 rounded-full transition-all cursor-pointer',
            'hover:bg-primary/20',
            'focus-visible:outline-none focus-visible:ring-2',
            'focus-visible:ring-ring focus-visible:ring-offset-2',
            value === ThumbsEnum.Up &&
              (invalid
                ? 'bg-primary/20 border-primary text-primary'
                : 'bg-primary text-primary-foreground'),
            disabled && 'cursor-not-allowed hover:bg-transparent'
          )}
        >
          <ThumbsUp
            size={iconSizes[size]}
            className={cn(
              'transition-colors',
              value === ThumbsEnum.Up
                ? invalid
                  ? 'text-muted-foreground'
                  : 'text-primary-foreground'
                : 'text-muted-foreground'
            )}
          />
        </button>
      </div>
      {invalid && <InvalidIcon message={invalid} />}
    </div>
  );
};

export { ThumbsRating };
