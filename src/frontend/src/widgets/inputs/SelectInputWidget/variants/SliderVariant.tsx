import React, { useCallback, useMemo, useState } from "react";
import { Slider } from "@/components/ui/slider";
import { InvalidIcon } from "@/components/InvalidIcon";
import { cn } from "@/lib/utils";
import { getWidth, inputStyles } from "@/lib/styles";
import { logger } from "@/lib/logger";
import { Densities } from "@/types/density";
import { SelectInputWidgetProps } from "../../select-types";
import { EventHandler } from "@/components/event-handler";
import { EMPTY_ARRAY } from "@/lib/constants";
import { sliderLabelVariant } from "../styles";

export const SliderVariant: React.FC<SelectInputWidgetProps & { eventHandler: EventHandler }> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = EMPTY_ARRAY,
  eventHandler,
  selectMany = false,
  ghost = false,
  density = Densities.Medium,
  "data-testid": dataTestId,
  width,
  events = EMPTY_ARRAY,
}) => {
  if (selectMany) {
    logger.warn(
      "SelectInput Slider variant does not support selectMany. Falling back to single-select.",
    );
  }

  const validOptions = useMemo(() => options.filter((o) => !o.disabled), [options]);

  const currentIndex = useMemo(() => {
    if (value == null) return -1;
    const strValue = String(value);
    return validOptions.findIndex((o) => String(o.value) === strValue);
  }, [value, validOptions]);

  const [localIndex, setLocalIndex] = useState(currentIndex);

  const handleSliderChange = useCallback((values: number[]) => {
    const newIndex = values[0];
    if (typeof newIndex === "number") {
      setLocalIndex(newIndex);
    }
  }, []);

  const handleSliderCommit = useCallback(
    (values: number[]) => {
      const newIndex = values[0];
      if (typeof newIndex === "number" && validOptions[newIndex]) {
        if (events.includes("OnChange"))
          eventHandler("OnChange", id, [validOptions[newIndex].value]);
      }
    },
    [eventHandler, id, validOptions, events],
  );

  if (validOptions.length === 0) {
    return (
      <div
        className={cn(
          "flex items-center justify-center text-muted-foreground",
          sliderLabelVariant[String(density)],
        )}
        style={width ? getWidth(width) : undefined}
        data-testid={dataTestId}
      >
        No options available
      </div>
    );
  }

  const sliderValue = localIndex >= 0 ? localIndex : 0;
  const currentLabel = validOptions[sliderValue]?.label ?? "";
  const firstLabel = validOptions[0]?.label ?? "";
  const lastLabel = validOptions[validOptions.length - 1]?.label ?? "";
  const textSize = sliderLabelVariant[String(density)];

  return (
    <div
      className={cn(
        "relative w-full flex-1 flex flex-col gap-1 pt-6 pb-2 my-auto justify-center",
        ghost && "border-transparent shadow-none",
        disabled && "opacity-50 cursor-not-allowed",
      )}
      style={width ? getWidth(width) : undefined}
      data-testid={dataTestId}
      onBlur={(e) => {
        if (disabled) return;
        if (!e.currentTarget.contains(e.relatedTarget)) {
          if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
        }
      }}
      onFocus={(e) => {
        if (disabled) return;
        if (!e.currentTarget.contains(e.relatedTarget)) {
          if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
        }
      }}
      tabIndex={disabled ? -1 : 0}
    >
      <div className="relative">
        <Slider
          min={0}
          max={validOptions.length - 1}
          step={1}
          value={[sliderValue]}
          disabled={disabled}
          density={density}
          tooltipValue={validOptions[sliderValue]?.tooltip || currentLabel}
          onValueChange={handleSliderChange}
          onValueCommit={handleSliderCommit}
          className={cn(invalid && inputStyles.invalidInput)}
        />
        {validOptions.length > 1 && (
          <div
            className="absolute w-full flex justify-between px-[2px]"
            style={{
              top: "50%",
              transform: "translateY(-50%)",
              pointerEvents: "none",
            }}
          >
            {validOptions.map((option, i) => (
              <div
                key={option.value}
                className={cn(
                  "rounded-full",
                  i === sliderValue ? "bg-transparent" : "bg-muted-foreground/40",
                  density === Densities.Small
                    ? "w-1 h-1"
                    : density === Densities.Large
                      ? "w-1.5 h-1.5"
                      : "w-1 h-1",
                )}
              />
            ))}
          </div>
        )}
      </div>
      <div
        className={cn("flex w-full items-center justify-between gap-1", textSize)}
        aria-hidden="true"
      >
        <span className="text-muted-foreground">{firstLabel}</span>
        <span className="text-muted-foreground">{lastLabel}</span>
      </div>
      {invalid && (
        <div className="absolute right-2.5 translate-y-1/2 -top-1.5">
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
};
