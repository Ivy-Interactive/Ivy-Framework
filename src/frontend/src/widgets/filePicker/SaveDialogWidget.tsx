import React, { useCallback, useEffect, useRef } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { hasSaveFilePicker } from './browserSupport';
import { getFullUrl, acceptToPickerTypes } from './shared';

interface SaveDialogWidgetProps {
  id: string;
  triggerCount: number;
  suggestedName?: string;
  accept?: string;
  downloadUrl?: string;
  events: string[];
}

export const SaveDialogWidget: React.FC<SaveDialogWidgetProps> = ({
  id,
  triggerCount,
  suggestedName,
  accept,
  downloadUrl,
  events = [],
}) => {
  const handleEvent = useEventHandler();
  const lastTriggerRef = useRef(0);

  const hasOnCancel = Array.isArray(events) && events.includes('OnCancel');
  const hasOnSaved = Array.isArray(events) && events.includes('OnSaved');

  const fetchContent = useCallback(async (): Promise<Blob | null> => {
    if (!downloadUrl) return null;
    try {
      const response = await fetch(getFullUrl(downloadUrl));
      if (!response.ok) {
        throw new Error(`Download failed: ${response.statusText}`);
      }
      return await response.blob();
    } catch (error) {
      console.error('Download error:', error);
      return null;
    }
  }, [downloadUrl]);

  const saveModern = useCallback(async () => {
    try {
      const blob = await fetchContent();
      if (!blob) return;

      const options: SaveFilePickerOptions = {};
      if (suggestedName) {
        options.suggestedName = suggestedName;
      }
      const types = acceptToPickerTypes(accept);
      if (types) {
        options.types = types;
      }

      const handle = await showSaveFilePicker(options);
      const writable = await handle.createWritable();
      await writable.write(blob);
      await writable.close();

      if (hasOnSaved) {
        handleEvent('OnSaved', id, [{ success: true, fileName: handle.name }]);
      }
    } catch (err: unknown) {
      if (err instanceof DOMException && err.name === 'AbortError') {
        if (hasOnCancel) {
          handleEvent('OnCancel', id, []);
        }
      } else {
        console.error('Save dialog error:', err);
      }
    }
  }, [
    fetchContent,
    suggestedName,
    accept,
    hasOnSaved,
    hasOnCancel,
    handleEvent,
    id,
  ]);

  const saveFallback = useCallback(async () => {
    const blob = await fetchContent();
    if (!blob) return;

    const blobUrl = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = blobUrl;
    a.download = suggestedName || 'download';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(blobUrl);

    // In fallback mode we can't detect cancel, so always report success
    if (hasOnSaved) {
      handleEvent('OnSaved', id, [
        { success: true, fileName: suggestedName || 'download' },
      ]);
    }
  }, [fetchContent, suggestedName, hasOnSaved, handleEvent, id]);

  // Watch triggerCount for changes to open dialog
  useEffect(() => {
    if (triggerCount > lastTriggerRef.current) {
      lastTriggerRef.current = triggerCount;

      if (hasSaveFilePicker) {
        saveModern();
      } else {
        saveFallback();
      }
    }
  }, [triggerCount, saveModern, saveFallback]);

  // Renders nothing visible
  return null;
};
