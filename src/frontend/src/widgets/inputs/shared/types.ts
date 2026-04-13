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
