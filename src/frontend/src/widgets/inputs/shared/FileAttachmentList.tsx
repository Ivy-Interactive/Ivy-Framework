import React from "react";
import { X } from "lucide-react";
import { cva } from "class-variance-authority";
import { Button } from "@/components/ui/button";
import { Densities } from "@/types/density";
import { FileItem, FileUploadStatus } from "./types";
import { formatBytes } from "@/lib/formatters";

const compactContainerVariant = cva(
  "flex flex-nowrap overflow-x-auto flex-1 min-w-0 slim-scrollbar",
  {
    variants: {
      density: {
        Small: "gap-1 py-1",
        Medium: "gap-2 py-1.5",
        Large: "gap-2 py-2",
      },
    },
    defaultVariants: { density: "Medium" },
  },
);

const compactItemVariant = cva(
  "flex items-center border border-muted-foreground/25 rounded-md bg-muted/30",
  {
    variants: {
      density: {
        Small: "gap-1 px-1.5 py-0.5 text-[10px]",
        Medium: "gap-2 px-2 py-1 text-xs",
        Large: "gap-2 px-2.5 py-1.5 text-sm",
      },
    },
    defaultVariants: { density: "Medium" },
  },
);

const compactCancelVariant = cva("shrink-0 p-0", {
  variants: {
    density: {
      Small: "h-4 w-4",
      Medium: "h-5 w-5",
      Large: "h-6 w-6",
    },
  },
  defaultVariants: { density: "Medium" },
});

const compactCancelIconVariant = cva("", {
  variants: {
    density: {
      Small: "h-2.5 w-2.5",
      Medium: "h-3 w-3",
      Large: "h-4 w-4",
    },
  },
  defaultVariants: { density: "Medium" },
});

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
    const fileName = clientId.replace(/^upload-[a-f0-9-]+-/, "");

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
        density === Densities.Small
          ? "h-3 w-3"
          : density === Densities.Large
            ? "h-5 w-5"
            : "h-4 w-4";

      return (
        <div
          key={clientId}
          data-file-item
          className={`flex items-center gap-3 ${cardPadding} border border-muted-foreground/25 rounded-md bg-transparent`}
        >
          <div className="flex-1 min-w-0">
            <p className={`${cardText} font-medium truncate`}>{fileName}</p>
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
            aria-label="Cancel upload"
            className={`${cancelBtnSize} shrink-0`}
            onClick={(e) => {
              e.stopPropagation();
              onCancel?.(clientId);
            }}
          >
            <X className={cancelIconSize} />
          </Button>
        </div>
      );
    }

    return (
      <div key={clientId} data-file-item className={compactItemVariant({ density })}>
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
          aria-label="Cancel upload"
          className={compactCancelVariant({ density })}
          onClick={(e) => {
            e.stopPropagation();
            onCancel?.(clientId);
          }}
        >
          <X className={compactCancelIconVariant({ density })} />
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
      <div className="space-y-2 slim-scrollbar">
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
                  aria-label="Remove file"
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
    <div className={compactContainerVariant({ density })}>
      {/* Client-side uploading files */}
      {uploadProgress &&
        Array.from(uploadProgress.entries()).map(([clientId, progress]) =>
          renderProgressItem(clientId, progress),
        )}

      {/* Server-tracked files */}
      {files.map((file) => {
        const isLoading = file.status === FileUploadStatus.Loading;
        return (
          <div key={file.id} data-file-item className={compactItemVariant({ density })}>
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
                aria-label="Remove file"
                className={compactCancelVariant({ density })}
                onClick={(e) => {
                  e.stopPropagation();
                  onCancel?.(file.id);
                }}
              >
                <X className={compactCancelIconVariant({ density })} />
              </Button>
            )}
          </div>
        );
      })}
    </div>
  );
};
