import React, { useMemo, useCallback, useState, useEffect } from 'react';
import { useEventHandler } from '@/components/EventHandlerContext';
import { InvalidIcon } from '@/components/InvalidIcon';
import { isValidColor, normalizeColor, getColorDisplayValue, ColorInputVariant } from '@/lib/colorUtils';

interface ColorInputWidgetProps {
  id: string;
  value: string | null;
  variant: ColorInputVariant;
  label?: string;
  description?: string;
  error?: string;
  disabled?: boolean;
  paletteOptions?: string[]; // for 'palette' variant
}

// Shared props for all variant components
interface BaseVariantProps {
  id: string;
  value: string | null;
  label?: string;
  description?: string;
  error?: string;
  disabled?: boolean;
  paletteOptions?: string[];
  onChange: (value: string | null) => void;
}

const PickerVariant: React.FC<BaseVariantProps> = ({
  id,
  value,
  label,
  description,
  error,
  disabled,
  onChange,
}) => (
  <div className="flex flex-col space-y-1">
    {label && <label htmlFor={id} className="font-medium">{label}</label>}
    <div className="relative w-min">
      <input
        id={id}
        type="color"
        value={value || '#000000'}
        onChange={e => onChange(e.target.value)}
        disabled={disabled}
        className="w-10 h-10 p-1 border border-gray-300 rounded-lg cursor-pointer"
        aria-invalid={!!error}
      />
      {error && (
        <div className="absolute right-0 top-0 h-4 w-4">
          <InvalidIcon message={error} />
        </div>
      )}
    </div>
    {description && <span className="text-xs text-gray-500">{description}</span>}
    {error && <span className="text-xs text-red-500">{error}</span>}
  </div>
);

const TextVariant: React.FC<BaseVariantProps> = ({
  id,
  value,
  label,
  description,
  error,
  disabled,
  onChange,
}) => {
  const [localValue, setLocalValue] = useState(value ?? '');
  const [validationError, setValidationError] = useState<string | null>(null);

  // Keep local value in sync with prop
  useEffect(() => {
    setLocalValue(value ?? '');
    setValidationError(null);
  }, [value]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    setLocalValue(newValue);
    
    // Clear validation error when user starts typing
    if (validationError) {
      setValidationError(null);
    }
    
    // If empty, allow it
    if (!newValue.trim()) {
      onChange(null);
      return;
    }
    
    // Validate color
    if (!isValidColor(newValue)) {
      setValidationError('Invalid color format. Use #RRGGBB, rgb(r,g,b), or a color name.');
      onChange(newValue); // Still propagate the value for real-time feedback
      return;
    }
    
    // Normalize and propagate valid color
    const normalized = normalizeColor(newValue);
    onChange(normalized);
  };

  const handleBlur = () => {
    if (localValue.trim() && !isValidColor(localValue)) {
      setValidationError('Invalid color format. Use #RRGGBB, rgb(r,g,b), or a color name.');
    }
  };

  const displayValue = getColorDisplayValue(localValue);
  const hasError = error || validationError;

  return (
    <div className="flex flex-col space-y-1">
      {label && <label htmlFor={id} className="font-medium">{label}</label>}
      <div className="relative w-min">
        <input
          id={id}
          type="text"
          value={displayValue}
          onChange={handleChange}
          onBlur={handleBlur}
          disabled={disabled}
          placeholder="#RRGGBB, rgb(), or name"
          className="w-32 p-1 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400"
          aria-invalid={!!hasError}
          autoComplete="off"
        />
        {hasError && (
          <div className="absolute right-0 top-0 h-4 w-4">
            <InvalidIcon message={hasError} />
          </div>
        )}
      </div>
      {description && <span className="text-xs text-gray-500">{description}</span>}
      {hasError && <span className="text-xs text-red-500">{hasError}</span>}
    </div>
  );
};

