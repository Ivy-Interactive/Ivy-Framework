import React, { useCallback, useEffect, useRef, useState } from "react";
import { useEventHandler } from "@/components/event-handler";
import { InvalidIcon } from "@/components/InvalidIcon";
import {
  hasDirectoryPicker,
  pickDirectory,
  pickDirectoryFullPath,
} from "@/widgets/filePicker/browserSupport";
import { inputVariant } from "@/components/ui/input/variant";
import { inputStyles } from "@/lib/styles";
import { cn } from "@/lib/utils";
import { FolderOpen, X } from "lucide-react";
import { Densities } from "@/types/density";
import { EMPTY_ARRAY } from "@/lib/constants";

interface FolderInputWidgetProps {
  id: string;
  value?: string | null;
  disabled?: boolean;
  invalid?: string;
  placeholder?: string;
  nullable?: boolean;
  autoFocus?: boolean;
  mode?: "Name" | "FullPath";
  events?: string[];
  density?: Densities;
}

export const FolderInputWidget: React.FC<FolderInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  placeholder = "Select a folder...",
  nullable = true,
  autoFocus = false,
  mode = "Name",
  events = EMPTY_ARRAY,
  density,
}) => {
  const handleEvent = useEventHandler();
  const inputRef = useRef<HTMLInputElement>(null);
  const isFullPath = mode === "FullPath";

  const [localValue, setLocalValue] = useState(value ?? "");

  useEffect(() => {
    setLocalValue(value ?? "");
  }, [value]);

  const hasOnChange = Array.isArray(events) && events.includes("OnChange");
  const hasOnBlur = Array.isArray(events) && events.includes("OnBlur");
  const hasOnFocus = Array.isArray(events) && events.includes("OnFocus");

  const emitChange = useCallback(
    (newValue: string | null) => {
      if (hasOnChange) {
        handleEvent("OnChange", id, [newValue]);
      }
    },
    [hasOnChange, handleEvent, id],
  );

  const openModernPicker = useCallback(async () => {
    const result = isFullPath ? await pickDirectoryFullPath() : await pickDirectory();
    if (result.kind === "selected") {
      const picked = isFullPath && result.path ? result.path : result.name;
      setLocalValue(picked);
      emitChange(picked);
    }
  }, [emitChange, isFullPath]);

  const handleFallbackChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const fileList = e.target.files;
      if (!fileList || fileList.length === 0) return;

      const firstFile = fileList[0] as File & { webkitRelativePath?: string };
      const relativePath = firstFile.webkitRelativePath || firstFile.name;
      const folderName = relativePath.split("/")[0] || relativePath;

      emitChange(folderName);
      e.target.value = "";
    },
    [emitChange],
  );

  const handleBrowse = useCallback(() => {
    if (disabled) return;
    if (hasDirectoryPicker) {
      openModernPicker();
    } else {
      inputRef.current?.click();
    }
  }, [disabled, openModernPicker]);

  const handleClear = useCallback(() => {
    if (disabled) return;
    emitChange(null);
  }, [disabled, emitChange]);

  const isInvalid = invalid != null && invalid !== "";
  const showClear = nullable && value != null && value !== "" && !disabled;

  return (
    <div className="relative flex w-full items-center">
      <div
        className={cn(
          inputVariant({ density }),
          "flex items-center gap-2 bg-background dark:bg-white/5 pr-1",
          isInvalid && inputStyles.invalidInput,
          disabled
            ? "cursor-not-allowed opacity-50"
            : isFullPath
              ? "cursor-text"
              : "cursor-pointer",
        )}
        onClick={isFullPath ? undefined : handleBrowse}
        onKeyDown={
          isFullPath
            ? undefined
            : (e) => {
                if (e.key === "Enter" || e.key === " ") {
                  e.preventDefault();
                  handleBrowse();
                }
              }
        }
        role={isFullPath ? undefined : "button"}
        onBlur={
          isFullPath
            ? undefined
            : () => {
                if (hasOnBlur) handleEvent("OnBlur", id, []);
              }
        }
        onFocus={
          isFullPath
            ? undefined
            : () => {
                if (hasOnFocus) handleEvent("OnFocus", id, []);
              }
        }
        tabIndex={isFullPath || disabled ? undefined : 0}
        autoFocus={!isFullPath && autoFocus ? true : undefined}
      >
        {isFullPath ? (
          <input
            type="text"
            className="flex-1 truncate bg-transparent outline-none border-none p-0 text-sm"
            value={localValue}
            placeholder={placeholder}
            onChange={(e) => setLocalValue(e.target.value)}
            onBlur={(e) => {
              emitChange(e.target.value || null);
              if (hasOnBlur) handleEvent("OnBlur", id, []);
            }}
            onFocus={() => {
              if (hasOnFocus) handleEvent("OnFocus", id, []);
            }}
            onClick={(e) => e.stopPropagation()}
            disabled={disabled}
            autoFocus={autoFocus}
          />
        ) : (
          <span className={cn("flex-1 truncate", !value && "text-muted-foreground")}>
            {value || placeholder}
          </span>
        )}

        <div className="flex items-center gap-0.5">
          {isInvalid && <InvalidIcon message={invalid} />}

          {showClear && (
            <button
              type="button"
              className="flex h-6 w-6 items-center justify-center rounded-sm text-muted-foreground hover:text-foreground transition-colors"
              onClick={(e) => {
                e.stopPropagation();
                handleClear();
                if (isFullPath) setLocalValue("");
              }}
              tabIndex={-1}
              aria-label="Clear selection"
            >
              <X className="h-3.5 w-3.5" />
            </button>
          )}

          <button
            type="button"
            className="flex h-6 w-6 items-center justify-center text-muted-foreground hover:text-foreground transition-colors rounded-sm"
            onClick={(e) => {
              e.stopPropagation();
              handleBrowse();
            }}
            tabIndex={-1}
            aria-label="Browse for folder"
            disabled={disabled}
          >
            <FolderOpen className="h-4 w-4" />
          </button>
        </div>
      </div>

      {/* Hidden file input for fallback (non-Chromium) browsers */}
      {!hasDirectoryPicker && (
        <input
          ref={inputRef}
          type="file"
          // @ts-expect-error webkitdirectory is non-standard but widely supported
          webkitdirectory=""
          onChange={handleFallbackChange}
          style={{ display: "none" }}
        />
      )}
    </div>
  );
};
