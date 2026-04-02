import React, { createContext, useCallback, useContext, useMemo, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { ImageOverlay } from "./ImageOverlay";

interface ImageEntry {
  src: string;
  alt: string;
}

interface ImageOverlayContextValue {
  register: (id: string, src: string, alt: string) => void;
  unregister: (id: string) => void;
  open: (id: string) => void;
}

const ImageOverlayContext = createContext<ImageOverlayContextValue | null>(null);

export const useImageOverlayContext = () => useContext(ImageOverlayContext);

export const ImageOverlayProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const imagesRef = useRef<Map<string, ImageEntry>>(new Map());
  const [overlayState, setOverlayState] = useState<{
    images: ImageEntry[];
    currentIndex: number;
  } | null>(null);

  const register = useCallback((id: string, src: string, alt: string) => {
    imagesRef.current.set(id, { src, alt });
  }, []);

  const unregister = useCallback((id: string) => {
    imagesRef.current.delete(id);
  }, []);

  const open = useCallback((id: string) => {
    const images = Array.from(imagesRef.current.entries());
    const index = images.findIndex(([key]) => key === id);
    if (index === -1) return;
    setOverlayState({
      images: images.map(([, entry]) => entry),
      currentIndex: index,
    });
  }, []);

  const handleNavigate = useCallback((index: number) => {
    setOverlayState((prev) => {
      if (!prev) return null;
      const len = prev.images.length;
      const newIndex = ((index % len) + len) % len;
      return { ...prev, currentIndex: newIndex };
    });
  }, []);

  const handleClose = useCallback(() => {
    setOverlayState(null);
  }, []);

  const contextValue = useMemo(
    () => ({ register, unregister, open }),
    [register, unregister, open],
  );

  return (
    <ImageOverlayContext.Provider value={contextValue}>
      {children}
      {overlayState &&
        createPortal(
          <ImageOverlay
            src={overlayState.images[overlayState.currentIndex].src}
            alt={overlayState.images[overlayState.currentIndex].alt}
            images={overlayState.images}
            currentIndex={overlayState.currentIndex}
            onNavigate={handleNavigate}
            onClose={handleClose}
          />,
          document.body,
        )}
    </ImageOverlayContext.Provider>
  );
};
