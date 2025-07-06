import React, { useCallback, useState, useRef } from 'react';
import { Input } from '@/components/ui/input';
import { useEventHandler } from '@/components/EventHandlerContext';
import { Upload, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { getWidth } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';

interface FileInput {
  name: string;
  size: number;
  type: string;
  lastModified: Date;
  content?: string;
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
}

export const FileInputWidget: React.FC<FileInputWidgetProps> = ({
  id,
  value,
  disabled,
  invalid,
  width,
  events,
  accept,
  multiple = false,
}) => {
  const handleEvent = useEventHandler();
  const [isDragging, setIsDragging] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  const convertFileToUploadFile = async (file: File): Promise<FileInput> => {
    const arrayBuffer = await file.arrayBuffer();
    const base64 = btoa(String.fromCharCode(...new Uint8Array(arrayBuffer)));

    return {
      name: file.name,
      size: file.size,
      type: file.type,
      lastModified: new Date(file.lastModified),
      content: base64,
    };
  };

  const handleChange = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = e.target.files;
      if (!files) return;

      const selectedFiles = multiple
        ? await Promise.all(Array.from(files).map(convertFileToUploadFile))
        : await convertFileToUploadFile(files[0]);

      handleEvent('OnChange', id, [selectedFiles]);
    },
    [id, events, multiple, handleEvent, convertFileToUploadFile]
  );

  const handleClear = useCallback(() => {
    handleEvent('OnChange', id, [null]);
  }, [id, events, handleEvent]);

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

      const selectedFiles = multiple
        ? await Promise.all(files.map(convertFileToUploadFile))
        : await convertFileToUploadFile(files[0]);

      handleEvent('OnChange', id, [selectedFiles]);
    },
    [id, events, multiple, handleEvent, disabled, convertFileToUploadFile]
  );

  const handleClick = useCallback(() => {
    if (!disabled && inputRef.current) {
      inputRef.current.click();
    }
  }, [disabled]);

  const displayValue = value
    ? Array.isArray(value)
      ? value.map(f => f.name).join(', ')
      : value.name
    : '';

  return (
    <div
      className="relative"
      style={{ ...getWidth(width) }}
      onDragEnter={handleDragEnter}
      onDragLeave={handleDragLeave}
      onDragOver={handleDragOver}
      onDrop={handleDrop}
    >
      <div
        className={cn(
          'relative rounded-md border-2 border-dashed transition-colors min-h-[100px]',
          isDragging && !disabled
            ? 'border-primary bg-primary/5'
            : 'border-muted-foreground/25',
          disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'
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
        <div className="absolute inset-0 flex flex-col items-center justify-center p-4 text-center">
          <Upload className="h-6 w-6 mb-2 text-muted-foreground" />
          <p className="text-sm text-muted-foreground">
            {displayValue || (
              <>
                Drag and drop your {multiple ? 'files' : 'file'} here or click
                to select
                {/* {accept && (
                  <span className="block mt-1 text-xs">
                    Accepted file types: {accept}
                  </span>
                )} */}
              </>
            )}
          </p>
        </div>
        {value && !disabled && (
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="absolute right-2 top-2 h-6 w-6 z-10"
            onClick={e => {
              e.stopPropagation();
              handleClear();
            }}
          >
            <X className="h-4 w-4" />
          </Button>
        )}
      </div>
      {invalid && <InvalidIcon message={invalid} />}
    </div>
  );
};
