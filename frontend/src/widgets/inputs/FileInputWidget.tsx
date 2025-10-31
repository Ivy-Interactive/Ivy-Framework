import React, { useCallback, useState, useRef } from 'react';
import { Input } from '@/components/ui/input';
import { Upload, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { getWidth } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Sizes } from '@/types/sizes';
import { useEventHandler } from '@/components/event-handler';
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
  multiple?: boolean;
  maxFiles?: number;
  placeholder?: string;
  uploadUrl?: string;
  size?: Sizes;
}

export const FileInputWidget: React.FC<FileInputWidgetProps> = ({
  id,
  value,
  disabled,
  invalid,
  events,
  width,
  accept,
  multiple = false,
  maxFiles,
  placeholder,
  uploadUrl,
  size = Sizes.Medium,
}) => {
  const handleEvent = useEventHandler();
  const [isDragging, setIsDragging] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  const hasCancelHandler = events.includes('OnCancel');

  const uploadFile = useCallback(
    async (file: File): Promise<void> => {
      if (!uploadUrl) return;

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
    [uploadUrl]
  );

  const handleChange = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = e.target.files;
      if (!files) return;

      // Check max files limit
      if (maxFiles && files.length > maxFiles) {
        // Only take the first maxFiles files
        const limitedFiles = Array.from(files).slice(0, maxFiles);
        if (multiple) {
          await Promise.all(limitedFiles.map(uploadFile));
        } else {
          await uploadFile(limitedFiles[0]);
        }
        // Reset the input so selecting the same file again triggers onChange
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
    },
    [multiple, uploadFile, maxFiles]
  );

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

      // Check max files limit
      if (maxFiles && files.length > maxFiles) {
        // Only take the first maxFiles files
        const limitedFiles = files.slice(0, maxFiles);
        if (multiple) {
          await Promise.all(limitedFiles.map(uploadFile));
        } else {
          await uploadFile(limitedFiles[0]);
        }
        return;
      }

      if (multiple) {
        await Promise.all(files.map(uploadFile));
      } else {
        await uploadFile(files[0]);
      }
    },
    [multiple, disabled, uploadFile, maxFiles]
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
        className="flex items-center gap-3 p-3 border border-muted-foreground/25 rounded-md bg-background"
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
            className="h-8 w-8 flex-shrink-0"
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
      {/* Invalid icon in top right corner, above input */}
      {invalid && (
        <div className="absolute top-2 right-2 z-20 pointer-events-none">
          <InvalidIcon message={invalid} />
        </div>
      )}
      <div
        className={cn(
          fileInputVariants({ size }),
          isDragging && !disabled
            ? 'border-primary bg-primary/5'
            : 'border-muted-foreground/25',
          disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
          hasFiles ? 'overflow-y-auto' : ''
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
          disabled={disabled}
          className="hidden"
        />

        {/* Show upload prompt when no files */}
        {!hasFiles && (
          <div className="absolute inset-0 flex flex-col items-center justify-center text-center">
            <Upload className={uploadIconVariants({ size })} />
            <p className={textVariants({ size })}>
              {placeholder ||
                `Drag and drop your ${multiple ? 'files' : 'file'} here or click to select`}
            </p>
          </div>
        )}

        {/* Show file list when files are present */}
        {hasFiles && (
          <div className="space-y-2 w-full">
            {fileList.map(file => renderFileItem(file))}
          </div>
        )}
      </div>
    </div>
  );
};
