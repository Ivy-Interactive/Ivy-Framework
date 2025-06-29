'use client';

import { useState } from 'react';
import { Star } from 'lucide-react';
import { motion } from 'framer-motion';
import { cn } from "@/lib/utils";
import { inputStyles } from '@/lib/styles';
import { InvalidIcon } from "@/components/InvalidIcon";

interface StarRatingProps {
  totalStars?: number;
  value: number;
  onRate?: (rating: number) => void;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
  disabled?: boolean;
  invalid?: string;
}

export function StarRating({ 
  totalStars = 5, 
  value = 0,
  onRate,
  size = 'md',
  className,
  disabled = false,
  invalid,
}: StarRatingProps) {
  const [hover, setHover] = useState(0);

  const handleRating = (star: number) => {
    if (disabled) return;
    onRate?.(value === star ? 0 : star);
  };

  const starSizes = {
    sm: 'h-4 w-4',
    md: 'h-6 w-6',
    lg: 'h-8 w-8'
  };

  return (
    <div className="relative">
      <div className={cn(
        "flex items-center gap-1",
        disabled && "opacity-50",
        className
      )}>
        {Array.from({ length: totalStars }, (_, index) => index + 1).map((star) => (
          <motion.button
            key={star}
            type="button"
            className={cn(
              "relative focus-visible:outline-none focus-visible:ring-2",
              "focus-visible:ring-ring focus-visible:ring-offset-2",
              disabled && "cursor-not-allowed",
              invalid && inputStyles.invalid
            )}
            onClick={() => handleRating(star)}
            onMouseEnter={() => !disabled && setHover(star)}
            onMouseLeave={() => !disabled && setHover(0)}
            whileHover={!disabled ? { scale: 1.3, rotate: -10 } : undefined}
            whileTap={!disabled ? { scale: 0.9, rotate: 15 } : undefined}
            disabled={disabled}
          >
            <motion.div
              className={cn(
                "transition-colors duration-300",
                (hover || value) >= star 
                  ? "text-primary" 
                  : "text-muted"
              )}
              initial={{ scale: 1 }}
              animate={{
                scale: (hover || value) >= star ? 1.2 : 1,
              }}
              transition={{ 
                duration: 0.3,
                ease: "easeOut"
              }}
            >
              <Star 
                className={cn(
                  starSizes[size],
                  "fill-current stroke-[1.5px]"
                )} 
              />
            </motion.div>
          </motion.button>
        ))}
      </div>
      {invalid && (
        <div className="absolute right-0 top-1/2 -translate-y-1/2 h-4 w-4">
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
}
