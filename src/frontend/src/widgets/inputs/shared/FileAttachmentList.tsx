import React from "react";
import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Densities } from "@/types/density";
import { FileItem, FileUploadStatus } from "./types";
import { formatBytes } from "../file-input-validation";

interface FileAttachmentListProps {
  files: FileItem[];
  uploadProgress?: Map<string, number>;
  onCancel?: (fileId: string) => void;
  hasCancelHandler: boolean;
  variant?: "compact" | "card";
  density?: Densities;
}

export const FileAttachmentList: React.FC<FileAttachmentListProps> = ({
  files,
  uploadProgress,
  onCancel,
  hasCancelHandler,
  variant = "compact",
  density = Densities.Medium,
}) => {
  const hasUploadingFiles = uploadProgress && uploadProgress.size > 0;
  if (files.length === 0 && !hasUploadingFiles) return null;

  const renderProgressItem = (clientId: string, progress: number) => {
    const fileName = clientId.replace(/^upload-\d+-/, "");

    if (variant === "card") {
      return (
        <div
          key={clientId}
          data-file-item
          className="flex items-center gap-3 p-3 border border-muted-foreground/25 rounded-md bg-transparent"
        >
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium truncate">{fileName}</p>
            <div className="mt-2">
              <div className="w-full bg-muted rounded-full h-1.5">
                <div
                  className="bg-primary h-1.5 rounded-full transition-all duration-300"
                  style={{ width: `${progress * 100}%` }}
                />
              </div>
            </div>
          </div>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="h-8 w-8 shrink-0"
            onClick={(e) => {
              e.stopPropagation();
              onCancel?.(clientId);
            }}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      );
    }

    return (
      <div
        key={clientId}
        data-file-item
        className="flex items-center gap-2 px-2 py-1 border border-muted-foreground/25 rounded-md bg-muted/30 text-xs"
      >
        <span className="truncate max-w-[150px]" title={fileName}>
          {fileName}
        </span>
        <div className="w-12 bg-muted rounded-full h-1">
          <div
            className="bg-primary h-1 rounded-full transition-all duration-300"
            style={{ width: `${progress * 100}%` }}
          />
        </div>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className="h-5 w-5 shrink-0 p-0"
          onClick={(e) => {
            e.stopPropagation();
            onCancel?.(clientId);
          }}
        >
          <X className="h-3 w-3" />
        </Button>
      </div>
    );
  };

  if (variant === "card") {
    const cardPadding =
      density === Densities.Small ? "p-2" : density === Densities.Large ? "p-4" : "p-3";
    const cardText =
      density === Densities.Small
        ? "text-xs"
        : density === Densities.Large
          ? "text-base"
          : "text-sm";
    const cancelBtnSize =
      density === Densities.Small
        ? "h-6 w-6"
        : density === Densities.Large
          ? "h-10 w-10"
          : "h-8 w-8";
    const cancelIconSize =
      density === Densities.Small ? "h-3 w-3" : density === Densities.Large ? "h-5 w-5" : "h-4 w-4";

    return (
      <div className="space-y-2">
        {/* Client-side uploading files */}
        {uploadProgress &&
          Array.from(uploadProgress.entries()).map(([clientId, progress]) =>
            renderProgressItem(clientId, progress),
          )}

        {/* Server-tracked files */}
        {files.map((file) => {
          const isLoading = file.status === FileUploadStatus.Loading;
          // Prefer server-side progress when available, fall back to any client-side progress
          const clientProgress =
            uploadProgress &&
            Array.from(uploadProgress.entries()).find(([key]) =>
              key.endsWith(`-${file.length}-${file.fileName}`),
            );
          const fileProgress =
            isLoading && file.progress === 0 && clientProgress
              ? clientProgress[1]
              : (file.progress ?? 0);

          return (
            <div
              key={file.id}
              data-file-item
              className={`flex items-center gap-3 ${cardPadding} border border-muted-foreground/25 rounded-md bg-transparent`}
            >
              <div className="flex-1 min-w-0">
                <p className={`${cardText} font-medium truncate`}>{file.fileName}</p>
                {isLoading && (
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
                  className={`${cancelBtnSize} shrink-0`}
                  onClick={(e) => {
                    e.stopPropagation();
                    onCancel?.(file.id);
                  }}
                >
                  <X className={cancelIconSize} />
                </Button>
              )}
            </div>
          );
        })}
      </div>
    );
  }

  return (
    <div className="flex flex-wrap gap-2 px-3 pb-2">
      {/* Client-side uploading files */}
      {uploadProgress &&
        Array.from(uploadProgress.entries()).map(([clientId, progress]) =>
          renderProgressItem(clientId, progress),
        )}

      {/* Server-tracked files */}
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
                  style={{ width: `${(file.progress ?? 0) * 100}%` }}
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