const PickerTextVariant: React.FC<BaseVariantProps> = ({
  id,
  value,
  label,
  description,
  error,
  disabled,
  onChange,
}) => {
  // Local state for the text input to allow editing invalid/partial values
  const [textValue, setTextValue] = useState(value ?? '');
  const [validationError, setValidationError] = useState<string | null>(null);

  // Keep local text in sync with value prop
  useEffect(() => {
    setTextValue(value ?? '');
    setValidationError(null);
  }, [value]);

  // Handle text input change
  const handleTextChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setTextValue(val);
    
    // Clear validation error when user starts typing
    if (validationError) {
      setValidationError(null);
    }
    
    // If empty, allow it
    if (!val.trim()) {
      onChange(null);
      return;
    }
    
    // Validate color
    if (!isValidColor(val)) {
      setValidationError('Invalid color format. Use #RRGGBB, rgb(r,g,b), or a color name.');
      onChange(val); // Still propagate the value for real-time feedback
      return;
    }
    
    // Normalize and propagate valid color
    const normalized = normalizeColor(val);
    onChange(normalized);
  };

  // Handle color picker change
  const handlePickerChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const pickerValue = e.target.value;
    setTextValue(pickerValue);
    setValidationError(null);
    onChange(pickerValue);
  };

  const handleTextBlur = () => {
    if (textValue.trim() && !isValidColor(textValue)) {
      setValidationError('Invalid color format. Use #RRGGBB, rgb(r,g,b), or a color name.');
    }
  };

  const displayValue = getColorDisplayValue(textValue);
  const hasError = error || validationError;

  return (
    <div className="flex flex-col space-y-1">
      {label && <label htmlFor={id} className="font-medium">{label}</label>}
      <div className="flex items-center space-x-2">
        <div className="relative w-min">
          <input
            id={id + '-picker'}
            type="color"
            value={value || '#000000'}
            onChange={handlePickerChange}
            disabled={disabled}
            className="w-10 h-10 p-1 border border-gray-300 rounded-lg cursor-pointer"
            aria-invalid={!!hasError}
          />
          {hasError && (
            <div className="absolute right-0 top-0 h-4 w-4">
              <InvalidIcon message={hasError} />
            </div>
          )}
        </div>
        <div className="relative w-min">
          <input
            id={id + '-text'}
            type="text"
            value={displayValue}
            onChange={handleTextChange}
            onBlur={handleTextBlur}
            disabled={disabled}
            placeholder="#RRGGBB, rgb(), or name"
            className="w-32 p-1 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400"
            aria-invalid={!!hasError}
            autoComplete="off"
          />
          {hasError && (
            <div className="absolute right-0 top-0 h-4 w-4">
              <InvalidIcon message={hasError} />
            </div>
          )}
        </div>
      </div>
      {description && <span className="text-xs text-gray-500">{description}</span>}
      {hasError && <span className="text-xs text-red-500">{hasError}</span>}
    </div>
  );
};

const PaletteVariant: React.FC<BaseVariantProps> = ({
  id,
  value,
  label,
  description,
  error,
  disabled,
  paletteOptions = [],
  onChange,
}) => {
  return (
    <div className="flex flex-col space-y-1">
      {label && <label htmlFor={id} className="font-medium">{label}</label>}
      <div className="flex items-center space-x-2">
        {paletteOptions.map((color) => (
          <button
            key={color}
            type="button"
            className={`w-8 h-8 rounded border-2 flex items-center justify-center transition-colors focus:outline-none ${
              value === color ? 'border-blue-500 ring-2 ring-blue-300' : 'border-gray-300'
            } ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}`}
            style={{ backgroundColor: color }}
            onClick={() => !disabled && onChange(color)}
            aria-label={color}
            disabled={disabled}
          >
            {value === color && (
              <span className="block w-3 h-3 rounded-full border-2 border-white bg-white/50" />
            )}
          </button>
        ))}
        {/* Optionally, allow clearing selection */}
        <button
          type="button"
          className={`w-8 h-8 rounded border-2 flex items-center justify-center transition-colors focus:outline-none border-gray-300 text-gray-400 bg-white ${
            value === null ? 'ring-2 ring-blue-300' : ''
          } ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}`}
          onClick={() => !disabled && onChange(null)}
          aria-label="Clear color"
          disabled={disabled}
        >
          ×
        </button>
        {error && (
          <div className="relative h-4 w-4 ml-2">
            <InvalidIcon message={error} />
          </div>
        )}
      </div>
      {description && <span className="text-xs text-gray-500">{description}</span>}
      {error && <span className="text-xs text-red-500">{error}</span>}
    </div>
  );
};

const VariantComponents = {
  Picker: PickerVariant,
  Text: TextVariant,
  PickerText: PickerTextVariant,
  Palette: PaletteVariant,
};

export const ColorInputWidget: React.FC<ColorInputWidgetProps> = ({
  id,
  value,
  variant,
  label,
  description,
  error,
  disabled = false,
  paletteOptions,
}) => {
  const eventHandler = useEventHandler();

  const handleChange = useCallback(
    (newValue: string | null) => {
      if (disabled) return;
      eventHandler('OnChange', id, [newValue]);
    },
    [disabled, eventHandler, id]
  );

  const VariantComponent = useMemo(() => VariantComponents[variant], [variant]);

  return (
    <VariantComponent
      id={id}
      value={value}
      label={label}
      description={description}
      error={error}
      disabled={disabled}
      paletteOptions={paletteOptions}
      onChange={handleChange}
    />
  );
};


