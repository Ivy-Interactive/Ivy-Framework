import React, { useCallback, useEffect, useRef } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { hasDirectoryPicker } from './browserSupport';

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
  events = [],
}) => {
  const handleEvent = useEventHandler();
  const inputRef = useRef<HTMLInputElement>(null);
  const lastTriggerRef = useRef(0);
  const pendingDialogRef = useRef(false);

  const hasOnCancel = Array.isArray(events) && events.includes('OnCancel');
  const hasOnFolderSelected =
    Array.isArray(events) && events.includes('OnFolderSelected');

  const openModernDialog = useCallback(async () => {
    try {
      const dirHandle = await showDirectoryPicker();
      const entries: FolderDialogEntry[] = [];

      for await (const [name, handle] of dirHandle.entries()) {
        entries.push({
          name,
          kind: handle.kind, // "file" or "directory"
          relativePath: name,
        });
      }

      if (hasOnFolderSelected) {
        handleEvent('OnFolderSelected', id, [entries]);
      }
    } catch (err: unknown) {
      if (err instanceof DOMException && err.name === 'AbortError') {
        if (hasOnCancel) {
          handleEvent('OnCancel', id, []);
        }
      } else {
        console.error('Folder dialog error:', err);
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
            handleEvent('OnCancel', id, []);
          }
        }
      }, 300);
      window.removeEventListener('focus', onFocus);
    };
    window.addEventListener('focus', onFocus);

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
          (file as File & { webkitRelativePath?: string }).webkitRelativePath ||
          file.name;

        // Add the file entry
        entryMap.set(relativePath, {
          name: file.name,
          kind: 'file',
          relativePath,
        });

        // Extract and add parent directory entries
        const parts = relativePath.split('/');
        for (let i = 1; i < parts.length; i++) {
          const dirPath = parts.slice(0, i).join('/');
          if (!entryMap.has(dirPath)) {
            entryMap.set(dirPath, {
              name: parts[i - 1],
              kind: 'directory',
              relativePath: dirPath,
            });
          }
        }
      }

      if (hasOnFolderSelected) {
        handleEvent('OnFolderSelected', id, [Array.from(entryMap.values())]);
      }

      // Reset input
      e.target.value = '';
    },
    [hasOnFolderSelected, handleEvent, id]
  );

  // Watch triggerCount for changes to open dialog
  useEffect(() => {
    if (triggerCount > lastTriggerRef.current) {
      lastTriggerRef.current = triggerCount;

      if (hasDirectoryPicker) {
        openModernDialog();
      } else {
        openFallbackDialog();
      }
    }
  }, [triggerCount, openModernDialog, openFallbackDialog]);

  // Render hidden input for fallback path
  if (!hasDirectoryPicker) {
    return (
      <input
        ref={inputRef}
        type="file"
        // @ts-expect-error webkitdirectory is non-standard but widely supported
        webkitdirectory=""
        onChange={handleInputChange}
        style={{ display: 'none' }}
      />
    );
  }

  // Modern path renders nothing
  return null;
};
