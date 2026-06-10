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
import withTooltip from "@/hoc/withTooltip";
import { Loader2 } from "lucide-react";
import { Densities } from "@/types/density";
import {
  labelSizeVariant,
  descriptionSizeVariant,
  boolInputControlGapVariant,
  boolInputRowMinHeightVariant,
} from "@/components/ui/input/bool-input-variant";
import { EMPTY_ARRAY } from "@/lib/constants";
import { InvalidIcon } from "@/components/InvalidIcon";
import {
  normalizeInputDensity,
  textInputAffixCellClasses,
  textInputAffixInvalidIconClasses,
  textInputSuffixGlyphSlotClasses,
  textInputSuffixWithTrailingClusterClasses,
  textInputSizeVariant,
  textInputTrailingIconSizeVariant,
} from "@/components/ui/input/text-input-variant";

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
  slots?: { Prefix?: React.ReactNode[]; Suffix?: React.ReactNode[] };
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
  inAffixShell?: boolean;
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
  onClick?: () => void;
  disabled?: boolean;
}> = React.memo(({ id, label, description, density = Densities.Medium, onClick, disabled }) => {
  if (!label && !description) return null;
  const d = normalizeInputDensity(density);

  const cursorClass = onClick ? (disabled ? "cursor-not-allowed" : "cursor-pointer") : undefined;

  return (
    <div onClick={onClick} className={cursorClass}>
      {label && (
        <Label htmlFor={id} className={labelSizeVariant({ density: d })}>
          {label}
        </Label>
      )}
      {description && <p className={descriptionSizeVariant({ density: d })}>{description}</p>}
    </div>
  );
});

const DivWithTooltip = withTooltip(
  React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>((props, ref) => (
    <div {...props} ref={ref} />
  )),
);

