import { useRef, useEffect } from "react";

interface UseDialogBlurTrackingOptions {
  enabled: boolean;
  onBlur: () => void;
}

interface UseDialogBlurTrackingResult {
  markDialogOpened: () => void;
  markFilesSelected: () => void;
  markBlurFired: () => void;
}

export function useDialogBlurTracking({
  enabled,
  onBlur,
}: UseDialogBlurTrackingOptions): UseDialogBlurTrackingResult {
  const dialogWasOpenRef = useRef(false);
  const filesSelectedInCurrentDialogRef = useRef(false);
  const blurFiredRef = useRef(false);

  // Detect when file dialog closes without selection (cancel case only)
  useEffect(() => {
    if (!enabled) return;

    const handleWindowFocus = () => {
      if (dialogWasOpenRef.current) {
        dialogWasOpenRef.current = false;
        // Use queueMicrotask to allow onChange to run first
        // This prevents double blur when files are selected
        queueMicrotask(() => {
          // Check if files were actually selected by looking at the flag
          // If files were selected, blur will be handled by handleChange after upload
          // Only fire blur if no files were selected (cancel case) and we haven't already fired
          if (!filesSelectedInCurrentDialogRef.current && !blurFiredRef.current) {
            blurFiredRef.current = true;
            onBlur();
          }
        });
      }
    };

    window.addEventListener("focus", handleWindowFocus);

    return () => {
      window.removeEventListener("focus", handleWindowFocus);
    };
  }, [enabled, onBlur]);

  const markDialogOpened = () => {
    dialogWasOpenRef.current = true;
    filesSelectedInCurrentDialogRef.current = false;
    blurFiredRef.current = false;
  };

  const markFilesSelected = () => {
    filesSelectedInCurrentDialogRef.current = true;
  };

  const markBlurFired = () => {
    if (!blurFiredRef.current) {
      blurFiredRef.current = true;
      onBlur();
    }
  };

  return { markDialogOpened, markFilesSelected, markBlurFired };
}
