import * as React from "react";
import { X } from "lucide-react";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import {
  normalizeInputDensity,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
  textInputTrailingOverlayClasses,
} from "@/components/ui/input/text-input-variant";

interface ClearAndInvalidIconsProps {
  showClear?: boolean;
  invalid?: string;
  density?: Densities;
  onClear: (e?: React.MouseEvent) => void;
}

/** Standalone date/time field trailing cluster (no affix shell). */
export const ClearAndInvalidIcons: React.FC<ClearAndInvalidIconsProps> = ({
  showClear = false,
  invalid,
  density = Densities.Medium,
  onClear,
}) => {
  if (!showClear && !invalid) {
    return null;
  }

  const densityKey = normalizeInputDensity(density);

  return (
    <div className={textInputTrailingOverlayClasses(density)}>
      {showClear && (
        <button
          type="button"
          tabIndex={-1}
          aria-label="Clear"
          onClick={onClear}
          className={textInputTrailingIconButtonClasses(true, density)}
        >
          <X className={textInputTrailingIconSizeVariant({ density: densityKey })} />
        </button>
      )}
      {invalid && (
        <InvalidIcon
          message={invalid}
          className={textInputTrailingInvalidSlotClasses(true, density)}
          iconClassName={textInputTrailingIconSizeVariant({ density: densityKey })}
        />
      )}
    </div>
  );
};
