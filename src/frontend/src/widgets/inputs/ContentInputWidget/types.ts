import { Densities } from "@/types/density";

export enum FileUploadStatus {
  Pending = "Pending",
  Aborted = "Aborted",
  Loading = "Loading",
  Failed = "Failed",
  Finished = "Finished",
}

export interface FileItem {
  id: string;
  fileName: string;
  contentType: string;
  length: number;
  progress: number;
  status: FileUploadStatus;
}

export interface ContentInputWidgetProps {
  id: string;
  value?: string;
  disabled?: boolean;
  invalid?: string;
  placeholder?: string;
  maxLength?: number;
  rows?: number;
  autoFocus?: boolean;
  nullable?: boolean;
  events: string[];
  width?: string;
  density?: Densities;
  uploadUrl?: string;
  accept?: string;
  maxFileSize?: number;
  maxFiles?: number;
  files?: FileItem[];
  "data-testid"?: string;
}