const LoadingOverlay: React.FC<{
  density?: Densities;
  "data-testid"?: string;
}> = ({ density = Densities.Medium, "data-testid": dataTestId }) => {
  const sizeClass =
    density === Densities.Small ? "size-4" : density === Densities.Large ? "size-5" : "size-4";
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
      inAffixShell = false,
      autoFocus,
      onCheckedChange,
      "data-testid": dataTestId,
    }: CheckboxVariantProps) => {
      const d = normalizeInputDensity(density);
      // Toggle cycle for label clicks: null → true → false → null (if nullable)
      const handleLabelClick = () => {
        if (disabled || loading) return;
        if (nullable) {
          if (value === null) onCheckedChange(true);
          else if (value === true) onCheckedChange(false);
          else onCheckedChange(null);
        } else {
          onCheckedChange(!value);
        }
      };

      const checkboxElement = (
        <div className="relative flex shrink-0">
          <Checkbox
            id={id}
            checked={value}
            onCheckedChange={onCheckedChange}
            disabled={disabled || loading}
            nullable={nullable}
            autoFocus={autoFocus}
            density={density}
            className={cn(invalid && inputStyles.invalid)}
            data-testid={dataTestId}
          />
          {loading && <LoadingOverlay density={density} data-testid={dataTestId} />}
        </div>
      );

      return (
        <div
          className={cn(
            "flex items-center",
            boolInputControlGapVariant({ density: d }),
            !inAffixShell && boolInputRowMinHeightVariant({ density: d }),
            description && "items-start",
          )}
          onClick={(e) => e.stopPropagation()}
          role="presentation"
        >
          <DivWithTooltip
            tooltipText={invalid}
            className={cn(description && "mt-1.5", "flex shrink-0")}
          >
            {checkboxElement}
          </DivWithTooltip>
          <InputLabel
            id={id}
            label={label}
            description={description}
            density={density}
            onClick={handleLabelClick}
            disabled={disabled || loading}
          />
        </div>
      );
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
      inAffixShell = false,
      autoFocus,
      icon,
      onCheckedChange,
      "data-testid": dataTestId,
    }: SwitchVariantProps) => {
      const d = normalizeInputDensity(density);
      const handleLabelClick = () => {
        if (disabled || loading) return;
        onCheckedChange(!value);
      };

      const switchElement = (
        <div className="relative flex shrink-0">
          <Switch
            id={id}
            checked={!!value}
            onCheckedChange={onCheckedChange}
            disabled={disabled || loading}
            icon={icon}
            density={density}
            autoFocus={autoFocus}
            className={cn(invalid && inputStyles.invalid)}
            data-testid={dataTestId}
          />
          {loading && <LoadingOverlay density={density} data-testid={dataTestId} />}
        </div>
      );

      return (
        <div
          className={cn(
            "flex items-center",
            boolInputControlGapVariant({ density: d }),
            !inAffixShell && boolInputRowMinHeightVariant({ density: d }),
            description && "items-start",
          )}
          onClick={(e) => e.stopPropagation()}
          role="presentation"
        >
          <DivWithTooltip
            tooltipText={invalid}
            className={cn(description && "mt-1.5", "flex shrink-0")}
          >
            {switchElement}
          </DivWithTooltip>
          <InputLabel
            id={id}
            label={label}
            description={description}
            density={density}
            onClick={handleLabelClick}
            disabled={disabled || loading}
          />
        </div>
      );
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
      inAffixShell = false,
      autoFocus,
      onPressedChange,
      "data-testid": dataTestId,
    }: ToggleVariantProps) => {
      const d = normalizeInputDensity(density);
      const handleLabelClick = () => {
        if (disabled || loading) return;
        onPressedChange(!value);
      };

      const toggleElement = (
        <div className="relative flex shrink-0">
          <Toggle
            id={id}
            pressed={!!value}
            onPressedChange={onPressedChange}
            disabled={disabled || loading}
            density={density}
            aria-label={label}
            autoFocus={autoFocus}
            className={cn(invalid && inputStyles.invalid)}
            data-testid={dataTestId}
          >
            {icon && <Icon name={icon} />}
          </Toggle>
          {loading && <LoadingOverlay density={density} data-testid={dataTestId} />}
        </div>
      );

      return (
        <div
          className={cn(
            "flex items-center",
            boolInputControlGapVariant({ density: d }),
            !inAffixShell && boolInputRowMinHeightVariant({ density: d }),
            description && "items-start",
          )}
          onClick={(e) => e.stopPropagation()}
          role="presentation"
        >
          <DivWithTooltip
            tooltipText={invalid}
            className={cn(description && "mt-1.5", "flex shrink-0")}
          >
            {toggleElement}
          </DivWithTooltip>
          <InputLabel
            id={id}
            label={label}
            description={description}
            density={density}
            onClick={handleLabelClick}
            disabled={disabled || loading}
          />
        </div>
      );
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
  slots,
  "data-testid": dataTestId,
}) => {
  const eventHandler = useEventHandler();

  const normalizedValue = nullable && value === undefined ? null : value;

  const [localValue, setLocalValue] = useOptimisticValue(normalizedValue, false);

  const handleChange = useCallback(
    (newValue: boolean | null) => {
      if (disabled || loading) return;
      setLocalValue(newValue);
      if (events.includes("OnChange")) eventHandler("OnChange", id, [newValue]);
    },
    [disabled, loading, eventHandler, id, setLocalValue, events],
  );

  const VariantComponent = useMemo(() => VariantComponents[variant], [variant]);
  const densityKey = normalizeInputDensity(density);

  const prefixContent = slots?.Prefix;
  const suffixContent = slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  const trailingBesideSuffix = hasSuffix;
  const showTrailing = Boolean(invalid);
  const controlInvalid = trailingBesideSuffix && showTrailing ? undefined : invalid;

  const variantContent = (
    <VariantComponent
      id={id}
      label={label}
      description={description}
      value={localValue}
      disabled={disabled}
      loading={loading}
      nullable={nullable}
      icon={icon}
      invalid={controlInvalid}
      density={density}
      inAffixShell={hasAffixes}
      autoFocus={autoFocus}
      onCheckedChange={handleChange}
      onPressedChange={handleChange}
      data-testid={dataTestId}
    />
  );

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
      {hasAffixes ? (
        <div
          className={cn(
            "relative flex items-stretch rounded-field border bg-transparent shadow-sm transition-colors dark:bg-white/5",
            invalid ? "border-destructive" : "border-input dark:border-white/10",
            disabled && "cursor-not-allowed opacity-50",
          )}
        >
          {hasPrefix && (
            <div className={textInputAffixCellClasses("prefix", density)}>{prefixContent}</div>
          )}
          <div
            className={cn(
              "relative flex min-w-0 flex-1 items-center",
              textInputSizeVariant({ density: densityKey }),
              boolInputRowMinHeightVariant({ density: densityKey }),
              "w-auto",
            )}
          >
            {variantContent}
          </div>
          {hasSuffix && (
            <div
              className={cn(
                textInputAffixCellClasses("suffix", density),
                trailingBesideSuffix &&
                  showTrailing &&
                  textInputSuffixWithTrailingClusterClasses(density),
              )}
            >
              {trailingBesideSuffix && showTrailing && (
                <>
                  {invalid && (
                    <InvalidIcon
                      message={invalid}
                      className={textInputAffixInvalidIconClasses()}
                      iconClassName={textInputTrailingIconSizeVariant({ density: densityKey })}
                    />
                  )}
                </>
              )}
              {trailingBesideSuffix && showTrailing ? (
                <span className={textInputSuffixGlyphSlotClasses(density)}>{suffixContent}</span>
              ) : (
                suffixContent
              )}
            </div>
          )}
        </div>
      ) : (
        variantContent
      )}
    </div>
  );
};

export default React.memo(BoolInputWidget);
