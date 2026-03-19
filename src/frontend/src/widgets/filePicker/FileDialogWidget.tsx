import React, { useCallback, useEffect, useRef } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { hasFileSystemAccess } from './browserSupport';
import { validateFile, uploadFile, acceptToPickerTypes } from './shared';

interface FileDialogFileInfo {
  fileName: string;
  contentType: string;
  size: number;
}

interface FileDialogWidgetProps {
  id: string;
  triggerCount: number;
  accept?: string;
  multiple?: boolean;
  maxFileSize?: number;
  minFileSize?: number;
  mode: 'Upload' | 'PathOnly';
  uploadUrl?: string;
  events: string[];
}

export const FileDialogWidget: React.FC<FileDialogWidgetProps> = ({
  id,
  triggerCount,
  accept,
  multiple = false,
  maxFileSize,
  minFileSize,
  mode = 'Upload',
  uploadUrl,
  events = [],
}) => {
  const handleEvent = useEventHandler();
  const inputRef = useRef<HTMLInputElement>(null);
  const lastTriggerRef = useRef(0);
  const pendingDialogRef = useRef(false);

  const hasOnCancel = Array.isArray(events) && events.includes('OnCancel');
  const hasOnFilesSelected =
    Array.isArray(events) && events.includes('OnFilesSelected');

  const fileToInfo = (file: File): FileDialogFileInfo => ({
    fileName: file.name,
    contentType: file.type,
    size: file.size,
  });

  const handleFiles = useCallback(
    async (files: File[]) => {
      // Validate all files
      const validFiles = files.filter(f =>
        validateFile(f, accept, maxFileSize, minFileSize)
      );
      if (validFiles.length === 0) return;

      if (mode === 'Upload' && uploadUrl) {
        // Upload files then fire event
        try {
          for (const file of validFiles) {
            await uploadFile(uploadUrl, file);
          }
          if (hasOnFilesSelected) {
            handleEvent('OnFilesSelected', id, [validFiles.map(fileToInfo)]);
          }
        } catch (error) {
          console.error('File upload error:', error);
        }
      } else {
        // PathOnly mode - just fire event with metadata
        if (hasOnFilesSelected) {
          handleEvent('OnFilesSelected', id, [validFiles.map(fileToInfo)]);
        }
      }
    },
    [
      accept,
      maxFileSize,
      minFileSize,
      mode,
      uploadUrl,
      hasOnFilesSelected,
      handleEvent,
      id,
    ]
  );

  const openModernDialog = useCallback(async () => {
    try {
      const options: OpenFilePickerOptions = {
        multiple,
      };
      const types = acceptToPickerTypes(accept);
      if (types) {
        options.types = types;
      }
      const handles = await showOpenFilePicker(options);
      const files: File[] = [];
      for (const handle of handles) {
        const file = await handle.getFile();
        files.push(file);
      }
      if (files.length > 0) {
        await handleFiles(files);
      }
    } catch (err: unknown) {
      if (err instanceof DOMException && err.name === 'AbortError') {
        // User cancelled
        if (hasOnCancel) {
          handleEvent('OnCancel', id, []);
        }
      } else {
        console.error('File dialog error:', err);
      }
    }
  }, [multiple, accept, handleFiles, hasOnCancel, handleEvent, id]);

  const openFallbackDialog = useCallback(() => {
    if (!inputRef.current) return;

    pendingDialogRef.current = true;

    // Set up cancel detection via window focus
    const onFocus = () => {
      // Use a small delay to let onChange fire first
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
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      pendingDialogRef.current = false;
      const fileList = e.target.files;
      if (!fileList || fileList.length === 0) return;

      const files = Array.from(fileList);
      await handleFiles(files);

      // Reset input so same file can be re-selected
      e.target.value = '';
    },
    [handleFiles]
  );

  // Watch triggerCount for changes to open dialog
  useEffect(() => {
    if (triggerCount > lastTriggerRef.current) {
      lastTriggerRef.current = triggerCount;

      if (hasFileSystemAccess) {
        openModernDialog();
      } else {
        openFallbackDialog();
      }
    }
  }, [triggerCount, openModernDialog, openFallbackDialog]);

  // Render hidden input for fallback path
  if (!hasFileSystemAccess) {
    return (
      <input
        ref={inputRef}
        type="file"
        accept={accept}
        multiple={multiple}
        onChange={handleInputChange}
        style={{ display: 'none' }}
      />
    );
  }

  // Modern path renders nothing
  return null;
};
