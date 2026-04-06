import React, { useCallback, useState, useRef, useEffect } from "react";
import { Input } from "@/components/ui/input";
import { Upload } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { getWidth } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import { useEventHandler } from "@/components/event-handler";
import { toast } from "@/hooks/use-toast";
import {
  fileInputVariant,
  uploadIconVariant,
  textVariant,
} from "@/components/ui/input/file-input-variant";
import { validateFileWithToast, validateFileCount } from "./file-input-validation";
import { uploadFileWithProgress } from "@/widgets/filePicker/shared";
import { EMPTY_ARRAY } from "@/lib/constants";
import { FileItem } from "./shared/types";
import { FileAttachmentList } from "./shared/FileAttachmentList";

interface FileInputWidgetProps {
  id: string;
  value?: FileItem | FileItem[] | null;
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
  variant?: "Default" | "Drop";
  autoFocus?: boolean;
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
  variant = "Drop",
  autoFocus,
}) => {
  const handleEvent = useEventHandler();
  const [isDragging, setIsDragging] = useState(false);
  const [uploadProgress, setUploadProgress] = useState<Map<string, number>>(new Map());
  const inputRef = useRef<HTMLInputElement>(null);
  const filesSelectedInCurrentDialogRef = useRef(false);
  const dialogWasOpenRef = useRef(false);
  const blurFiredRef = useRef(false);
  const abortControllersRef = useRef<Map<string, () => void>>(new Map());

  // Abort any pending uploads when the component unmounts
  useEffect(() => {
    return () => {
      abortControllersRef.current.forEach((abort) => abort());
    };
  }, []);

  // Be defensive in case events is undefined at runtime
  const hasCancelHandler = Array.isArray(events) && events.includes("OnCancel");
  const hasBlurHandler = Array.isArray(events) && events.includes("OnBlur");

  const uploadFile = useCallback(
    async (file: File): Promise<void> => {
      if (!uploadUrl) return;

      // Validate file before upload - show toast on error
      if (!validateFileWithToast({ file, accept, maxFileSize, minFileSize })) {
        return;
      }

      const clientFileId = `upload-${crypto.randomUUID()}-${file.size}-${file.name}`;

      setUploadProgress((prev) => new Map(prev).set(clientFileId, 0));

      const { promise, abort } = uploadFileWithProgress(uploadUrl, file, (progress) => {
        setUploadProgress((prev) => new Map(prev).set(clientFileId, progress));
      });

      abortControllersRef.current.set(clientFileId, abort);

      try {
        await promise;
      } catch (error: any) {
        if (error.message !== "Upload aborted") {
          console.error("File upload error:", error);
          toast({
            title: "Upload failed",
            description: error.message || `Could not upload ${file.name}`,
            variant: "destructive",
          });
        }
      } finally {
        setUploadProgress((prev) => {
          const next = new Map(prev);
          next.delete(clientFileId);
          return next;
        });
        abortControllersRef.current.delete(clientFileId);
      }
    },
    [uploadUrl, accept, maxFileSize, minFileSize],
  );

  const handleBlur = useCallback(() => {
    if (hasBlurHandler) {
      handleEvent("OnBlur", id, []);
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
      const currentFileCount = Array.isArray(value) ? value.length : value ? 1 : 0;

      const countValidation = validateFileCount(currentFileCount, files.length, maxFiles);
      if (!countValidation.valid) {
        toast({
          title: countValidation.title || "Too many files",
          description: countValidation.error,
          variant: "destructive",
        });
        e.target.value = "";
        return;
      }

      if (multiple) {
        await Promise.all(Array.from(files).map(uploadFile));
      } else {
        await uploadFile(files[0]);
      }

      // Reset the input so selecting the same file again triggers onChange
      e.target.value = "";

      // Dialog closed after file selection - trigger blur after upload completes
      // This ensures the server state is updated before blur fires
      // Only fire blur if window focus handler hasn't already fired it
      if (!blurFiredRef.current) {
        blurFiredRef.current = true;
        handleBlur();
      }
    },
    [multiple, uploadFile, maxFiles, value, handleBlur],
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
          if (!filesSelectedInCurrentDialogRef.current && !blurFiredRef.current) {
            blurFiredRef.current = true;
            handleEvent("OnBlur", id, []);
          }
        });
      }
    };

    window.addEventListener("focus", handleWindowFocus);

    return () => {
      window.removeEventListener("focus", handleWindowFocus);
    };
  }, [hasBlurHandler, handleEvent, id]);

  const handleCancel = useCallback(
    (fileId: string) => {
      // Check if this is a client-side upload in progress
      const abort = abortControllersRef.current.get(fileId);
      if (abort) {
        abort();
        abortControllersRef.current.delete(fileId);
      } else if (hasCancelHandler) {
        handleEvent("OnCancel", id, [fileId]);
      }
      // Also clear file input to allow re-selecting same file
      if (inputRef.current) {
        inputRef.current.value = "";
      }
    },
    [hasCancelHandler, handleEvent, id],
  );

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  }, []);

  const handleDragOver = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!disabled && !isDragging) {
        setIsDragging(true);
      }
    },
    [disabled, isDragging],
  );

  const handleDragEnter = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!disabled) {
        setIsDragging(true);
      }
    },
    [disabled],
  );

  const handleDrop = useCallback(
    async (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      setIsDragging(false);

      if (disabled) return;

      const files = Array.from(e.dataTransfer.files);
      if (files.length === 0) return;

      // Check max files limit (including already uploaded files)
      const currentFileCount = Array.isArray(value) ? value.length : value ? 1 : 0;

      const countValidation = validateFileCount(currentFileCount, files.length, maxFiles);
      if (!countValidation.valid) {
        toast({
          title: countValidation.title || "Too many files",
          description: countValidation.error,
          variant: "destructive",
        });
        return;
      }

      if (multiple) {
        await Promise.all(files.map(uploadFile));
      } else {
        await uploadFile(files[0]);
      }
    },
    [multiple, disabled, uploadFile, maxFiles, value],
  );

  const openFileDialog = useCallback(() => {
    if (!disabled && inputRef.current) {
      if (hasBlurHandler) {
        dialogWasOpenRef.current = true;
        filesSelectedInCurrentDialogRef.current = false;
        blurFiredRef.current = false;
      }
      inputRef.current.click();
    }
  }, [disabled, hasBlurHandler]);

  const hasAutoFocusedRef = useRef(false);
  useEffect(() => {
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      hasAutoFocusedRef.current = true;
      openFileDialog();
    }
  }, [autoFocus, disabled, openFileDialog]);

  const handleClick = useCallback(
    (e: React.MouseEvent) => {
      // Don't trigger file selection if clicking on a file item or a non-trigger button
      const target = e.target as HTMLElement;
      const button = target.closest("button");

      if (
        (button && !button.hasAttribute("data-file-input-trigger")) ||
        target.closest("[data-file-item]")
      ) {
        return;
      }

      // For Default variant, only the trigger button should open the dialog
      if (variant === "Default" && !target.closest("[data-file-input-trigger]")) {
        return;
      }

      openFileDialog();
    },
    [variant, openFileDialog],
  );

  const handleButtonClick = useCallback(
    (e: React.MouseEvent) => {
      e.stopPropagation();
      handleClick(e);
    },
    [handleClick],
  );

  // Check if we have any files to display
  const hasFiles = value && (Array.isArray(value) ? value.length > 0 : true);
  const hasUploadingFiles = uploadProgress && uploadProgress.size > 0;
  const fileList = Array.isArray(value)
    ? (value as FileItem[])
    : value
      ? ([value] as FileItem[])
      : [];

  const shouldShowFileList = hasFiles || hasUploadingFiles;

  return (
    <div
      className="relative w-full"
      style={{ ...getWidth(width) }}
      onDragEnter={handleDragEnter}
      onDragLeave={handleDragLeave}
      onDragOver={handleDragOver}
      onDrop={handleDrop}
      onBlur={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          if (events?.includes("OnBlur")) handleEvent("OnBlur", id, []);
        }
      }}
      onFocus={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          if (events?.includes("OnFocus")) handleEvent("OnFocus", id, []);
        }
      }}
    >
      {/* Invalid icon in top right corner - only for required field validation */}
      {invalid && variant === "Drop" && (
        <div className="absolute top-2 right-2 z-20 pointer-events-none">
          <InvalidIcon message={invalid} />
        </div>
      )}

      <div
        className={cn(
          fileInputVariant({ variant, density, isDragging }),
          disabled ? "opacity-50 cursor-not-allowed" : "cursor-default",
        )}
        onClick={handleClick}
        role="button"
        tabIndex={disabled ? -1 : 0}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            openFileDialog();
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

        {variant === "Default" ? (
          <div className="flex flex-col gap-2 w-full">
            <div className="flex items-center gap-2">
              <Button
                type="button"
                variant="outline"
                size={
                  density === Densities.Small
                    ? "sm"
                    : density === Densities.Large
                      ? "lg"
                      : "default"
                }
                className={cn(
                  "flex items-center gap-2",
                  isDragging && "border-primary ring-2 ring-primary",
                )}
                disabled={disabled}
                data-file-input-trigger
                onClick={handleButtonClick}
              >
                <Upload className="h-4 w-4" />
                {placeholder || `Select ${multiple ? "files" : "file"}`}
              </Button>
              {invalid && (
                <div className="pointer-events-none">
                  <InvalidIcon message={invalid} />
                </div>
              )}
            </div>
            {shouldShowFileList && (
              <div className="w-full">
                <FileAttachmentList
                  files={fileList}
                  uploadProgress={uploadProgress}
                  onCancel={handleCancel}
                  hasCancelHandler={hasCancelHandler}
                  variant="card"
                />
              </div>
            )}
          </div>
        ) : (
          <div
            className={cn(
              "flex flex-col items-center justify-center text-center w-full",
              shouldShowFileList ? "p-0" : "p-4",
            )}
          >
            <Upload className={uploadIconVariant({ density })} />
            {!shouldShowFileList && (
              <p className={textVariant({ density })}>
                {placeholder ||
                  `Drag and drop your ${multiple ? "files" : "file"} here or click to select`}
              </p>
            )}
            {/* Show file list when files are present in Drop variant */}
            {shouldShowFileList && (
              <div className="w-full mt-4">
                <FileAttachmentList
                  files={fileList}
                  uploadProgress={uploadProgress}
                  onCancel={handleCancel}
                  hasCancelHandler={hasCancelHandler}
                  variant="card"
                />
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
