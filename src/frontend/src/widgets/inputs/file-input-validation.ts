export const formatBytes = (bytes: number): string => {
  const sizes = ["B", "KB", "MB", "GB", "TB"];
  if (bytes === 0) return "0 B";
  const i = Math.floor(Math.log(bytes) / Math.log(1024));
  const size = bytes / Math.pow(1024, i);
  return `${size.toFixed(size >= 10 ? 0 : 2)} ${sizes[i]}`;
};

export interface FileValidationContext {
  file: File;
  accept?: string;
  maxFileSize?: number;
  minFileSize?: number;
}

export interface ValidationResult {
  valid: boolean;
  title?: string;
  error?: string;
}

export function validateSingleFile(ctx: FileValidationContext): ValidationResult {
  const { file, accept, maxFileSize, minFileSize } = ctx;

  // Type validation
  if (accept) {
    const acceptedTypes = accept.split(",").map((t) => t.trim().toLowerCase());
    if (acceptedTypes.length > 0) {
      const fileName = file.name.toLowerCase();
      const fileType = file.type.toLowerCase();

      const isValidType = acceptedTypes.some((type) => {
        if (type.endsWith("/*")) {
          const baseType = type.split("/")[0];
          return fileType.startsWith(`${baseType}/`);
        }
        if (type.includes("/")) {
          return type === fileType;
        }
        if (type.startsWith(".")) {
          return fileName.endsWith(type);
        }
        return false;
      });

      if (!isValidType) {
        return {
          valid: false,
          title: "Invalid file type",
          error: `File '${file.name}' has an invalid type. Accepted types: ${accept}`,
        };
      }
    }
  }

  // Size validation
  if (minFileSize && file.size < minFileSize) {
    const minSizeFormatted = formatBytes(minFileSize);
    const fileSizeFormatted = formatBytes(file.size);
    return {
      valid: false,
      title: "File too small",
      error: `File '${file.name}' is ${fileSizeFormatted}. Minimum allowed size is ${minSizeFormatted}.`,
    };
  }

  if (maxFileSize && file.size > maxFileSize) {
    const maxSizeFormatted = formatBytes(maxFileSize);
    const fileSizeFormatted = formatBytes(file.size);
    return {
      valid: false,
      title: "File too large",
      error: `File '${file.name}' is ${fileSizeFormatted}. Maximum allowed size is ${maxSizeFormatted}.`,
    };
  }

  return { valid: true };
}

export function validateFileCount(
  currentCount: number,
  addingCount: number,
  maxFiles?: number,
): ValidationResult {
  if (maxFiles && currentCount + addingCount > maxFiles) {
    const remaining = maxFiles - currentCount;
    const errorDescription =
      remaining > 0
        ? `You can only upload ${remaining} more file${remaining !== 1 ? "s" : ""}. Maximum is ${maxFiles} files total.`
        : `Maximum of ${maxFiles} file${maxFiles !== 1 ? "s" : ""} already reached.`;
    return {
      valid: false,
      title: "Too many files",
      error: errorDescription,
    };
  }
  return { valid: true };
}
