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
  textInputAffixControlColumnClasses,
  textInputAffixInvalidIconClasses,
  textInputAffixPrefixCellClasses,
  textInputAffixSuffixCellClasses,
  textInputFieldShellClasses,
  textInputSuffixGlyphSlotClasses,
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
  disabled?: boolean;
}> = React.memo(({ id, label, description, density = Densities.Medium, disabled }) => {
  if (!label && !description) return null;
  const d = normalizeInputDensity(density);

  // Native htmlFor association toggles the control — do not re-add a JS click handler here.
  return (
    <Label
      htmlFor={id}
      className={cn(
        "min-w-0 flex-1 self-stretch flex flex-col justify-center",
        disabled ? "cursor-not-allowed" : "cursor-pointer",
      )}
    >
      {label && (
        <span className={cn("block truncate", labelSizeVariant({ density: d }))}>{label}</span>
      )}
      {description && (
        <span className={cn("block", descriptionSizeVariant({ density: d }))}>{description}</span>
      )}
    </Label>
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
            className={cn(invalid && inputStyles.invalidInput)}
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
            className={cn(invalid && inputStyles.invalidInput)}
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
            className={cn(invalid && inputStyles.invalidInput)}
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

  const normalizedValue = useMemo(() => {
    if (value === undefined || value === null) return nullable ? null : false;
    if (value === true || (value as any) === 1) return true;
    if (value === false || (value as any) === 0) return false;
    return !!value;
  }, [value, nullable]);

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
          className={textInputFieldShellClasses({
            invalid,
            disabled,
          })}
        >
          {hasPrefix && (
            <div className={textInputAffixPrefixCellClasses(density, prefixContent)}>
              {prefixContent}
            </div>
          )}
          <div className={textInputAffixControlColumnClasses(density)}>{variantContent}</div>
          {hasSuffix && (
            <div
              className={textInputAffixSuffixCellClasses(density, suffixContent, {
                showTrailing: trailingBesideSuffix && showTrailing,
              })}
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
