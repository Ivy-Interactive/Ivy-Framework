import * as React from 'react';
import { X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { InvalidIcon } from '@/components/InvalidIcon';
import { dateTimeInputIconVariants } from '@/components/ui/input/date-time-input-variants';
import { Scales } from '@/types/scale';

interface ClearButtonProps {
  onClick: (e?: React.MouseEvent) => void;
  scale?: Scales;
}

export const ClearButton: React.FC<ClearButtonProps> = ({
  onClick,
  scale = Scales.Medium,
}) => {
  return (
    <button
      type="button"
      tabIndex={-1}
      aria-label="Clear"
      onClick={onClick}
      className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
    >
      <X
        className={cn(
          dateTimeInputIconVariants({ scale }),
          'text-muted-foreground hover:text-foreground'
        )}
      />
    </button>
  );
};

interface ActionIconsProps {
  showClear: boolean | undefined;
  invalid?: string;
  onClear: (e?: React.MouseEvent) => void;
  scale?: Scales;
}

export const ActionIcons: React.FC<ActionIconsProps> = ({
  showClear,
  invalid,
  onClear,
  scale = Scales.Medium,
}) => {
  if (!showClear && !invalid) {
    return null;
  }

  return (
    <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center gap-2">
      {showClear && <ClearButton onClick={onClear} scale={scale} />}
      {invalid && <InvalidIcon message={invalid} />}
    </div>
  );
};
