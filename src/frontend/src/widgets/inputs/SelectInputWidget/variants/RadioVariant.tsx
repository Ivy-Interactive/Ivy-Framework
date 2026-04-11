import React from "react";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Label } from "@/components/ui/label";
import { Tooltip, TooltipProvider, TooltipTrigger, TooltipContent } from "@/components/ui/tooltip";
import Icon from "@/components/Icon";
import { X } from "lucide-react";
import { InvalidIcon } from "@/components/InvalidIcon";
import { cn } from "@/lib/utils";
import { getWidth, inputStyles } from "@/lib/styles";
import { logger } from "@/lib/logger";
import { Densities } from "@/types/density";
import { xIconVariant } from "@/components/ui/input/text-input-variant";
import { SelectInputWidgetProps } from "../../select-types";
import { useSelectValueHandler } from "../../select-utils";
import { EMPTY_ARRAY } from "@/lib/constants";
import { selectContainerVariant, selectTextVariant, circleSizeVariant } from "../styles";

export const RadioVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  options = EMPTY_ARRAY,
  eventHandler,
  nullable = false,
  ghost = false,
  density = Densities.Medium,
  events = EMPTY_ARRAY,
  "data-testid": dataTestId,
  width,
}) => {
  const validOptions = options.filter(
    (option) => option.value != null && option.value.toString().trim() !== "",
  );
  const stringValue = value != null && value.toString().trim() !== "" ? value.toString() : "";

  const hasValue = stringValue !== "";

  const handleValueChange = useSelectValueHandler(
    id,
    value,
    validOptions,
    eventHandler,
    false, // Always single select for RadioVariant
    nullable,
  );
  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  return (
    <div
      className={cn(
        selectContainerVariant({ density }),
        invalid && "border-destructive focus-within:ring-destructive",
        ghost &&
          "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
      )}
      style={styles}
      onBlur={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
        }
      }}
      onFocus={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
        }
      }}
    >
      <div className="flex items-center gap-4">
        <div className="flex-1">
          <RadioGroup
            value={stringValue}
            onValueChange={handleValueChange}
            disabled={disabled}
            className="flex flex-col gap-4"
            data-testid={dataTestId}
          >
            {validOptions.map((option) => {
              const isOptionDisabled = disabled || option.disabled;
              return (
                <div key={option.value} className="flex items-center space-x-2">
                  <RadioGroupItem
                    value={option.value.toString()}
                    id={`${id}-${option.value}`}
                    disabled={isOptionDisabled}
                    className={cn(
                      "border-input text-input",
                      circleSizeVariant[density],
                      stringValue === option.value.toString() && !invalid
                        ? "border-primary text-primary"
                        : undefined,
                      stringValue === option.value.toString() && invalid
                        ? inputStyles.invalidInput
                        : undefined,
                      isOptionDisabled && "opacity-50 cursor-not-allowed",
                    )}
                  />
                  {option.tooltip ? (
                    <TooltipProvider>
                      <Tooltip>
                        <TooltipTrigger asChild>
                          <Label
                            htmlFor={`${id}-${option.value}`}
                            className={cn(
                              "cursor-pointer leading-none flex items-center gap-2",
                              selectTextVariant[density],
                              stringValue === option.value.toString() && invalid
                                ? inputStyles.invalidInput
                                : undefined,
                              isOptionDisabled && "opacity-50 cursor-not-allowed",
                            )}
                          >
                            {option.icon && (
                              <Icon name={option.icon} className="h-4 w-4 flex-shrink-0" />
                            )}
                            {option.description ? (
                              <div className="flex flex-col">
                                <span>{option.label}</span>
                                <span className="text-xs text-muted-foreground mt-0.5 font-normal">
                                  {option.description}
                                </span>
                              </div>
                            ) : (
                              option.label
                            )}
                          </Label>
                        </TooltipTrigger>
                        <TooltipContent>{option.tooltip}</TooltipContent>
                      </Tooltip>
                    </TooltipProvider>
                  ) : (
                    <Label
                      htmlFor={`${id}-${option.value}`}
                      className={cn(
                        "cursor-pointer leading-none flex items-center gap-2",
                        selectTextVariant[density],
                        stringValue === option.value.toString() && invalid
                          ? inputStyles.invalidInput
                          : undefined,
                        isOptionDisabled && "opacity-50 cursor-not-allowed",
                      )}
                    >
                      {option.icon && <Icon name={option.icon} className="h-4 w-4 flex-shrink-0" />}
                      {option.description ? (
                        <div className="flex flex-col">
                          <span>{option.label}</span>
                          <span className="text-xs text-muted-foreground mt-0.5 font-normal">
                            {option.description}
                          </span>
                        </div>
                      ) : (
                        option.label
                      )}
                    </Label>
                  )}
                </div>
              );
            })}
          </RadioGroup>
        </div>
        {((nullable && hasValue && !disabled) || invalid) && (
          <div className="flex items-center gap-1">
            {nullable && hasValue && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear"
                onClick={() => {
                  logger.debug("Select input clear button clicked", { id });
                  eventHandler("OnChange", id, [null]);
                }}
                className="flex-shrink-0 p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
              >
                <X className={xIconVariant({ density })} />
              </button>
            )}
            {invalid && <InvalidIcon message={invalid} className="pointer-events-auto" />}
          </div>
        )}
      </div>
    </div>
  );
};
