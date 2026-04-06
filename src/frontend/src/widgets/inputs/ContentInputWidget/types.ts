import { Densities } from "@/types/density";
import { FileUploadStatus, FileItem } from "../shared/types";

export { FileUploadStatus, type FileItem };

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
  shortcutKey?: string;
  files?: FileItem[];
  "data-testid"?: string;
}
