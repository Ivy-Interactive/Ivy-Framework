import { validateFileWithToast } from "@/widgets/inputs/file-input-validation";

/**
 * Get the full upload URL, accounting for the ivy-host meta tag.
 */
export function getFullUrl(path: string): string {
  const ivyHostMeta = document.querySelector('meta[name="ivy-host"]');
  if (ivyHostMeta) {
    const host = ivyHostMeta.getAttribute("content");
    return host + path;
  }
  return path;
}

/**
 * Validate a file against accept, maxFileSize, and minFileSize constraints.
 * Shows a toast on validation failure and returns false.
 */
export function validateFile(
  file: File,
  accept?: string,
  maxFileSize?: number,
  minFileSize?: number,
): boolean {
  return validateFileWithToast({ file, accept, maxFileSize, minFileSize });
}

/**
 * Upload a file to the given upload URL using FormData.
 */
export async function uploadFile(uploadUrl: string, file: File): Promise<void> {
  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch(getFullUrl(uploadUrl), {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    throw new Error(`Upload failed: ${response.statusText}`);
  }
}

/**
 * Convert an accept string (e.g. "image/*,.pdf") to the FileSystemAccess API types format.
 */
export function acceptToPickerTypes(accept?: string): FilePickerAcceptType[] | undefined {
  if (!accept) return undefined;

  const acceptMap: Record<string, FileExtension[]> = {};
  const parts = accept.split(",").map((s) => s.trim());

  for (const part of parts) {
    if (part.includes("/")) {
      // MIME type like "image/*" or "application/pdf"
      if (!acceptMap[part]) {
        acceptMap[part] = [];
      }
    } else if (part.startsWith(".")) {
      // File extension like ".pdf"
      const mimeType = "application/octet-stream";
      const existing = acceptMap[mimeType] || [];
      existing.push(part as FileExtension);
      acceptMap[mimeType] = existing;
    }
  }

  if (Object.keys(acceptMap).length === 0) return undefined;

  return [
    {
      description: "Accepted files",
      accept: acceptMap as Record<MIMEType, FileExtension[]>,
    },
  ];
}
