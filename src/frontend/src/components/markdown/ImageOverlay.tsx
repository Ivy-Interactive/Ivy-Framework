import React, { useEffect, useRef } from "react";
import { createPortal } from "react-dom";
import { validateImageUrl } from "@/lib/url";

interface ImageOverlayProps {
  src: string | undefined;
  alt: string | undefined;
  onClose: () => void;
  images?: { src: string; alt: string }[];
  currentIndex?: number;
  onNavigate?: (index: number) => void;
}

export const ImageOverlay: React.FC<ImageOverlayProps> = ({
  src,
  alt,
  onClose,
  images,
  currentIndex = 0,
  onNavigate,
}) => {
  const overlayRef = useRef<HTMLDivElement>(null);

  const hasNavigation = images && images.length > 1 && onNavigate;

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
      if (hasNavigation) {
        if (e.key === "ArrowLeft") {
          e.preventDefault();
          const len = images.length;
          onNavigate((((currentIndex - 1) % len) + len) % len);
        } else if (e.key === "ArrowRight") {
          e.preventDefault();
          onNavigate((currentIndex + 1) % images.length);
        }
      }
      handleTabKey(e);
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [onClose, hasNavigation, images, currentIndex, onNavigate]);

  // Validate and sanitize image URL to prevent open redirect vulnerabilities
  const validatedSrc = src ? validateImageUrl(src) : null;
  if (!validatedSrc) {
    // Invalid URL, don't render image
    return null;
  }

  const content = (
    <div
      ref={overlayRef}
      className="fixed inset-0 bg-black/70 flex items-center justify-center z-50 cursor-zoom-out overflow-hidden"
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
      role="dialog"
      aria-modal="true"
      aria-label="Image Overlay"
    >
      <div className="relative max-w-[90vw] max-h-[90vh] flex items-center">
        {hasNavigation && (
          <button
            className="absolute -left-12 top-1/2 -translate-y-1/2 bg-black/50 hover:bg-black/70 text-white rounded-full w-10 h-10 flex items-center justify-center transition-colors z-10"
            onClick={(e) => {
              e.stopPropagation();
              const len = images.length;
              onNavigate((((currentIndex - 1) % len) + len) % len);
            }}
            aria-label="Previous image"
          >
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <polyline points="15 18 9 12 15 6" />
            </svg>
          </button>
        )}
        <div className="relative">
          <img src={validatedSrc} alt={alt} className="max-w-full max-h-[90vh] object-contain" />
          <button
            className="absolute top-4 right-4 bg-black/50 hover:bg-black/70 text-white rounded-full w-8 h-8 flex items-center justify-center transition-colors"
            onClick={onClose}
          >
            ✕
          </button>
          {hasNavigation && (
            <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-black/50 text-white text-sm px-3 py-1 rounded-full">
              {currentIndex + 1} / {images.length}
            </div>
          )}
        </div>
        {hasNavigation && (
          <button
            className="absolute -right-12 top-1/2 -translate-y-1/2 bg-black/50 hover:bg-black/70 text-white rounded-full w-10 h-10 flex items-center justify-center transition-colors z-10"
            onClick={(e) => {
              e.stopPropagation();
              onNavigate((currentIndex + 1) % images.length);
            }}
            aria-label="Next image"
          >
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <polyline points="9 18 15 12 9 6" />
            </svg>
          </button>
        )}
      </div>
    </div>
  );

  // When used standalone (not via context provider), wrap in portal
  // When used via context provider, the provider handles the portal
  if (!images) {
    return createPortal(content, document.body);
  }

  return content;
};
