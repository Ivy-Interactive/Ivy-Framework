import * as React from 'react';
import { X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { InvalidIcon } from '@/components/InvalidIcon';
import { dateTimeInputIconVariants } from '@/components/ui/input/date-time-input-variants';
import { Scales } from '@/types/scale';

interface ClearAndInvalidIconsProps {
  showClear?: boolean;
  invalid?: string;
  scale?: Scales;
  onClear: (e?: React.MouseEvent) => void;
}

export const ClearAndInvalidIcons: React.FC<ClearAndInvalidIconsProps> = ({
  showClear = false,
  invalid,
  scale = Scales.Medium,
  onClear,
}) => {
  if (!showClear && !invalid) {
    return null;
  }

  return (
    <div className="absolute right-2 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1">
      {showClear && (
        <button
          type="button"
          tabIndex={-1}
          aria-label="Clear"
          onClick={onClear}
          className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
        >
          <X
            className={cn(
              dateTimeInputIconVariants({ scale }),
              'text-muted-foreground hover:text-foreground'
            )}
          />
        </button>
      )}
      {/* Invalid icon - rightmost */}
      {invalid && (
        <InvalidIcon message={invalid} className="pointer-events-auto" />
      )}
    </div>
  );
};
