import React, { useCallback, useState, useRef, useEffect } from 'react';
import { Input } from '@/components/ui/input';
import { Upload, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { getWidth } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Densities } from '@/types/density';
import { useEventHandler } from '@/components/event-handler';
import { toast } from '@/hooks/use-toast';
import {
  fileInputVariant,
  uploadIconVariant,
  textVariant,
} from '@/components/ui/input/file-input-variant';
import { validateSingleFile, validateFileCount } from './file-input-validation';

const EMPTY_ARRAY: never[] = [];

enum FileInputStatus {
  Pending = 'Pending',
  Aborted = 'Aborted',
  Loading = 'Loading',
  Failed = 'Failed',
  Finished = 'Finished',
}

interface FileInput {
  id: string;
  fileName: string;
  contentType: string;
  length: number;
  progress: number;
  status: FileInputStatus;
}

interface FileInputWidgetProps {
  id: string;
  value?: FileInput | FileInput[] | null;
  disabled: boolean;
  invalid?: string;
  events: string[];
  width?: string;
  accept?: string;
  maxFileSize?: number;
  minFileSize?: number;
  multiple?: boolean;
  maxFiles?: number;
  placeholder?: string;
  uploadUrl?: string;
  density?: Densities;
}

export const FileInputWidget: React.FC<FileInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  events = EMPTY_ARRAY,
  width,
  accept,
  maxFileSize,
  minFileSize,
  multiple = false,
  maxFiles,
  placeholder,
  uploadUrl,
  density = Densities.Medium,
}) => {
  const handleEvent = useEventHandler();
  const [isDragging, setIsDragging] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const filesSelectedInCurrentDialogRef = useRef(false);
  const dialogWasOpenRef = useRef(false);
  const blurFiredRef = useRef(false);

  // Be defensive in case events is undefined at runtime
  const hasCancelHandler = Array.isArray(events) && events.includes('OnCancel');
  const hasBlurHandler = Array.isArray(events) && events.includes('OnBlur');

  const validateFile = useCallback(
    (file: File): boolean => {
      const result = validateSingleFile({
        file,
        accept,
        maxFileSize,
        minFileSize,
      });

      if (!result.valid) {
        toast({
          title: result.title || 'Validation Error',
          description: result.error,
          variant: 'destructive',
        });
        return false;
      }
      return true;
    },
    [accept, maxFileSize, minFileSize]
  );

  const uploadFile = useCallback(
    async (file: File): Promise<void> => {
      if (!uploadUrl) return;

      // Validate file before upload - show toast on error
      if (!validateFile(file)) {
        return;
      }

      // Get the correct host from meta tag or use relative URL
      const getUploadUrl = () => {
        const ivyHostMeta = document.querySelector('meta[name="ivy-host"]');
        if (ivyHostMeta) {
          const host = ivyHostMeta.getAttribute('content');
          return host + uploadUrl;
        }
        // If no meta tag, use relative URL (should work in production)
        return uploadUrl;
      };

      const formData = new FormData();
      formData.append('file', file);

      try {
        const response = await fetch(getUploadUrl(), {
          method: 'POST',
          body: formData,
        });

        if (!response.ok) {
          throw new Error(`Upload failed: ${response.statusText}`);
        }
      } catch (error) {
        console.error('File upload error:', error);
      }
    },
    [uploadUrl, validateFile]
  );

  const handleBlur = useCallback(() => {
    if (hasBlurHandler) {
      handleEvent('OnBlur', id, []);
    }
  }, [hasBlurHandler, handleEvent, id]);

  const handleChange = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = e.target.files;
      if (!files || files.length === 0) {
        // No files selected - dialog was likely cancelled
        // Blur will be handled by the window focus event listener
        filesSelectedInCurrentDialogRef.current = false;
        return;
      }

      // Mark that files were selected in this dialog session
      filesSelectedInCurrentDialogRef.current = true;

      // Check max files limit (including already uploaded files)
      const currentFileCount = Array.isArray(value)
        ? value.length
        : value
          ? 1
          : 0;

      const countValidation = validateFileCount(
        currentFileCount,
        files.length,
        maxFiles
      );
      if (!countValidation.valid) {
        toast({
          title: countValidation.title || 'Too many files',
          description: countValidation.error,
          variant: 'destructive',
        });
        e.target.value = '';
        return;
      }

      if (multiple) {
        await Promise.all(Array.from(files).map(uploadFile));
      } else {
        await uploadFile(files[0]);
      }

      // Reset the input so selecting the same file again triggers onChange
      e.target.value = '';

      // Dialog closed after file selection - trigger blur after upload completes
      // This ensures the server state is updated before blur fires
      // Only fire blur if window focus handler hasn't already fired it
      if (!blurFiredRef.current) {
        blurFiredRef.current = true;
        handleBlur();
      }
    },
    [multiple, uploadFile, maxFiles, value, handleBlur]
  );

  // Detect when file dialog closes without selection (cancel case only)
  useEffect(() => {
    if (!hasBlurHandler) return;

    const handleWindowFocus = () => {
      if (dialogWasOpenRef.current) {
        dialogWasOpenRef.current = false;
        // Use queueMicrotask to allow onChange to run first
        // This prevents double blur when files are selected
        queueMicrotask(() => {
          // Check if files were actually selected by looking at the flag
          // If files were selected, blur will be handled by handleChange after upload
          // Only fire blur if no files were selected (cancel case) and we haven't already fired
          if (
            !filesSelectedInCurrentDialogRef.current &&
            !blurFiredRef.current
          ) {
            blurFiredRef.current = true;
            handleEvent('OnBlur', id, []);
          }
        });
      }
    };

    window.addEventListener('focus', handleWindowFocus);

    return () => {
      window.removeEventListener('focus', handleWindowFocus);
    };
  }, [hasBlurHandler, handleEvent, id]);

  const handleCancel = useCallback(
    (fileId: string) => {
      if (hasCancelHandler) {
        handleEvent('OnCancel', id, [fileId]);
      }
      // Also clear file input to allow re-selecting same file
      if (inputRef.current) {
        inputRef.current.value = '';
      }
    },
    [hasCancelHandler, handleEvent, id]
  );

  const handleDragEnter = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!disabled) {
        setIsDragging(true);
      }
    },
    [disabled]
  );

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  }, []);

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  }, []);

  const handleDrop = useCallback(
    async (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      setIsDragging(false);

      if (disabled) return;

      const files = Array.from(e.dataTransfer.files);
      if (files.length === 0) return;

      // Check max files limit (including already uploaded files)
      const currentFileCount = Array.isArray(value)
        ? value.length
        : value
          ? 1
          : 0;

      const countValidation = validateFileCount(
        currentFileCount,
        files.length,
        maxFiles
      );
      if (!countValidation.valid) {
        toast({
          title: countValidation.title || 'Too many files',
          description: countValidation.error,
          variant: 'destructive',
        });
        return;
      }

      if (multiple) {
        await Promise.all(files.map(uploadFile));
      } else {
        await uploadFile(files[0]);
      }
    },
    [multiple, disabled, uploadFile, maxFiles, value]
  );

  const handleClick = useCallback(
    (e: React.MouseEvent) => {
      // Don't trigger file selection if clicking on a file item or button
      const target = e.target as HTMLElement;
      if (target.closest('button') || target.closest('[data-file-item]')) {
        return;
      }

      if (!disabled && inputRef.current) {
        // Track when dialog opens (for cancel detection)
        if (hasBlurHandler) {
          dialogWasOpenRef.current = true;
          filesSelectedInCurrentDialogRef.current = false;
          blurFiredRef.current = false;
        }
        inputRef.current.click();
      }
    },
    [disabled, hasBlurHandler]
  );

  // Render individual file item for multiple files view
  const renderFileItem = (file: FileInput) => {
    const isFileLoading = file.status === FileInputStatus.Loading;
    const fileProgress = file.progress ?? 0;

    return (
      <div
        key={file.id}
        data-file-item
        className="flex items-center gap-3 p-3 border border-muted-foreground/25 rounded-md bg-transparent"
      >
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium truncate">{file.fileName}</p>
          {isFileLoading && (
            <div className="mt-2">
              <div className="w-full bg-muted rounded-full h-1.5">
                <div
                  className="bg-primary h-1.5 rounded-full transition-all duration-300"
                  style={{ width: `${fileProgress * 100}%` }}
                />
              </div>
            </div>
          )}
        </div>
        {hasCancelHandler && (
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="h-8 w-8 shrink-0"
            onClick={e => {
              e.stopPropagation();
              handleCancel(file.id);
            }}
          >
            <X className="h-4 w-4" />
          </Button>
        )}
      </div>
    );
  };

  // Check if we have any files to display
  const hasFiles = value && (Array.isArray(value) ? value.length > 0 : true);
  const fileList = Array.isArray(value) ? value : value ? [value] : [];

  return (
    <div
      className="relative"
      style={{ ...getWidth(width) }}
      onDragEnter={handleDragEnter}
      onDragLeave={handleDragLeave}
      onDragOver={handleDragOver}
      onDrop={handleDrop}
    >
      {/* Invalid icon in top right corner - only for required field validation */}
      {invalid && (
        <div className="absolute top-2 right-2 z-20 pointer-events-none">
          <InvalidIcon message={invalid} />
        </div>
      )}
      <div
        className={cn(
          fileInputVariant({ density }),
          isDragging && !disabled
            ? 'border-primary bg-primary/5'
            : 'border-muted-foreground/25',
          disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
          hasFiles ? 'overflow-y-auto' : 'flex items-center justify-center',
          'p-4'
        )}
        onClick={handleClick}
        role="button"
        tabIndex={disabled ? -1 : 0}
        onKeyDown={e => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            handleClick(e as unknown as React.MouseEvent);
          }
        }}
      >
        <Input
          ref={inputRef}
          type="file"
          id={id}
          accept={accept}
          multiple={multiple}
          onChange={handleChange}
          disabled={disabled}
          className="hidden"
        />

        {/* Always show upload icon */}
        <div className="flex flex-col items-center justify-center text-center w-full">
          <Upload className={uploadIconVariant({ density })} />
          {!hasFiles && (
            <p className={textVariant({ density })}>
              {placeholder ||
                `Drag and drop your ${multiple ? 'files' : 'file'} here or click to select`}
            </p>
          )}
        </div>

        {/* Show file list when files are present */}
        {hasFiles && (
          <div className="space-y-2 w-full mt-4">
            {fileList.map(file => renderFileItem(file))}
          </div>
        )}
      </div>
    </div>
  );
};
