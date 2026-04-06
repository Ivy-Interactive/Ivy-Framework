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
 * Upload a file using XMLHttpRequest with progress tracking.
 * Returns a promise that resolves on success and an abort function.
 */
export function uploadFileWithProgress(
  uploadUrl: string,
  file: File,
  onProgress?: (progress: number) => void,
): { promise: Promise<void>; abort: () => void } {
  const xhr = new XMLHttpRequest();
  const fullUrl = getFullUrl(uploadUrl);
  const formData = new FormData();
  formData.append("file", file);

  const promise = new Promise<void>((resolve, reject) => {
    xhr.upload.onprogress = (e) => {
      if (e.lengthComputable && onProgress) {
        onProgress(e.loaded / e.total);
      }
    };
    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) resolve();
      else reject(new Error(`Upload failed: ${xhr.statusText}`));
    };
    xhr.onerror = () => reject(new Error("Upload failed"));
    xhr.onabort = () => reject(new Error("Upload aborted"));
    xhr.open("POST", fullUrl);
    xhr.send(formData);
  });

  return { promise, abort: () => xhr.abort() };
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
