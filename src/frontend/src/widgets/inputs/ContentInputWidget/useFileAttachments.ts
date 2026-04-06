import { useCallback, useState, useRef } from "react";
import { toast } from "@/hooks/use-toast";
import { validateFileWithToast, validateFileCount } from "../file-input-validation";

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
  const fileInputRef = useRef<HTMLInputElement>(null);

  const uploadFile = useCallback(
    async (file: File): Promise<void> => {
      if (!uploadUrl) return;
      if (!validateFileWithToast({ file, accept, maxFileSize })) return;

      const getUploadUrl = () => {
        const ivyHostMeta = document.querySelector('meta[name="ivy-host"]');
        if (ivyHostMeta) {
          const host = ivyHostMeta.getAttribute("content");
          return host + uploadUrl;
        }
        return uploadUrl;
      };

      const formData = new FormData();
      formData.append("file", file);

      try {
        const response = await fetch(getUploadUrl(), {
          method: "POST",
          body: formData,
        });
        if (!response.ok) {
          throw new Error(`Upload failed: ${response.statusText}`);
        }
      } catch (error) {
        console.error("File upload error:", error);
      }
    },
    [uploadUrl, accept, maxFileSize],
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

      await Promise.all(files.map(uploadFile));
    },
    [currentFileCount, maxFiles, uploadFile],
  );

  const handlePaste = useCallback(
    (e: React.ClipboardEvent) => {
      if (disabled || !uploadUrl) return;
      const items = e.clipboardData?.items;
      if (!items) return;

      const files: File[] = [];
      for (let i = 0; i < items.length; i++) {
        if (items[i].kind === "file") {
          const file = items[i].getAsFile();
          if (file) files.push(file);
        }
      }

      if (files.length > 0) {
        e.preventDefault();
        uploadFiles(files);
      }
      // If no file items, allow normal text paste to proceed
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

      const files = Array.from(e.dataTransfer.files);
      await uploadFiles(files);
    },
    [disabled, uploadFiles],
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
      await uploadFiles(Array.from(fileList));
      e.target.value = "";
    },
    [uploadFiles],
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
  };
}
