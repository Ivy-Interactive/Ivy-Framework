import React, { useCallback, useMemo } from 'react';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { Toggle } from '@/components/ui/toggle';
import Icon from '@/components/Icon';
import { useEventHandler } from '@/components/event-handler';
import { inputStyles } from '@/lib/styles';
import { cn } from '@/lib/utils';
import { Checkbox, NullableBoolean } from '@/components/ui/checkbox';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import { Scales } from '@/types/scale';
import {
  labelSizeVariants,
  descriptionSizeVariants,
} from '@/components/ui/input/bool-input-variants';

type VariantType = 'Checkbox' | 'Switch' | 'Toggle';

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
  scale?: Scales;
  'data-testid'?: string;
}

interface BaseVariantProps {
  id: string;
  label?: string;
  description?: string;
  invalid?: string;
  nullable?: boolean;
  value: NullableBoolean;
  disabled: boolean;
  scale?: Scales;
  'data-testid'?: string;
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
  scale?: Scales;
}> = React.memo(({ id, label, description, scale = Scales.Medium }) => {
  if (!label && !description) return null;

  return (
    <div>
      {label && (
        <Label htmlFor={id} className={labelSizeVariants({ scale })}>
          {label}
        </Label>
      )}
      {description && (
        <p className={descriptionSizeVariants({ scale })}>{description}</p>
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
      scale = Scales.Medium,
      onCheckedChange,
      'data-testid': dataTestId,
    }: CheckboxVariantProps) => {
      const checkboxElement = (
        <Checkbox
          id={id}
          checked={value}
          onCheckedChange={onCheckedChange}
          disabled={disabled}
          nullable={nullable}
          className={cn(invalid && inputStyles.invalid)}
          data-testid={dataTestId}
          scale={scale}
        />
      );

      const content = (
        <div
          className="flex items-center gap-2"
          onClick={e => e.stopPropagation()}
        >
          {withTooltip(checkboxElement, invalid)}
          <InputLabel
            id={id}
            label={label}
            description={description}
            scale={scale}
          />
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
      scale = Scales.Medium,
      onCheckedChange,
      'data-testid': dataTestId,
    }: SwitchVariantProps) => {
      const switchElement = (
        <Switch
          id={id}
          checked={!!value}
          onCheckedChange={onCheckedChange}
          disabled={disabled}
          scale={scale}
          className={cn(invalid && inputStyles.invalid)}
          data-testid={dataTestId}
        />
      );

      const content = (
        <div
          className="flex items-center gap-2"
          onClick={e => e.stopPropagation()}
        >
          {withTooltip(switchElement, invalid)}
          <InputLabel
            id={id}
            label={label}
            description={description}
            scale={scale}
          />
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
      scale = Scales.Medium,
      onPressedChange,
      'data-testid': dataTestId,
    }: ToggleVariantProps) => {
      const toggleElement = (
        <Toggle
          id={id}
          pressed={!!value}
          onPressedChange={onPressedChange}
          disabled={disabled}
          aria-label={label}
          className={cn(invalid && inputStyles.invalid)}
          scale={scale}
          data-testid={dataTestId}
        >
          {icon && <Icon name={icon} />}
        </Toggle>
      );

      const content = (
        <div
          className="flex items-center space-x-2"
          onClick={e => e.stopPropagation()}
        >
          {withTooltip(toggleElement, invalid)}
          <InputLabel
            id={id}
            label={label}
            description={description}
            scale={scale}
          />
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
  scale = Scales.Medium,
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();

  // Normalize undefined to null when nullable
  const normalizedValue = nullable && value === undefined ? null : value;

  const handleChange = useCallback(
    (newValue: boolean | null) => {
      if (disabled) return;
      eventHandler('OnChange', id, [newValue]);
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
      scale={scale}
      onCheckedChange={handleChange}
      onPressedChange={handleChange}
      data-testid={dataTestId}
    />
  );
};

export default React.memo(BoolInputWidget);
