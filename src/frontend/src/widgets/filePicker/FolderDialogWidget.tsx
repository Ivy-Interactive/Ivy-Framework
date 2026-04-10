import React, { useCallback, useEffect, useRef } from "react";
import { useEventHandler } from "@/components/event-handler";
import { getHasDirectoryPicker, pickDirectoryFullPath } from "./browserSupport";
import { EMPTY_ARRAY } from "@/lib/constants";

interface FolderDialogEntry {
  name: string;
  kind: string;
  relativePath: string;
}

interface FolderDialogWidgetProps {
  id: string;
  triggerCount: number;
  events: string[];
}

export const FolderDialogWidget: React.FC<FolderDialogWidgetProps> = ({
  id,
  triggerCount,
  events = EMPTY_ARRAY,
}) => {
  const handleEvent = useEventHandler();
  const inputRef = useRef<HTMLInputElement>(null);
  const lastTriggerRef = useRef(0);
  const pendingDialogRef = useRef(false);

  const hasOnCancel = Array.isArray(events) && events.includes("OnCancel");
  const hasOnFolderSelected = Array.isArray(events) && events.includes("OnFolderSelected");

  const openModernDialog = useCallback(async () => {
    try {
      const result = await pickDirectoryFullPath();
      if (result.kind !== "selected") {
        if (result.kind === "cancelled" && hasOnCancel) {
          handleEvent("OnCancel", id, []);
        }
        return;
      }

      // Note: pickDirectoryFullPath returns a custom result, but FolderDialogWidget
      // expects to iterate over entries. Since the desktop bridge returns a single folder,
      // we wrap it in a mock handle for backward compatibility in this widget.
      const entries: FolderDialogEntry[] = [
        {
          name: result.name,
          kind: "directory",
          relativePath: result.path || result.name,
        },
      ];

      if (hasOnFolderSelected) {
        handleEvent("OnFolderSelected", id, [entries]);
      }
    } catch (err: unknown) {
      if (err instanceof DOMException && err.name === "AbortError") {
        if (hasOnCancel) {
          handleEvent("OnCancel", id, []);
        }
      } else {
        console.error("Folder dialog error:", err);
      }
    }
  }, [hasOnFolderSelected, hasOnCancel, handleEvent, id]);

  const openFallbackDialog = useCallback(() => {
    if (!inputRef.current) return;

    pendingDialogRef.current = true;

    const onFocus = () => {
      setTimeout(() => {
        if (pendingDialogRef.current) {
          pendingDialogRef.current = false;
          if (hasOnCancel) {
            handleEvent("OnCancel", id, []);
          }
        }
      }, 300);
      window.removeEventListener("focus", onFocus);
    };
    window.addEventListener("focus", onFocus);

    inputRef.current.click();
  }, [hasOnCancel, handleEvent, id]);

  const handleInputChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      pendingDialogRef.current = false;
      const fileList = e.target.files;
      if (!fileList || fileList.length === 0) return;

      // Extract folder entries from the file list
      const entryMap = new Map<string, FolderDialogEntry>();
      const files = Array.from(fileList);

      for (const file of files) {
        const relativePath =
          (file as File & { webkitRelativePath?: string }).webkitRelativePath || file.name;

        // Add the file entry
        entryMap.set(relativePath, {
          name: file.name,
          kind: "file",
          relativePath,
        });

        // Extract and add parent directory entries
        const parts = relativePath.split("/");
        for (let i = 1; i < parts.length; i++) {
          const dirPath = parts.slice(0, i).join("/");
          if (!entryMap.has(dirPath)) {
            entryMap.set(dirPath, {
              name: parts[i - 1],
              kind: "directory",
              relativePath: dirPath,
            });
          }
        }
      }

      if (hasOnFolderSelected) {
        handleEvent("OnFolderSelected", id, [Array.from(entryMap.values())]);
      }

      // Reset input
      e.target.value = "";
    },
    [hasOnFolderSelected, handleEvent, id],
  );

  // Watch triggerCount for changes to open dialog
  useEffect(() => {
    if (triggerCount > lastTriggerRef.current) {
      lastTriggerRef.current = triggerCount;

      if (getHasDirectoryPicker()) {
        openModernDialog();
      } else {
        openFallbackDialog();
      }
    }
  }, [triggerCount, openModernDialog, openFallbackDialog]);

  return (
    <div className="hidden">
      {!getHasDirectoryPicker() && (
        <input
          ref={inputRef}
          type="file"
          // @ts-expect-error webkitdirectory is non-standard but widely supported
          webkitdirectory=""
          onChange={handleInputChange}
        />
      )}
    </div>
  );
};
