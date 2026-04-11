import { useCallback, useState, useRef } from "react";
import { toast } from "@/hooks/use-toast";
import { validateFileWithToast, validateFileCount } from "../file-input-validation";
import { useUploadWithProgress } from "../shared/useUploadWithProgress";

interface UseFileAttachmentsOptions {
  uploadUrl?: string;
  accept?: string;
  maxFileSize?: number;
  maxFiles?: number;
  currentFileCount: number;
  disabled?: boolean;
  onCancel?: (fileId: string) => void;
}

export function useFileAttachments(options: UseFileAttachmentsOptions) {
  const { uploadUrl, accept, maxFileSize, maxFiles, currentFileCount, disabled } = options;
  const [isDragging, setIsDragging] = useState(false);
  const { uploadProgress, uploadSingleFile, cancelUpload } = useUploadWithProgress();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleUploadFile = useCallback(
    async (file: File): Promise<void> => {
      if (!uploadUrl) return;
      if (!validateFileWithToast({ file, accept, maxFileSize })) return;

      await uploadSingleFile(uploadUrl, file);
    },
    [uploadUrl, accept, maxFileSize, uploadSingleFile],
  );

  const uploadFiles = useCallback(
    async (files: File[]) => {
      if (files.length === 0) return;

      const countValidation = validateFileCount(currentFileCount, files.length, maxFiles);
      if (!countValidation.valid) {
        toast({
          title: countValidation.title || "Too many files",
          description: countValidation.error,
          variant: "destructive",
        });
        return;
      }

      await Promise.all(files.map(handleUploadFile));
    },
    [currentFileCount, maxFiles, handleUploadFile],
  );

  const handlePaste = useCallback(
    (e: React.ClipboardEvent) => {
      const items = e.clipboardData?.items;
      if (!items) return;

      const files: File[] = [];
      for (let i = 0; i < items.length; i++) {
        if (items[i].kind === "file") {
          const file = items[i].getAsFile();
          if (file) files.push(file);
        }
      }

      if (files.length === 0) return; // No files — let normal text paste proceed

      e.preventDefault(); // Prevent binary garbage in textarea

      if (disabled || !uploadUrl) {
        if (!uploadUrl) {
          toast({
            title: "Upload not available",
            description: "File attachments require an upload URL to be configured.",
            variant: "destructive",
          });
        }
        return;
      }

      uploadFiles(files);
    },
    [disabled, uploadUrl, uploadFiles],
  );

  const handleDragEnter = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!disabled) setIsDragging(true);
    },
    [disabled],
  );

  const handleDragOver = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!disabled && !isDragging) setIsDragging(true);
    },
    [disabled, isDragging],
  );

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  }, []);

  const handleDrop = useCallback(
    async (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      setIsDragging(false);
      if (disabled) return;
      if (!uploadUrl) {
        toast({
          title: "Upload not available",
          description: "File uploads are not configured for this input.",
          variant: "destructive",
        });
        return;
      }

      const files = Array.from(e.dataTransfer.files);
      await uploadFiles(files);
    },
    [disabled, uploadUrl, uploadFiles],
  );

  const openFilePicker = useCallback(() => {
    if (!disabled && fileInputRef.current) {
      fileInputRef.current.click();
    }
  }, [disabled]);

  const handleFileInputChange = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const fileList = e.target.files;
      if (!fileList || fileList.length === 0) return;
      if (!uploadUrl) {
        toast({
          title: "Upload not available",
          description: "File uploads are not configured for this input.",
          variant: "destructive",
        });
        e.target.value = "";
        return;
      }
      await uploadFiles(Array.from(fileList));
      e.target.value = "";
    },
    [uploadUrl, uploadFiles],
  );

  const dragHandlers = {
    onDragEnter: handleDragEnter,
    onDragOver: handleDragOver,
    onDragLeave: handleDragLeave,
    onDrop: handleDrop,
  };

  return {
    isDragging,
    dragHandlers,
    handlePaste,
    openFilePicker,
    handleFileInputChange,
    fileInputRef,
    uploadProgress,
    cancelUpload,
  };
}
