import React, { useState, useCallback, useMemo, useRef, useEffect } from "react";
import { useEventHandler } from "@/components/event-handler";
import { Densities } from "@/types/density";
import { TextInputWidgetProps, TextInputVariant } from "./types";
import { useShortcut } from "@/lib/useShortcut";
import { useOptimisticValue } from "../shared/useOptimisticValue";
import { DefaultVariant, TextareaVariant, PasswordVariant, SearchVariant } from "./variants";
import { EMPTY_ARRAY } from "@/lib/constants";
import { useDictation } from "./hooks/useDictation";

export const TextInputWidget: React.FC<TextInputWidgetProps> = ({
  id,
  placeholder,
  value,
  variant = "Text",
  disabled = false,
  invalid,
  nullable = false,
  width,
  height,
  events = EMPTY_ARRAY,
  shortcutKey,
  density = Densities.Medium,
  slots,
  maxLength,
  minLength,
  pattern,
  rows,
  autoFocus,
  ghost = false,
  dictation,
  dictationUploadUrl,
  dictationTranscription,
  dictationTranscriptionVersion,
  "data-testid": dataTestId,
}) => {
  const eventHandler = useEventHandler();
  const [isFocused, setIsFocused] = useState(false);
  const [minLengthError, setMinLengthError] = useState<string | undefined>(undefined);
  const [patternError, setPatternError] = useState<string | undefined>(undefined);
  const inputRef = useRef<HTMLInputElement | HTMLTextAreaElement | null>(null);

  // Normalize null/undefined to empty string for display (HTML inputs can't have null values)
  const serverValue = value ?? "";
  const [localValue, setLocalValue] = useOptimisticValue(
    serverValue,
    isFocused,
    (a: string, b: string) => a === b,
  );

  useShortcut(id, shortcutKey, () => {
    if (inputRef.current) {
      inputRef.current.focus();
      setIsFocused(true);
      if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
    }
  });

  const { isRecording, startRecording, stopRecording } = useDictation({
    dictationUploadUrl,
  });

  // Handle transcription results pushed from the server
  const lastVersionRef = useRef(dictationTranscriptionVersion ?? 0);
  useEffect(() => {
    const version = dictationTranscriptionVersion ?? 0;
    if (version > lastVersionRef.current && dictationTranscription) {
      lastVersionRef.current = version;
      const current = localValue;
      const separator = current.length > 0 && !current.endsWith(" ") ? " " : "";
      const newValue = current + separator + dictationTranscription;
      setLocalValue(newValue);
      if (events.includes("OnChange")) eventHandler("OnChange", id, [newValue]);
    }
  }, [
    dictationTranscriptionVersion,
    dictationTranscription,
    localValue,
    setLocalValue,
    events,
    eventHandler,
    id,
  ]);

  const handleDictationToggle = useCallback(() => {
    if (isRecording) {
      stopRecording();
    } else {
      startRecording();
    }
  }, [isRecording, startRecording, stopRecording]);

  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>) => {
      const newValue = e.target.value;
      setLocalValue(newValue);
      // Clear the minLength error as soon as the value satisfies the constraint
      if (minLength !== undefined && newValue.length >= minLength) {
        setMinLengthError(undefined);
      }
      // Clear the pattern error as soon as the value matches the pattern
      if (pattern && newValue.length > 0) {
        try {
          if (new RegExp(pattern).test(newValue)) {
            setPatternError(undefined);
          }
        } catch {
          // Invalid regex — ignore
        }
      } else if (pattern && newValue.length === 0) {
        setPatternError(undefined);
      }
      if (events.includes("OnChange")) eventHandler("OnChange", id, [newValue]);
    },
    [eventHandler, id, events, minLength, pattern, setLocalValue],
  );

  const handleBlur = useCallback(() => {
    setIsFocused(false);
    // Show validation error if value is non-empty but below the minimum length
    if (minLength !== undefined && localValue.length > 0 && localValue.length < minLength) {
      setMinLengthError(`Minimum ${minLength} characters required`);
    }
    // Show validation error if value is non-empty but doesn't match the pattern
    if (pattern && localValue.length > 0) {
      try {
        if (!new RegExp(pattern).test(localValue)) {
          setPatternError("Please match the requested format");
        }
      } catch {
        // Invalid regex — ignore
      }
    }
    if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
  }, [eventHandler, id, events, minLength, pattern, localValue]);

  const handleSubmit = useCallback(() => {
    if (events.includes("OnSubmit")) eventHandler("OnSubmit", id, []);
  }, [eventHandler, id, events]);

  const handleFocus = useCallback(() => {
    setIsFocused(true);
    if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
  }, [eventHandler, id, events]);

  const handleClear = useCallback(
    (e: React.MouseEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!events.includes("OnChange")) return;
      if (disabled) return;
      // For nullable inputs, set to null; otherwise set to empty string
      const clearedValue = nullable ? null : "";
      setLocalValue(clearedValue ?? "");
      eventHandler("OnChange", id, [clearedValue]);
    },
    [eventHandler, id, events, disabled, nullable, setLocalValue],
  );

  // Server-provided `invalid` takes precedence; fall back to local validation errors
  const effectiveInvalid = invalid ?? minLengthError ?? patternError;

  const commonProps = useMemo(
    () => ({
      id,
      placeholder,
      value: localValue,
      disabled,
      invalid: effectiveInvalid,
      nullable,
      width,
      height,
      events,
      shortcutKey,
      density,
      slots,
      maxLength,
      minLength,
      pattern,
      rows,
      autoFocus,
      ghost,
      dictation,
      isRecording,
      onDictationToggle: handleDictationToggle,
      "data-testid": dataTestId,
    }),
    [
      id,
      placeholder,
      localValue,
      disabled,
      effectiveInvalid,
      nullable,
      events,
      width,
      height,
      shortcutKey,
      density,
      slots,
      maxLength,
      minLength,
      pattern,
      rows,
      autoFocus,
      ghost,
      dictation,
      isRecording,
      handleDictationToggle,
      dataTestId,
    ],
  );

  switch (variant) {
    case TextInputVariant.Password:
      return (
        <PasswordVariant
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          onClear={handleClear}
          onSubmit={handleSubmit}
          inputRef={inputRef}
          density={density}
        />
      );
    case TextInputVariant.Textarea:
      return (
        <TextareaVariant
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          onClear={handleClear}
          onSubmit={handleSubmit}
          inputRef={inputRef}
          isFocused={isFocused}
          density={density}
        />
      );
    case TextInputVariant.Search:
      return (
        <SearchVariant
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          onClear={handleClear}
          onSubmit={handleSubmit}
          inputRef={inputRef}
          isFocused={isFocused}
          density={density}
        />
      );
    default:
      return (
        <DefaultVariant
          type={variant.toLowerCase() as Lowercase<TextInputVariant>}
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          onClear={handleClear}
          onSubmit={handleSubmit}
          inputRef={inputRef}
          isFocused={isFocused}
          density={density}
        />
      );
  }
};
