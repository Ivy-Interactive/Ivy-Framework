import React, { useCallback, useState, useRef, useEffect } from 'react';
import { Input } from '@/components/ui/input';
import { Upload, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { getWidth } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Scales } from '@/types/scale';
import { useEventHandler } from '@/components/event-handler';
import { toast } from '@/hooks/use-toast';
import {
  fileInputVariants,
  uploadIconVariants,
  textVariants,
} from '@/components/ui/input/file-input-variants';

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
  multiple?: boolean;
  maxFiles?: number;
  placeholder?: string;
  uploadUrl?: string;
  scale?: Scales;
}

export const FileInputWidget: React.FC<FileInputWidgetProps> = ({
  id,
  value,
  disabled,
  invalid,
  events,
  width,
  accept,
  maxFileSize,
  multiple = false,
  maxFiles,
  placeholder,
  uploadUrl,
  scale = Scales.Medium,
}) => {
  const handleEvent = useEventHandler();
  const [isDragging, setIsDragging] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  // Be defensive in case events is undefined at runtime
  const hasCancelHandler = Array.isArray(events) && events.includes('OnCancel');
  const hasBlurHandler = Array.isArray(events) && events.includes('OnBlur');

  const formatBytes = (bytes: number): string => {
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    if (bytes === 0) return '0 B';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    const size = bytes / Math.pow(1024, i);
    return `${size.toFixed(size >= 10 ? 0 : 2)} ${sizes[i]}`;
  };

  const validateFile = useCallback(
    (file: File): boolean => {
      // Validate file size
      if (maxFileSize && file.size > maxFileSize) {
        const maxSizeFormatted = formatBytes(maxFileSize);
        const fileSizeFormatted = formatBytes(file.size);
        toast({
          title: 'File too large',
          description: `File '${file.name}' is ${fileSizeFormatted}. Maximum allowed size is ${maxSizeFormatted}.`,
          variant: 'destructive',
        });
        return false;
      }
      return true;
    },
    [maxFileSize]
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
        // Blur will be handled by the input's blur event
        return;
      }

      // Check max files limit (including already uploaded files)
      const currentFileCount = Array.isArray(value)
        ? value.length
        : value
          ? 1
          : 0;
      if (maxFiles && currentFileCount + files.length > maxFiles) {
        const remaining = maxFiles - currentFileCount;
        toast({
          title: 'Too many files',
          description:
            remaining > 0
              ? `You can only upload ${remaining} more file${remaining !== 1 ? 's' : ''}. Maximum is ${maxFiles} files total.`
              : `Maximum of ${maxFiles} file${maxFiles !== 1 ? 's' : ''} already reached.`,
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

      // Dialog closed after file selection - trigger blur
      if (hasBlurHandler) {
        handleBlur();
      }
    },
    [multiple, uploadFile, maxFiles, value, hasBlurHandler, handleBlur]
  );

  // Detect when file dialog closes (for cancel case)
  useEffect(() => {
    if (!hasBlurHandler) return;

    const inputElement = inputRef.current;
    let dialogWasOpen = false;

    const handleInputFocus = () => {
      dialogWasOpen = true;
    };

    const handleWindowFocus = () => {
      if (dialogWasOpen) {
        dialogWasOpen = false;
        // Dialog closed - fire blur
        handleBlur();
      }
    };

    if (inputElement) {
      inputElement.addEventListener('focus', handleInputFocus);
    }
    window.addEventListener('focus', handleWindowFocus);

    return () => {
      if (inputElement) {
        inputElement.removeEventListener('focus', handleInputFocus);
      }
      window.removeEventListener('focus', handleWindowFocus);
    };
  }, [hasBlurHandler, handleBlur]);

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
      if (maxFiles && currentFileCount + files.length > maxFiles) {
        const remaining = maxFiles - currentFileCount;
        toast({
          title: 'Too many files',
          description:
            remaining > 0
              ? `You can only upload ${remaining} more file${remaining !== 1 ? 's' : ''}. Maximum is ${maxFiles} files total.`
              : `Maximum of ${maxFiles} file${maxFiles !== 1 ? 's' : ''} already reached.`,
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
        inputRef.current.click();
      }
    },
    [disabled]
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
          fileInputVariants({ scale }),
          isDragging && !disabled
            ? 'border-primary bg-primary/5'
            : 'border-muted-foreground/25',
          disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
          hasFiles ? 'overflow-y-auto' : 'flex items-center justify-center',
          'p-4'
        )}
        onClick={handleClick}
      >
        <Input
          ref={inputRef}
          type="file"
          id={id}
          accept={accept}
          multiple={multiple}
          onChange={handleChange}
          onBlur={hasBlurHandler ? handleBlur : undefined}
          disabled={disabled}
          className="hidden"
        />

        {/* Always show upload icon */}
        <div className="flex flex-col items-center justify-center text-center w-full">
          <Upload className={uploadIconVariants({ scale })} />
          {!hasFiles && (
            <p className={textVariants({ scale })}>
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
