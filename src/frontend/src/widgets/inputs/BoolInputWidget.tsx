import React, { useCallback, useMemo } from "react";
import { useOptimisticValue } from "./shared/useOptimisticValue";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Toggle } from "@/components/ui/toggle";
import Icon from "@/components/Icon";
import { useEventHandler } from "@/components/event-handler";
import { inputStyles } from "@/lib/styles";
import { cn } from "@/lib/utils";
import { Checkbox, NullableBoolean } from "@/components/ui/checkbox";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { Loader2 } from "lucide-react";
import { Densities } from "@/types/density";
import {
  labelSizeVariant,
  descriptionSizeVariant,
  boolInputRowMinHeightVariant,
} from "@/components/ui/input/bool-input-variant";
import { EMPTY_ARRAY } from "@/lib/constants";

type VariantType = "Checkbox" | "Switch" | "Toggle";

interface BoolInputWidgetProps {
  id: string;
  label?: string;
  description?: string;
  value: NullableBoolean;
  disabled?: boolean;
  loading?: boolean;
  nullable?: boolean;
  invalid?: string;
  variant: VariantType;
  icon?: string;
  density?: Densities;
  autoFocus?: boolean;
  events?: string[];
  "data-testid"?: string;
}

interface BaseVariantProps {
  id: string;
  label?: string;
  description?: string;
  invalid?: string;
  nullable?: boolean;
  value: NullableBoolean;
  disabled: boolean;
  loading: boolean;
  density?: Densities;
  autoFocus?: boolean;
  "data-testid"?: string;
}

interface CheckboxVariantProps extends BaseVariantProps {
  nullable: boolean;
  onCheckedChange: (checked: boolean | null) => void;
}

interface SwitchVariantProps extends BaseVariantProps {
  icon?: string;
  onCheckedChange: (checked: boolean) => void;
}

interface ToggleVariantProps extends BaseVariantProps {
  icon?: string;
  onPressedChange: (pressed: boolean) => void;
  autoFocus?: boolean;
}

const InputLabel: React.FC<{
  id: string;
  label?: string;
  description?: string;
  density?: Densities;
}> = React.memo(({ id, label, description, density = Densities.Medium }) => {
  if (!label && !description) return null;

  return (
    <div>
      {label && (
        <Label htmlFor={id} className={labelSizeVariant({ density })}>
          {label}
        </Label>
      )}
      {description && <p className={descriptionSizeVariant({ density })}>{description}</p>}
    </div>
  );
});

const withTooltip = (content: React.ReactNode, invalid?: string) => {
  if (!invalid) return content;

  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger asChild>{content}</TooltipTrigger>
        <TooltipContent className="bg-popover text-popover-foreground shadow-md">
          {invalid}
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
};

const LoadingOverlay: React.FC<{
  density?: Densities;
  "data-testid"?: string;
}> = ({ density = Densities.Medium, "data-testid": dataTestId }) => {
  const sizeClass =
    density === Densities.Small ? "h-4 w-4" : density === Densities.Large ? "h-5 w-5" : "h-4 w-4";
  return (
    <div
      className="absolute inset-0 flex items-center justify-center rounded-md bg-background/80"
      data-testid={dataTestId ? `${dataTestId}-loading` : undefined}
    >
      <Loader2 className={cn(sizeClass, "animate-spin text-muted-foreground")} />
    </div>
  );
};

