import { useState, useRef, useCallback, useEffect } from "react";
import { uploadFileWithProgress } from "@/widgets/filePicker/shared";
import { toast } from "@/hooks/use-toast";

export function useUploadWithProgress() {
  const [uploadProgress, setUploadProgress] = useState<Map<string, number>>(new Map());
  const abortControllersRef = useRef<Map<string, () => void>>(new Map());

  // Abort any pending uploads when the component unmounts
  useEffect(() => {
    return () => {
      abortControllersRef.current.forEach((abort) => abort());
    };
  }, []);

  const uploadSingleFile = useCallback(async (uploadUrl: string, file: File): Promise<void> => {
    const clientFileId = `upload-${crypto.randomUUID()}-${file.size}-${file.name}`;

    setUploadProgress((prev) => new Map(prev).set(clientFileId, 0));

    const { promise, abort } = uploadFileWithProgress(uploadUrl, file, (progress) => {
      setUploadProgress((prev) => new Map(prev).set(clientFileId, progress));
    });

    abortControllersRef.current.set(clientFileId, abort);

    try {
      await promise;
    } catch (error: any) {
      if (error.message !== "Upload aborted") {
        console.error("File upload error:", error);
        toast({
          title: "Upload failed",
          description: error.message || `Could not upload ${file.name}`,
          variant: "destructive",
        });
      }
    } finally {
      setUploadProgress((prev) => {
        const next = new Map(prev);
        next.delete(clientFileId);
        return next;
      });
      abortControllersRef.current.delete(clientFileId);
    }
  }, []);

  const cancelUpload = useCallback((clientFileId: string) => {
    const abort = abortControllersRef.current.get(clientFileId);
    if (abort) {
      abort();
      abortControllersRef.current.delete(clientFileId);
    }
  }, []);

  return { uploadProgress, uploadSingleFile, cancelUpload };
}
