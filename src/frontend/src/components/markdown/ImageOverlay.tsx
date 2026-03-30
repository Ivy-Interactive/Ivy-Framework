import React, { useEffect, useRef } from "react";
import { createPortal } from "react-dom";
import { validateImageUrl } from "@/lib/url";

interface ImageOverlayProps {
  src: string | undefined;
  alt: string | undefined;
  onClose: () => void;
}

export const ImageOverlay: React.FC<ImageOverlayProps> = ({ src, alt, onClose }) => {
  const overlayRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const overlay = overlayRef.current;
    if (!overlay) return;

    // Find all focusable elements
    const focusableElements = overlay.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])',
    );
    const firstElement = focusableElements[0] as HTMLElement;
    const lastElement = focusableElements[focusableElements.length - 1] as HTMLElement;

    // Focus the first element (close button)
    firstElement?.focus();

    const handleTabKey = (e: KeyboardEvent) => {
      if (e.key === "Tab") {
        if (e.shiftKey) {
          if (document.activeElement === firstElement) {
            lastElement.focus();
            e.preventDefault();
          }
        } else {
          if (document.activeElement === lastElement) {
            firstElement.focus();
            e.preventDefault();
          }
        }
      }
    };

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onClose();
      }
      handleTabKey(e);
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [onClose]);

  // Validate and sanitize image URL to prevent open redirect vulnerabilities
  const validatedSrc = src ? validateImageUrl(src) : null;
  if (!validatedSrc) {
    // Invalid URL, don't render image
    return null;
  }

  return createPortal(
    <div
      ref={overlayRef}
      className="fixed inset-0 bg-black/70 flex items-center justify-center z-50 cursor-zoom-out"
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
      role="dialog"
      aria-modal="true"
      aria-label="Image Overlay"
    >
      <div className="relative max-w-[90vw] max-h-[90vh]">
        <img src={validatedSrc} alt={alt} className="max-w-full max-h-[90vh] object-contain" />
        <button
          className="absolute top-4 right-4 bg-black/50 text-white rounded-full w-8 h-8 flex items-center justify-center"
          onClick={onClose}
        >
          ✕
        </button>
      </div>
    </div>,
    document.body,
  );
};
