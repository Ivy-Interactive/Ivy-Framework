import React from "react";
import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { FileItem, FileUploadStatus } from "./types";
import { formatBytes } from "../file-input-validation";

interface FileAttachmentListProps {
  files: FileItem[];
  onCancel?: (fileId: string) => void;
  hasCancelHandler: boolean;
}

export const FileAttachmentList: React.FC<FileAttachmentListProps> = ({
  files,
  onCancel,
  hasCancelHandler,
}) => {
  if (files.length === 0) return null;

  return (
    <div className="flex flex-wrap gap-2 px-3 pb-2">
      {files.map((file) => {
        const isLoading = file.status === FileUploadStatus.Loading;
        return (
          <div
            key={file.id}
            data-file-item
            className="flex items-center gap-2 px-2 py-1 border border-muted-foreground/25 rounded-md bg-muted/30 text-xs"
          >
            <span className="truncate max-w-[150px]" title={file.fileName}>
              {file.fileName}
            </span>
            <span className="text-muted-foreground whitespace-nowrap">
              {formatBytes(file.length)}
            </span>
            {isLoading && (
              <div className="w-12 bg-muted rounded-full h-1">
                <div
                  className="bg-primary h-1 rounded-full transition-all duration-300"
                  style={{ width: `${file.progress * 100}%` }}
                />
              </div>
            )}
            {hasCancelHandler && (
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="h-5 w-5 shrink-0 p-0"
                onClick={(e) => {
                  e.stopPropagation();
                  onCancel?.(file.id);
                }}
              >
                <X className="h-3 w-3" />
              </Button>
            )}
          </div>
        );
      })}
    </div>
  );
};