const VariantComponents = {
  Checkbox: React.memo(
    ({
      id,
      label,
      description,
      value,
      disabled,
      loading,
      nullable,
      invalid,
      density = Densities.Medium,
      autoFocus,
      onCheckedChange,
      "data-testid": dataTestId,
    }: CheckboxVariantProps) => {
      const checkboxElement = (
        <div className="relative flex shrink-0">
          <Checkbox
            id={id}
            checked={value}
            onCheckedChange={onCheckedChange}
            disabled={disabled || loading}
            nullable={nullable}
            autoFocus={autoFocus}
            className={cn(invalid && inputStyles.invalid)}
            data-testid={dataTestId}
          />
          {loading && <LoadingOverlay data-testid={dataTestId} />}
        </div>
      );

      const content = (
        <div
          className={cn(
            "flex gap-2 items-center",
            boolInputRowMinHeightVariant({ density }),
            description && "items-start",
          )}
          onClick={(e) => e.stopPropagation()}
          role="presentation"
        >
          <div className={cn(description && "mt-1.5", "flex shrink-0")}>
            {withTooltip(checkboxElement, invalid)}
          </div>
          <InputLabel id={id} label={label} description={description} />
        </div>
      );

      return content;
    },
  ),

  Switch: React.memo(
    ({
      id,
      label,
      description,
      value,
      disabled,
      loading,
      invalid,
      density = Densities.Medium,
      autoFocus,
      icon,
      onCheckedChange,
      "data-testid": dataTestId,
    }: SwitchVariantProps) => {
      const switchElement = (
        <div className="relative flex shrink-0">
          <Switch
            id={id}
            checked={!!value}
            onCheckedChange={onCheckedChange}
            disabled={disabled || loading}
            icon={icon}
            autoFocus={autoFocus}
            className={cn(invalid && inputStyles.invalid)}
            data-testid={dataTestId}
          />
          {loading && <LoadingOverlay data-testid={dataTestId} />}
        </div>
      );

      const content = (
        <div
          className={cn(
            "flex gap-2 items-center",
            boolInputRowMinHeightVariant({ density }),
            description && "items-start",
          )}
          onClick={(e) => e.stopPropagation()}
          role="presentation"
        >
          <div className={cn(description && "mt-1.5", "flex shrink-0")}>
            {withTooltip(switchElement, invalid)}
          </div>
          <InputLabel id={id} label={label} description={description} />
        </div>
      );

      return content;
    },
  ),

  Toggle: React.memo(
    ({
      id,
      label,
      description,
      value,
      disabled,
      loading,
      icon,
      invalid,
      density = Densities.Medium,
      autoFocus,
      onPressedChange,
      "data-testid": dataTestId,
    }: ToggleVariantProps) => {
      const toggleElement = (
        <div className="relative flex shrink-0">
          <Toggle
            id={id}
            pressed={!!value}
            onPressedChange={onPressedChange}
            disabled={disabled || loading}
            aria-label={label}
            autoFocus={autoFocus}
            className={cn(invalid && inputStyles.invalid)}
            data-testid={dataTestId}
          >
            {icon && <Icon name={icon} />}
          </Toggle>
          {loading && <LoadingOverlay data-testid={dataTestId} />}
        </div>
      );

      const content = (
        <div
          className={cn(
            "flex space-x-2 items-center",
            boolInputRowMinHeightVariant({ density }),
            description && "items-start",
          )}
          onClick={(e) => e.stopPropagation()}
          role="presentation"
        >
          <div className={cn(description && "mt-1.5", "flex shrink-0")}>
            {withTooltip(toggleElement, invalid)}
          </div>
          <InputLabel id={id} label={label} description={description} />
        </div>
      );

      return content;
    },
  ),
};

export const BoolInputWidget: React.FC<BoolInputWidgetProps> = ({
  id,
  label,
  description,
  value = null,
  disabled = false,
  loading = false,
  invalid,
  nullable = false,
  variant = "Checkbox",
  icon,
  density = Densities.Medium,
  autoFocus,
  events = EMPTY_ARRAY,
  "data-testid": dataTestId,
}) => {
  const eventHandler = useEventHandler();

  // Normalize undefined to null when nullable
  const normalizedValue = nullable && value === undefined ? null : value;

  const [localValue, setLocalValue] = useOptimisticValue(normalizedValue, false);

  const handleChange = useCallback(
    (newValue: boolean | null) => {
      if (disabled || loading) return;
      setLocalValue(newValue);
      eventHandler("OnChange", id, [newValue]);
    },
    [disabled, loading, eventHandler, id, setLocalValue],
  );

  const VariantComponent = useMemo(() => VariantComponents[variant], [variant]);

  return (
    <div
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
      <VariantComponent
        id={id}
        label={label}
        description={description}
        value={localValue}
        disabled={disabled}
        loading={loading}
        nullable={nullable}
        icon={icon}
        invalid={invalid}
        density={density}
        autoFocus={autoFocus}
        onCheckedChange={handleChange}
        onPressedChange={handleChange}
        data-testid={dataTestId}
      />
    </div>
  );
};

export default React.memo(BoolInputWidget);
