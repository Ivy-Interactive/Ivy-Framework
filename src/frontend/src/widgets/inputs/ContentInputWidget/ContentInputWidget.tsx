import React, { useCallback, useRef, useEffect } from "react";
import { Paperclip } from "lucide-react";
import { cn } from "@/lib/utils";
import { getWidth } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import { useEventHandler } from "@/components/event-handler";
import { textareaSizeVariant } from "@/components/ui/input/text-input-variant";
import { useOptimisticValue } from "../shared/useOptimisticValue";
import { useFileAttachments } from "./useFileAttachments";
import { FileAttachmentList } from "./FileAttachmentList";
import { ContentInputWidgetProps } from "./types";
import { EMPTY_ARRAY } from "@/lib/constants";

export const ContentInputWidget: React.FC<ContentInputWidgetProps> = ({
  id,
  value = "",
  disabled = false,
  invalid,
  placeholder,
  maxLength,
  rows,
  autoFocus,
  events = EMPTY_ARRAY,
  width,
  density = Densities.Medium,
  uploadUrl,
  accept,
  maxFileSize,
  maxFiles,
  files = EMPTY_ARRAY,
}) => {
  const handleEvent = useEventHandler();
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const [isFocused, setIsFocused] = React.useState(false);
  const [localValue, setLocalValue] = useOptimisticValue(value ?? "", isFocused);

  const hasChangeHandler = Array.isArray(events) && events.includes("OnChange");
  const hasBlurHandler = Array.isArray(events) && events.includes("OnBlur");
  const hasFocusHandler = Array.isArray(events) && events.includes("OnFocus");
  const hasSubmitHandler = Array.isArray(events) && events.includes("OnSubmit");
  const hasCancelHandler = Array.isArray(events) && events.includes("OnCancel");

  const fileList = Array.isArray(files) ? files : [];

  const {
    isDragging,
    dragHandlers,
    handlePaste,
    openFilePicker,
    handleFileInputChange,
    fileInputRef,
  } = useFileAttachments({
    uploadUrl,
    accept,
    maxFileSize,
    maxFiles,
    currentFileCount: fileList.length,
    disabled,
  });

  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLTextAreaElement>) => {
      const newValue = e.target.value;
      setLocalValue(newValue);
      if (hasChangeHandler) {
        handleEvent("OnChange", id, [newValue]);
      }
    },
    [hasChangeHandler, handleEvent, id, setLocalValue],
  );

  const handleBlur = useCallback(
    (e: React.FocusEvent) => {
      // Only fire blur when focus leaves the entire component
      if (e.currentTarget.contains(e.relatedTarget)) return;
      setIsFocused(false);
      if (hasBlurHandler) {
        handleEvent("OnBlur", id, []);
      }
    },
    [hasBlurHandler, handleEvent, id],
  );

  const handleFocus = useCallback(
    (e: React.FocusEvent) => {
      // Only fire focus when entering the component from outside
      if (e.currentTarget.contains(e.relatedTarget)) return;
      setIsFocused(true);
      if (hasFocusHandler) {
        handleEvent("OnFocus", id, []);
      }
    },
    [hasFocusHandler, handleEvent, id],
  );

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (hasSubmitHandler && e.key === "Enter" && (e.metaKey || e.ctrlKey)) {
        e.preventDefault();
        handleEvent("OnSubmit", id, []);
      }
    },
    [hasSubmitHandler, handleEvent, id],
  );

  const handleCancel = useCallback(
    (fileId: string) => {
      if (hasCancelHandler) {
        handleEvent("OnCancel", id, [fileId]);
      }
    },
    [hasCancelHandler, handleEvent, id],
  );

  useEffect(() => {
    if (autoFocus && textareaRef.current) {
      textareaRef.current.focus();
    }
  }, [autoFocus]);

  return (
    <div
      className="relative w-full"
      style={{ ...getWidth(width) }}
      onBlur={handleBlur}
      onFocus={handleFocus}
      {...dragHandlers}
    >
      <div
        className={cn(
          "rounded-field border bg-transparent shadow-sm transition-colors dark:bg-white/5 dark:border-white/10",
          isDragging ? "border-primary ring-2 ring-primary/20 border-dashed" : "border-input",
          disabled && "opacity-50 cursor-not-allowed",
          invalid && "border-destructive",
        )}
      >
        <div className="relative">
          <textarea
            ref={textareaRef}
            id={id}
            placeholder={placeholder}
            value={localValue}
            disabled={disabled}
            maxLength={maxLength ?? undefined}
            rows={rows ?? 3}
            autoFocus={autoFocus}
            onChange={handleChange}
            onPaste={handlePaste}
            onKeyDown={handleKeyDown}
            className={cn(
              textareaSizeVariant({ density }),
              "w-full bg-transparent border-0 shadow-none outline-none resize-none",
              "focus:outline-none focus:ring-0",
              "placeholder:text-muted-foreground",
              "disabled:cursor-not-allowed",
              invalid && "pr-8",
            )}
          />
          {invalid && (
            <div className="absolute right-2 top-2 pointer-events-none z-10">
              <InvalidIcon message={invalid} />
            </div>
          )}
        </div>

        {fileList.length > 0 && (
          <FileAttachmentList
            files={fileList}
            onCancel={handleCancel}
            hasCancelHandler={hasCancelHandler}
          />
        )}

        <div className="flex items-center gap-1 px-2 pb-1.5">
          {uploadUrl && (
            <button
              type="button"
              tabIndex={-1}
              disabled={disabled}
              onClick={openFilePicker}
              className={cn(
                "p-1 rounded hover:bg-accent focus:outline-none transition-colors",
                disabled ? "cursor-not-allowed opacity-50" : "cursor-pointer",
              )}
              aria-label="Attach file"
            >
              <Paperclip className="h-4 w-4 text-muted-foreground" />
            </button>
          )}
          {isDragging && <span className="text-xs text-primary ml-1">Drop files here</span>}
        </div>

        <input
          ref={fileInputRef}
          type="file"
          accept={accept}
          multiple={maxFiles !== 1}
          onChange={handleFileInputChange}
          className="hidden"
          tabIndex={-1}
        />
      </div>
    </div>
  );
};
