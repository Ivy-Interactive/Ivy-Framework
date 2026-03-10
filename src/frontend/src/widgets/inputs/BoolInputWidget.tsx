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
import { Loader2 } from 'lucide-react';
import { Scales } from '@/types/scale';
import {
  labelSizeVariants,
  descriptionSizeVariants,
  boolInputRowMinHeightVariants,
} from '@/components/ui/input/bool-input-variants';

type VariantType = 'Checkbox' | 'Switch' | 'Toggle';

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
  loading: boolean;
  scale?: Scales;
  'data-testid'?: string;
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

const LoadingOverlay: React.FC<{ scale?: Scales; 'data-testid'?: string }> = ({
  scale = Scales.Medium,
  'data-testid': dataTestId,
}) => {
  const sizeClass =
    scale === Scales.Small
      ? 'h-4 w-4'
      : scale === Scales.Large
        ? 'h-5 w-5'
        : 'h-4 w-4';
  return (
    <div
      className="absolute inset-0 flex items-center justify-center rounded-md bg-background/80"
      data-testid={dataTestId ? `${dataTestId}-loading` : undefined}
    >
      <Loader2
        className={cn(sizeClass, 'animate-spin text-muted-foreground')}
      />
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
      scale = Scales.Medium,
      onCheckedChange,
      'data-testid': dataTestId,
    }: CheckboxVariantProps) => {
      const checkboxElement = (
        <div className="relative flex shrink-0">
          <Checkbox
            id={id}
            checked={value}
            onCheckedChange={onCheckedChange}
            disabled={disabled || loading}
            nullable={nullable}
            className={cn(invalid && inputStyles.invalid)}
            data-testid={dataTestId}
            scale={scale}
          />
          {loading && <LoadingOverlay scale={scale} data-testid={dataTestId} />}
        </div>
      );

      const content = (
        <div
          className={cn(
            'flex gap-2 items-center',
            boolInputRowMinHeightVariants({ scale }),
            description && 'items-start'
          )}
          onClick={e => e.stopPropagation()}
          role="presentation"
        >
          <div className={cn(description && 'mt-1.5', 'flex shrink-0')}>
            {withTooltip(checkboxElement, invalid)}
          </div>
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
      loading,
      invalid,
      scale = Scales.Medium,
      icon,
      onCheckedChange,
      'data-testid': dataTestId,
    }: SwitchVariantProps) => {
      const switchElement = (
        <div className="relative flex shrink-0">
          <Switch
            id={id}
            checked={!!value}
            onCheckedChange={onCheckedChange}
            disabled={disabled || loading}
            scale={scale}
            icon={icon}
            className={cn(invalid && inputStyles.invalid)}
            data-testid={dataTestId}
          />
          {loading && <LoadingOverlay scale={scale} data-testid={dataTestId} />}
        </div>
      );

      const content = (
        <div
          className={cn(
            'flex gap-2 items-center',
            boolInputRowMinHeightVariants({ scale }),
            description && 'items-start'
          )}
          onClick={e => e.stopPropagation()}
          role="presentation"
        >
          <div className={cn(description && 'mt-1.5', 'flex shrink-0')}>
            {withTooltip(switchElement, invalid)}
          </div>
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
      loading,
      icon,
      invalid,
      scale = Scales.Medium,
      onPressedChange,
      'data-testid': dataTestId,
    }: ToggleVariantProps) => {
      const toggleElement = (
        <div className="relative flex shrink-0">
          <Toggle
            id={id}
            pressed={!!value}
            onPressedChange={onPressedChange}
            disabled={disabled || loading}
            aria-label={label}
            className={cn(invalid && inputStyles.invalid)}
            scale={scale}
            data-testid={dataTestId}
          >
            {icon && <Icon name={icon} />}
          </Toggle>
          {loading && <LoadingOverlay scale={scale} data-testid={dataTestId} />}
        </div>
      );

      const content = (
        <div
          className={cn(
            'flex space-x-2 items-center',
            boolInputRowMinHeightVariants({ scale }),
            description && 'items-start'
          )}
          onClick={e => e.stopPropagation()}
          role="presentation"
        >
          <div className={cn(description && 'mt-1.5', 'flex shrink-0')}>
            {withTooltip(toggleElement, invalid)}
          </div>
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
  value = null,
  disabled = false,
  loading = false,
  invalid,
  nullable = false,
  variant = 'Checkbox',
  icon,
  scale = Scales.Medium,
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();

  // Normalize undefined to null when nullable
  const normalizedValue = nullable && value === undefined ? null : value;

  const handleChange = useCallback(
    (newValue: boolean | null) => {
      if (disabled || loading) return;
      eventHandler('OnChange', id, [newValue]);
    },
    [disabled, loading, eventHandler, id]
  );

  const VariantComponent = useMemo(() => VariantComponents[variant], [variant]);

  return (
    <VariantComponent
      id={id}
      label={label}
      description={description}
      value={normalizedValue}
      disabled={disabled}
      loading={loading}
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
