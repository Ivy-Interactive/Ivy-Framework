import { useEventHandler } from "@/components/EventHandlerContext";
import Icon from "@/components/Icon";
import { Checkbox, NullableBoolean } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Toggle } from "@/components/ui/toggle";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { inputStyles } from "@/lib/styles";
import { cn } from "@/lib/utils";
import React, { useCallback, useMemo } from "react";

type VariantType = "Checkbox" | "Switch" | "Toggle";

interface BoolInputWidgetProps {
  id: string;
  label?: string;
  description?: string;
  value: NullableBoolean;
  disabled?: boolean;
  nullable?: boolean;
  invalid?: string;
  variant: VariantType;
  icon?: string;
}

interface BaseVariantProps {
  id: string;
  label?: string;
  description?: string;
  invalid?: string;
  nullable?: boolean;
  value: NullableBoolean;
  disabled: boolean;
}

interface CheckboxVariantProps extends BaseVariantProps {
  nullable: boolean;
  onCheckedChange: (checked: boolean | null) => void;
}

interface SwitchVariantProps extends BaseVariantProps {
  onCheckedChange: (checked: boolean) => void;
}

interface ToggleVariantProps extends BaseVariantProps {
  icon?: string;
  onPressedChange: (pressed: boolean) => void;
}

const InputLabel: React.FC<{
  id: string;
  label?: string;
  description?: string;
}> = React.memo(({ id, label, description }) => {
  if (!label && !description) return null;

  return (
    <div className="grid gap-1.5 leading-none bg-background">
      {label && <Label htmlFor={id}>{label}</Label>}
      {description && (
        <p className="text-sm text-muted-foreground">{description}</p>
      )}
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

const VariantComponents = {
  Checkbox: React.memo(
    ({
      id,
      label,
      description,
      value,
      disabled,
      nullable,
      invalid,
      onCheckedChange,
    }: CheckboxVariantProps) => {
      const checkboxElement = (
        <Checkbox
          id={id}
          checked={value}
          onCheckedChange={onCheckedChange}
          disabled={disabled}
          nullable={nullable}
          className={cn(invalid && inputStyles.invalid)}
        />
      );

      const content = (
        <div
          className="flex items-start space-x-2"
          onClick={(e) => e.stopPropagation()}
        >
          {withTooltip(checkboxElement, invalid)}
          <InputLabel id={id} label={label} description={description} />
        </div>
      );

      return content;
    }
  ),

  Switch: React.memo(
    ({
      id,
      label,
      description,
      value,
      disabled,
      invalid,
      onCheckedChange,
    }: SwitchVariantProps) => {
      const switchElement = (
        <Switch
          id={id}
          checked={!!value}
          onCheckedChange={onCheckedChange}
          disabled={disabled}
          className={cn(invalid && inputStyles.invalid)}
        />
      );

      const content = (
        <div
          className="flex items-start space-x-2"
          onClick={(e) => e.stopPropagation()}
        >
          {withTooltip(switchElement, invalid)}
          <InputLabel id={id} label={label} description={description} />
        </div>
      );

      return content;
    }
  ),

  Toggle: React.memo(
    ({
      id,
      label,
      description,
      value,
      disabled,
      icon,
      invalid,
      onPressedChange,
    }: ToggleVariantProps) => {
      const toggleElement = (
        <Toggle
          id={id}
          pressed={!!value}
          onPressedChange={onPressedChange}
          disabled={disabled}
          aria-label={label}
          className={cn(invalid && inputStyles.invalid)}
        >
          {icon && <Icon className="h-4 w-4" name={icon} />}
        </Toggle>
      );

      const content = (
        <div
          className="flex items-start space-x-2"
          onClick={(e) => e.stopPropagation()}
        >
          {withTooltip(toggleElement, invalid)}
          <InputLabel id={id} label={label} description={description} />
        </div>
      );

      return content;
    }
  ),
};

export const BoolInputWidget: React.FC<BoolInputWidgetProps> = ({
  id,
  label,
  description,
  value,
  disabled = false,
  invalid,
  nullable = false,
  variant,
  icon,
}) => {
  const eventHandler = useEventHandler();

  // Normalize undefined to null when nullable
  const normalizedValue = nullable && value === undefined ? null : value;

  const handleChange = useCallback(
    (newValue: boolean | null) => {
      if (disabled) return;
      eventHandler("OnChange", id, [newValue]);
    },
    [disabled, eventHandler, id]
  );

  const VariantComponent = useMemo(() => VariantComponents[variant], [variant]);

  return (
    <VariantComponent
      id={id}
      label={label}
      description={description}
      value={normalizedValue}
      disabled={disabled}
      nullable={nullable}
      icon={icon}
      invalid={invalid}
      onCheckedChange={handleChange}
      onPressedChange={handleChange}
    />
  );
};

export default React.memo(BoolInputWidget);
