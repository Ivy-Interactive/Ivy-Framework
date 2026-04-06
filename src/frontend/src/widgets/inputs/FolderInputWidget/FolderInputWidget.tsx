import React, { useCallback, useRef } from "react";
import { useEventHandler } from "@/components/event-handler";
import { InvalidIcon } from "@/components/InvalidIcon";
import { hasDirectoryPicker, pickDirectory } from "@/widgets/filePicker/browserSupport";
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
  events = EMPTY_ARRAY,
  density,
}) => {
  const handleEvent = useEventHandler();
  const inputRef = useRef<HTMLInputElement>(null);

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
    const result = await pickDirectory();
    if (result.kind === "selected") {
      emitChange(result.name);
    }
  }, [emitChange]);

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
          "flex items-center gap-2 bg-background pr-1",
          isInvalid && inputStyles.invalidInput,
          disabled ? "cursor-not-allowed opacity-50" : "cursor-pointer",
        )}
        onClick={handleBrowse}
        onBlur={() => {
          if (hasOnBlur) handleEvent("OnBlur", id, []);
        }}
        onFocus={() => {
          if (hasOnFocus) handleEvent("OnFocus", id, []);
        }}
        tabIndex={disabled ? undefined : 0}
        autoFocus={autoFocus}
      >
        <span className={cn("flex-1 truncate", !value && "text-muted-foreground")}>
          {value || placeholder}
        </span>

        <div className="flex items-center gap-0.5">
          {isInvalid && <InvalidIcon message={invalid} />}

          {showClear && (
            <button
              type="button"
              className="flex h-6 w-6 items-center justify-center rounded-sm text-muted-foreground hover:text-foreground transition-colors"
              onClick={(e) => {
                e.stopPropagation();
                handleClear();
              }}
              tabIndex={-1}
              aria-label="Clear selection"
            >
              <X className="h-3.5 w-3.5" />
            </button>
          )}

          <span className="flex h-6 w-6 items-center justify-center text-muted-foreground">
            <FolderOpen className="h-4 w-4" />
          </span>
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
